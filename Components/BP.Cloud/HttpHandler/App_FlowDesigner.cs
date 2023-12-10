using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using BP.DA;
using BP.Sys;
using BP.Web;
using BP.Port;
using BP.En;
using BP.WF;
using BP.WF.Template;
using BP.CCBill;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 流程设计器
    /// </summary>
    public class App_FlowDesigner : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App_FlowDesigner()
        {

        }
        /// <summary>
        /// 初始化目录文件
        /// </summary>
        /// <returns></returns>
        public string Icon_Init()
        {
            //模版.
            DataTable dtFrm = new DataTable();
            dtFrm.Columns.Add("No");
            dtFrm.Columns.Add("Name");
            dtFrm.TableName = "Frms";

            //目录文件.
            string path =  BP.Difference.SystemConfig.PathOfWebApp + "App/FlowDesigner/FlowICON/";

            //获得根目录.
            DirectoryInfo root = new DirectoryInfo(path);//目录信息
            foreach (FileInfo file in root.GetFiles("*.*"))
            {
                DataRow dr = dtFrm.NewRow();
                dr["No"] = file.FullName;
                dr["Name"] = file.Name;
                dtFrm.Rows.Add(dr); //加入到里面.
            }
            return BP.Tools.Json.ToJson(dtFrm);
        }
        /// <summary>
        /// 从云服务上安装
        /// </summary>
        /// <returns></returns>
        public string NewFlowByTemplateFromCloud_Save()
        {
            try
            {
                string frmID = this.GetRequestVal("FrmID");

                string file =  BP.Difference.SystemConfig.PathOfWebApp + "AppTemplate/DataTemp/" + frmID + ".xml";
                if (System.IO.File.Exists(file) == false)
                {
                    //读取表单的数据.
                    string url = "http://template." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?FrmID=" + frmID;
                    string docs = BP.DA.DataType.ReadURLContext(url, 2000);
                    DataType.WriteFile(file, docs);
                }

                //读取模版信息到 dataset.
                DataSet ds = new DataSet();
                ds.ReadXml(file);

                DataTable dtFrm = ds.Tables["Frm"];

                //初始化这个属性.
                BP.FrmTemplate.Frm frm = new BP.FrmTemplate.Frm();
                frm.Row.LoadDataTable(dtFrm, dtFrm.Rows[0]);

                #region 如果导入的是一个流程.
                if (frm.WorkModel == 0 || frm.WorkModel == 1)
                {
                    string fileName = frm.Name;  //ds.Tables["Sys_MapData"].Rows[0]["Name"].ToString();


                    //执行创建流程.
                    string flowNo = this.AppInstall_NewFlow(frm.SpecFlowNo);

                    //开始节点ID.
                    int nodeID = int.Parse(flowNo + "01");

                    //导入表单到开始节点上.
                    MapData.ImpMapData("ND" + nodeID, ds);

                    //更新名字.
                    Flow fl = new Flow(flowNo);
                    fl.Name = fileName;
                    fl.DirectUpdate();
                    return flowNo;
                }
                #endregion 如果导入的是一个流程.

                #region 如果导入的是一个实体单据.
                if (frm.WorkModel == 2)
                {
                    FrmDict dict = new FrmDict();
                    dict.No = WebUser.OrgNo + "_" + frm.No;

                    int idx = 0;
                    while (dict.IsExits == false)
                    {
                        idx++;
                        dict.No = WebUser.OrgNo + "_" + frm.No + "_" + idx;
                    }

                    //导入到表单去.
                    MapData.ImpMapData(dict.No, ds);

                    //执行其他的更新.
                    MapData md = new MapData(dict.No);
                    md.HisEntityType = 2; //dictno.
                    md.OrgNo = WebUser.OrgNo;
                    if (idx != 0)
                        md.Name = md.Name + "_" + idx;

                    md.PTable = dict.No;
                    md.Update();

                    //初始化流程编号.
                    Init_FlowNo(md, frm.WorkModel);

                    //如果有关联的单据ID,就应该把他导入过来. @zhoupeng.
                    if (DataType.IsNullOrEmpty(frm.RefDictSpecFrmsNo) == false)
                    {
                        /*xxxx*/

                    }
                    return "dict@" + md.No;
                }
                #endregion 如果导入的是一个实体台账.

                #region 如果导入的是一个单据
                if (frm.WorkModel == 3)
                {
                    FrmDict dict = new FrmDict();
                    dict.No = WebUser.OrgNo + "_" + frm.No;

                    int idx = 0;
                    while (dict.IsExits == false)
                    {
                        idx++;
                        dict.No = WebUser.OrgNo + "_" + frm.No + "_" + idx;
                    }

                    //导入到表单去.
                    MapData.ImpMapData(dict.No, ds);

                    //执行其他的更新.
                    MapData md = new MapData(dict.No);
                    md.HisEntityType = 1; //dictno.
                    md.OrgNo = WebUser.OrgNo;
                    md.PTable = dict.No;
                    if (idx != 0)
                        md.Name = md.Name + "_" + idx;
                    md.Update();

                    //初始化流程编号.
                    Init_FlowNo(md, frm.WorkModel);

                    return "bill@" + md.No;
                }
                #endregion 如果导入的是一个单据.

                return "err@没有处理的workModel=" + frm.WorkModel;

            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }

        public string AppInstall_NewFlow(string flowNo)
        {
            try
            {
                string flowName = this.GetRequestVal("FlowName");
                string sortNo = this.GetRequestVal("FlowSort").Trim();

                #region 处理流程模版.
                string path = "";
                if (DataType.IsNullOrEmpty(flowNo) == false)
                {
                    path =  BP.Difference.SystemConfig.PathOfWebApp + "App/FlowDesigner/" + flowNo + ".xml";
                    if (System.IO.File.Exists(path) == false)
                    {
                        //下载这个流程模版到本机上去.
                        //读取表单的数据.
                        string url = "http://template." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?FlowNo=" + flowNo;
                        string docs = BP.DA.DataType.ReadURLContext(url, 2000);
                        DataType.WriteFile(path, docs);
                    }
                }
                else
                {
                    path =  BP.Difference.SystemConfig.PathOfWebApp + "App/FlowDesigner/NewFlowByTemplate.xml";
                }
                #endregion 处理流程模版.


                //装载模版.
                Flow flow = BP.WF.Template.TemplateGlo.LoadFlowTemplate(sortNo, path, ImpFlowTempleteModel.AsNewFlow, "", flowName);

                try
                {
                    //    if (flow.worn)
                    //把开始节点的审核组件打开.
                    Node nd = new Node(int.Parse(flow.No + "01"));
                    nd.FrmWorkCheckSta = FrmWorkCheckSta.Enable;
                    nd.Update();

                    BP.WF.Template.FrmNodeComponent mynd = new FrmNodeComponent(int.Parse(flow.No + "01"));
                    nd.Update();
                }
                catch
                {
                }

                //清空WF_Emp 的StartFlows ,让其重新计算.
                DBAccess.RunSQL("UPDATE  WF_Emp Set StartFlows ='' WHERE OrgNo='" + WebUser.OrgNo + "'");
                return flow.No;
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 初始化流程编号
        /// </summary>
        public void Init_FlowNo(MapData md, int workModel)
        {
            //插入一笔数据,
            Flow fl = new Flow();
            fl.No = fl.GenerNewNo;
            fl.Name = md.Name;
            fl.PTable = md.PTable;
            fl.OrgNo = WebUser.OrgNo;

            string sortNo = this.GetRequestVal("FlowSort").Trim();
            fl.FK_FlowSort = sortNo;

            fl.DirectInsert();

            //更新他的workmodel.
            DBAccess.RunSQL("UPDATE WF_Flow SET WorkModel=" + workModel + " WHERE No='" + fl.No + "'");
        }
        /// <summary>
        /// 新建流程模版
        /// </summary>
        /// <param name="flowNo">指定的流程编号</param>
        /// <returns></returns>
        public string NewFlow_Save()
        {
            try
            {
                string flowName = this.GetRequestVal("FlowName");
                string sortNo = this.GetRequestVal("FlowSort").Trim();
                int workModel = this.GetRequestValInt("WorkModel");
                string bindFrm = this.GetRequestVal("DictFrm");
                if (workModel == 0 || workModel == 1)
                {
                    #region 处理流程模版.
                    string path =  BP.Difference.SystemConfig.PathOfWebApp + "App/FlowDesigner/NewFlowByTemplate.xml";
                    #endregion 处理流程模版.

                    //装载模版.
                    Flow flow = BP.WF.Template.TemplateGlo.LoadFlowTemplate(sortNo, path, ImpFlowTempleteModel.AsNewFlow, "", flowName);

                    flow.PTable = "ND" + int.Parse(flow.No) + "Rpt";

                    //处理表单方案.
                    Nodes nds = new Nodes(flow.No);
                    foreach (Node nd in nds)
                    {
                        if (nd.IsStartNode == true)
                        {
                            nd.FrmWorkCheckSta = FrmWorkCheckSta.Enable;
                            nd.Update();

                            //让其执行一下更新.
                            BP.WF.Template.FrmNodeComponent mynd = new FrmNodeComponent(nd.NodeID);
                            mynd.Update();
                            continue;
                        }

                        if (nd.FrmWorkCheckSta == FrmWorkCheckSta.Disable)
                        {
                            nd.FrmWorkCheckSta = FrmWorkCheckSta.Readonly;
                            nd.Update();
                        }

                        BP.WF.Template.FrmNode fn = new FrmNode(nd.NodeID, "ND" + int.Parse(nd.FK_Flow + "01"));
                        fn.FrmSln = FrmSln.Readonly;
                        if (nd.FrmWorkCheckSta == FrmWorkCheckSta.Disable)
                            nd.FrmWorkCheckSta = FrmWorkCheckSta.Readonly;
                        fn.Save();


                        BP.WF.Template.FrmNodeComponent myndFrm = new FrmNodeComponent(nd.NodeID);
                        myndFrm.Update();
                    }

                    //设置为极简模式.
                    flow.FlowDevModel = FlowDevModel.JiJian;
                    flow.Update();

                    //清空WF_Emp 的StartFlows ,让其重新计算.
                    DBAccess.RunSQL("UPDATE  WF_Emp Set StartFlows ='' WHERE OrgNo='" + WebUser.OrgNo + "'");
                    return flow.No;
                }
                //实体台账
                if (workModel == 2)
                {
                    //保存数据到流程WF_Flow中
                    string flowNo = NewFlow_GenerFlowNo(flowName, sortNo, workModel);

                    //保存数据到Sys_MapData中
                    FrmDict dict = new FrmDict();
                    dict.No = "Frm_" + flowNo;
                    dict.Name = flowName;
                    dict.EntityType = EntityType.FrmDict;
                    dict.FrmType = FrmType.FoolForm;
                    dict.BillNoFormat = "{LSH4}"; //设置单据格式.
                    if (dict.RetrieveFromDBSources() == 0)
                        dict.Insert();
                    else
                        throw new Exception("err@" + dict.No + "表单已存在");

                    //执行其他的更新.
                    MapData md = new MapData(dict.No);
                    md.No = "Frm_" + flowNo;
                    md.Name = flowName;
                    md.HisEntityType = 2;
                    md.OrgNo = WebUser.OrgNo;
                    md.PTable = md.No;
                    md.Update();
                    return "dict@" + dict.No;
                }

                //单据
                if (workModel == 3)
                {
                    //保存数据到流程WF_Flow中
                    string flowNo = NewFlow_GenerFlowNo(flowName, sortNo, workModel);

                    //保存数据到Sys_MapData中
                    FrmBill bill = new FrmBill();
                    bill.No = "Frm_" + flowNo;
                    bill.Name = flowName;
                    bill.EntityType = EntityType.FrmBill;
                    bill.FrmType = FrmType.FoolForm;
                    bill.BillNoFormat = "NO:{yyyy}-{LSH4}"; //设置单据格式.
                    bill.RefDict = bindFrm;
                    if (bill.RetrieveFromDBSources() == 0)
                        bill.Insert();
                    else
                        throw new Exception("err@" + bill.No + "表单已存在");

                    //执行其他的更新.
                    MapData md = new MapData(bill.No);
                    md.No = "Frm_" + flowNo;
                    md.Name = flowName;
                    md.HisEntityType = 1;
                    md.OrgNo = WebUser.OrgNo;
                    md.PTable = md.No;
                    md.Update();
                    return "bill@" + bill.No;
                }
                return "";

            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        public string NewFlow_GenerFlowNo(string flowName, string flowSort, int workModel)
        {
            //插入一笔数据,
            Flow fl = new Flow();
            fl.No = fl.GenerNewNo;
            fl.Name = flowName;
            fl.OrgNo = WebUser.OrgNo;
            string sortNo = flowSort;
            fl.FK_FlowSort = sortNo;

            fl.DirectInsert();
            string ptable = "Frm_" + fl.No;

            //更新他的workmodel.
            DBAccess.RunSQL("UPDATE WF_Flow SET WorkModel=" + workModel + ",PTable='" + ptable + "' WHERE No='" + fl.No + "'");
            return fl.No;
        }
        /// <summary>
        /// 加载模版
        /// </summary>
        /// <returns></returns>
        public string NewFlowByTemplate_Save()
        {
            try
            {
                string sortName = this.GetRequestVal("SortName");

                //路径.
                string fileName = this.GetRequestVal("FileName");
                fileName = fileName.Replace(".png", ".xml");
                string filePath =  BP.Difference.SystemConfig.PathOfWebApp + "App/FlowDesigner/AppTemplate/" + sortName + "\\" + fileName;

                //读取模版信息到 dataset.
                DataSet ds = new DataSet();
                ds.ReadXml(filePath);

                //执行创建流程.
                //Admin_Portal en = new Admin_Portal();
                string flowNo = this.NewFlow_Save(); // en.Default_NewFlow();

                //开始节点ID.
                int nodeID = int.Parse(flowNo + "01");

                //流程名称.
                fileName = fileName.Replace(".xml", "");

                //导入表单到开始节点上.
                MapData.ImpMapData("ND" + nodeID, ds);

                //更新名字.
                Flow fl = new Flow(flowNo);
                fl.Name = fileName;

                //设置单据编号.
                if (DataType.IsNullOrEmpty(fl.BillNoFormat) == true)
                    fl.BillNoFormat = "NO:{yyyy}-{LSH4}";



                fl.DirectUpdate();

                return flowNo;
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 模版初始化.
        /// </summary>
        /// <returns></returns>
        public string Template_Init()
        {
            //数据容器.
            DataSet ds = new DataSet();

            //类别目录.
            DataTable dtSort = new DataTable();
            dtSort.Columns.Add("No");
            dtSort.Columns.Add("Name");
            dtSort.TableName = "Sorts";

            //模版.
            DataTable dtFrm = new DataTable();
            dtFrm.Columns.Add("No");
            dtFrm.Columns.Add("Name");
            dtFrm.Columns.Add("SortNo");

            dtFrm.Columns.Add("Path");
            dtFrm.TableName = "Frms";

            //读取路径.
            string path =  BP.Difference.SystemConfig.PathOfWebApp + "App/FlowDesigner/AppTemplate/";

            //获得根目录.
            DirectoryInfo root = new DirectoryInfo(path);//目录信息

            //获得根目录下的子目录.
            var dirs = root.GetDirectories();
            foreach (DirectoryInfo mydir in dirs)
            {
                DataRow dr = dtSort.NewRow();
                dr["No"] = mydir.Name.Substring(0, 2); //编号.
                dr["Name"] = mydir.Name;
                dtSort.Rows.Add(dr);
            }
            ds.Tables.Add(dtSort); //加入到dataset.

            //获取模版.
            foreach (DirectoryInfo mydir in dirs)
            {
                foreach (FileInfo file in mydir.GetFiles("*.png"))
                {
                    //如果包含icon  当做图标处理  加载
                    if (file.Name.ToLower().Contains("icon."))
                        continue;

                    DataRow dr = dtFrm.NewRow();
                    dr["No"] = file.Name;
                    dr["Name"] = file.Name;
                    dr["SortNo"] = mydir.Name.Substring(0, 2);
                    dr["Path"] = mydir.Name + "/" + file.Name;
                    dtFrm.Rows.Add(dr); //加入到里面.
                }
            }
            ds.Tables.Add(dtFrm); //加入到dataset.

            //返回数据.
            return BP.Tools.Json.ToJson(ds);
        }
    }
}
