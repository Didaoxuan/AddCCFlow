using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using BP.DA;
using BP.Sys;
using BP.Web;
using BP.Port;
using BP.En;
using BP.WF;
using BP.WF.Template;
using BP.WF.Data;
using LitJson;
using System.Net;
using System.IO;
using BP.CCBill;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class App_OneFrm : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App_OneFrm()
        {

        }
        /// <summary>
        /// 获得菜单
        /// </summary>
        /// <returns></returns>
        public string Default_Menu()
        {
            if (WebUser.No == null)
                return "err@/App/index.htm";

            //类别.
            DataTable dtSort = new DataTable("Sorts");
            dtSort.Columns.Add("No");
            dtSort.Columns.Add("Name");
            dtSort.Columns.Add("ICON");

            int workModel = GetRequestValInt("WorkModel");

            //菜单.
            DataTable dtMenu = new DataTable("Menus");
            dtMenu.Columns.Add("No");
            dtMenu.Columns.Add("Name");
            dtMenu.Columns.Add("SortNo");
            dtMenu.Columns.Add("ICON");
            dtMenu.Columns.Add("Url");

            #region 类别.
            DataRow dr = dtSort.NewRow();
            dr["No"] = "01";
            if(workModel==2)
                dr["Name"] = "实体表单业务数据";
            if (workModel == 3)
                dr["Name"] = "单据业务数据";
            dr["ICON"] = "FrmSearch.png";
            dtSort.Rows.Add(dr);
            if (WebUser.IsAdmin)
            {
                dr = dtSort.NewRow();
                dr["No"] = "02";
                if (workModel == 2)
                    dr["Name"] = "本实体运维管理";
                if (workModel == 3)
                    dr["Name"] = "本单据运维管理";
                dr["ICON"] = "System.png";
                dtSort.Rows.Add(dr);
            }
            #endregion 类别.

            #region 流程查询-菜单.
            dr = dtMenu.NewRow();
            dr["No"] = "MyStartFrms";
            dr["Name"] = "我创建的";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFrm/FrmSearch.htm?SearchType=My";
            dr["ICON"] = "SearchMy.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "MyDeptFrms";
            dr["Name"] = "我部门创建的";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFrm/FrmSearch.htm?SearchType=MyDept";
            dr["ICON"] = "SearchMyDept.png";
            dtMenu.Rows.Add(dr);

           

            dr = dtMenu.NewRow();
            dr["No"] = "Org";
            dr["Name"] = "部门数据概况";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFrm/DataPanelDept.htm";
            dr["ICON"] = "Organization.png";
            dtMenu.Rows.Add(dr);

            //dr = dtMenu.NewRow();
            //dr["No"] = "Org";
            //dr["Name"] = "组织数据概况";
            //dr["SortNo"] = "01";
            //dr["Url"] = "/App/OneFrm/DataPanelOrg.htm";
            //dr["ICON"] = "Organization.png";
            //dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "FX";
            dr["Name"] = "综合分析";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFrm/Group.htm?GroupType=My";
            dr["ICON"] = "ZongHeFenXi.png";
            dtMenu.Rows.Add(dr);

            #endregion 流程查询-菜单.

            #region 系统管理-菜单.
            if (WebUser.IsAdmin==true)
            {
                dr = dtMenu.NewRow();
                dr["No"] = "Template";
                dr["Name"] = "模版设计";
                dr["SortNo"] = "02";
                dr["Url"] = "/WF/Admin/FoolFormDesigner/Designer.htm?FrmID=Frm_043";
                dr["ICON"] = "Template.png";
                dtMenu.Rows.Add(dr);

           

                dr = dtMenu.NewRow();
                dr["No"] = "FlowDatas";
                dr["Name"] = "数据运维";
                dr["SortNo"] = "02";
                dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.Cloud.GWFAdmins";
                dr["ICON"] = "YunWei.png";
                dtMenu.Rows.Add(dr);

             
            }
            #endregion 系统管理-菜单.

            //组装数据.
            DataSet ds = new DataSet();
            ds.Tables.Add(dtSort);
            ds.Tables.Add(dtMenu);

            //返回数据.
            return BP.Tools.Json.ToJson(ds);
        }
        /// <summary>
        /// 初始化Home
        /// </summary>
        /// <returns></returns>
        public string DataPanelDept_Init()
        {
            Hashtable ht = new Hashtable();
            MapData mapData = new MapData(this.FrmID);
            string ptable = mapData.PTable;
            if (DBAccess.IsExitsObject(ptable) == false)
            {
                GEEntity en = new GEEntity(this.FrmID);
                en.CheckPhysicsTable();
            }
        

            //获取我创建的数量
            string sql = "SELECT Count(*) From " + ptable + " WHERE Starter='" + WebUser.No + "'";
            ht.Add("My_Create", DBAccess.RunSQLReturnValInt(sql));
            //我部门创建的
            sql="SELECT Count(*) From " + ptable + " WHERE FK_Dept='" + WebUser.FK_Dept + "'";
            ht.Add("Dept_Create", DBAccess.RunSQLReturnValInt(sql));
            return BP.Tools.Json.ToJson(ht);
        }

        /// <summary>
        /// 获取Rpt表的分析项
        /// </summary>
        /// <returns></returns>
        public string DataPanelDept_GetAnalyseGroupByRpt()
        {
            //获取的是系统字段WFState和表单中Int类型的字段
            MapAttrs mapattrs = new MapAttrs();
            QueryObject qo = new QueryObject(mapattrs);
            qo.AddWhere(MapAttrAttr.FK_MapData, "ND" + int.Parse(this.FK_Flow) + "01");
            qo.addAnd();
            qo.AddWhereNotIn(MapAttrAttr.MyDataType, "1,4,6,7");
            qo.addAnd();
            qo.AddWhere(MapAttrAttr.UIVisible, true);
            qo.addAnd();
            qo.AddWhere(MapAttrAttr.LGType, "!=", (int)FieldTypeS.Enum);
          
            qo.addOrderBy(MapAttrAttr.GroupID, MapAttrAttr.Idx);
            qo.DoQuery();
            //增加系统表的状态
            MapAttr attr = new MapAttr("ND" + int.Parse(this.FK_Flow) + "Rpt_WFState");
            mapattrs.AddEntity(attr);
            return BP.Tools.Json.ToJson(mapattrs.ToDataTableField());
        }

        public string DataPanelDept_GetAnalyseBySpecifyField_DataSet()
        {
            DataSet ds = new DataSet();
            string field = this.GetRequestVal("KeyOfEn");
            string groupBy = this.GetRequestVal("FK_NY");
            if (DataType.IsNullOrEmpty(field) || DataType.IsNullOrEmpty(groupBy))
                throw new Exception("分析条件，分析项不能为空");

            DataTable dt;
            //流程状态，只分析发起数量和完成数量
            if (field.Equals("WFState") == true)
            {
                string sql = "SELECT SUBSTRING(FK_NY, 6, 2) AS FK_NY, WFState, count(OID) as Num FROM ND" + int.Parse(this.FK_Flow) + "Rpt WHERE FK_Dept = 'ccs1'  AND FK_NY> DateName(year, GetDate()) + '-00' GROUP BY FK_NY ,WFState";
                dt = DBAccess.RunSQLReturnTable(sql);
            }
            else
            {
                //按照申请人的区分
                string sql = "SELECT SUBSTRING(FK_NY, 6, 2) AS FK_NY, FlowStarter, sum(" + field+") as Num FROM ND" + int.Parse(this.FK_Flow) + "Rpt WHERE FK_Dept = 'ccs1'  AND FK_NY> DateName(year, GetDate()) + '-00' GROUP BY FK_NY ,FlowStarter";
                dt = DBAccess.RunSQLReturnTable(sql);

            }
            return BP.Tools.Json.ToJson(ds);
        }
        /// <summary>
        /// 获取单个实体、单据的本部门的数据分析
        /// </summary>
        /// <returns></returns>
        public string DataPanelDept_GetAnalyseByFrmIDDept_DataSet()
        {
            MapData mapData = new MapData(this.FrmID);
            string ptable = mapData.PTable;
            DataSet ds = new DataSet();
            //按照月份
            //我创建的
            string sql = "SELECT SUBSTRING(RDT, 6, 2) AS FK_NY, count(OID) AS Num FROM "+ ptable+" WHERE BillState >=1 AND Starter='" + WebUser.No+"' AND RDT>DateName(year,GetDate())+'-00-00' GROUP BY RDT ";
            DataTable DictBillByNYOfMy = DBAccess.RunSQLReturnTable(sql);
            DictBillByNYOfMy.TableName = "DictBillByNYOfMy";
            ds.Tables.Add(DictBillByNYOfMy);
            //我部门创建的
            sql = "SELECT SUBSTRING(RDT, 6, 2) AS FK_NY, count(OID) as Num FROM "+ ptable+ " WHERE BillState >=1 AND FK_Dept='" + WebUser.FK_Dept + "'AND RDT>DateName(year,GetDate())+'-00-00' GROUP BY RDT ";
            DataTable DictBillByNYOfMyOfDept = DBAccess.RunSQLReturnTable(sql);
            DictBillByNYOfMyOfDept.TableName = "DictBillByNYOfMyOfDept";
            ds.Tables.Add(DictBillByNYOfMyOfDept);


            //按照人员创建的数量
            sql = "SELECT StarterName, count(OID) as Num FROM "+ptable+ " WHERE BillState >0 AND FK_Dept='" + WebUser.FK_Dept  + "' GROUP BY StarterName ";
            DataTable DictBillByEmp = DBAccess.RunSQLReturnTable(sql);
            DictBillByEmp.TableName = "DictBillByEmp";
            ds.Tables.Add(DictBillByEmp);
           

            return BP.Tools.Json.ToJson(ds);
        }


      
    }
}
