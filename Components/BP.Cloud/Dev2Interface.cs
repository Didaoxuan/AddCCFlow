﻿using BP.DA;
using BP.Sys;
using BP.Web;
using BP.WF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Threading.Tasks;
using BP.Difference;


namespace BP.Cloud
{
    /// <summary>
    /// 云的公共类
    /// </summary>
    public class Dev2Interface
    {
        /// <summary>
        /// 多组织登录接口.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="orgNo"></param>
        public static string Port_Login(string userID, string orgNo)
        {
            BP.Cloud.Emp emp = new BP.Cloud.Emp();
            int i = emp.Retrieve("UserID", userID, "OrgNo", orgNo);
            if (i == 0)
                throw new Exception("err@用户名[" + userID + "],OrgNo[" + orgNo + "]不存在.");

            //调用登录.
            string str= Port_Login(emp);

            return BP.WF.Dev2Interface.Port_GenerToken("PC");
        }
        /// <summary>
        /// 登录
        /// </summary>
        public static string Port_Login(BP.Cloud.Emp emp)
        {
            // cookie操作，为适应不同平台，统一使用HttpContextHelper
            Dictionary<string, string> cookieValues = new Dictionary<string, string>();

            cookieValues.Add("No", emp.UserID);
            cookieValues.Add("Name", HttpUtility.UrlEncode(emp.Name));

            cookieValues.Add("FK_Dept", emp.FK_Dept);
            cookieValues.Add("FK_DeptName", HttpUtility.UrlEncode(emp.FK_DeptText));

            cookieValues.Add("OrgNo", emp.OrgNo);
            cookieValues.Add("OrgName", emp.OrgName);

            string token = "";

            cookieValues.Add("Tel", emp.Tel);
            cookieValues.Add("Lang", "CH");
            cookieValues.Add("Token", WebUser.Token);

            BP.Difference.HttpContextHelper.ResponseCookieAdd(cookieValues, null, "CCS");

            //给 session 赋值.
            HttpContextHelper.SessionSet("No", emp.UserID);
            HttpContextHelper.SessionSet("Name", emp.Name);
            HttpContextHelper.SessionSet("FK_Dept", emp.FK_Dept);
            HttpContextHelper.SessionSet("FK_DeptText", emp.FK_DeptText);
            HttpContextHelper.SessionSet("OrgNo", emp.OrgNo);
            HttpContextHelper.SessionSet("OrgName", emp.OrgName);
           /* BP.Difference.HttpContextHelper.Current.Session["No"] = emp.UserID;
            BP.Difference.HttpContextHelper.Current.Session["Name"] = emp.Name;
            BP.Difference.HttpContextHelper.Current.Session["FK_Dept"] = emp.FK_Dept;
            BP.Difference.HttpContextHelper.Current.Session["FK_DeptText"] = emp.FK_DeptText;
            BP.Difference.HttpContextHelper.Current.Session["OrgNo"] = emp.OrgNo;
            BP.Difference.HttpContextHelper.Current.Session["OrgName"] = emp.OrgName;*/

            return token;
        }
        public static DataTable DB_StarFlows(string userNo, string domain = null)
        {
            string sql = "SELECT A.ICON, A.No,A.Name,a.IsBatchStart,";
            sql += " a.FK_FlowSort,B.Name AS FK_FlowSortText,B.Domain,A.IsStartInMobile, A.Idx,";
            sql += " a.WorkModel,"; // 0=内部流程1=外部流程2=实体台账3=表单.
            sql += " a.PTable as FrmID "; // 表单ID,为实体台账的时候存储的表单ID.
            sql += " FROM WF_Flow A, WF_FlowSort B  ";
            sql += " WHERE   A.IsCanStart=1 AND A.FK_FlowSort=B.No  AND A.OrgNo='" + WebUser.OrgNo + "' ";
            sql += " ORDER BY A.Idx ";
            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            return dt;
        }
        /// <summary>
        /// 获取未完成的流程(也称为在途流程:我参与的但是此流程未完成)
        /// </summary>
        /// <returns>返回从数据视图WF_GenerWorkflow查询出来的数据.</returns>
        public static DataTable DB_GenerRuning(string userNo = null, bool isContainFuture = false, string domain = null)
        {
            if (userNo == null)
                userNo = WebUser.No;

            DataTable dt = DB_GenerRuning(userNo, null, false, null, isContainFuture);

            /*暂时屏蔽type的拼接，拼接后转json会报错 于庆海修改*/
            /*dt.Columns.Add("Type");
            foreach (DataRow row in dt.Rows)
            {
                row["Type"] = "RUNNING";
            }*/

            dt.DefaultView.Sort = "RDT DESC";
            return dt.DefaultView.ToTable();
        }

        /// <summary>
        /// 获取未完成的流程(也称为在途流程:我参与的但是此流程未完成)
        /// 该接口为在途菜单提供数据,在在途工作中，可以执行撤销发送。
        /// </summary>
        /// <param name="userNo">操作员</param>
        /// <param name="fk_flow">流程编号</param>
        /// <param name="isMyStarter">是否仅仅查询我发起的在途流程</param>
        /// <returns>返回从数据视图WF_GenerWorkflow查询出来的数据.</returns>
        public static DataTable DB_GenerRuning(string userNo, string fk_flow,
            bool isMyStarter = false, string domain = null, bool isContainFuture = false)
        {
            string dbStr =  BP.Difference.SystemConfig.AppCenterDBVarStr;
            Paras ps = new Paras();

            string domainSQL = "";
            if (domain == null)
                domainSQL = " AND Domain='" + domain + "' ";
            //获取用户当前所在的节点
            String currNode = "";
            switch (DBAccess.AppCenterDBType)
            {
                case DBType.Oracle:
                    currNode = "(SELECT FK_Node FROM (SELECT FK_Node FROM WF_GenerWorkerlist WHERE FK_Emp='" + WebUser.No + "' AND  OrgNo='" + WebUser.OrgNo + "'  Order by RDT DESC ) WHERE RowNum=1)";
                    break;
                case DBType.MySQL:
                case DBType.PostgreSQL:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                case DBType.UX:
                    currNode = "(SELECT  FK_Node FROM WF_GenerWorkerlist WHERE FK_Emp='" + WebUser.No + "' AND  OrgNo='" + WebUser.OrgNo + "' Order by RDT DESC LIMIT 1)";
                    break;
                case DBType.MSSQL:
                    currNode = "(SELECT TOP 1 FK_Node FROM WF_GenerWorkerlist WHERE FK_Emp='" + WebUser.No + "' AND  OrgNo='" + WebUser.OrgNo + "' Order by RDT DESC)";
                    break;
                default:
                    break;
            }


            //授权模式.
            string sql = "";
            string futureSQL = "";
            if (isContainFuture == true)
            {
                switch (DBAccess.AppCenterDBType)
                {
                    case DBType.MySQL:
                        futureSQL = " UNION SELECT A.WorkID,A.StarterName,A.Title,A.DeptName,D.Name AS NodeName,A.RDT,B.FK_Node,A.FK_Flow,A.FID,A.FlowName,C.EmpName AS TodoEmps," + currNode + " AS CurrNode ,1 AS RunType FROM WF_GenerWorkFlow A, WF_SelectAccper B,"
                                + "(SELECT GROUP_CONCAT(B.EmpName SEPARATOR ';') AS EmpName, B.WorkID,B.FK_Node FROM WF_GenerWorkFlow A, WF_SelectAccper B WHERE A.WorkID = B.WorkID  group By B.FK_Node) C,WF_Node D"
                                + " WHERE A.WorkID = B.WorkID AND B.WorkID = C.WorkID AND B.FK_Node = C.FK_Node AND A.FK_Node = D.NodeID AND B.FK_Emp = '" + WebUser.No + "'"
                                + " AND B.FK_Node Not in(Select DISTINCT FK_Node From WF_GenerWorkerlist G where G.WorkID = B.WorkID)AND A.WFState != 3";
                        break;
                    case DBType.MSSQL:
                        futureSQL = " UNION SELECT A.WorkID,A.StarterName,A.Title,A.DeptName,D.Name AS NodeName,A.RDT,B.FK_Node,A.FK_Flow,A.FID,A.FlowName,C.EmpName AS TodoEmps ," + currNode + " AS CurrNode ,1 AS RunType FROM WF_GenerWorkFlow A, WF_SelectAccper B,"
                                + "(SELECT EmpName=STUFF((Select ';'+FK_Emp+','+EmpName From WF_SelectAccper t Where t.FK_Node=B.FK_Node FOR xml path('')) , 1 , 1 , '') , B.WorkID,B.FK_Node FROM WF_GenerWorkFlow A, WF_SelectAccper B WHERE A.WorkID = B.WorkID  group By B.FK_Node,B.WorkID) C,WF_Node D"
                                + " WHERE A.WorkID = B.WorkID AND B.WorkID = C.WorkID AND B.FK_Node = C.FK_Node AND A.FK_Node = D.NodeID AND B.FK_Emp = '" + WebUser.No + "'"
                                + " AND B.FK_Node Not in(Select DISTINCT FK_Node From WF_GenerWorkerlist G where G.WorkID = B.WorkID)AND A.WFState != 3";
                        break;
                    default:
                        break;

                }
            }




            //非授权模式，

            if (DataType.IsNullOrEmpty(fk_flow))
            {
                if (isMyStarter == true)
                {
                    sql = "SELECT DISTINCT a.WorkID,a.StarterName,a.Title,a.DeptName,a.NodeName,a.RDT,a.FK_Node,a.FK_Flow,a.FID ,a.FlowName,a.TodoEmps," + currNode + " AS CurrNode,0 AS RunType FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B WHERE A.TodoEmps  not like '%" + WebUser.No + ",%' AND A.WorkID=B.WorkID AND B.FK_Emp=" + dbStr + "FK_Emp AND B.IsEnable=1 AND  (B.IsPass=1 or B.IsPass < -1) AND  A.Starter=" + dbStr + "Starter  AND  OrgNo='" + WebUser.OrgNo + "'";
                    if (isContainFuture == true)
                    {
                        sql += futureSQL;
                    }
                    ps.SQL = sql;
                    ps.Add("FK_Emp", userNo);
                    ps.Add("Starter", userNo);
                }
                else
                {
                    sql = "SELECT DISTINCT a.WorkID,a.StarterName,a.Title,a.DeptName,a.NodeName,a.RDT,a.FK_Node,a.FK_Flow,a.FID ,a.FlowName,a.TodoEmps ," + currNode + " AS CurrNode,0 AS RunType FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B WHERE A.TodoEmps  not like '%" + WebUser.No + ",%' AND A.WorkID=B.WorkID AND B.FK_Emp=" + dbStr + "FK_Emp AND B.IsEnable=1 AND  (B.IsPass=1 or B.IsPass < -1)  AND  OrgNo='" + WebUser.OrgNo + "'";
                    if (isContainFuture == true)
                    {
                        sql += futureSQL;
                    }
                    ps.SQL = sql;
                    ps.Add("FK_Emp", userNo);
                }
            }
            else
            {
                if (isMyStarter == true)
                {
                    sql = "SELECT DISTINCT a.WorkID,a.StarterName,a.Title,a.DeptName,a.NodeName,a.RDT,a.FK_Node,a.FK_Flow,a.FID ,a.FlowName,a.TodoEmps ," + currNode + " AS CurrNode,0 AS RunType FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B WHERE A.TodoEmps  not like '%" + WebUser.No + ",%' AND A.FK_Flow=" + dbStr + "FK_Flow  AND A.WorkID=B.WorkID AND B.FK_Emp=" + dbStr + "FK_Emp AND B.IsEnable=1 AND (B.IsPass=1 or B.IsPass < -1 ) AND  A.Starter=" + dbStr + "Starter  AND  OrgNo='" + WebUser.OrgNo + "'";
                    if (isContainFuture == true)
                    {
                        sql += futureSQL;
                    }
                    ps.SQL = sql;
                    ps.Add("FK_Flow", fk_flow);
                    ps.Add("FK_Emp", userNo);
                    ps.Add("Starter", userNo);
                }
                else
                {
                    sql = "SELECT DISTINCT a.WorkID,a.StarterName,a.Title,a.DeptName,a.NodeName,a.RDT,a.FK_Node,a.FK_Flow,a.FID ,a.FlowName,a.TodoEmps ," + currNode + " AS CurrNode,0 AS RunType  FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B WHERE A.TodoEmps  not like '%" + WebUser.No + ",%' AND A.FK_Flow=" + dbStr + "FK_Flow  AND A.WorkID=B.WorkID AND B.FK_Emp=" + dbStr + "FK_Emp AND B.IsEnable=1 AND (B.IsPass=1 or B.IsPass < -1 )  AND  OrgNo='" + WebUser.OrgNo + "'";
                    if (isContainFuture == true)
                    {
                        sql += futureSQL;
                    }
                    ps.SQL = sql;
                    ps.Add("FK_Flow", fk_flow);
                    ps.Add("FK_Emp", userNo);
                }
            }

            //获得sql.
            DataTable dt = DBAccess.RunSQLReturnTable(ps);
            if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel== FieldCaseModel.UpperCase)
            {
                dt.Columns["WORKID"].ColumnName = "WorkID";
                dt.Columns["STARTERNAME"].ColumnName = "StarterName";
                dt.Columns["TITLE"].ColumnName = "Title";
                dt.Columns["NODENAME"].ColumnName = "NodeName";
                dt.Columns["RDT"].ColumnName = "RDT";
                dt.Columns["FK_FLOW"].ColumnName = "FK_Flow";
                dt.Columns["FID"].ColumnName = "FID";
                dt.Columns["FK_NODE"].ColumnName = "FK_Node";
                dt.Columns["FLOWNAME"].ColumnName = "FlowName";
                dt.Columns["DEPTNAME"].ColumnName = "DeptName";
                dt.Columns["TODOEMPS"].ColumnName = "TodoEmps";
                dt.Columns["CURRNODE"].ColumnName = "CurrNode";
                dt.Columns["RUNTYPE"].ColumnName = "RunType";
            }

            if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel == FieldCaseModel.Lowercase)
            {
                dt.Columns["workid"].ColumnName = "WorkID";
                dt.Columns["startername"].ColumnName = "StarterName";
                dt.Columns["title"].ColumnName = "Title";
                dt.Columns["nodename"].ColumnName = "NodeName";
                dt.Columns["rdt"].ColumnName = "RDT";
                dt.Columns["fk_flow"].ColumnName = "FK_Flow";
                dt.Columns["fid"].ColumnName = "FID";
                dt.Columns["fk_node"].ColumnName = "FK_Node";
                dt.Columns["flowname"].ColumnName = "FlowName";
                dt.Columns["deptname"].ColumnName = "DeptName";
                dt.Columns["todoemps"].ColumnName = "TodoEmps";
                dt.Columns["currnode"].ColumnName = "CurrNode";
                dt.Columns["runtype"].ColumnName = "RunType";
            }

            return dt;
        }

        public static DataTable DB_GenerRuningNotMyStart(string userNo)
        {
            string dbStr =  BP.Difference.SystemConfig.AppCenterDBVarStr;
            Paras ps = new Paras();

            //获取用户当前所在的节点
            String currNode = "";
            switch (DBAccess.AppCenterDBType)
            {
                case DBType.Oracle:
                    currNode = "(SELECT FK_Node FROM (SELECT G.FK_Node FROM WF_GenerWorkFlow A,WF_GenerWorkerlist G WHERE G.WorkID = A.WorkID AND G.FK_Emp='" + WebUser.No + "' Order by G.RDT DESC ) WHERE RowNum=1)";
                    break;
                case DBType.MySQL:
                case DBType.PostgreSQL:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                case DBType.UX:
                    currNode = "(SELECT  FK_Node FROM WF_GenerWorkerlist G WHERE   G.WorkID = A.WorkID AND FK_Emp='" + WebUser.No + "' Order by RDT DESC LIMIT 1)";
                    break;
                case DBType.MSSQL:
                    currNode = "(SELECT TOP 1 FK_Node FROM WF_GenerWorkerlist G WHERE  G.WorkID = A.WorkID AND FK_Emp='" + WebUser.No + "' Order by RDT DESC)";
                    break;
                default:
                    break;
            }

            string sql = "SELECT DISTINCT a.WorkID,a.StarterName,a.Title,a.DeptName,a.NodeName,a.RDT," +
                "a.FK_Node,a.FK_Flow,a.FID ,a.FlowName,a.TodoEmps ," + currNode + " AS CurrNode,0 AS RunType FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B WHERE A.TodoEmps  not like '%" + WebUser.No + ",%' AND A.WorkID=B.WorkID AND B.FK_Emp=" + dbStr + "FK_Emp AND B.IsEnable=1 AND  (B.IsPass=1 or B.IsPass < -1)  AND  OrgNo='" + WebUser.OrgNo + "' AND A.Starter!=" + dbStr + "Starter";

            ps.SQL = sql;
            ps.Add("FK_Emp", userNo);
            ps.Add("Starter", userNo);

            //获得sql.
            DataTable dt = DBAccess.RunSQLReturnTable(ps);
            if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel!=FieldCaseModel.None)
            {
                dt.Columns[0].ColumnName = "WorkID";
                dt.Columns[1].ColumnName = "StarterName";
                dt.Columns[2].ColumnName = "Title";
                dt.Columns[3].ColumnName = "DeptName";
                dt.Columns[4].ColumnName = "NodeName";
                dt.Columns[5].ColumnName = "RDT";
                dt.Columns[6].ColumnName = "FK_Node";
                dt.Columns[7].ColumnName = "FK_Flow";
                dt.Columns[8].ColumnName = "FID";
                dt.Columns[9].ColumnName = "FlowName";
                dt.Columns[10].ColumnName = "TodoEmps";
                dt.Columns[11].ColumnName = "CurrNode";
                dt.Columns[12].ColumnName = "RunType";
            }
            return dt;
        }

        /// <summary>
        /// 获取某一个人已完成的流程
        /// </summary>
        /// <param name="userNo">用户编码</param>
        /// <param name="isMyStart">是否是用户发起的</param>
        /// <returns></returns>
        public static DataTable DB_FlowCompleteNotMyStart(string userNo)
        {
            Paras ps = new Paras();
            string dbstr =  BP.Difference.SystemConfig.AppCenterDBVarStr;
            ps.SQL = "SELECT 'RUNNING' AS Type, T.* FROM WF_GenerWorkFlow T WHERE T.Starter!=" + dbstr + "Starter AND (T.Emps LIKE '%@" + userNo + "@%' OR  T.Emps LIKE '%@" + userNo + ",%') AND T.FID=0 AND T.WFState=" + (int)WFState.Complete + " ORDER BY  RDT DESC";
            ps.Add("Starter", userNo);
            DataTable dt = DBAccess.RunSQLReturnTable(ps);

            //需要翻译.
            if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel == FieldCaseModel.UpperCase)
            {
                dt.Columns["TYPE"].ColumnName = "Type";
                dt.Columns["WORKID"].ColumnName = "WorkID";
                dt.Columns["FK_FLOWSORT"].ColumnName = "FK_FlowSort";
                dt.Columns["SYSTYPE"].ColumnName = "SysType";
                dt.Columns["FK_FLOW"].ColumnName = "FK_Flow";
                dt.Columns["FLOWNAME"].ColumnName = "FlowName";
                dt.Columns["TITLE"].ColumnName = "Title";
                dt.Columns["WFSTA"].ColumnName = "WFSta";
                dt.Columns["WFSTATE"].ColumnName = "WFState";
                dt.Columns["STARTER"].ColumnName = "Starter";
                dt.Columns["STARTERNAME"].ColumnName = "StarterName";
                dt.Columns["SENDER"].ColumnName = "Sender";
                dt.Columns["FK_NODE"].ColumnName = "FK_Node";
                dt.Columns["NODENAME"].ColumnName = "NodeName";
                dt.Columns["FK_DEPT"].ColumnName = "FK_Dept";
                dt.Columns["DEPTNAME"].ColumnName = "DeptName";
                dt.Columns["SDTOFNODE"].ColumnName = "SDTOfNode";
                dt.Columns["SDTOFFLOW"].ColumnName = "SDTOfFlow";
                dt.Columns["PFLOWNO"].ColumnName = "PflowNo";
                dt.Columns["PWORKID"].ColumnName = "PWorkID";
                dt.Columns["PNODEID"].ColumnName = "PNodeID";
                dt.Columns["PEMP"].ColumnName = "PEmp";
                dt.Columns["GUESTNO"].ColumnName = "GuestNo";
                dt.Columns["GUESTNAME"].ColumnName = "GuestName";
                dt.Columns["BILLNO"].ColumnName = "BillNo";
                dt.Columns["FLOWNOTE"].ColumnName = "FlowNote";
                dt.Columns["TODOEMPS"].ColumnName = "TodoEmps";
                dt.Columns["TODOEMPSNUM"].ColumnName = "TodoEmpsNum";
                dt.Columns["TASKSTA"].ColumnName = "TaskSta";
                dt.Columns["ATPARA"].ColumnName = "AtPara";
                dt.Columns["EMPS"].ColumnName = "Emps";
                dt.Columns["DOMAIN"].ColumnName = "Domain";
                dt.Columns["SENDDT"].ColumnName = "SendDT";
                dt.Columns["WEEKNUM"].ColumnName = "WeekNum";
            }
            if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel == FieldCaseModel.Lowercase)
            {
                dt.Columns["type"].ColumnName = "Type";
                dt.Columns["workid"].ColumnName = "WorkID";
                dt.Columns["fk_flowsort"].ColumnName = "FK_FlowSort";
                dt.Columns["systype"].ColumnName = "SysType";
                dt.Columns["fk_flow"].ColumnName = "FK_Flow";
                dt.Columns["flowname"].ColumnName = "FlowName";
                dt.Columns["title"].ColumnName = "Title";
                dt.Columns["wfsta"].ColumnName = "WFSta";
                dt.Columns["wfstate"].ColumnName = "WFState";
                dt.Columns["starter"].ColumnName = "Starter";
                dt.Columns["startername"].ColumnName = "StarterName";
                dt.Columns["sender"].ColumnName = "Sender";
                dt.Columns["fk_node"].ColumnName = "FK_Node";
                dt.Columns["nodename"].ColumnName = "NodeName";
                dt.Columns["fk_dept"].ColumnName = "FK_Dept";
                dt.Columns["deptname"].ColumnName = "DeptName";
                dt.Columns["sdtofnode"].ColumnName = "SDTOfNode";
                dt.Columns["sdtofflow"].ColumnName = "SDTOfFlow";
                dt.Columns["pflowno"].ColumnName = "PflowNo";
                dt.Columns["pworkid"].ColumnName = "PWorkID";
                dt.Columns["pnodeid"].ColumnName = "PNodeID";
                dt.Columns["pemp"].ColumnName = "PEmp";
                dt.Columns["guestno"].ColumnName = "GuestNo";
                dt.Columns["guestname"].ColumnName = "GuestName";
                dt.Columns["billno"].ColumnName = "BillNo";
                dt.Columns["flownote"].ColumnName = "FlowNote";
                dt.Columns["todoemps"].ColumnName = "TodoEmps";
                dt.Columns["todoempsnum"].ColumnName = "TodoEmpsNum";
                dt.Columns["tasksta"].ColumnName = "TaskSta";
                dt.Columns["atpara"].ColumnName = "AtPara";
                dt.Columns["emps"].ColumnName = "Emps";
                dt.Columns["domain"].ColumnName = "Domain";
                dt.Columns["senddt"].ColumnName = "SendDT";
                dt.Columns["weeknum"].ColumnName = "WeekNum";
            }
            return dt;
        }
        /// <summary>
        /// 待办工作数量
        /// </summary>
        public static int Todolist_EmpWorks
        {
            get
            {
                Paras ps = new Paras();
                string dbstr =  BP.Difference.SystemConfig.AppCenterDBVarStr;

                /*不是授权状态*/
                if (BP.WF.Glo.IsEnableTaskPool == true)
                {
                    ps.SQL = "SELECT count(WorkID) as Num FROM WF_EmpWorks WHERE FK_Emp=" + dbstr + "FK_Emp AND OrgNo=" + dbstr + "OrgNo AND TaskSta!=1 ";
                }
                else
                {
                    ps.SQL = "SELECT count(WorkID) as Num FROM WF_EmpWorks WHERE  FK_Emp=" + dbstr + "FK_Emp AND OrgNo=" + dbstr + "OrgNo";
                }

                ps.Add("FK_Emp", BP.Web.WebUser.No);
                ps.Add("OrgNo", BP.Web.WebUser.OrgNo);

                //  Log.DebugWriteInfo(ps.SQL);
                return DBAccess.RunSQLReturnValInt(ps);
            }
        }

        /// <summary>
        /// 抄送数量
        /// </summary>
        public static int Todolist_CCWorks
        {
            get
            {
                Paras ps = new Paras();

                ps.SQL = "SELECT count(MyPK) as Num FROM WF_CCList WHERE CCTo=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "FK_Emp AND OrgNo=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "OrgNo";
                ps.Add("FK_Emp", BP.Web.WebUser.No);
                ps.Add("OrgNo", BP.Web.WebUser.OrgNo);
                return DBAccess.RunSQLReturnValInt(ps, 0);
            }
        }


        /// <summary>
        /// 退回给当前用户的数量
        /// </summary>
        public static int Todolist_ReturnNum
        {
            get
            {
                string sql = "SELECT  COUNT(WorkID) AS Num from WF_GenerWorkFlow where WFState=5 and (TodoEmps like '%" + WebUser.No + ",%' OR TodoEmps like '%" + WebUser.No + ";%') AND OrgNo='" + WebUser.OrgNo + "' AND WorkID in (SELECT distinct WorkID FROM WF_ReturnWork WHERE ReturnToEmp='" + BP.Web.WebUser.No + "')";

                return DBAccess.RunSQLReturnValInt(sql);
            }
        }
        /// <summary>
        /// 待办逾期的数量
        /// </summary>
        public static int Todolist_OverWorkNum
        {
            get
            {
                string sql = "";
                string whereSQL = "AND convert(varchar(100),A.SDT,120)<CONVERT(varchar(100), GETDATE(), 120) AND A.WFState=2 AND A.FK_Node NOT like '%01' AND ListType=0";


                if (BP.WF.Glo.IsEnableTaskPool == true)
                    sql = "SELECT  Count(*) FROM WF_EmpWorks A WHERE  TaskSta=0 AND A.FK_Emp='" + WebUser.No + "' " + whereSQL;
                else
                    sql = "SELECT Count(*) FROM WF_EmpWorks A WHERE  A.FK_Emp='" + WebUser.No + "' " + whereSQL;

                //获得授权信息.
                Auths aths = new Auths();
                aths.Retrieve(AuthAttr.AutherToEmpNo, WebUser.No);
                foreach (Auth ath in aths)
                {

                    string todata = ath.TakeBackDT.Replace("-", "");
                    if (DataType.IsNullOrEmpty(ath.TakeBackDT) == false)
                    {
                        int mydt = int.Parse(todata);
                        int nodt = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                        if (mydt < nodt)
                            continue;
                        sql += " UNION ";

                        if (ath.AuthType == AuthorWay.SpecFlows)
                            sql += "SELECT Count(*) FROM WF_EmpWorks A WHERE  FK_Emp='" + ath.Auther + "' AND FK_Flow='" + ath.FlowNo + "' " + whereSQL;
                        else
                            sql += "SELECT Count(*) FROM WF_EmpWorks A WHERE  FK_Emp='" + ath.Auther + "' " + whereSQL;


                    }
                }

                //string sql = "SELECT COUNT(*) FROM WF_EmpWorks WHERE OrgNo='" + WebUser.OrgNo + "' AND FK_Emp='" + WebUser.No + "' AND A.FK_Node=B.FK_Node  AND  convert(varchar(100),A.SDT,120)>CONVERT(varchar(100), GETDATE(), 120)";

                return DBAccess.RunSQLReturnValInt(sql);
            }
        }
    }
}
