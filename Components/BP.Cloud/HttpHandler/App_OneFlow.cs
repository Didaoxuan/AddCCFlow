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
using BP.Difference;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class App_OneFlow : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App_OneFlow()
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
            dr["Name"] = "本流程操作";
            dr["ICON"] = "FlowCenter.png";
            dtSort.Rows.Add(dr);

            dr = dtSort.NewRow();
            dr["No"] = "02";
            dr["Name"] = "本流程业务数据";
            dr["ICON"] = "FlowSearch.png";
            dtSort.Rows.Add(dr);
            if (WebUser.IsAdmin && 1==2)
            {
                dr = dtSort.NewRow();
                dr["No"] = "03";
                dr["Name"] = "本流程运维管理";
                dr["ICON"] = "System.png";
                dtSort.Rows.Add(dr);
            }
            #endregion 类别.

            #region 流程中心-菜单.
     
            dr = dtMenu.NewRow();
            dr["No"] = "Todolist";
            dr["Name"] = "待办";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFlow/Todolist.htm";
            dr["ICON"] = "Todolist.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "Runing";
            dr["Name"] = "未完成";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFlow/Runing.htm";
            dr["ICON"] = "Runing.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "CC";
            dr["Name"] = "抄送";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFlow/CC.htm";
            dr["ICON"] = "CC.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "Draf";
            dr["Name"] = "草稿";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/OneFlow/Draf.htm";
            dr["ICON"] = "Draft.png";
            dtMenu.Rows.Add(dr);

            #endregion 流程中心-菜单.

            #region 流程查询-菜单.
            dr = dtMenu.NewRow();
            dr["No"] = "MyStartFlows";
            dr["Name"] = "我发起的";
            dr["SortNo"] = "02";
            dr["Url"] = "/App/OneFlow/RptSearch.htm?SearchType=My";
            dr["ICON"] = "SearchMy.png";
            dtMenu.Rows.Add(dr);

           

            dr = dtMenu.NewRow();
            dr["No"] = "MyJoinFlows";
            dr["Name"] = "我审批的";
            dr["SortNo"] = "02";
            dr["Url"] = "/App/OneFlow/RptSearch.htm?SearchType=MyJoin";
            dr["ICON"] = "SearchMyCheck.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "Org";
            dr["Name"] = "部门数据概况";
            dr["SortNo"] = "02";
            dr["Url"] = "/App/OneFlow/DataPanelDept.htm";
            dr["ICON"] = "Organization.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "Org";
            dr["Name"] = "组织数据概况";
            dr["SortNo"] = "02";
            dr["Url"] = "/App/OneFlow/DataPanelOrg.htm";
            dr["ICON"] = "Organization.png";
            dtMenu.Rows.Add(dr);

           

            if (WebUser.IsAdmin == true)
            {
                dr = dtMenu.NewRow();
                dr["No"] = "MyDeptFlows";
                dr["Name"] = "我部门发起的";
                dr["SortNo"] = "02";
                dr["Url"] = "/App/OneFlow/RptSearch.htm?SearchType=MyDept";
                dr["ICON"] = "SearchMyDept.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "FX";
                dr["Name"] = "综合分析";
                dr["SortNo"] = "02";
                dr["Url"] = "/WF/Comm/Group.htm?EnsName=BP.Cloud.GWFAdmins";
                dr["ICON"] = "ZongHeFenXi.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "FlowDatas";
                dr["Name"] = "数据运维";
                dr["SortNo"] = "02";
                dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.Cloud.GWFAdmins";
                dr["ICON"] = "YunWei.png";
                dtMenu.Rows.Add(dr);
            }
            #endregion 流程查询-菜单.

            #region 系统管理-菜单.
            if (WebUser.IsAdmin==true && 1==2)
            {
                dr = dtMenu.NewRow();
                dr["No"] = "Template";
                dr["Name"] = "模版设计";
                dr["SortNo"] = "03";
                dr["Url"] = "/App/FlowDesigner/Designer.htm?RunModel=2";
                dr["ICON"] = "Template.png";
                dtMenu.Rows.Add(dr);

           

                dr = dtMenu.NewRow();
                dr["No"] = "FlowDatas";
                dr["Name"] = "数据运维";
                dr["SortNo"] = "03";
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
        public string Default_Init()
        {
            Hashtable ht = new Hashtable();
            ht.Add("UserNo", BP.Web.WebUser.No);
            ht.Add("UserName", BP.Web.WebUser.Name);

            //系统名称.
            ht.Add("SysName", BP.Difference.SystemConfig.SysName);
            ht.Add("CustomerName", BP.Difference.SystemConfig.CustomerName);

            ht.Add("Todolist_EmpWorks", BP.WF.Dev2Interface.Todolist_EmpWorks);
            ht.Add("Todolist_Runing", BP.WF.Dev2Interface.Todolist_Runing);
            ht.Add("Todolist_Sharing", BP.WF.Dev2Interface.Todolist_Sharing);
            ht.Add("Todolist_CCWorks", BP.WF.Dev2Interface.Todolist_CCWorks);
            ht.Add("Todolist_Apply", BP.WF.Dev2Interface.Todolist_Apply); //申请下来的任务个数.
            ht.Add("Todolist_Draft", BP.WF.Dev2Interface.Todolist_Draft); //草稿数量.
            ht.Add("Todolist_Complete", BP.WF.Dev2Interface.Todolist_Complete); //完成数量.
            ht.Add("UserDeptName", WebUser.FK_DeptName);

            //我发起
            MyStartFlows myStartFlows = new MyStartFlows();
            QueryObject obj = new QueryObject(myStartFlows);
            obj.AddWhere(MyStartFlowAttr.Starter, WebUser.No);
            obj.addAnd();
            //运行中\已完成\挂起\退回\转发\加签\批处理\
            obj.addLeftBracket();
            obj.AddWhere("WFState=2 or WFState=3 or WFState=4 or WFState=5 or WFState=6 or WFState=8 or WFState=10");
            obj.addRightBracket();
            obj.DoQuery();
            ht.Add("Todolist_MyStartFlow", myStartFlows.Count);

            //我参与
            MyJoinFlows myFlows = new MyJoinFlows();
            obj = new QueryObject(myFlows);
            obj.AddWhere("Emps like '%" + WebUser.No + "%'");
            obj.DoQuery();
            ht.Add("Todolist_MyFlow", myFlows.Count);

            return BP.Tools.Json.ToJsonEntityModel(ht);
        }

        /// <summary>
        /// 初始化待办.
        /// </summary>
        /// <returns></returns>
        public string Todolist_Init()
        {
            string fk_node = this.GetRequestVal("FK_Node");
            string showWhat = this.GetRequestVal("ShowWhat");
            DataTable dt = BP.WF.Dev2Interface.DB_GenerEmpWorksOfDataTable(WebUser.No,this.FK_Node,showWhat,this.Domain);
            return BP.Tools.Json.ToJson(dt);
        }

        /// <summary>
        /// 在途
        /// </summary>
        /// <param name="UserNo">人员编号</param>
        /// <param name="fk_flow">流程编号</param>
        /// <returns>运行中的流程</returns>
        public string Runing_Init()
        {
            DataTable dt = null;
            bool isContainFuture = this.GetRequestValBoolen("IsContainFuture");
            dt = BP.Cloud.Dev2Interface.DB_GenerRuning(WebUser.No,this.FK_Flow, isContainFuture, this.Domain); //获得指定域的在途.
            return BP.Tools.Json.ToJson(dt);
        }

        /// <summary>
        /// 抄送
        /// </summary>
        /// <returns></returns>
        public string CC_Init()
        {
            string sta = this.GetRequestVal("Sta");
            if (DataType.IsNullOrEmpty(sta) )
                sta = "-1";

           
            DataTable dt = null;
            if (sta == "-1")
                dt = BP.WF.Dev2Interface.DB_CCList(null,this.FK_Flow);

            if (sta == "0")
                dt = BP.WF.Dev2Interface.DB_CCList_UnRead(BP.Web.WebUser.No,null,this.FK_Flow);

            if (sta == "1")
                dt = BP.WF.Dev2Interface.DB_CCList_Read(null,this.FK_Flow);

            if (sta == "2")
                dt = BP.WF.Dev2Interface.DB_CCList_Delete(null,this.FK_Flow);

            return BP.Tools.Json.ToJson(dt);
        }
        
        public string Draf_Init()
        {
            DataTable dt = BP.WF.Dev2Interface.DB_GenerDraftDataTable(this.FK_Flow);
            return BP.Tools.Json.ToJson(dt);
        }

        /// <summary>
        /// 获取本部门的数据统计
        /// </summary>
        /// <returns></returns>
        public string DataPanelDept_GetGenerWorksByDept()
        {
            Hashtable ht = new Hashtable();
            string dbStr =  BP.Difference.SystemConfig.AppCenterDBVarStr;
            Paras ps = new Paras();
            ps.SQL = "SELECT COUNT(*) AS count,WFState FROM WF_GenerWorkFlow Where WFState >1 AND FID=0 AND FK_Flow=" + dbStr + "FK_Flow AND FK_Dept=" + dbStr + "FK_Dept GROUP BY WFState";
            ps.Add("FK_Flow", this.FK_Flow);
            ps.Add("FK_Dept", WebUser.FK_Dept);
            DataTable dt = DBAccess.RunSQLReturnTable(ps);
            int allCount = 0;
            int count = 0;
            int wfstate = 0;
            foreach(DataRow dr in dt.Rows)
            {
                count = int.Parse(dr[0].ToString());
                wfstate = int.Parse(dr[1].ToString());
                allCount += count;
                if(wfstate == (int)WFState.Complete)
                    ht.Add("Dept_GWF_CompleteCounts", count);
                if (wfstate == (int)WFState.ReturnSta)
                    ht.Add("Dept_GWF_ReturnCounts", count);
                if (wfstate == (int)WFState.Delete)
                    ht.Add("Dept_GWF_DeleteCounts", count);
                
            }
            //本部门的查询显示数量
            ps.SQL = "Select Count(DISTINCT WorkID) From("
                + "SELECT DISTINCT G.WorkID From WF_GenerWorkFlow G,WF_GenerWorkerlist L Where  G.WorkID=L.WorkID AND G.WFState=2 AND G.FK_Flow=" + dbStr + "FK_Flow AND L.IsPass=0 AND L.FK_Dept=" + dbStr + "FK_Dept"
            +" UNION SELECT DISTINCT WorkID FROM WF_GenerWorkFlow Where WFState > 1 AND WFState!=3 AND FK_Flow =" + dbStr + "FK_Flow AND FK_Dept=" + dbStr + "FK_Dept) AS A";
  
            ps.Add("FK_Flow", this.FK_Flow);
            ps.Add("FK_Dept", WebUser.FK_Dept);
            int runingCount = DBAccess.RunSQLReturnValInt(ps);
            ht.Add("Dept_GWF_RuningCounts", runingCount);
            ht.Add("Dept_GWF_Counts", allCount);
            string sql = "";
            switch (SystemConfig.AppCenterDBType)
            {
                case DBType.MySQL:
                case DBType.UX:
                case DBType.PostgreSQL:
                    sql=" AND  STR_TO_DATE(SDT,'%Y-%m-%d %H:%i') <  NOW()";
                    break;
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                   sql=" AND  to_char(to_date(SDT,'yyyy-mm-dd,hh24:mi'),'yyyymmdd HH:mm')  < to_char(sysdate ,'yyyymmdd HH:mm')";
                    break;
                case DBType.MSSQL:
                    sql = " AND  convert(varchar(100),SDT,120) < CONVERT(varchar(100), GETDATE(), 120)";
                    break;
                default:
                    throw new Exception(SystemConfig.AppCenterDBType + "还没增加该数据库类型的解析");
            }
            ht.Add("Dept_GWF_OverCounts", DBAccess.RunSQLReturnValInt("SELECT COUNT(*) FROM WF_GenerWorkerList WHERE  IsPass=0 AND FK_Flow='"+this.FK_Flow+"' AND FK_Dept='" +WebUser.FK_Dept + "' "+sql));

            //个人的查询显示数量
            ps.SQL = "SELECT COUNT(DISTINCT WorkID) AS count,WFState FROM WF_GenerWorkFlow Where WFState >1 AND FK_Flow=" + dbStr + "FK_Flow AND (Emps like '%@"+WebUser.No+",%'OR TodoEmps like '%"+WebUser.No+",%')  GROUP BY WFState ";
            ps.Add("FK_Flow", this.FK_Flow);
            ps.Add("FK_Dept", WebUser.FK_Dept);
            dt = DBAccess.RunSQLReturnTable(ps);
            allCount = 0;
            foreach (DataRow dr in dt.Rows)
            {
                count = int.Parse(dr[0].ToString());
                wfstate = int.Parse(dr[1].ToString());
                allCount += count;
                if (wfstate == (int)WFState.Runing)
                    ht.Add("My_GWF_RuningCounts", count);
                if (wfstate == (int)WFState.Complete)
                    ht.Add("My_GWF_CompleteCounts", count);
                if (wfstate == (int)WFState.ReturnSta)
                    ht.Add("My_GWF_ReturnCounts", count);
                if (wfstate == (int)WFState.Delete)
                    ht.Add("My_GWF_DeleteCounts", count);

            }
            ht.Add("My_GWF_Counts", allCount);
            switch (SystemConfig.AppCenterDBType)
            {
                case DBType.MySQL:
                case DBType.UX:
                case DBType.PostgreSQL:
                    sql = " AND  STR_TO_DATE(SDT,'%Y-%m-%d %H:%i') <  NOW()";
                    break;
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    sql = " AND  to_char(to_date(SDT,'yyyy-mm-dd,hh24:mi'),'yyyymmdd HH:mm')  < to_char(sysdate ,'yyyymmdd HH:mm')";
                    break;
                case DBType.MSSQL:
                    sql = " AND  convert(varchar(100),SDT,120) < CONVERT(varchar(100), GETDATE(), 120)";
                    break;
                default:
                    throw new Exception(SystemConfig.AppCenterDBType + "还没增加该数据库类型的解析");
            }
            ht.Add("My_GWF_OverCounts", DBAccess.RunSQLReturnValInt("SELECT COUNT(*) FROM WF_GenerWorkerList WHERE  IsPass=0 AND FK_Emp='" + WebUser.No + "' AND FK_Flow='" + this.FK_Flow + "' "+sql));
            
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
        /// 获取单个流程的本部门的数据分析
        /// </summary>
        /// <returns></returns>
        public string DataPanelDept_GetAnalyseByFlowNoDept_DataSet()
        {
            DataSet ds = new DataSet();
            string dbSQL = "";
            switch (SystemConfig.AppCenterDBType)
            {
                case DBType.MySQL:
                case DBType.PostgreSQL:
                case DBType.UX:
                    dbSQL = " AND FK_NY>YEAR(CURDATE())+'-00' ";
                    break;
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    dbSQL = " AND FK_NY>to_char(sysdate, 'yyyy' )+'-00' ";
                    break;
                case DBType.MSSQL:
                    dbSQL = " AND FK_NY>DateName(year,GetDate())+'-00' ";
                    break;
                default:
                    throw new Exception(SystemConfig.AppCenterDBType + "还没增加该数据库类型的解析");
            }
            //按照月份（统计的内容有发起数，完成数量，业务字段）
            //发起数量
            string sql = "SELECT SUBSTRING(FK_NY, 6, 2) AS FK_NY, count(WorkID) AS Num FROM WF_GenerWorkFlow WHERE WFState >1 AND FK_Dept='" + WebUser.FK_Dept+"' AND FK_Flow='"+this.FK_Flow+ "' "+ dbSQL+" GROUP BY FK_NY ";
            DataTable FlowStartByNY = DBAccess.RunSQLReturnTable(sql);
            FlowStartByNY.TableName = "FlowStartByNY";
            ds.Tables.Add(FlowStartByNY);
            //完成数量
            sql = "SELECT SUBSTRING(FK_NY, 6, 2) AS FK_NY, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState=3 AND FK_Dept='" + WebUser.FK_Dept + "' AND FK_Flow='" + this.FK_Flow + "' " + dbSQL + " GROUP BY FK_NY ";
            DataTable FlowCompleteByNY = DBAccess.RunSQLReturnTable(sql);
            FlowCompleteByNY.TableName = "FlowCompleteByNY";
            ds.Tables.Add(FlowCompleteByNY);


            //按照人员
            //发起数量
            sql = "SELECT StarterName, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState >1 AND FK_Dept='" + WebUser.FK_Dept + "' AND FK_Flow='" + this.FK_Flow + "' GROUP BY StarterName ";
            DataTable FlowStartByEmp = DBAccess.RunSQLReturnTable(sql);
            FlowStartByEmp.TableName = "FlowStartByEmp";
            ds.Tables.Add(FlowStartByEmp);
            //完成数量
            sql = "SELECT StarterName, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState=3 AND FK_Dept='" + WebUser.FK_Dept + "' AND FK_Flow='" + this.FK_Flow + "' GROUP BY StarterName ";
            DataTable FlowCompleteByEmp = DBAccess.RunSQLReturnTable(sql);
            FlowCompleteByEmp.TableName = "FlowCompleteByEmp";
            ds.Tables.Add(FlowCompleteByEmp);

            return BP.Tools.Json.ToJson(ds);
        }


        /// <summary>
        /// 获取指定流程的数据统计
        /// </summary>
        /// <returns></returns>
        public string DataPanelOrg_GetGenerWorksByFlowNo()
        {
            Hashtable ht = new Hashtable();
            string dbStr =  BP.Difference.SystemConfig.AppCenterDBVarStr;
            Paras ps = new Paras();
            ps.SQL = "SELECT COUNT(*) AS count,WFState FROM WF_GenerWorkFlow Where WFState >1 AND FID=0 AND FK_Flow=" + dbStr + "FK_Flow GROUP BY WFState";
            ps.Add("FK_Flow", this.FK_Flow);
            DataTable dt = DBAccess.RunSQLReturnTable(ps);
            int allCount = 0;
            int count = 0;
            int wfstate = 0;
            int returnCount = 0;
            foreach (DataRow dr in dt.Rows)
            {
                count = int.Parse(dr[0].ToString());
                wfstate = int.Parse(dr[1].ToString());
                allCount += count;
                if (wfstate == (int)WFState.Complete)
                    ht.Add("GWF_CompleteCounts", count);
                if (wfstate == (int)WFState.ReturnSta)
                {
                    returnCount = count;
                    ht.Add("GWF_ReturnCounts", count);
                }
                  
                if (wfstate == (int)WFState.Delete)
                    ht.Add("GWF_DeleteCounts", count);

            }
            //本部门的查询显示数量
            ps.SQL = "SELECT Count(DISTINCT G.WorkID) From WF_GenerWorkFlow G,WF_GenerWorkerlist L Where  G.WorkID=L.WorkID AND G.WFState=2 AND G.FK_Flow=" + dbStr + "FK_Flow AND L.IsPass=0";
            ps.Add("FK_Flow", this.FK_Flow);
            int runingCount = DBAccess.RunSQLReturnValInt(ps);
            ht.Add("GWF_RuningCounts", runingCount + returnCount);
            ht.Add("GWF_Counts", allCount);
            string sql = "";
            switch (SystemConfig.AppCenterDBType)
            {
                case DBType.MySQL:
                case DBType.UX:
                case DBType.PostgreSQL:
                    sql = " AND  STR_TO_DATE(B.SDT,'%Y-%m-%d %H:%i') <  NOW()";
                    break;
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    sql = " AND  to_char(to_date(B.SDT,'yyyy-mm-dd,hh24:mi'),'yyyymmdd HH:mm')  < to_char(sysdate ,'yyyymmdd HH:mm')";
                    break;
                case DBType.MSSQL:
                    sql = " AND  convert(varchar(100),B.SDT,120) < CONVERT(varchar(100), GETDATE(), 120)";
                    break;
                default:
                    throw new Exception(SystemConfig.AppCenterDBType + "还没增加该数据库类型的解析");
            }
            ht.Add("GWF_OverCounts", DBAccess.RunSQLReturnValInt("SELECT COUNT(DISTINCT A.WorkID) FROM WF_GenerWorkFlow A,WF_GenerWorkerList B WHERE A.WorkID=B.WorkID AND A.WFState>1 AND  B.IsPass=0 AND B.FK_Flow='" + this.FK_Flow + "' "+sql));

            return BP.Tools.Json.ToJson(ht);
        }


        /// <summary>
        /// 获取单个流程的数据分析
        /// </summary>
        /// <returns></returns>
        public string DataPanelOrg_GetAnalyseByFlowNo_DataSet()
        {
            DataSet ds = new DataSet();
            string dbSQL = "";
            switch (SystemConfig.AppCenterDBType)
            {
                case DBType.MySQL:
                case DBType.PostgreSQL:
                case DBType.UX:
                    dbSQL = " AND FK_NY>YEAR(CURDATE())+'-00' ";
                    break;
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    dbSQL = " AND FK_NY>to_char(sysdate, 'yyyy' )+'-00' ";
                    break;
                case DBType.MSSQL:
                    dbSQL = " AND FK_NY>DateName(year,GetDate())+'-00' ";
                    break;
                default:
                    throw new Exception(SystemConfig.AppCenterDBType + "还没增加该数据库类型的解析");
            }
            //按照月份（统计的内容有发起数，完成数量，业务字段）
            //发起数量
            string sql = "SELECT SUBSTRING(FK_NY, 6, 2) AS FK_NY, count(WorkID) AS Num FROM WF_GenerWorkFlow WHERE WFState >1  AND FK_Flow='" + this.FK_Flow + "' "+ dbSQL+" GROUP BY FK_NY ";
            DataTable FlowStartByNY = DBAccess.RunSQLReturnTable(sql);
            FlowStartByNY.TableName = "FlowStartByNY";
            ds.Tables.Add(FlowStartByNY);
            //完成数量
            sql = "SELECT SUBSTRING(FK_NY, 6, 2) AS FK_NY, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState=3 AND  FK_Flow='" + this.FK_Flow + "' " + dbSQL + "  GROUP BY FK_NY ";
            DataTable FlowCompleteByNY = DBAccess.RunSQLReturnTable(sql);
            FlowCompleteByNY.TableName = "FlowCompleteByNY";
            ds.Tables.Add(FlowCompleteByNY);


            //按照部门
            //发起数量
            sql = "SELECT DeptName, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState >1  AND FK_Flow='" + this.FK_Flow + "' GROUP BY DeptName ";
            DataTable FlowStartByEmp = DBAccess.RunSQLReturnTable(sql);
            FlowStartByEmp.TableName = "FlowStartByDept";
            ds.Tables.Add(FlowStartByEmp);
            //完成数量
            sql = "SELECT DeptName, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState=3  AND FK_Flow='" + this.FK_Flow + "' GROUP BY DeptName ";
            DataTable FlowCompleteByEmp = DBAccess.RunSQLReturnTable(sql);
            FlowCompleteByEmp.TableName = "FlowCompleteByDept";
            ds.Tables.Add(FlowCompleteByEmp);

            return BP.Tools.Json.ToJson(ds);
        }

    }
}
