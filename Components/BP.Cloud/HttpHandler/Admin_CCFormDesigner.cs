﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Web;
using BP.WF;
using BP.Web;
using BP.Sys;
using BP.DA;
using BP.En;
using BP.WF.Template;
using BP.WF.Template.SFlow;
using BP.CCBill;

namespace BP.Cloud.HttpHandler
{
    public class Admin_CCFormDesigner : BP.WF.HttpHandler.DirectoryPageBase
    {
        #region 执行父类的重写方法.
        /// <summary>
        /// 构造函数
        /// </summary>
        public Admin_CCFormDesigner()
        {
        }
        /// <summary>
        /// 创建枚举类型字段
        /// </summary>
        /// <returns></returns>
        public string FrmEnumeration_NewEnumField()
        {
            UIContralType ctrl = UIContralType.RadioBtn;
            string ctrlDoType = GetRequestVal("ctrlDoType");
            if (ctrlDoType == "DDL")
                ctrl = UIContralType.DDL;
            else
                ctrl = UIContralType.RadioBtn;

            string fk_mapdata = this.GetRequestVal("FK_MapData");
            string keyOfEn = this.GetRequestVal("KeyOfEn");
            string fieldDesc = this.GetRequestVal("Name");
            string enumKeyOfBind = this.GetRequestVal("UIBindKey"); //要绑定的enumKey.
            float x = this.GetRequestValFloat("x");
            float y = this.GetRequestValFloat("y");

            BP.Sys.CCFormAPI.NewEnumField(fk_mapdata, keyOfEn, fieldDesc, enumKeyOfBind, ctrl);
            return "绑定成功.";
        }
        /// <summary>
        /// 创建外键字段.
        /// </summary>
        /// <returns></returns>
        public string NewSFTableField()
        {
            try
            {
                string fk_mapdata = this.GetRequestVal("FK_MapData");
                string keyOfEn = this.GetRequestVal("KeyOfEn");
                string fieldDesc = this.GetRequestVal("Name");
                string sftable = this.GetRequestVal("UIBindKey");
                float x = float.Parse(this.GetRequestVal("x"));
                float y = float.Parse(this.GetRequestVal("y"));

                //调用接口,执行保存.
                BP.Sys.CCFormAPI.SaveFieldSFTable(fk_mapdata, keyOfEn, fieldDesc, sftable, x, y);
                return "设置成功";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }

        /// <summary>
        /// 转换拼音
        /// </summary>
        /// <returns></returns>
        public string ParseStringToPinyin()
        {
            string name = GetRequestVal("name");
            string flag = GetRequestVal("flag");
            //暂时没发现此方法在哪里有调用，edited by liuxc,2017-9-25
            return BP.Sys.CCFormAPI.ParseStringToPinyinField(name, Equals(flag, "true"), true, 20);
        }

        /// <summary>
        /// 创建隐藏字段.
        /// </summary>
        /// <returns></returns>
        public string NewHidF()
        {
            MapAttr mdHid = new MapAttr();
            mdHid.setMyPK(this.FK_MapData + "_" + this.KeyOfEn);
            mdHid.setFK_MapData(this.FK_MapData);
            mdHid.setKeyOfEn(this.KeyOfEn);
            mdHid.Name = this.Name;
            mdHid.MyDataType = int.Parse(this.GetRequestVal("FieldType"));
            mdHid.HisEditType = EditType.Edit;
            mdHid.setMaxLen(100);
            mdHid.setMinLen(0);
            mdHid.setLGType(FieldTypeS.Normal);
            mdHid.setUIVisible(false);
            mdHid.setUIIsEnable(false);
            mdHid.Insert();

            return "创建成功..";
        }
        #endregion 执行父类的重写方法.

        #region 创建表单.
        public string NewFrmGuide_GenerPinYin()
        {
            string isQuanPin = this.GetRequestVal("IsQuanPin");
            string name = this.GetRequestVal("TB_Name");

            //表单No长度最大100，因有前缀CCFrm_，因此此处设置最大94，added by liuxc,2017-9-25
            string str = BP.Sys.CCFormAPI.ParseStringToPinyinField(name, Equals(isQuanPin, "1"), true, 94);

            MapData md = new MapData();
            md.No = str;
            if (md.RetrieveFromDBSources() == 0)
                return str;

            return "err@表单ID:" + str + "已经被使用.";
        }
        /// <summary>
        /// 获得系统的表
        /// </summary>
        /// <returns></returns>
        public string NewFrmGuide_Init()
        {
            DataSet ds = new DataSet();

            SFDBSrc src = new SFDBSrc("local");
            ds.Tables.Add(src.ToDataTableField("SFDBSrc"));

            DataTable tables = src.GetTables(true);
            tables.TableName = "Tables";
            ds.Tables.Add(tables);
            return BP.Tools.Json.ToJson(ds);

        }
        public string NewFrmGuide_Create()
        {
            BP.Cloud.Sys.MapData md = new BP.Cloud.Sys.MapData();
            md.Name = this.GetRequestVal("TB_Name");

            //表单编号.
            //md.No =   BP.Web.WebUser.OrgNo+"_" + this.GetRequestVal("TB_No");
            md.No = DataType.ParseStringForNo(this.GetRequestVal("TB_No_Org"), 100);

            //表单类型.
            md.HisFrmTypeInt = this.GetRequestValInt("DDL_FrmType");

            //表单的物理表.
            if (md.HisFrmType == BP.Sys.FrmType.Url || md.HisFrmType == BP.Sys.FrmType.Entity)
                md.PTable = this.GetRequestVal("TB_PTable_Org");
            else
                md.PTable = DataType.ParseStringForNo(this.GetRequestVal("TB_PTable_Org"), 100);

            string sort = this.GetRequestVal("FK_FrmSort");
            if (DataType.IsNullOrEmpty(sort) == true)
                sort = this.GetRequestVal("DDL_FrmTree");

            md.FK_FrmSort = sort;
            md.FK_FormTree = sort;

            md.AppType = "0";//独立表单
            md.DBSrc = this.GetRequestVal("DDL_DBSrc");
            if (md.IsExits == true)
                return "err@表单ID:" + md.No + "已经存在.";

            switch (md.HisFrmType)
            {
                //自由，傻瓜，SL表单不做判断
                case BP.Sys.FrmType.FoolForm:
                case BP.Sys.FrmType.Develop:
                case BP.Sys.FrmType.ChapterFrm:
                    break;
                case BP.Sys.FrmType.Url:
                case BP.Sys.FrmType.Entity:
                    md.Url = md.PTable;
                    md.PTable = "";
                    break;
                //如果是以下情况，导入模式
                case BP.Sys.FrmType.WordFrm:
                case BP.Sys.FrmType.WPSFrm:
                case BP.Sys.FrmType.ExcelFrm:
                case BP.Sys.FrmType.VSTOForExcel:
                    break;
                default:
                    throw new Exception("未知表单类型." + md.HisFrmType.ToString());
            }
            md.Insert();

            //增加上OID字段.
            BP.Sys.CCFormAPI.RepareCCForm(md.No);

            BP.Sys.EntityType entityType = (EntityType)this.GetRequestValInt("EntityType");

            #region 如果是单据.
            if (entityType == EntityType.FrmBill)
            {
                BP.CCBill.FrmBill bill = new FrmBill(md.No);
                bill.EntityType = EntityType.FrmBill;
                bill.BillNoFormat = "ccbpm{yyyy}-{MM}-{dd}-{LSH4}";

                //设置默认的查询条件.
                bill.SetPara("IsSearchKey", 1);
                bill.SetPara("DTSearchWay", 0);

                bill.Update();
                bill.CheckEnityTypeAttrsFor_Bill();
            }
            #endregion 如果是单据.

            #region 如果是实体 EnityNoName .
            if (entityType == EntityType.FrmDict)
            {
                BP.CCBill.FrmDict entityDict = new FrmDict(md.No);
                entityDict.BillNoFormat = "3"; //编码格式.001,002,003.
                entityDict.BtnNewModel = 0;

                //设置默认的查询条件.
                entityDict.SetPara("IsSearchKey", 1);
                entityDict.SetPara("DTSearchWay", 0);

                entityDict.EntityType = EntityType.FrmDict;

                entityDict.Update();
                entityDict.CheckEnityTypeAttrsFor_EntityNoName();
            }
            #endregion 如果是实体 EnityNoName .

            //创建表与字段.
            GEEntity en = new GEEntity(md.No);
            en.CheckPhysicsTable();

            if (md.HisFrmType == BP.Sys.FrmType.WPSFrm || md.HisFrmType == BP.Sys.FrmType.WordFrm || md.HisFrmType == BP.Sys.FrmType.ExcelFrm)
            {
                /*把表单模版存储到数据库里 */
                return "url@../../WF/Comm/RefFunc/En.htm?EnName=BP.WF.Template.Frm.MapFrmExcel&PKVal=" + md.No;
            }

            if (md.HisFrmType == BP.Sys.FrmType.Entity)
                return "url@../../WF/Comm/Ens.htm?EnsName=" + md.PTable;

       
            if (md.HisFrmType == BP.Sys.FrmType.Develop)
                return "url@../../WF/Admin/DevelopDesigner/Designer.htm?FK_MapData=" + md.No + "&FrmID=" + md.No + "&EntityType=" + this.GetRequestVal("EntityType");


            return "url@../../WF/Admin/FoolFormDesigner/Designer.htm?IsFirst=1&FK_MapData=" + md.No + "&EntityType=" + this.GetRequestVal("EntityType");
        }
      
        #endregion 创建表单.

        /// <summary>
        /// 增加一个枚举类型
        /// </summary>
        /// <returns></returns>
        public string SysEnumList_SaveEnumField()
        {

            //   BP.WF.Dev2Interface.Flow_DoRebackWorkFlow("(this.WorkID, 1605, "xxx", "重新加印.");

            MapAttr attr = new MapAttr();
            attr.setMyPK(this.FK_MapData + "_" + this.KeyOfEn);
            if (attr.RetrieveFromDBSources() != 0)
                return "err@字段名[" + this.KeyOfEn + "]已经存在.";

            attr.setFK_MapData(this.FK_MapData);
            attr.setKeyOfEn(this.KeyOfEn);
            //attr.UIBindKey = this.GetRequestVal("EnumMainNo");
            attr.UIBindKey = this.GetRequestVal("EnumKey");

            attr.GroupID = this.GetRequestValInt("GroupFeid");
            int uiContralType = this.GetRequestValInt("UIContralType");

            if (uiContralType != 0)
                attr.UIContralType = (UIContralType)uiContralType;
            else
                attr.setUIContralType(UIContralType.DDL);
            if (attr.UIContralType == UIContralType.CheckBok)
                attr.setMyDataType(DataType.AppString);
            else
                attr.setMyDataType(DataType.AppInt);
            attr.setLGType(FieldTypeS.Enum);

            SysEnumMain sem = new SysEnumMain();
            sem.No = attr.UIBindKey;
            if (sem.RetrieveFromDBSources() != 0)
                attr.Name = sem.Name;
            else
                attr.setName("枚举" + attr.UIBindKey);

            //paras参数
            Paras ps = new Paras();
            ps.SQL = "SELECT OID FROM Sys_GroupField A WHERE A.FrmID=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "FrmID AND ( CtrlType='' OR CtrlType IS NULL ) ORDER BY OID DESC ";
            ps.Add("FrmID", this.FK_MapData);
            attr.GroupID = DBAccess.RunSQLReturnValInt(ps, 0);
            attr.Insert();
            return attr.MyPK;
        }


        public string LetLogin()
        {
            BP.Port.Emp emp = new BP.Port.Emp("admin");
            WebUser.SignInOfGener(emp);
            return "登录成功.";
        }

        public string GoToFrmDesigner_Init()
        {
            //根据不同的表单类型转入不同的表单设计器上去.
            BP.Sys.MapData md = new BP.Sys.MapData(this.FK_MapData);
            if (md.HisFrmType == BP.Sys.FrmType.FoolForm)
            {
                /* 傻瓜表单 需要翻译. */
                return "url@../../WF/Admin/FoolFormDesigner/Designer.htm?IsFirst=1&FK_MapData=" + this.FK_MapData;
            }

            if (md.HisFrmType == BP.Sys.FrmType.Develop)
            {
                /* 开发者表单 */
                return "url@../../WF/Admin/DevelopDesigner/Designer.htm?FK_MapData=" + this.FK_MapData + "&FrmID=" + this.FK_MapData + "&IsFirst=1";
            }

            if (md.HisFrmType == BP.Sys.FrmType.VSTOForExcel)
            {
                /* 自由表单 */
                return "url@../../WF/Admin/CCFormDesigner/FormDesigner.htm?FK_MapData=" + this.FK_MapData;
            }

            if (md.HisFrmType == BP.Sys.FrmType.Url)
            {
                /* 自由表单 */
                return "url@../../WF/Comm/En.htm?EnName=BP.WF.Template.Frm.MapDataURL&No=" + this.FK_MapData;
            }

            if (md.HisFrmType == BP.Sys.FrmType.Entity)
                return "url@../../WF/Comm/Ens.htm?EnsName=" + md.PTable;

            return "err@没有判断的表单转入类型" + md.HisFrmType.ToString();
        }

        public string PublicNoNameCtrlCreate()
        {
            try
            {
                float x = float.Parse(this.GetRequestVal("x"));
                float y = float.Parse(this.GetRequestVal("y"));
                BP.Sys.CCFormAPI.CreatePublicNoNameCtrl(this.FrmID, this.GetRequestVal("CtrlType"),
                    this.GetRequestVal("No"),
                    this.GetRequestVal("Name"), x, y);
                return "true";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        public string NewImage()
        {

            try
            {
                BP.Sys.CCFormAPI.NewImage(this.GetRequestVal("FrmID"),
                    this.GetRequestVal("KeyOfEn"), this.GetRequestVal("Name"),
                    //int.Parse(this.GetRequestVal("FieldType")),
                    float.Parse(this.GetRequestVal("x")),
                   float.Parse(this.GetRequestVal("y"))
                   );
                return "true";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }


        }
        public string NewField()
        {
            try
            {
                BP.Sys.CCFormAPI.NewField(this.GetRequestVal("FrmID"),
                    this.GetRequestVal("KeyOfEn"), this.GetRequestVal("Name"),
                    int.Parse(this.GetRequestVal("FieldType")),
                    float.Parse(this.GetRequestVal("x")),
                   float.Parse(this.GetRequestVal("y"))
                   );
                return "true";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 生成所有表单元素.
        /// </summary>
        /// <returns></returns>
        public string CCForm_AllElements_ResponseJson()
        {
            try
            {
                DataSet ds = new DataSet();

                MapData mapData = new MapData(this.FK_MapData);

                //属性.
                MapAttrs attrs = new MapAttrs(this.FK_MapData);
                attrs.Retrieve(MapAttrAttr.FK_MapData, this.FK_MapData, MapAttrAttr.UIVisible, 1);
                ds.Tables.Add(attrs.ToDataTableField("Sys_MapAttr"));

                FrmBtns btns = new FrmBtns(this.FK_MapData);
                ds.Tables.Add(btns.ToDataTableField("Sys_FrmBtn"));

                FrmRBs rbs = new FrmRBs(this.FK_MapData);
                ds.Tables.Add(rbs.ToDataTableField("Sys_FrmRB"));
 

                FrmImgs imgs = new FrmImgs(this.FK_MapData);
                ds.Tables.Add(imgs.ToDataTableField("Sys_FrmImg"));

                FrmImgAths imgAths = new FrmImgAths(this.FK_MapData);
                ds.Tables.Add(imgAths.ToDataTableField("Sys_FrmImgAth"));

                FrmAttachments aths = new FrmAttachments(this.FK_MapData);
                ds.Tables.Add(aths.ToDataTableField("Sys_FrmAttachment"));

                MapDtls dtls = new MapDtls();
                dtls.Retrieve(MapDtlAttr.FK_MapData, this.FK_MapData, MapDtlAttr.FK_Node, 0);
                ds.Tables.Add(dtls.ToDataTableField("Sys_MapDtl"));

                BP.Sys.FrmUI.MapFrameExts mapFrameExts = new BP.Sys.FrmUI.MapFrameExts(this.FK_MapData);
                ds.Tables.Add(mapFrameExts.ToDataTableField("Sys_MapFrame"));

                //组织节点组件信息.
                string sql = "";
                if (this.FK_Node > 100)
                {
                    sql += "select '轨迹图' AS Name,'FlowChart' AS No,FrmTrackSta Sta,FrmTrack_X X,FrmTrack_Y Y,FrmTrack_H H,FrmTrack_W  W from WF_Node WHERE nodeid=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "nodeid";
                    sql += " union select '审核组件'AS Name, 'FrmCheck'AS No,FWCSta Sta,FWC_X X,FWC_Y Y,FWC_H H, FWC_W W from WF_Node WHERE nodeid=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "nodeid";
                    sql += " union select '子流程' AS Name,'SubFlowDtl'AS  No,SFSta Sta,SF_X X,SF_Y Y,SF_H H, SF_W W from WF_Node  WHERE nodeid=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "nodeid";
                    sql += " union select '子线程' AS Name, 'ThreadDtl'AS  No,FrmThreadSta Sta,FrmThread_X X,FrmThread_Y Y,FrmThread_H H,FrmThread_W W from WF_Node WHERE nodeid=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "nodeid";
                    sql += " union select '流转自定义' AS Name,'FrmTransferCustom' AS  No,FTCSta Sta,FTC_X X,FTC_Y Y,FTC_H H,FTC_W  W FROM WF_Node WHERE nodeid=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "nodeid";
                    Paras ps = new Paras();
                    ps.SQL = sql;
                    ps.Add("nodeid", this.FK_Node);
                    DataTable dt = null;

                    try
                    {
                        dt = DBAccess.RunSQLReturnTable(ps);
                    }
                    catch (Exception ex)
                    {
                        FrmSubFlow sb = new FrmSubFlow();
                        sb.CheckPhysicsTable();

                        TransferCustom tc = new TransferCustom();
                        tc.CheckPhysicsTable();

                      

                        FrmTrack ftd = new FrmTrack();
                        ftd.CheckPhysicsTable();

                        FrmTransferCustom ftd1 = new FrmTransferCustom();
                        ftd1.CheckPhysicsTable();

                        throw ex;
                    }

                    dt.TableName = "FigureCom";
                    dt.Columns[0].ColumnName = "Name";
                    dt.Columns[1].ColumnName = "No";
                    dt.Columns[2].ColumnName = "Sta";
                    dt.Columns[3].ColumnName = "X";
                    dt.Columns[4].ColumnName = "Y";
                    dt.Columns[5].ColumnName = "H";
                    dt.Columns[6].ColumnName = "W";
                   
                    ds.Tables.Add(dt);
                }

                return BP.Tools.Json.ToJson(ds);
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        #region 表格处理.
        public string Tables_Init()
        {
            BP.Sys.SFTables tabs = new BP.Sys.SFTables();
            tabs.RetrieveAll();
            DataTable dt = tabs.ToDataTableField();
            dt.Columns.Add("RefNum", typeof(int));

            foreach (DataRow dr in dt.Rows)
            {
                //求引用数量.
                int refNum = BP.DA.DBAccess.RunSQLReturnValInt("SELECT COUNT(KeyOfEn) FROM Sys_MapAttr WHERE UIBindKey='" + dr["No"] + "'", 0);
                dr["RefNum"] = refNum;
            }
            return BP.Tools.Json.ToJson(dt);
        }
        public string Tables_Delete()
        {
            try
            {
                BP.Sys.SFTable tab = new BP.Sys.SFTable();
                tab.No = this.No;
                tab.Delete();
                return "删除成功.";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }

        public string TableRef_Init()
        {
            BP.Sys.MapAttrs mapAttrs = new BP.Sys.MapAttrs();
            mapAttrs.RetrieveByAttr(BP.Sys.MapAttrAttr.UIBindKey, this.FK_SFTable);

            DataTable dt = mapAttrs.ToDataTableField();
            return BP.Tools.Json.ToJson(dt);
        }


        #endregion

        #region 复制表单
        
        public void DoCopyFrm()
        {
            string fromFrmID = GetRequestVal("FromFrmID");
            string toFrmID = GetRequestVal("ToFrmID");
            string toFrmName = GetRequestVal("ToFrmName");
            #region 原表单信息
            //表单信息
            MapData fromMap = new MapData(fromFrmID);
            //单据信息
            FrmBill fromBill = new FrmBill();
            fromBill.No = fromFrmID;
            int billCount = fromBill.RetrieveFromDBSources();
            //实体单据
            FrmDict fromDict = new FrmDict();
            fromDict.No = fromFrmID;
            int DictCount = fromDict.RetrieveFromDBSources();
            #endregion 原表单信息

            #region 复制表单
            MapData toMapData = new MapData();
            toMapData = fromMap;
            toMapData.No = toFrmID;
            toMapData.Name = toFrmName;
            toMapData.Insert();
            if (billCount != 0)
            {
                FrmBill toBill = new FrmBill();
                toBill = fromBill;
                toBill.No = toFrmID;
                toBill.Name = toFrmName;
                toBill.EntityType = EntityType.FrmBill;
                toBill.Update();
            }
            if (DictCount != 0)
            {
                FrmDict toDict = new FrmDict();
                toDict = fromDict;
                toDict.No = toFrmID;
                toDict.Name = toFrmName;
                toDict.EntityType = EntityType.FrmDict;
                toDict.Update();
            }
            #endregion 复制表单

            MapData.ImpMapData(toFrmID, BP.Sys.CCFormAPI.GenerHisDataSet_AllEleInfo(fromFrmID));

            //清空缓存
            toMapData.RepairMap();
            BP.Difference.SystemConfig.DoClearCash();


        }
        #endregion 复制表单

    }
}
