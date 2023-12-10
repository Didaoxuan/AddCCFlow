using System;
using System.Collections.Generic;
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

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class Admin_TestingContainer : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Admin_TestingContainer()
        {

        }

        #region TestFlow2020_Init
        /// <summary>
        /// 发起.
        /// </summary>
        /// <returns></returns>
        public string TestFlow2020_StartIt()
        {
            string sid = this.GetRequestVal("Token");
            if (WebUser.IsAdmin == false)
                return "err@非管理员无法测试";

            //用户编号.
            string userNo = this.GetRequestVal("UserNo");

            //BP.Cloud.Dev2Interface.Port_Login(userID:,)
            //判断是否可以测试该流程？ 

            //组织url发起该流程.
            string url = "../../../WF/Admin/TestingContainer/Default.html?RunModel=2&FK_Flow=" + this.FK_Flow + "&UserNo=" + userNo;
            url += "&OrgNo=" + WebUser.OrgNo;
            url += "&Adminer=" + WebUser.No;
            url += "&AdminerToken=" + sid;
            return url;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public string TestFlow2020_Init()
        {
            //清除缓存.
            BP.Difference.SystemConfig.DoClearCash();

            if (BP.Web.WebUser.IsAdmin == false)
                return "err@您不是管理员，无法执行该操作.";

            FlowExt fl = new FlowExt(this.FK_Flow);
            //if ( fl.Tester.Length <= 1)
            //{
            //    string msg = "err@二级管理员[" + BP.Web.WebUser.Name + "]您好,您尚未为该流程配置测试人员.";
            //    msg += "您需要在流程属性里的底部[设置流程发起测试人]的属性里，设置可以发起的测试人员,多个人员用逗号分开.";
            //    return msg;
            //}

            #region 检查是否是测试用户
            int nodeid = int.Parse(this.FK_Flow + "01");
            DataTable dt = null;
            string sql = "";
            BP.WF.Node nd = new BP.WF.Node(nodeid);
            if (nd.IsGuestNode)
            {
                /*如果是 guest 节点，就让其跳转到 guest登录界面，让其发起流程。*/
                //这个地址需要配置.
                return "url@/SDKFlowDemo/GuestApp/Login.htm?FK_Flow=" + this.FK_Flow;
            }
            #endregion 测试人员.


            #region 从配置里获取-测试人员.
            /* 检查是否设置了测试人员，如果设置了就按照测试人员身份进入
             * 设置测试人员的目的是太多了人员影响测试效率.
             * */
            if (fl.Tester.Length > 2)
            {
                // 构造人员表.
                DataTable dtEmps = new DataTable();
                dtEmps.Columns.Add("No");
                dtEmps.Columns.Add("Name");
                dtEmps.Columns.Add("FK_DeptText");

                string[] strs = fl.Tester.Split(',');
                foreach (string str in strs)
                {
                    if (DataType.IsNullOrEmpty(str) == true)
                        continue;

                    BP.Cloud.Emp emp = new BP.Cloud.Emp();
                    emp.OrgNo = this.GetRequestVal("OrgNo");
                    emp.UserID = str;
                    emp.No = emp.OrgNo + "_" + emp.UserID;
                    //emp.SetValByKey("UserID", str);
                    int i = emp.RetrieveFromDBSources();
                    if (i == 0)
                        continue;

                    DataRow dr = dtEmps.NewRow();
                    dr["No"] = emp.No;
                    dr["Name"] = emp.Name;
                    dr["FK_DeptText"] = emp.FK_DeptText;
                    dtEmps.Rows.Add(dr);
                }
                if (dtEmps.Rows.Count > 0)
                return BP.Tools.Json.ToJson(dtEmps);
            }
            #endregion 测试人员.

            #region 从设置里获取-测试人员.
            try
            {
                switch (nd.HisDeliveryWay)
                {
                    case DeliveryWay.ByStation:
                    case DeliveryWay.ByStationOnly:
                        sql = "SELECT A.No AS No,A.Name AS Name,B.Name AS FK_DeptText  FROM Port_Emp A ,Port_Dept B,Port_DeptEmpStation C,WF_NodeStation D WHERE  A.FK_Dept=B.No AND C.FK_Emp=A.No AND C.FK_Dept=B.No AND D.FK_Station=C.FK_Station AND A.OrgNo='" + WebUser.OrgNo + "' AND  D.FK_Node=" + nd.NodeID;
                        // emps.RetrieveInSQL_Order("select fk_emp from Port_Empstation WHERE fk_station in (select fk_station from WF_NodeStation WHERE FK_Node=" + nodeid + " )", "FK_Dept");
                        break;
                    case DeliveryWay.ByDept:
                        sql = "select A.No,A.Name,D.Name AS FK_DeptText from Port_Emp A,Port_Dept D where A.FK_Dept=D.No AND A.OrgNo='" + WebUser.OrgNo + "' AND A.FK_Dept in (select FK_Dept from WF_NodeDept where FK_Node='" + nodeid + "') ";
                        //emps.RetrieveInSQL("");
                        break;
                    case DeliveryWay.ByBindEmp:
                        if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.SAAS)
                            sql = "SELECT A.No,A.Name,D.Name AS FK_DeptText FROM Port_Emp A,Port_Dept D where  A.FK_Dept=D.No AND  A.OrgNo='" + WebUser.OrgNo + "' AND A.No in (select FK_Emp from WF_NodeEmp where FK_Node='" + nodeid + "') ";
                        else
                            sql = "SELECT A.No,A.Name,D.Name AS FK_DeptText FROM Port_Emp A,Port_Dept D where  A.FK_Dept=D.No AND  A.OrgNo='" + WebUser.OrgNo + "' AND A.UserID in (select FK_Emp from WF_NodeEmp where FK_Node='" + nodeid + "') ";

                        //emps.RetrieveInSQL("select fk_emp from wf_NodeEmp WHERE fk_node=" + int.Parse(this.FK_Flow + "01") + " ");
                        break;
                    case DeliveryWay.ByDeptAndStation:

                        sql = "SELECT A.No AS No,A.Name AS Name ,B.Name AS FK_DeptText"
                                  + " FROM  Port_Emp A,Port_Dept B, Port_DeptEmpStation C,WF_NodeDept D,WF_NodeStation E"
                                  + " WHERE A.FK_Dept = B.No AND (A.No=C.FK_Emp OR A.UserID=C.FK_Emp) AND B.No=C.FK_Dept AND C.FK_Dept = D.FK_Dept AND C.FK_Station=E.FK_Station AND D.FK_Node=E.FK_Node"
                                  + " AND E.FK_Node="+nodeid
                                  + " AND C.OrgNo='"+WebUser.OrgNo + "'"
                                  + " ORDER BY "
                                  + "        C.FK_Emp";

                        break;
                    case DeliveryWay.BySelected: //所有的人员多可以启动, 2016年11月开始约定此规则.
                        if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.Single)
                            sql = "SELECT a.No,a.Name,b.Name as FK_DeptText FROM Port_Emp A,Port_Dept B WHERE  A.FK_Dept=B.No ";
                        else
                            sql = "SELECT a.No,a.Name,b.Name as FK_DeptText FROM Port_Emp A,Port_Dept B WHERE a.OrgNo='" + WebUser.OrgNo + "' AND A.FK_Dept=B.No AND B.OrgNo='" + WebUser.OrgNo + "'  ";
                        dt = BP.DA.DBAccess.RunSQLReturnTable(sql);
                        break;
                    case DeliveryWay.BySQL:
                        if (DataType.IsNullOrEmpty(nd.DeliveryParas))
                            return "err@您设置的按SQL访问开始节点，但是您没有设置sql.";
                        break;
                    default:
                        break;
                }

                dt = BP.DA.DBAccess.RunSQLReturnTable(sql);
                if (dt.Rows.Count == 0)
                    return "err@您按照:" + nd.HisDeliveryWay + "的方式设置的开始节点的访问规则，但是开始节点没有人员。" + sql;

                if (dt.Rows.Count > 2000)
                    return "err@可以发起开始节点的人员太多，会导致系统崩溃变慢，您需要在流程属性里设置可以发起的测试用户.";

                //返回数据源.
                return BP.Tools.Json.ToJson(dt);
                #endregion 从设置里获取-测试人员.

            }
            catch (Exception ex)
            {
                return "err@<h2>您没有正确的设置开始节点的访问规则，这样导致没有可启动的人员，<a href='http://bbs.citydo.com.cn/showtopic-4103.aspx' target=_blank ><font color=red>点击这查看解决办法</font>.</a>。</h2> 系统错误提示:" + ex.Message + "<br><h3>也有可能你你切换了OSModel导致的，什么是OSModel,请查看在线帮助文档 <a href='http://ccbpm.mydoc.io' target=_blank>http://ccbpm.mydoc.io</a>  .</h3>";
            }
        }
        #endregion

    }
}
