﻿using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Collections;
using Microsoft.Win32;
using BP.Sys;
using BP.DA;
using BP.En;
using BP.Web;
using BP.Port;
using BP.WF.Data;
using BP.WF.Template;
using System.Net;
using System.IO;
using System.Collections.Generic;
using BP.Sys.FrmUI;
using System.Linq;
using System.Text.RegularExpressions;
using BP.Tools;
using BP.Difference;
using BP.WF.Template.SFlow;
using BP.WF.Template.CCEn;
using BP.WF.Template.Frm;
using System.Reflection;

namespace BP.WF
{
    /// <summary>
    /// 全局(方法处理)
    /// </summary>
    public class Glo
    {
        #region 生成文档.
        public static string TableSrcListFields(string nameSpace, string desc)
        {
            Entities ens = BP.En.ClassFactory.GetEns("BP.Port.Emps");

            var html = desc;
            Hashtable al = ClassFactory.Htable_Ens;
            int idx = 0;
            foreach (string item in al.Keys)
            {
                if (DataType.IsNullOrEmpty(item) == true)
                    continue;
                if (item.Contains(nameSpace) == false)
                    continue;
                idx++;
                Entity myen = ClassFactory.GetEns(item).GetNewEntity;
                try
                {

                    string name = myen.EnDesc;
                }
                catch (Exception x)
                {
                    continue;
                }
                try
                {

                    BP.WF.HttpHandler.WF_Comm_Sys sysHanderl = new HttpHandler.WF_Comm_Sys();
                    html += "<br/><br/>";
                    html += sysHanderl.SystemClass_Fields_Ext(item);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return html;
        }
        public static string TableSrcList(string nameSpace, string desc)
        {
            Entities ens = BP.En.ClassFactory.GetEns("BP.Port.Emps");

            var html = desc;
            html += "<table>";
            html += "<tr>";
            html += "<th>#</th>";
            html += "<th>描述</th>";
            html += "<th>表名</th>";
            html += "<th>主键</th>";
            html += "<th>字段数</th>";
            html += "</tr>";

            Hashtable al = ClassFactory.Htable_Ens;
            int idx = 0;
            foreach (string item in al.Keys)
            {
                if (DataType.IsNullOrEmpty(item) == true)
                    continue;
                if (item.Contains(nameSpace) == false)
                    continue;
                idx++;
                Entity myen = ClassFactory.GetEns(item).GetNewEntity;
                try
                {

                    string name = myen.EnDesc;
                }
                catch (Exception x)
                {
                    continue;
                }
                html += "<tr>";
                html += "<td>" + idx + "</td>";
                html += "<td>" + myen.EnDesc + "</td>";
                html += "<td>" + myen.EnMap.PhysicsTable + "</td>";
                html += "<td>" + myen.PK + "</td>";
                html += "<td>" + myen.EnMap.Attrs.Count + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        #endregion 生成文档.

        #region 新建节点-流程-默认值.

        #endregion 默认值.

        /// <summary>
        /// 检查执行SQL的合法性.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool CheckSQL(string sql)
        {
            if (WebUser.No.Equals(sql) == true)
                return true;

            if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.SAAS)
            {
                if (sql.Contains("OrgNo") == false || sql.Contains(BP.Web.WebUser.OrgNo) == false)
                    throw new Exception("err@执行的SQL必须包含OrgNo字段,或者字段值.");
            }
            return false;
        }

        /// <summary>
        /// 删除垃圾数据
        /// </summary>
        public static void ClearDustDB()
        {

            string sqls = "UPDATE WF_Node SET IsCCFlow=0";
            sqls += "@UPDATE WF_Node  SET IsCCFlow=1 WHERE NodeID IN (SELECT NodeID FROM WF_Cond a WHERE a.NodeID= NodeID AND CondType=1 )";
            DBAccess.RunSQLs(sqls);

            // 删除垃圾数据. 
            DBAccess.RunSQL("DELETE FROM WF_NodeEmp WHERE FK_Emp  NOT IN (SELECT " + BP.Sys.Base.Glo.UserNoWhitOutAS + " FROM Port_Emp )");
            DBAccess.RunSQL("DELETE FROM WF_Emp WHERE NO NOT IN (SELECT No FROM Port_Emp )");

            DBAccess.RunSQL("UPDATE WF_Emp SET Name=(SELECT Name From Port_Emp WHERE Port_Emp.No=WF_Emp.No),FK_Dept=(select FK_Dept from Port_Emp where Port_Emp.No=WF_Emp.No)");

            // 更新是否是有完成条件的节点。
            DBAccess.RunSQL("DELETE FROM WF_Direction WHERE Node=0 OR ToNode=0");
            DBAccess.RunSQL("DELETE FROM WF_Direction WHERE Node  NOT IN (SELECT NODEID FROM WF_Node )");
            DBAccess.RunSQL("DELETE FROM WF_Direction WHERE ToNode  NOT IN (SELECT NODEID FROM WF_Node) ");
        }
        /// <summary>
        /// 签批组件SQL
        /// </summary>
        public static string SQLOfBillNo
        {
            get
            {
                string sql = "";
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.MSSQL:
                    case DBType.MySQL:
                        sql = "SELECT '' AS No, '-请选择-' as Name ";
                        break;
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        sql = "SELECT '' AS No, '-请选择-' as Name FROM DUAL ";
                        break;
                    case DBType.PostgreSQL:
                    case DBType.UX:
                    default:
                        sql = "SELECT '' AS No, '-请选择-' as Name FROM Port_Emp WHERE 1=2 ";
                        break;
                }
                sql += " union ";
                sql += " SELECT KeyOfEn AS No,Name From Sys_MapAttr WHERE UIContralType=0 AND UIVisible=1 AND UIIsEnable=1 AND FK_MapData='@FK_Frm'";
                return sql;
            }
        }
        /// <summary>
        /// 签批组件SQL
        /// </summary>
        public static string SQLOfCheckField
        {
            get
            {
                string sql = "";
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.MSSQL:
                    case DBType.MySQL:
                        sql = "SELECT '' AS No, '-请选择-' as Name ";
                        break;
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        sql = "SELECT '' AS No, '-请选择-' as Name FROM DUAL ";
                        break;
                    case DBType.PostgreSQL:
                    case DBType.UX:
                    default:
                        sql = "SELECT '' AS No, '-请选择-' as Name FROM Port_Emp WHERE 1=2 ";
                        break;
                }
                sql += " union ";
                sql += " SELECT KeyOfEn AS No,Name From Sys_MapAttr WHERE UIContralType=14 AND FK_MapData='@FK_Frm'";
                return sql;
            }
        }

        #region 获取[新建-节点-流程]默认值.
        /// <summary>
        /// 新建节点的审核意见默认值.
        /// </summary>
        public static string DefVal_WF_Node_FWCDefInfo
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("DefVal_WF_Node_FWCDefInfo",
                    "同意");
            }
        }
        #endregion 获取[新建流程]默认值.

        #region 高级配置.
        /// <summary>
        /// CCBPMRunModel
        /// </summary>
        public static CCBPMRunModel CCBPMRunModel
        {
            get
            {
                return BP.Difference.SystemConfig.CCBPMRunModel;
            }
        }
        public static string GenerGanttDataOfSubFlows(Int64 workID)
        {
            GenerWorkFlow gwf = new GenerWorkFlow(workID);

            //增加子流程数据.
            GenerWorkFlows gwfs = new GenerWorkFlows();
            gwfs.Retrieve("PWorkID", workID);
            string json = "[";


            //主流程的计划完成日期，与实际完成日期的两个时间段.
            json += " { id:'001', name:'总体计划', type:0,";
            json += "series:[{ ";
            json += " name: '项目计划', ";
            json += " start:  " + ToData(gwf.RDTOfSetting) + ", ";
            json += " end: " + ToData(gwf.SDTOfFlow) + ",";
            json += " TodoSta: " + gwf.TodoSta + ", ";
            json += " color: 'blue' ";
            json += "},";

            json += "{ name: '实际执行', ";
            json += " start:  " + ToData(gwf.RDT) + ",";
            if (gwf.WFState == WFState.Complete)
                json += " end: " + ToData(gwf.SendDT) + ",";
            else
                json += " end:' " + DateTime.Now.ToString("yyyy-MM-dd") + "',";

            json += " TodoSta: " + gwf.TodoSta + ",";

            if (gwf.WFSta == WFSta.Complete)//流程运行结束
            {
                if (ToData(gwf.SendDT).CompareTo(ToData(gwf.SDTOfFlow)) <= 0)//正常
                    json += " color: 'green' ";
                else
                    json += " color: 'red' ";
            }
            else //未完成
            {
                string sendDt = DataType.ParseSysDate2DateTime(gwf.SendDT).ToString("yyyy-MM-dd");
                string wadingDtOfF = DataType.AddDays(sendDt, 3, TWay.Holiday).ToString("yyyy-MM-dd");
                if (sendDt.CompareTo(ToData(gwf.SDTOfFlow)) > 0)//逾期
                    json += " color: 'red' ";
                else
                {
                    if (wadingDtOfF.CompareTo(ToData(gwf.SDTOfFlow)) > 0)
                        json += " color: 'yellow' ";
                    else
                        json += " color: 'green' ";

                }

            }
            json += "}]";
            json += "},";


            //获得节点.
            Nodes nds = new Nodes(gwf.FK_Flow);
            nds.Retrieve("FK_Flow", gwf.FK_Flow, "Step");

            //流转自定义
            string sql = "SELECT FK_Node AS NodeID,NodeName AS Name From WF_TransferCustom WHERE WorkID=" + gwf.WorkID + " AND IsEnable=1 Order By Idx";
            DataTable dtYL = DBAccess.RunSQLReturnTable(sql);
            bool isFirstYLT = true;
            Nodes nodes = new Nodes();
            //含有流转自定义文件
            if (dtYL.Rows.Count != 0)
            {
                foreach (Node nd in nds)
                {
                    if (nd.GetParaBoolen("IsYouLiTai") == true)
                    {
                        if (isFirstYLT == true)
                        {
                            foreach (DataRow dr in dtYL.Rows)
                            {
                                nodes.AddEntity(nds.GetEntityByKey(Int32.Parse(dr["NodeID"].ToString())));
                            }
                        }

                        isFirstYLT = false;
                        continue;
                    }
                    nodes.AddEntity(nd);
                }
            }
            else
            {
                nodes.AddEntities(nds);
            }



            SubFlows subs = new SubFlows();
            var trackTable = "ND" + int.Parse(gwf.FK_Flow) + "Track";
            sql = "SELECT FID \"FID\",NDFrom \"NDFrom\",NDFromT \"NDFromT\",NDTo  \"NDTo\", NDToT \"NDToT\", ActionType \"ActionType\",ActionTypeText \"ActionTypeText\",Msg \"Msg\",RDT \"RDT\",EmpFrom \"EmpFrom\",EmpFromT \"EmpFromT\", EmpToT \"EmpToT\",EmpTo \"EmpTo\" FROM " + trackTable +
                  " WHERE WorkID=" + gwf.WorkID + " ORDER BY RDT ASC";

            DataTable dt = DBAccess.RunSQLReturnTable(sql);



            int idxNode = 0;
            foreach (Node nd in nodes)
            {
                idxNode++;

                string[] dtArray = GetNodeRealTime(dt, nd.NodeID);
                if (nd.IsStartNode == true)
                    continue;

                //里程碑.
                json += " { id:'" + nd.NodeID + "', name:'" + nd.Name + "', type:0,";
                json += "series:[{ ";
                json += " name: '计划', ";
                json += " start:  " + ToData(gwf.GetParaString("PlantStartDt" + nd.NodeID)) + ", ";
                json += " end: " + ToData(gwf.GetParaString("CH" + nd.NodeID)) + ",";
                json += " TodoSta: " + gwf.TodoSta + ", ";
                json += " color: 'blue' ";
                json += "},";

                json += "{ name: '实际执行', ";
                if (nd.IsStartNode == true)
                {
                    json += " start:  " + ToData(gwf.RDT) + ",";
                    json += " end: " + ToData(dtArray[1]) + ",";
                    json += " TodoSta: " + gwf.TodoSta + ",";
                    json += " color: 'green' ";
                }
                else
                {
                    string start = "";
                    string end = "";
                    bool isPass = false;
                    json += " start:  " + ToData(dtArray[0]) + ",";


                    if (DataType.IsNullOrEmpty(dtArray[1]) == true && DataType.IsNullOrEmpty(dtArray[0]) == true)
                        json += " end:'',";
                    else
                    {
                        if (DataType.IsNullOrEmpty(dtArray[1]) == false)
                            isPass = true;
                        json += " end: " + ToData(dtArray[1]) + ",";
                        end = DataType.ParseSysDate2DateTime(dtArray[1]).ToString("yyyy-MM-dd");
                    }

                    json += " TodoSta: " + gwf.TodoSta + ",";
                    string plantCHDt = ToData(gwf.GetParaString("CH" + nd.NodeID));
                    if (isPass && end.CompareTo(plantCHDt) <= 0)
                        json += " color: 'green' "; //正常

                    if (isPass && end.CompareTo(plantCHDt) > 0)
                        json += " color: 'red' "; //逾期
                    if (isPass == false)
                    {
                        if (DataType.IsNullOrEmpty(end) == true)
                            end = DateTime.Now.ToString("yyyy-MM-dd");
                        if (end.CompareTo(plantCHDt) > 0)
                            json += " color: 'red' ";
                        else
                        {
                            // 预警计算
                            string warningDt = DataType.AddDays(end, 3, TWay.Holiday).ToString("yyyy-MM-dd");
                            if (warningDt.CompareTo(plantCHDt) >= 0)
                                json += " color: 'yellow' ";
                            else
                                json += " color: 'green' ";
                        }

                    }


                }

                json += "}]},";

                //获取子流程
                subs = new SubFlows(nd.NodeID);
                if (subs.Count == 0)
                {
                    continue;
                }

                string series = "";
                foreach (SubFlow sub in subs)
                {
                    if (sub.FK_Node != nd.NodeID)
                        continue;
                    json += " { id:'" + sub.FK_Node + "', name:'" + sub.SubFlowNo + " - " + sub.SubFlowName + "',type:1,series:[ ";
                    //增加子流成.
                    int idx = 0;
                    string dtlsSubFlow = "";
                    //获取流程启动的开始和结束
                    Int64 firstStartWorkID = 0;
                    Int64 endStartWorkID = 0;
                    GenerWorkFlow firstStartGwf = null;
                    GenerWorkFlow endStartGwf = null;
                    bool IsFirst = true;
                    foreach (GenerWorkFlow subGWF in gwfs)
                    {
                        if (subGWF.FK_Flow != sub.SubFlowNo)
                            continue;
                        if (IsFirst == true)
                        {
                            firstStartWorkID = subGWF.WorkID;
                            endStartWorkID = subGWF.WorkID;
                            firstStartGwf = subGWF;
                            endStartGwf = subGWF;
                            continue;

                        }
                        if (firstStartWorkID > subGWF.WorkID)
                        {
                            firstStartWorkID = subGWF.WorkID;
                            firstStartGwf = subGWF;
                        }

                        if (endStartWorkID < subGWF.WorkID)
                        {
                            endStartWorkID = subGWF.WorkID;
                            endStartGwf = subGWF;
                        }

                    }

                    //没有启动子流程
                    if (firstStartWorkID == 0)
                    {
                        json += "{ ";
                        json += " name: '" + sub.SubFlowNo + " - " + sub.SubFlowName + "', ";
                        json += " start:  " + ToData(DataType.CurrentDate) + ", ";
                        json += " end:  " + ToData(DataType.CurrentDate) + ", ";
                        json += " TodoSta: -1, ";
                        json += " color: 'brue' ";
                        json += "}";
                    }
                    //子流程被启动了一次
                    if (firstStartWorkID != 0 && firstStartWorkID == endStartWorkID)
                    {
                        json += "{ ";
                        json += " name: '计划',";
                        json += " start:  " + ToData(firstStartGwf.RDT) + ",";
                        json += " end: " + ToData(firstStartGwf.SDTOfFlow) + ",";
                        json += " TodoSta: -2, ";
                        json += " color: '#9999FF' ";
                        json += "},";

                        json += "{ ";
                        json += " name: '实际',";
                        json += " start:  " + ToData(firstStartGwf.RDT) + ",";
                        if (firstStartGwf.WFState == WFState.Complete)
                            json += " end: " + ToData(firstStartGwf.SendDT) + ",";
                        else
                            json += " end: '" + DateTime.Now.ToString("yyyy-MM-dd") + "',";

                        json += " TodoSta: " + firstStartGwf.TodoSta + ", ";

                        if (firstStartGwf.WFSta == WFSta.Complete)//流程运行结束
                        {
                            if (ToData(firstStartGwf.SendDT).CompareTo(ToData(firstStartGwf.SDTOfFlow)) <= 0)//正常
                                json += " color: '#66ff66' ";  //绿色
                            else
                                json += " color: '#ff8888' ";//红色
                        }
                        else //未完成
                        {
                            string sendDt = DataType.ParseSysDate2DateTime(firstStartGwf.SendDT).ToString("yyyy-MM-dd");
                            string wadingDtOfF = DataType.AddDays(sendDt, 3, TWay.Holiday).ToString("yyyy-MM-dd");
                            if (sendDt.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)//逾期
                                json += " color: '#ff8888' "; //红色
                            else
                            {
                                if (wadingDtOfF.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)
                                    json += " color: '#ffff77' ";//黄色
                                else
                                    json += " color: '#66ff66' ";//绿色
                            }


                        }
                        json += "}";
                    }
                    //子流程被启动了多次
                    if (firstStartWorkID != 0 && firstStartWorkID != endStartWorkID)
                    {
                        json += "{ ";
                        json += " name: '计划',";
                        json += " start:  " + ToData(firstStartGwf.RDT) + ",";
                        json += " end: " + ToData(firstStartGwf.SDTOfFlow) + ",";
                        json += " TodoSta: -2, ";
                        json += " color: '#9999FF' ";
                        json += "},";

                        json += "{ ";
                        json += " name: '实际',";
                        json += " start:  " + ToData(firstStartGwf.RDT) + ",";
                        if (endStartGwf.WFState == WFState.Complete)
                            json += " end: " + ToData(endStartGwf.SendDT) + ",";
                        else
                            json += " end: '" + DateTime.Now.ToString("yyyy-MM-dd") + "',";

                        json += " TodoSta: " + endStartGwf.TodoSta + ", ";

                        if (endStartGwf.WFSta == WFSta.Complete)//流程运行结束
                        {
                            if (ToData(endStartGwf.SendDT).CompareTo(ToData(firstStartGwf.SDTOfFlow)) <= 0)//正常
                                json += " color: '#66ff66' ";  //绿色
                            else
                                json += " color: '#ff8888' ";//红色
                        }
                        else //未完成
                        {
                            string sendDt = DataType.ParseSysDate2DateTime(endStartGwf.SendDT).ToString("yyyy-MM-dd");
                            string wadingDtOfF = DataType.AddDays(sendDt, 3, TWay.Holiday).ToString("yyyy-MM-dd");
                            if (sendDt.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)//逾期
                                json += " color: '#ff8888' "; //红色
                            else
                            {
                                if (wadingDtOfF.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)
                                    json += " color: '#ffff77' ";//黄色
                                else
                                    json += " color: '#66ff66' ";//绿色
                            }


                        }

                        json += "}";
                    }



                    json += "]},";

                    json += GetSubFlowJson(sub, workID);

                }

            }
            json = json.Substring(0, json.Length - 1);
            json += "]";

            return json;
        }


        private static string GetSubFlowJson(SubFlow sub, Int64 workID)
        {
            //获取子流程的子流程
            SubFlows ssubFlows = new SubFlows();
            ssubFlows.Retrieve(SubFlowYanXuAttr.FK_Flow, sub.SubFlowNo);

            //获取流程的信息
            GenerWorkFlows sgwfs = new GenerWorkFlows();
            sgwfs.RetrieveInSQL(GenerWorkFlowAttr.PWorkID, "Select WorkID From WF_GenerWorkFlow Where PWorkID=" + workID + " And FK_Flow='" + sub.SubFlowNo + "'");

            //获取子流程
            if (ssubFlows.Count == 0)
            {
                return "";
            }
            StringBuilder json = new StringBuilder();
            string series = "";
            foreach (SubFlow ssub in ssubFlows)
            {
                if (ssub.FK_Flow != sub.SubFlowNo)
                    continue;
                json.Append(" { id:'" + sub.FK_Node + "', name:'" + ssub.SubFlowNo + " - " + ssub.SubFlowName + "',type:2,series:[ ");
                //增加子流成.
                int idx = 0;
                string dtlsSubFlow = "";
                //获取流程启动的开始和结束
                Int64 firstStartWorkID = 0;
                Int64 endStartWorkID = 0;
                GenerWorkFlow firstStartGwf = null;
                GenerWorkFlow endStartGwf = null;
                bool IsFirst = true;
                foreach (GenerWorkFlow subGWF in sgwfs)
                {
                    if (subGWF.FK_Flow != ssub.SubFlowNo)
                        continue;
                    if (IsFirst == true)
                    {
                        firstStartWorkID = subGWF.WorkID;
                        endStartWorkID = subGWF.WorkID;
                        firstStartGwf = subGWF;
                        endStartGwf = subGWF;
                        continue;

                    }
                    if (firstStartWorkID > subGWF.WorkID)
                    {
                        firstStartWorkID = subGWF.WorkID;
                        firstStartGwf = subGWF;
                    }

                    if (endStartWorkID < subGWF.WorkID)
                    {
                        endStartWorkID = subGWF.WorkID;
                        endStartGwf = subGWF;
                    }

                }

                //没有启动子流程
                if (firstStartWorkID == 0)
                {
                    json.Append("{ ");
                    json.Append(" name: '" + ssub.SubFlowNo + " - " + ssub.SubFlowName + "', ");
                    json.Append(" start:  " + ToData(DataType.CurrentDate) + ", ");
                    json.Append(" end:  " + ToData(DataType.CurrentDate) + ", ");
                    json.Append(" TodoSta: -1, ");
                    json.Append(" color: 'brue' ");
                    json.Append("}");
                }
                //子流程被启动了一次
                if (firstStartWorkID != 0 && firstStartWorkID == endStartWorkID)
                {
                    json.Append("{ ");
                    json.Append(" name: '计划',");
                    json.Append(" start:  " + ToData(firstStartGwf.RDT) + ",");
                    json.Append(" end: " + ToData(firstStartGwf.SDTOfFlow) + ",");
                    json.Append(" TodoSta: -2, ");
                    json.Append(" color: '#9999FF' ");
                    json.Append("},");

                    json.Append("{ ");
                    json.Append(" name: '实际',");
                    json.Append(" start:  " + ToData(firstStartGwf.RDT) + ",");
                    if (firstStartGwf.WFState == WFState.Complete)
                        json.Append(" end: " + ToData(firstStartGwf.SendDT) + ",");
                    else
                        json.Append(" end: '" + DateTime.Now.ToString("yyyy-MM-dd") + "',");

                    json.Append(" TodoSta: " + firstStartGwf.TodoSta + ", ");

                    if (firstStartGwf.WFSta == WFSta.Complete)//流程运行结束
                    {
                        if (ToData(firstStartGwf.SendDT).CompareTo(ToData(firstStartGwf.SDTOfFlow)) <= 0)//正常
                            json.Append(" color: '#66ff66' ");  //绿色
                        else
                            json.Append(" color: '#ff8888' ");//红色
                    }
                    else //未完成
                    {
                        string sendDt = DataType.ParseSysDate2DateTime(firstStartGwf.SendDT).ToString("yyyy-MM-dd");
                        string wadingDtOfF = DataType.AddDays(sendDt, 3, TWay.Holiday).ToString("yyyy-MM-dd");
                        if (sendDt.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)//逾期
                            json.Append(" color: '#ff8888' "); //红色
                        else
                        {
                            if (wadingDtOfF.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)
                                json.Append(" color: '#ffff77' ");//黄色
                            else
                                json.Append(" color: '#66ff66' ");//绿色
                        }


                    }
                    json.Append("}");
                }
                //子流程被启动了多次
                if (firstStartWorkID != 0 && firstStartWorkID != endStartWorkID)
                {
                    json.Append("{ ");
                    json.Append(" name: '计划',");
                    json.Append(" start:  " + ToData(firstStartGwf.RDT) + ",");
                    json.Append(" end: " + ToData(firstStartGwf.SDTOfFlow) + ",");
                    json.Append(" TodoSta: -2, ");
                    json.Append(" color: '#9999FF' ");
                    json.Append("},");

                    json.Append("{ ");
                    json.Append(" name: '实际',");
                    json.Append(" start:  " + ToData(firstStartGwf.RDT) + ",");
                    if (endStartGwf.WFState == WFState.Complete)
                        json.Append(" end: " + ToData(endStartGwf.SendDT) + ",");
                    else
                        json.Append(" end: '" + DateTime.Now.ToString("yyyy-MM-dd") + "',");

                    json.Append(" TodoSta: " + endStartGwf.TodoSta + ", ");

                    if (endStartGwf.WFSta == WFSta.Complete)//流程运行结束
                    {
                        if (ToData(endStartGwf.SendDT).CompareTo(ToData(firstStartGwf.SDTOfFlow)) <= 0)//正常
                            json.Append(" color: '#66ff66' ");  //绿色
                        else
                            json.Append(" color: '#ff8888' ");//红色
                    }
                    else //未完成
                    {
                        string sendDt = DataType.ParseSysDate2DateTime(endStartGwf.SendDT).ToString("yyyy-MM-dd");
                        string wadingDtOfF = DataType.AddDays(sendDt, 3, TWay.Holiday).ToString("yyyy-MM-dd");
                        if (sendDt.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)//逾期
                            json.Append(" color: '#ff8888' "); //红色
                        else
                        {
                            if (wadingDtOfF.CompareTo(ToData(firstStartGwf.SDTOfFlow)) > 0)
                                json.Append(" color: '#ffff77' ");//黄色
                            else
                                json.Append(" color: '#66ff66' ");//绿色
                        }


                    }

                    json.Append("}");
                }



                json.Append("]},");



            }
            return json.ToString();
        }

        public static string[] GetNodeRealTime(DataTable dt, Int32 nodeID)
        {
            string startDt = "";
            string endDt = "";
            foreach (DataRow dr in dt.Rows)
            {
                int NDFrom = Int32.Parse(dr["NDFrom"].ToString());
                if (NDFrom == nodeID)
                {
                    endDt = dr["RDT"].ToString();
                    break;
                }


            }
            foreach (DataRow dr in dt.Rows)
            {
                int NDTo = Int32.Parse(dr["NDTo"].ToString());
                if (NDTo == nodeID)
                {
                    if (DataType.IsNullOrEmpty(endDt) == true)
                    {
                        startDt = dr["RDT"].ToString();
                        break;
                    }
                    if (dr["RDT"].ToString().CompareTo(endDt) <= 0)
                    {
                        startDt = dr["RDT"].ToString();
                        break;
                    }
                }


            }
            string[] dtArray = { startDt, endDt };
            return dtArray;
        }

        /// <summary>
        /// 生成甘特图
        /// </summary>
        /// <param name="workID"></param>
        /// <returns></returns>
        public static string GenerGanttDataOfSubFlowsV20(Int64 workID)
        {
            GenerWorkFlow gwf = new GenerWorkFlow(workID);
            //增加子流程数据.
            GenerWorkFlows gwfs = new GenerWorkFlows();
            gwfs.Retrieve("PWorkID", workID);

            string json = "[";

            json += " { id:'" + gwf.FK_Flow + "', name:'" + gwf.FlowName + "',";

            json += " series:[";
            json += "{ name: \"计划时间\", start:  " + ToData(gwf.SDTOfFlow) + ", end: " + ToData(gwf.SDTOfFlow) + ", color: \"#f0f0f0\" },";
            json += "{ name: \"实际工作时间\", start: " + ToData(gwf.RDT) + ", end: " + ToData(gwf.SendDT) + " , color: \"#f0f0f0\" }";
            json += "]";

            if (gwfs.Count == 0)
            {
                json += "}";
                json += "]";
                return json;
            }
            else
            {
                json += "},";
            }

            //增加子流成.
            int idx = 0;
            foreach (GenerWorkFlow subGWF in gwfs)
            {
                idx++;

                json += " { id:'" + subGWF.FK_Flow + "', name:'" + subGWF.FlowName + "',";

                json += " series:[";
                json += "{ name: \"实际工作时间\", start:  " + ToData(gwf.RDT) + ", end: " + ToData(gwf.SendDT) + " }";
                json += "]";

                if (idx == gwfs.Count)
                {
                    json += "}";
                    json += "]";
                    return json;
                }
                else
                {
                    json += "},";
                }
            }

            json += "]";

            return json;
        }

        public static string ToData(string dtStr)
        {

            DateTime dt = DataType.ParseSysDate2DateTime(dtStr);

            return "'" + dt.ToString("yyyy-MM-dd") + "'";

        }
        /// <summary>
        /// 生成甘特图
        /// </summary>
        /// <returns></returns>
        public static string GenerGanttDataOfSubFlowsV1(Int64 workID)
        {
            //定义解构.
            DataTable dtFlows = new DataTable();
            dtFlows.Columns.Add("id");
            dtFlows.Columns.Add("name");

            DataTable dtSeries = new DataTable();
            dtSeries.TableName = "series";
            dtSeries.Columns.Add("name");
            dtSeries.Columns.Add("start");
            dtSeries.Columns.Add("end");
            dtSeries.Columns.Add("color");
            dtSeries.Columns.Add("RefPK");

            //增加主流程数据.
            GenerWorkFlow gwf = new GenerWorkFlow(workID);
            DataRow dr = dtFlows.NewRow();
            dr["id"] = gwf.FK_Flow;
            dr["name"] = gwf.FlowName;
            dtFlows.Rows.Add(dr);

            DataRow drItem = dtSeries.NewRow();
            drItem["name"] = "项目计划日期";
            drItem["start"] = gwf.RDT;
            drItem["end"] = gwf.SDTOfFlow;
            drItem["color"] = "#f0f0f0";
            drItem["RefPK"] = gwf.FK_Flow;
            dtSeries.Rows.Add(drItem);

            drItem = dtSeries.NewRow();
            drItem["name"] = "项目启动日期";
            drItem["start"] = gwf.RDT;
            drItem["end"] = gwf.SDTOfFlow;
            drItem["color"] = "#f0f0f0";
            drItem["RefPK"] = gwf.FK_Flow;
            dtSeries.Rows.Add(drItem);


            //增加子流程数据.
            GenerWorkFlows gwfs = new GenerWorkFlows();
            gwfs.Retrieve("PWorkID", workID);

            foreach (GenerWorkFlow subFlow in gwfs)
            {
                dr = dtFlows.NewRow();
                dr["id"] = subFlow.FK_Flow;
                dr["name"] = subFlow.FlowName;
                dtFlows.Rows.Add(dr);


                drItem = dtSeries.NewRow();
                drItem["name"] = "启动日期";
                drItem["start"] = subFlow.RDT;
                drItem["end"] = subFlow.SDTOfFlow;
                drItem["color"] = "#f0f0f0";
                drItem["RefPK"] = subFlow.FK_Flow;
                dtSeries.Rows.Add(drItem);
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dtFlows);
            ds.Tables.Add(dtSeries);

            return ToJsonOfGantt(ds);
        }
        public static string ToJsonOfGantt(DataSet ds)
        {
            string json = "[";

            DataTable dtFlows = ds.Tables[0];
            DataTable dtSeries = ds.Tables[1];

            json += "]";

            return "";

        }
        #endregion 高级配置.

        #region 多语言处理.
        private static Hashtable _Multilingual_Cache = null;
        public static DataTable getMultilingual_DT(string className)
        {
            if (_Multilingual_Cache == null)
                _Multilingual_Cache = new Hashtable();

            if (_Multilingual_Cache.ContainsKey(className) == false)
            {
                DataSet ds = DataType.CXmlFileToDataSet(BP.Difference.SystemConfig.PathOfData + "lang/xml/" + className + ".xml");
                DataTable dt = ds.Tables[0];

                _Multilingual_Cache.Add(className, dt);
            }

            return _Multilingual_Cache[className] as DataTable;
        }
        /// <summary>
        /// 转换语言.
        /// </summary>
        public static string multilingual(string defaultMsg, string className, string key, string p0 = null, string p1 = null, string p2 = null, string p3 = null)
        {
            int num = 4;
            string[] paras = new string[num];
            if (p0 != null)
                paras[0] = p0;

            if (p1 != null)
                paras[1] = p1;

            if (p2 != null)
                paras[2] = p2;

            if (p3 != null)
                paras[3] = p3;

            return multilingual(defaultMsg, className, key, paras);
        }
        /// <summary>
        /// 获取多语言
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="key"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string multilingual(string defaultMsg, string className, string key, string[] paramList)
        {
            if (BP.Web.WebUser.SysLang.Equals("zh-cn") || BP.Web.WebUser.SysLang.Equals("CH"))
                return String.Format(defaultMsg, paramList);

            DataTable dt = getMultilingual_DT(className);

            string val = "";
            foreach (DataRow dr in dt.Rows)
            {
                if ((string)dr.ItemArray[0] == key)
                {
                    switch (BP.Web.WebUser.SysLang)
                    {
                        case "zh-cn":
                            val = (string)dr.ItemArray[1];
                            break;
                        case "zh-tw":
                            val = (string)dr.ItemArray[2];
                            break;
                        case "zh-hk":
                            val = (string)dr.ItemArray[3];
                            break;
                        case "en-us":
                            val = (string)dr.ItemArray[4];
                            break;
                        case "ja-jp":
                            val = (string)dr.ItemArray[5];
                            break;
                        case "ko-kr":
                            val = (string)dr.ItemArray[6];
                            break;
                        default:
                            val = (string)dr.ItemArray[1];
                            break;
                    }
                    break;
                }
            }
            return String.Format(val, paramList);
        }


        //public static void Multilingual_Demo()
        //{
        //    //普通的多语言处理.
        //    string msg = "您确定要删除吗？";
        //    msg = BP.WF.Glo.Multilingual_Public(msg, "confirm");


        //    //带有参数的语言处理..
        //    msg = "您确定要删除吗？删除{0}后，就不能恢复。";
        //    msg = BP.WF.Glo.Multilingual_Public(msg, "confirmDel", "zhangsan");

        //    //   BP.WF.Glo.Multilingual_Public("confirm",
        //}


        #endregion 多语言处理.

        #region 公共属性.
        /// <summary>
        /// 打印文件
        /// </summary>
        public static string PrintBackgroundWord
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["PrintBackgroundWord"];
                if (string.IsNullOrEmpty(s))
                    s = "驰骋工作流引擎@开源驰骋 - ccflow@openc";
                return s;
            }
        }
        /// <summary>
        /// 运行平台.
        /// </summary>
        public static Platform Platform
        {
            get
            {
                return Platform.CCFlow;
            }
        }

        /// <summary>
        /// 短消息写入类型
        /// </summary>
        public static ShortMessageWriteTo ShortMessageWriteTo
        {
            get
            {
                return (ShortMessageWriteTo)SystemConfig.GetValByKeyInt("ShortMessageWriteTo", 0);
            }
        }
        /// <summary>
        /// 当前选择的流程.
        /// </summary>
        public static string CurrFlow
        {
            get
            {
                return HttpContextHelper.SessionGet("CurrFlow") as string;
            }
            set
            {
                HttpContextHelper.SessionSet("CurrFlow", value);
            }
        }
        /// <summary>
        /// 用户编号.
        /// </summary>
        public static string UserNo = null;
        /// <summary>
        /// 运行平台(用于处理不同的平台，调用不同的URL)
        /// </summary>
        public static Plant Plant = WF.Plant.CCFlow;
        #endregion 公共属性.

        /// <summary>
        /// 升级满足云需要.
        /// </summary>
        public void UpdateCloud()
        {
            if (1 == 1)
                return;

            #region 为了适应云服务的要求.
            if (DBAccess.IsExitsTableCol(BP.Sys.Base.Glo.SysEnum(), "OrgNo") == false)
            {
                //检查数据表.
                SysEnum se = new SysEnum();
                se.CheckPhysicsTable();

                //更新值.
                DBAccess.RunSQL("UPDATE " + BP.Sys.Base.Glo.SysEnum() + " SET OrgNo='CCS'");

                //更新.
                DBAccess.RunSQL("UPDATE " + BP.Sys.Base.Glo.SysEnum() + " SET MyPK= EnumKey+'_'+Lang+'_'+IntKey+'_'+OrgNo");
            }
            if (DBAccess.IsExitsTableCol("Sys_EnumMain", "OrgNo") == false)
            {
                //检查数据表.
                SysEnumMain se = new SysEnumMain();
                se.CheckPhysicsTable();

                //更新值.
                DBAccess.RunSQL("UPDATE Sys_EnumMain SET OrgNo='CCS'");

                //更新.
                DBAccess.RunSQL("UPDATE Sys_EnumMain SET No= No+'_'+OrgNo ");
            }

            if (DBAccess.IsExitsTableCol("Sys_SFTable", "OrgNo") == false)
            {
                //检查数据表.
                BP.Sys.FrmUI.SFTable se = new BP.Sys.FrmUI.SFTable();
                se.CheckPhysicsTable();

                //更新值.
                DBAccess.RunSQL("UPDATE Sys_SFTable SET OrgNo='CCS',EnumKey=No");

                //更新.
                DBAccess.RunSQL("UPDATE Sys_SFTable SET No= No+'_'+OrgNo ");
            }

            #endregion 为了适应云服务的要求.
        }
        /// <summary>
        /// 检查GPM菜单.
        /// </summary>
        public static void CheckGPM()
        {
            ArrayList al = null;
            string info = "BP.En.Entity";
            al = BP.En.ClassFactory.GetObjects(info);

            #region 1, 修复表
            foreach (Object obj in al)
            {
                Entity en = null;
                en = obj as Entity;
                if (en == null)
                    continue;

                if (en.ToString() == null)
                    continue;

                if (en.ToString().Contains("BP.Port."))
                    continue;

                if (en.ToString().Contains("BP.GPM.") == false)
                    continue;

                //if (en.ToString().Contains("BP.GPM."))
                //    continue;
                //if (en.ToString().Contains("BP.Demo."))
                //    continue;

                string table = null;
                try
                {
                    table = en.EnMap.PhysicsTable;
                    if (table == null)
                        continue;
                    if (en.EnMap.PhysicsTable.ToLower().Contains("demo_") == true)
                        continue;
                }
                catch
                {
                    continue;
                }

                switch (table)
                {
                    case "WF_EmpWorks":
                    case "WF_GenerEmpWorkDtls":
                    case "WF_GenerEmpWorks":
                    case "V_GPM_EmpMenu":
                        continue;
                    case "Sys_Enum":
                        en.CheckPhysicsTable();
                        break;
                    default:
                        en.CheckPhysicsTable();
                        break;
                }

                en.PKVal = "123";
                try
                {
                    en.RetrieveFromDBSources();
                }
                catch (Exception ex)
                {
                    BP.DA.Log.DebugWriteError(ex.Message);
                    en.CheckPhysicsTable();
                }
            }
            #endregion 修复

            //删除视图.
            if (DBAccess.IsExitsObject("V_GPM_EmpMenu") == true)
                DBAccess.RunSQL("DROP VIEW V_GPM_EmpMenu");

            if (DBAccess.IsExitsObject("V_GPM_EmpGroupMenu") == true)
                DBAccess.RunSQL("DROP VIEW V_GPM_EmpGroupMenu");

            if (DBAccess.IsExitsObject("V_GPM_EmpGroup") == true)
                DBAccess.RunSQL("DROP VIEW V_GPM_EmpGroup");

            if (DBAccess.IsExitsObject("V_GPM_EmpStationMenu") == true)
                DBAccess.RunSQL("DROP VIEW V_GPM_EmpStationMenu");

            #region 6, 创建视图。
            string sqlscript = "";
            //MSSQL_GPM_VIEW 语法有所区别
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MSSQL)
                sqlscript = BP.Difference.SystemConfig.PathOfWebApp + "GPM/SQLScript/MSSQL_GPM_VIEW.sql";

            //MySQL 语法有所区别
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL)
                sqlscript = BP.Difference.SystemConfig.PathOfWebApp + "GPM/SQLScript/MySQL_GPM_VIEW.sql";

            //Oracle 语法有所区别
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.Oracle)
                sqlscript = BP.Difference.SystemConfig.PathOfWebApp + "GPM/SQLScript/Oracle_GPM_VIEW.sql";
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL || BP.Difference.SystemConfig.AppCenterDBType == DBType.UX)
                sqlscript = BP.Difference.SystemConfig.PathOfWebApp + "GPM/SQLScript/PostgreSQL_GPM_VIEW.sql";

            if (DataType.IsNullOrEmpty(sqlscript) == true)
                throw new Exception("err@没有判断的数据库类型:" + BP.Difference.SystemConfig.AppCenterDBType.ToString());

            DBAccess.RunSQLScriptGo(sqlscript);
            #endregion 创建视图

        }

        /// <summary>
        /// 检查流程
        /// </summary>
        /// <returns></returns>
        public static string CheckFlows()
        {
            string err = "";
            string sql = "";
            sql += "SELECT a.FK_Attr, a.AttrKey, a.AttrName, a.FK_Flow, B.Name as FlowName, b.OrgNo as OrgNo";
            sql += " FROM wf_cond a,wf_flow b WHERE a.FK_Flow=b.no  ";
            sql += " AND  FK_Attr NOT IN (SELECT MYPK FROM sys_mapattr ) AND LENGTH(FK_Attr) >0";

            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            if (dt.Rows.Count >= 1)
                err += "err@在配置如下流程的条件的时候，字段被删除，会导致流程运行错误:" + BP.Tools.Json.ToJson(dt);

            return err;
        }
        /// <summary>
        /// 升级pop窗体格式.
        /// </summary>
        /// <returns></returns>
        public static string UpdataTSModel()
        {

            #region 升级数据源
            SFDBSrcs ensSF = new SFDBSrcs();
            ensSF.RetrieveAll();
            if (ensSF.Count <= 1)
            {
                DBAccess.DropTableColumn("Sys_DBSrc", "DBSrcType");
                ensSF.GetNewEntity.CheckPhysicsTable();
                foreach (SFDBSrc item in ensSF)
                    item.DirectDelete();

                SFDBSrc src = new SFDBSrc();
                src.No = "local";
                src.Name = "本机数据库";
                src.DBSrcType = "local";
                src.SetPara("EditType", "1"); //只读不可删除.
                src.Insert();

                src = new SFDBSrc();
                src.No = "CCFromRef";
                src.Name = "本机JavaScript数据源";
                src.DBSrcType = "CCFromRef";
                src.SetPara("EditType", "1"); //只读不可删除.
                src.Insert();

                src = new SFDBSrc();
                src.No = "LocalHandler";
                src.Name = "内置Handler";
                src.DBSrcType = "LocalHandler";
                src.SetPara("EditType", "1"); //只读不可删除.
                src.Insert();

                src = new SFDBSrc();
                src.No = "LocalWS";
                src.Name = "内置WebServies";
                src.DBSrcType = "LocalWS";
                src.SetPara("EditType", "1"); //只读不可删除.
                src.Insert();
            }
            #endregion 升级.


            #region 枚举值 forTS
            SysEnumMains enumMains = new SysEnumMains();
            enumMains.RetrieveAll();
            foreach (SysEnumMain enumMain in enumMains)
            {
                SysEnums ens = new SysEnums();
                ens.Delete(SysEnumAttr.EnumKey, enumMain.No);

                //保存数据
                string cfgVal = enumMain.CfgVal;
                AtPara atPara = enumMain.atPara;
                string enName = atPara.GetValStrByKey("EnName");
                string[] strs = cfgVal.Split('@');
                foreach (string s in strs)
                {
                    if (DataType.IsNullOrEmpty(s) == true)
                        continue;

                    string[] vk = s.Split('=');
                    SysEnum se = new SysEnum();
                    se.EnumKey = enumMain.No;
                    se.Lang = BP.Web.WebUser.SysLang;
                    //根据EnName判断是Int枚举还是String枚举
                    if (enName.Equals("TS.Sys.SysEnumMainInt"))
                    {
                        se.IntKey = int.Parse(vk[0]);
                        if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.SAAS)
                            se.setMyPK(se.EnumKey + "_" + se.Lang + "_" + se.IntKey + "_" + BP.Web.WebUser.OrgNo); //关联的主键.

                        if (BP.Difference.SystemConfig.CCBPMRunModel != CCBPMRunModel.SAAS)
                            se.setMyPK(se.EnumKey + "_" + se.Lang + "_" + se.IntKey);
                    }

                    if (enName.Equals("TS.Sys.SysEnumMainString"))
                    {
                        se.StrKey = vk[0].ToString();
                        if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.SAAS)
                            se.setMyPK(se.EnumKey + "_" + se.Lang + "_" + se.StrKey + "_" + BP.Web.WebUser.OrgNo); //关联的主键.

                        if (BP.Difference.SystemConfig.CCBPMRunModel != CCBPMRunModel.SAAS)
                            se.setMyPK(se.EnumKey + "_" + se.Lang + "_" + se.StrKey);
                    }

                    string[] kvsValues = new string[vk.Length - 1];
                    for (int i = 0; i < kvsValues.Length; i++)
                    {
                        kvsValues[i] = vk[i + 1];
                    }

                    se.Lab = string.Join("=", kvsValues);
                    //如果存在就更新
                    if (se.IsExit("MyPK", se.MyPK))
                        se.DirectUpdate();
                    else
                        se.DirectInsert();
                }
            }
            #endregion

            #region 节点属性pop窗体.
            if (DBAccess.IsExitsTableCol("WF_Node", "NodeStations") == true)
            {
                //删除可能存在的垃圾数据.
                DBAccess.RunSQL("DELETE  FROM WF_NodeStation WHERE FK_Node NOT IN (SELECT NodeID FROM WF_Node)");
                DBAccess.RunSQL("DELETE  FROM WF_NodeStation WHERE FK_Station NOT IN (SELECT No FROM Port_Station)");

                //求节点集合.
                DataTable dt = DBAccess.RunSQLReturnTable("SELECT DISTINCT FK_Node FROM WF_NodeStation");
                foreach (DataRow dr in dt.Rows)
                {
                    string nodeID = dr[0].ToString();
                    string sql = "SELECT No,Name FROM Port_Station WHERE NO IN (SELECT FK_Station FROM WF_NodeStation WHERE FK_Node='" + nodeID + "')";
                    DataTable dt1 = DBAccess.RunSQLReturnTable(sql);
                    string ids = "";
                    string names = "";
                    foreach (DataRow item in dt1.Rows)
                    {
                        ids += item[0].ToString() + ",";
                        names += item[1].ToString() + ",";
                    }
                    sql = "UPDATE WF_Node SET NodeStations='" + ids + "',NodeStationsT='" + names + "' WHERE NodeID=" + nodeID;
                    DBAccess.RunSQL(sql);
                }
            }
            if (DBAccess.IsExitsTableCol("WF_Node", "NodeEmps") == true)
            {
                //删除可能存在的垃圾数据.
                DBAccess.RunSQL("DELETE  FROM WF_NodeEmp WHERE FK_Node NOT IN (SELECT NodeID FROM WF_Node)");
                DBAccess.RunSQL("DELETE  FROM WF_NodeEmp WHERE FK_Emp NOT IN (SELECT No FROM Port_Emp)");

                //求节点集合.
                DataTable dt = DBAccess.RunSQLReturnTable("SELECT DISTINCT FK_Node FROM WF_NodeEmp");
                foreach (DataRow dr in dt.Rows)
                {
                    string nodeID = dr[0].ToString();
                    string sql = "SELECT No,Name FROM Port_Emp WHERE NO IN (SELECT FK_Emp FROM WF_NodeEmp WHERE FK_Node='" + nodeID + "')";
                    DataTable dt1 = DBAccess.RunSQLReturnTable(sql);
                    string ids = "";
                    string names = "";
                    foreach (DataRow item in dt1.Rows)
                    {
                        ids += item[0].ToString() + ",";
                        names += item[1].ToString() + ",";
                    }
                    sql = "UPDATE WF_Node SET NodeEmps='" + ids + "',NodeEmpsT='" + names + "' WHERE NodeID=" + nodeID;
                    DBAccess.RunSQL(sql);
                }
            }
            if (DBAccess.IsExitsTableCol("WF_Node", "NodeDepts") == true)
            {
                //删除可能存在的垃圾数据.
                DBAccess.RunSQL("DELETE  FROM WF_NodeDept WHERE FK_Node NOT IN (SELECT NodeID FROM WF_Node)");
                DBAccess.RunSQL("DELETE  FROM WF_NodeDept WHERE FK_Dept NOT IN (SELECT No FROM Port_Dept)");

                //求节点集合.
                DataTable dt = DBAccess.RunSQLReturnTable("SELECT DISTINCT FK_Node FROM WF_NodeDept");
                foreach (DataRow dr in dt.Rows)
                {
                    string nodeID = dr[0].ToString();
                    string sql = "SELECT No,Name FROM Port_Dept WHERE NO IN (SELECT FK_Dept FROM WF_NodeDept WHERE FK_Node='" + nodeID + "')";
                    DataTable dt1 = DBAccess.RunSQLReturnTable(sql);
                    string ids = "";
                    string names = "";
                    foreach (DataRow item in dt1.Rows)
                    {
                        ids += item[0].ToString() + ",";
                        names += item[1].ToString() + ",";
                    }
                    sql = "UPDATE WF_Node SET NodeDepts='" + ids + "',NodeDeptsT='" + names + "' WHERE NodeID=" + nodeID;
                    DBAccess.RunSQL(sql);
                }
            }
            #endregion 节点属性pop窗体.

            #region 升级枚举编辑格式.. Sys_Enums
            if (DBAccess.IsExitsTableCol("Sys_EnumMain", "Val0") == false)
            {
                //补全SysEnumMain的字段（Idx0-29 , Val0-29）
                BP.Sys.SysEnumMain ses = new SysEnumMain();
                ses.CheckPhysicsTable();

                //删除可能存在的垃圾数据.
                DBAccess.RunSQL("DELETE  FROM Sys_EnumMain WHERE No NOT IN (SELECT EnumKey FROM Sys_Enum)");

                //求EnumKey集合.
                SysEnumMains sysEnumMains = new SysEnumMains();
                sysEnumMains.RetrieveAll();

                foreach (SysEnumMain sem in sysEnumMains)
                {
                    string cfgVal = sem.CfgVal;
                    AtPara atPara = sem.atPara;
                    string enName = atPara.GetValStrByKey("EnName");
                    //如果是Int型枚举就批量增加Idx0-29
                    if (enName == "TS.Sys.SysEnumMainInt")
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            sem.SetValByKey("Idx" + i, i);
                        }
                        //更新数据
                        sem.DirectUpdate();
                    }
                    //判断格式是否有@
                    if (cfgVal.IndexOf("@") == -1)
                    {
                        throw new Exception("err@SysEnumMain的CfgVal格式不正确,请检查.");
                    }

                    string[] strs = cfgVal.Split('@');
                    int idx = -1;
                    foreach (string str in strs)
                    {
                        if (DataType.IsNullOrEmpty(str))
                            continue;

                        //判断格式是否有=
                        if (str.IndexOf("=") == -1)
                            throw new Exception("err@SysEnumMain的CfgVal格式不正确,请检查.");

                        string[] strVal = str.Split('=');
                        idx++;
                        sem.SetValByKey("Idx" + idx, strVal[0]);
                        sem.SetValByKey("Val" + idx, strVal[1]);
                    }
                    //更新数据
                    sem.DirectUpdate();
                }
            }
            #endregion 升级枚举编辑格式.

            #region 升级外键格式.. Sys_SFTablDtls 不升级也可以用的少.
            if (DBAccess.IsExitsTableCol("Sys_SFTabl", "Name0") == false)
            {
                //补全Sys_SFTabl的字段（BH0-49 , Name0-49）
                BP.Sys.SFTable sft = new BP.Sys.SFTable();
                sft.CheckPhysicsTable();

                //删除可能存在的垃圾数据.
                DBAccess.RunSQL("DELETE  FROM Sys_SFTable WHERE No NOT IN (SELECT FK_SFTable FROM Sys_SFTableDtl)");

                //求SFTableDtls集合.
                DataTable dt = DBAccess.RunSQLReturnTable("SELECT DISTINCT FK_SFTable FROM Sys_SFTableDtl");

                foreach (DataRow dr in dt.Rows)
                {
                    string fk_SFTable = dr[0].ToString();
                    string sql = "SELECT BH,Name,Idx FROM Sys_SFTableDtl WHERE FK_SFTable='" + fk_SFTable + "'";
                    DataTable dt1 = DBAccess.RunSQLReturnTable(sql);
                    int i = -1;
                    foreach (DataRow item in dt1.Rows)
                    {
                        i++;
                        sql = "UPDATE Sys_SFTable SET BH" + i + "='" + item[0].ToString() + "',Name" + i + "='" + item[1].ToString() + "' WHERE No='" + fk_SFTable + "'";
                        DBAccess.RunSQL(sql);
                    }
                }
            }
            #endregion 升级外键格式.

            return "升级成功.";
        }
        #region 执行安装/升级.
        /// <summary>
        /// 当前版本号-为了升级使用.
        /// </summary>
        public static int Ver = 20230221;
        /// <summary>
        /// 执行升级
        /// </summary>
        /// <returns></returns>
        public static string UpdataCCFlowVer()
        {
            if (Glo.CCBPMRunModel == CCBPMRunModel.SAAS)
                return "info@SAAS模式需要手工升级.";

            //string checkErr = CheckFlows();
            //if (DataType.IsNullOrEmpty(checkErr) == false)
            //    return checkErr;

            #region 检查是否需要升级，并更新升级的业务逻辑.
            string updataNote = "";
            /*
             * 升级版本记录:
             * 20150330: 优化发起列表的效率, by:zhoupeng.
             * 2, 升级表单树,支持动态表单树.
             * 1, 执行一次Sender发送人的升级，原来由GenerWorkerList 转入WF_GenerWorkFlow.
             * 0, 静默升级启用日期.2014-12
             */

            if (DBAccess.IsExitsObject("Sys_Serial") == false)
                return "";

            //升级SQL
            UpdataCCFlowVerSQLScript();

            //判断数据库的版本.
            string sql = "SELECT IntVal FROM Sys_Serial WHERE CfgKey='Ver'";
            int currDBVer = DBAccess.RunSQLReturnValInt(sql, 0);
            if (currDBVer != null && currDBVer != 0 && currDBVer >= Ver)
                return null; //不需要升级.
            #region 升级流程模式的存储方式
            if(DBAccess.IsExitsTableCol("WF_Flow", "FlowDevModel") == false)
            {
                switch (SystemConfig.AppCenterDBType)
                {
                    case DBType.MSSQL:
                        DBAccess.RunSQL("ALTER TABLE WF_Flow ADD FlowDevModel INT NULL");
                        break;
                    case DBType.Oracle:
                    case DBType.Informix:
                    case DBType.PostgreSQL:
                    case DBType.UX:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        DBAccess.RunSQL("ALTER TABLE WF_Flow ADD FlowDevModel INTEGER NULL");
                        break;
                    case DBType.MySQL:
                        DBAccess.RunSQL("ALTER TABLE WF_Flow ADD FlowDevModel INT NULL");
                        break;
                    default:
                        break;
                }
            }
            Flows flows = new Flows();
            QueryObject qo = new QueryObject(flows);
            qo.AddWhere("AtPara", "Like", "%FlowDevModel=%");
            qo.DoQuery();
            foreach(Flow flow in flows)
            {
                int flowDevModel = flow.GetParaInt("FlowDevModel");
                flow.FlowDevModel = (FlowDevModel)flowDevModel;
                string atPara = flow.GetValStringByKey("AtPara");
                atPara = atPara.Replace("@FlowDevModel=" + flowDevModel, "");
                flow.SetValByKey("AtPara", atPara);
                flow.DirectUpdate();
            }
            #endregion 升级流程模式的存储方式
            #region 升级文本框字段类型.  TextModel=0普通文本，1密码，2=TextArea,3=富文本.
            //说明没有升级. TextModel=0
            if (DBAccess.IsExitsTableCol("Sys_MapAttr", "IsRichText") == true)
            {
                MapAttr ma = new MapAttr();
                ma.CheckPhysicsTable();

                sql = "UPDATE Sys_MapAttr SET TextModel=3 WHERE IsRichText=1 OR AtPara LIKE '%IsRichText=1%'";
                DBAccess.RunSQL(sql);

                sql = "UPDATE Sys_MapAttr SET TextModel=2 WHERE UIHeight >=40 OR IsSupperText=1";
                DBAccess.RunSQL(sql);

                sql = "UPDATE Sys_MapAttr SET TextModel=1 WHERE IsSecret=1";
                DBAccess.RunSQL(sql);

                DBAccess.DropTableColumn("Sys_MapAttr", "IsRichText");
                DBAccess.DropTableColumn("Sys_MapAttr", "IsSecret");
            }
            #endregion 升级文本框字段类型

            #region 统一升级主键. 给多对多的实体增加主键.
            if (DBAccess.IsExitsTableCol("WF_NodeStation", "MyPK") == false)
            {
                //1.首先要删除主键.
                DBAccess.DropTablePK("WF_NodeStation");
                if (BP.Difference.SystemConfig.AppCenterDBType.ToString() == "MySQL")
                {
                    DBAccess.RunSQL("ALTER TABLE WF_NodeStation ADD COLUMN MyPK VARCHAR(100)");
                    DBAccess.RunSQL("UPDATE WF_NodeStation SET MyPK= CONCAT(FK_Node,'_',FK_Station)");
                }
                else
                    if (BP.Difference.SystemConfig.AppCenterDBType.ToString() == "MSSQL")
                {
                    DBAccess.RunSQL("ALTER TABLE WF_NodeStation ADD  MyPK VARCHAR(100)");
                    DBAccess.RunSQL("UPDATE WF_NodeStation SET MyPK= CONVERT(varchar,FK_Node)+'_'+FK_Station");
                }

                //2. 自动创建.
                NodeStation ns = new NodeStation();
                ns.CheckPhysicsTable();
            }

            if (DBAccess.IsExitsObject("WF_NodeTeam") == false)
            {
                NodeTeam nt = new NodeTeam();
                nt.CheckPhysicsTable();
            }

            if (DBAccess.IsExitsTableCol("WF_NodeTeam", "MyPK") == false)
            {
                DBAccess.DropTablePK("WF_NodeTeam");
                if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL)
                {
                    DBAccess.RunSQL("ALTER TABLE WF_NodeTeam ADD COLUMN MyPK VARCHAR(100)");
                    DBAccess.RunSQL("UPDATE WF_NodeTeam SET MyPK= CONCAT(FK_Node,'_',FK_Team)");
                }
                else if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MSSQL)
                {
                    DBAccess.RunSQL("ALTER TABLE WF_NodeTeam ADD  MyPK VARCHAR(100)");
                    DBAccess.RunSQL("UPDATE WF_NodeTeam SET MyPK= CONVERT(varchar,FK_Node)+'_'+FK_Team");
                }
                //3. 执行更新.
            }

            if (DBAccess.IsExitsTableCol("WF_NodeEmp", "MyPK") == false)
            {

                DBAccess.DropTablePK("WF_NodeEmp");
                if (BP.Difference.SystemConfig.AppCenterDBType.ToString() == "MySQL")
                {
                    DBAccess.RunSQL("ALTER TABLE WF_NodeEmp ADD COLUMN MyPK VARCHAR(100)");
                    DBAccess.RunSQL("UPDATE WF_NodeEmp SET MyPK= CONCAT(FK_Node,'_',FK_Emp)");
                }
                else
           if (BP.Difference.SystemConfig.AppCenterDBType.ToString() == "MSSQL")
                {

                    DBAccess.RunSQL("ALTER TABLE WF_NodeEmp ADD  MyPK VARCHAR(100)");
                    DBAccess.RunSQL("UPDATE WF_NodeEmp SET MyPK= CONVERT(varchar,FK_Node)+'_'+FK_Emp");
                }
                NodeEmp ne1 = new NodeEmp();
                ne1.CheckPhysicsTable();
                //3. 执行更新.

            }
            #endregion 统一升级主键.


            //升级支持ts.
            // UpdataTSModel();

            //升级日志.
            UserLogLevel ul = new UserLogLevel();
            ul.CheckPhysicsTable();
            UserLogType ut = new UserLogType();
            ut.CheckPhysicsTable();

            //添加IsEnable
            BP.WF.Template.FlowTab fb = new BP.WF.Template.FlowTab();
            fb.CheckPhysicsTable();

            if (DBAccess.IsExitsTableCol("Sys_GroupField", "EnName") == true)
            {
                GroupField groupField = new GroupField();
                groupField.CheckPhysicsTable();
                DBAccess.RunSQL("UPDATE Sys_GroupField SET FrmID=enName WHERE FrmID IS null");
            }

            //升级.
            Auth ath = new Auth();
            ath.CheckPhysicsTable();

            //检查BPM.现在暂时不使用原菜单结构
            // if (!SystemConfig.OrganizationIsView)
            //    CheckGPM();

            MapData mapData = new MapData();
            mapData.CheckPhysicsTable();

            Direction dir = new Direction();
            dir.CheckPhysicsTable();

            #region 升级优化集团版的应用. 2020.04.03

            //--2020.05.28 升级方向条件;
            BP.WF.Template.Cond cond = new Cond();
            cond.CheckPhysicsTable();
            if (DBAccess.IsExitsTableCol("WF_Cond", "PRI") == true)
            {
                DBAccess.RunSQL("UPDATE WF_Cond SET Idx=PRI ");
                DBAccess.DropTableColumn("WF_Cond", "PRI");
            }

            //修改节点类型,合并属性.
            /*if (DBAccess.IsExitsTableCol("WF_Node", "SubThreadType") == true)
            {
                DBAccess.RunSQLReturnTable("UPDATE WF_Node SET RunModel=5 WHERE SubThreadType=1 ");
                DBAccess.DropTableColumn("WF_Node", "SubThreadType");
            }*/


            //--2020.05.25 修改节点自定义按钮功能;
            BP.WF.Template.NodeToolbar bar = new NodeToolbar();
            bar.CheckPhysicsTable();
            if (DBAccess.IsExitsTableCol("WF_NodeToolbar", "ShowWhere") == true)
            {
                DBAccess.RunSQL("UPDATE WF_NodeToolbar SET IsMyFlow = 1 Where ShowWhere = 1");
                DBAccess.RunSQL("UPDATE WF_NodeToolbar SET IsMyCC = 1 Where ShowWhere = 2");

                DBAccess.DropTableColumn("WF_NodeToolbar", "ShowWhere");
            }

            MapAttr myattr = new MapAttr();
            myattr.CheckPhysicsTable();

            MapDtlExt mde = new MapDtlExt();
            mde.CheckPhysicsTable();

            NodeExt ne = new NodeExt();
            ne.CheckPhysicsTable();

            FlowExt fe = new FlowExt();
            fe.CheckPhysicsTable();

            //检查frmTrack.
            BP.CCBill.Track tk = new BP.CCBill.Track();
            tk.CheckPhysicsTable();

            BP.Port.DeptEmpStation des = new BP.Port.DeptEmpStation();
            des.CheckPhysicsTable();

            BP.Port.DeptEmp de = new BP.Port.DeptEmp();
            de.CheckPhysicsTable();

            BP.Port.Emp emp1 = new BP.Port.Emp();
            emp1.CheckPhysicsTable();

            BP.WF.Port.Admin2Group.Org org = new BP.WF.Port.Admin2Group.Org();
            org.CheckPhysicsTable();

            BP.WF.Template.FlowSort fs = new BP.WF.Template.FlowSort();
            fs.CheckPhysicsTable();

            FlowOrg fo = new FlowOrg();
            fo.CheckPhysicsTable();

            BP.Sys.SysEnumMain sem = new SysEnumMain();
            sem.CheckPhysicsTable();

            BP.Sys.SysEnum myse = new SysEnum();
            myse.CheckPhysicsTable();

            //检查表.
            BP.WF.ReturnWork rw = new ReturnWork();
            rw.CheckPhysicsTable();

            //检查表.
            BP.Sys.GloVar gv = new GloVar();
            gv.CheckPhysicsTable();

            //检查表.
            BP.Sys.EnCfg cfg = new BP.Sys.EnCfg();
            cfg.CheckPhysicsTable();

            //检查表.
            SysFormTree frmTree = new SysFormTree();
            frmTree.CheckPhysicsTable();

            BP.Sys.SFTable sf = new BP.Sys.SFTable();
            sf.CheckPhysicsTable();

            FrmSubFlow sb = new FrmSubFlow();
            sb.CheckPhysicsTable();

            BP.WF.Template.PushMsg pm = new PushMsg();
            pm.CheckPhysicsTable();

            //修复数据表.
            BP.Sys.GroupField gf = new GroupField();
            gf.CheckPhysicsTable();

            #endregion 升级优化集团版的应用


            //检查子流程表.
            if (DBAccess.IsExitsObject("WF_NodeSubFlow") == true)
            {
                if (DBAccess.IsExitsTableCol("WF_NodeSubFlow", "OID") == true)
                {
                    DBAccess.RunSQL("DROP TABLE WF_NodeSubFlow");
                    SubFlowHand sub = new SubFlowHand();
                    sub.CheckPhysicsTable();
                }
            }

            // 升级fromjson .//NOTE:此处有何用？而且md变量在下方已经声明，编译都通不过，2017-05-20，liuxc
            //MapData md = new MapData();
            //md.FormJson = "";
            #endregion 检查是否需要升级，并更新升级的业务逻辑.

            #region 升级方向条件. 2020.06.02
            if (DBAccess.IsExitsTableCol("WF_Cond", "CondOrAnd") == true)
            {
                DataTable dt = DBAccess.RunSQLReturnTable("SELECT DISTINCT FK_Node, toNodeID, CondOrAnd, CondType  FROM wf_cond WHERE DataFrom!=100 ");
                foreach (DataRow dr in dt.Rows)
                {
                    int nodeID = int.Parse(dr["FK_Node"].ToString());
                    int toNodeID = int.Parse(dr["toNodeID"].ToString());

                    Conds conds = new Conds();
                    conds.Retrieve(CondAttr.FK_Node, nodeID,
                        CondAttr.ToNodeID, toNodeID, CondAttr.Idx);

                    //判断是否需要修复？
                    if (conds.Count == 1 || conds.Count == 0)
                        continue;

                    //判断是否有？
                    bool isHave = false;
                    foreach (Cond myCond in conds)
                    {
                        if (myCond.HisDataFrom == ConnDataFrom.CondOperator)
                            isHave = true;
                    }
                    if (isHave == true)
                        continue;



                    //获得类型.
                    int OrAndType = DBAccess.RunSQLReturnValInt("SELECT  CondOrAnd  FROM wf_cond WHERE FK_Node=" + nodeID, -1);
                    if (OrAndType == -1)
                        continue;

                    int idx = 0;
                    int idxSave = 0;
                    int count = conds.Count;
                    foreach (Cond item in conds)
                    {
                        idx++;

                        idxSave++;
                        item.Idx = idxSave;
                        item.Update();

                        if (count == idx)
                            continue;

                        Cond operCond = new Cond();
                        operCond.Copy(item);
                        operCond.setMyPK(DBAccess.GenerGUID());
                        operCond.HisDataFrom = ConnDataFrom.CondOperator;

                        if (OrAndType == 0)
                        {
                            operCond.OperatorValue = " OR ";
                            operCond.Note = " OR ";
                            operCond.OperatorValue = " OR ";
                            operCond.Note = " OR ";
                        }
                        else
                        {
                            operCond.OperatorValue = " AND ";
                            operCond.Note = " AND ";
                            operCond.OperatorValue = " AND ";
                            operCond.Note = " AND ";
                        }

                        idxSave++;
                        operCond.Idx = idxSave;
                        operCond.Insert();
                    }
                }

                //升级后删除指定的列.
                DBAccess.DropTableColumn("WF_Cond", "CondOrAnd");
                DBAccess.DropTableColumn("WF_Cond", "NodeID");
            }
            #endregion 升级方向条件.



            #region 升级视图. 解决厦门信息港的 - 流程监控与授权.
            if (DBAccess.IsExitsObject("V_MyFlowData") == false)
            {
                BP.WF.Template.PowerModel pm11 = new PowerModel();
                pm11.CheckPhysicsTable();

                sql = "CREATE VIEW V_MyFlowData ";
                sql += " AS ";
                sql += " SELECT A.*, B.EmpNo as MyEmpNo FROM WF_GenerWorkflow A, WF_PowerModel B ";
                sql += " WHERE  A.FK_Flow=B.FlowNo AND B.PowerCtrlType=1 AND A.WFState>= 2";
                sql += "    UNION  ";
                sql += " SELECT A.*, c.No as MyEmpNo FROM WF_GenerWorkflow A, WF_PowerModel B, Port_Emp C, Port_DeptEmpStation D";
                sql += " WHERE  A.FK_Flow=B.FlowNo  AND B.PowerCtrlType=0 AND C.No=D.FK_Emp AND B.StaNo=D.FK_Station AND A.WFState>=2";
                DBAccess.RunSQL(sql);
            }

            if (DBAccess.IsExitsObject("V_WF_AuthTodolist") == false)
            {
                BP.WF.Auth en = new Auth();
                en.CheckPhysicsTable();

                sql = "CREATE VIEW V_WF_AuthTodolist ";
                sql += " AS ";
                sql += " SELECT B.FK_Emp Auther,B.FK_EmpText AuthName,A.PWorkID,A.FK_Node,A.FID,A.WorkID,C.AutherToEmpNo,  C.TakeBackDT, A.FK_Flow, A.FlowName,A.Title ";
                sql += " FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B, WF_Auth C";
                sql += " WHERE A.WorkID=B.WorkID AND C.AuthType=1 AND B.FK_Emp=C.Auther AND B.IsPass=0 AND B.IsEnable=1 AND A.FK_Node = B.FK_Node AND A.WFState >=2";
                sql += "    UNION  ";
                sql += " SELECT B.FK_Emp Auther,B.FK_EmpText AuthName,A.PWorkID,A.FK_Node,A.FID,A.WorkID, C.AutherToEmpNo, C.TakeBackDT, A.FK_Flow, A.FlowName,A.Title";
                sql += " FROM WF_GenerWorkFlow A, WF_GenerWorkerlist B, WF_Auth C";
                sql += " WHERE A.WorkID=B.WorkID AND C.AuthType=2 AND B.FK_Emp=C.Auther AND B.IsPass=0 AND B.IsEnable=1 AND A.FK_Node = B.FK_Node AND A.WFState >=2 AND A.FK_Flow=C.FlowNo";
                DBAccess.RunSQL(sql);
            }
            #endregion 升级视图.

            //升级从表的 fk_node .
            //获取需要修改的从表
            string dtlNos = DBAccess.RunSQLReturnString("SELECT B.NO  FROM SYS_GROUPFIELD A, SYS_MAPDTL B WHERE A.CTRLTYPE='Dtl' AND A.CTRLID=B.NO AND FK_NODE!=0");
            if (DataType.IsNullOrEmpty(dtlNos) == false)
            {
                dtlNos = dtlNos.Replace(",", "','");
                dtlNos = "('" + dtlNos + "')";
                DBAccess.RunSQL("UPDATE SYS_MAPDTL SET FK_NODE=0 WHERE NO IN " + dtlNos);
            }
            FrmNode nff = new FrmNode();
            nff.CheckPhysicsTable();

            #region 更新节点名称.
            switch (SystemConfig.AppCenterDBType)
            {
                case DBType.MSSQL:
                    sql = " UPDATE WF_Direction SET ToNodeName = WF_Node.Name FROM WF_Node ";
                    sql += " WHERE WF_Direction.ToNode = WF_Node.NodeID ";
                    break;
                default:
                    sql = "UPDATE WF_Direction A, WF_Node B SET A.ToNodeName=B.Name WHERE A.ToNode=B.NodeID ";
                    break;
            }
            DBAccess.RunSQL(sql);

            //更新groupField.
            switch (BP.Difference.SystemConfig.AppCenterDBType)
            {
                case DBType.MySQL:
                    sql = "UPDATE Sys_MapDtl, Sys_GroupField B SET Sys_MapDtl.GroupField=B.OID WHERE Sys_MapDtl.No=B.CtrlID AND Sys_MapDtl.GroupField=''";
                    break;
                case DBType.MSSQL:
                default:
                    sql = "UPDATE Sys_MapDtl SET Sys_MapDtl.GroupField=Sys_GroupField.OID FROM Sys_GroupField WHERE Sys_MapDtl.No=Sys_GroupField.CtrlID AND Sys_MapDtl.GroupField=''";
                    break;
            }
            DBAccess.RunSQL(sql);
            #endregion 更新节点名称.

            #region 升级审核组件
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL)
                sql = "UPDATE WF_FrmNode F INNER JOIN(SELECT FWCSta,NodeID FROM WF_Node ) N on F.FK_Node = N.NodeID and  F.IsEnableFWC =1 SET F.IsEnableFWC = N.FWCSta;";
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MSSQL)
                sql = "UPDATE    F SET IsEnableFWC = N. FWCSta  FROM WF_FrmNode F,WF_Node N    WHERE F.FK_Node = N.NodeID AND F.IsEnableFWC =1";
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.Oracle || BP.Difference.SystemConfig.AppCenterDBType == DBType.KingBaseR3 || BP.Difference.SystemConfig.AppCenterDBType == DBType.KingBaseR6)
                sql = "UPDATE WF_FrmNode F  SET (IsEnableFWC)=(SELECT FWCSta FROM WF_Node N WHERE F.FK_Node = N.NodeID AND F.IsEnableFWC =1)";
            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL || BP.Difference.SystemConfig.AppCenterDBType == DBType.UX)
                sql = "UPDATE WF_FrmNode SET IsEnableFWC=(SELECT FWCSta FROM WF_Node N Where N.NodeID=WF_FrmNode.FK_Node AND WF_FrmNode.IsEnableFWC=1)";

            DBAccess.RunSQL(sql);
            #endregion 升级审核组件

            #region 升级填充数据.
            //pop自动填充
            MapExts exts = new MapExts();
            qo = new QueryObject(exts);
            qo.AddWhere(MapExtAttr.ExtType, " LIKE ", "Pop%");
            qo.DoQuery();
            foreach (MapExt ext in exts)
            {
                string mypk = ext.FK_MapData + "_" + ext.AttrOfOper;
                MapAttr ma = new MapAttr();
                ma.setMyPK(mypk);
                if (ma.RetrieveFromDBSources() == 0)
                {
                    ext.Delete();
                    continue;
                }

                if (ma.GetParaString("PopModel") == ext.ExtType)
                    continue; //已经修复了，或者配置了.

                ma.SetPara("PopModel", ext.ExtType);
                ma.Update();

                if (DataType.IsNullOrEmpty(ext.Tag4) == true)
                    continue;

                MapExt extP = new MapExt();
                extP.setMyPK(ext.MyPK + "_FullData");
                int i = extP.RetrieveFromDBSources();
                if (i == 1)
                    continue;

                extP.ExtType = "FullData";
                extP.setFK_MapData(ext.FK_MapData);
                extP.AttrOfOper = ext.AttrOfOper;
                extP.DBType = ext.DBType;
                extP.Doc = ext.Tag4;
                extP.Insert(); //执行插入.
            }


            //文本自动填充
            exts = new MapExts();
            exts.Retrieve(MapExtAttr.ExtType, MapExtXmlList.TBFullCtrl);
            foreach (MapExt ext in exts)
            {
                string mypk = ext.FK_MapData + "_" + ext.AttrOfOper;
                MapAttr ma = new MapAttr();
                ma.setMyPK(mypk);
                if (ma.RetrieveFromDBSources() == 0)
                {
                    ext.Delete();
                    continue;
                }
                string modal = ma.GetParaString("TBFullCtrl");
                if (DataType.IsNullOrEmpty(modal) == false)
                    continue; //已经修复了，或者配置了.

                if (DataType.IsNullOrEmpty(ext.Tag3) == false)
                    ma.SetPara("TBFullCtrl", "Simple");
                else
                    ma.SetPara("TBFullCtrl", "Table");

                ma.Update();

                if (DataType.IsNullOrEmpty(ext.Tag4) == true)
                    continue;

                MapExt extP = new MapExt();
                extP.setMyPK(ext.MyPK + "_FullData");
                int i = extP.RetrieveFromDBSources();
                if (i == 1)
                    continue;

                extP.ExtType = "FullData";
                extP.setFK_MapData(ext.FK_MapData);
                extP.AttrOfOper = ext.AttrOfOper;
                extP.DBType = ext.DBType;
                extP.Doc = ext.Tag4;

                //填充从表
                extP.Tag1 = ext.Tag1;
                //填充下拉框
                extP.Tag = ext.Tag;

                extP.Insert(); //执行插入.
            }

            //下拉框填充其他控件
            //文本自动填充
            exts = new MapExts();
            exts.Retrieve(MapExtAttr.ExtType, MapExtXmlList.DDLFullCtrl);
            foreach (MapExt ext in exts)
            {
                string mypk = ext.FK_MapData + "_" + ext.AttrOfOper;
                MapAttr ma = new MapAttr();
                ma.setMyPK(mypk);
                if (ma.RetrieveFromDBSources() == 0)
                {
                    ext.Delete();
                    continue;
                }
                string modal = ma.GetParaString("IsFullData");
                if (DataType.IsNullOrEmpty(modal) == false)
                    continue; //已经修复了，或者配置了.

                //启用填充其他控件
                ma.SetPara("IsFullData", 1);
                ma.Update();


                MapExt extP = new MapExt();
                extP.setMyPK(ext.MyPK + "_FullData");
                int i = extP.RetrieveFromDBSources();
                if (i == 1)
                    continue;

                extP.ExtType = "FullData";
                extP.setFK_MapData(ext.FK_MapData);
                extP.AttrOfOper = ext.AttrOfOper;
                extP.DBType = ext.DBType;
                extP.Doc = ext.Doc;


                //填充从表
                extP.Tag1 = ext.Tag1;
                //填充下拉框
                extP.Tag = ext.Tag;

                extP.Insert(); //执行插入.

            }

            //装载填充
            exts = new MapExts();
            exts.Retrieve(MapExtAttr.ExtType, MapExtXmlList.PageLoadFull);
            foreach (MapExt ext in exts)
            {
                mapData.No = ext.FK_MapData;
                if (mapData.RetrieveFromDBSources() == 0)
                {
                    ext.Delete();
                    continue;
                }
                if (DataType.IsNullOrEmpty(mapData.GetParaString("IsPageLoadFull")) == false)
                    continue;
                mapData.IsPageLoadFull = true;
                mapData.Update();

                //修改填充数据的值
                ext.Doc = ext.Tag;

                string tag1 = ext.Tag1;
                if (DataType.IsNullOrEmpty(tag1) == true)
                {
                    ext.Update();
                    continue;
                }
                MapDtls dtls = mapData.MapDtls;
                foreach (MapDtl dtl in dtls)
                {
                    tag1 = tag1.Replace("*" + dtl.No + "=", "$" + dtl.No + ":");
                }
                ext.Tag1 = tag1;
                ext.Update();
            }
            #endregion 升级 填充数据.

            string msg = "";
            try
            {

                #region 升级事件.
                if (DBAccess.IsExitsTableCol("Sys_FrmEvent", "DoType") == true)
                {
                    BP.Sys.FrmEvent frmevent = new FrmEvent();
                    frmevent.CheckPhysicsTable();

                    DBAccess.RunSQL("UPDATE Sys_FrmEvent SET EventDoType=DoType  ");
                    DBAccess.RunSQL("ALTER TABLE Sys_FrmEvent  DROP COLUMN	DoType  ");
                }
                #endregion

                #region 修复丢失的发起人.
                Flows fls = new Flows();
                fls.GetNewEntity.CheckPhysicsTable();

                foreach (Flow item in fls)
                {
                    if (DBAccess.IsExitsObject(item.PTable) == false)
                        continue;
                    try
                    {
                        DBAccess.RunSQL(" UPDATE " + item.PTable + " SET FlowStarter =(SELECT Starter FROM WF_GENERWORKFLOW WHERE " + item.PTable + ".Oid=WF_GENERWORKFLOW.WORKID)");
                    }
                    catch (Exception ex)
                    {
                        //   GERpt rpt=new GERpt(
                    }
                }
                #endregion 修复丢失的发起人.

                #region 给city 设置拼音.
                if (DBAccess.IsExitsObject("CN_City") == true && 1 == 2)
                {
                    if (DBAccess.IsExitsTableCol("CN_City", "PinYin") == true)
                    {
                        /*为cn_city 设置拼音.*/
                        sql = "SELECT No,Names FROM CN_City ";
                        DataTable dtCity = DBAccess.RunSQLReturnTable(sql);

                        foreach (DataRow dr in dtCity.Rows)
                        {
                            string no = dr["No"].ToString();
                            string name = dr["Names"].ToString();
                            string pinyin1 = DataType.ParseStringToPinyin(name);
                            string pinyin2 = DataType.ParseStringToPinyinJianXie(name);
                            string pinyin = "," + pinyin1 + "," + pinyin2 + ",";
                            DBAccess.RunSQL("UPDATE CN_City SET PinYin='" + pinyin + "' WHERE NO='" + no + "'");
                        }
                    }
                }
                #endregion 给city 设置拼音.

                //增加列FlowStars
                BP.WF.Port.WFEmp wfemp = new BP.WF.Port.WFEmp();
                wfemp.CheckPhysicsTable();

                #region 更新wf_emp. 的字段类型. 2019.06.19
                DBType dbtype = BP.Difference.SystemConfig.AppCenterDBType;

                //if (DBAccess.IsExitsTableCol("WF_Emp", "StartFlows") == true)
                //    DBAccess.RunSQL("ALTER TABLE WF_Emp DROP Column StartFlows");

                //if (dbtype == DBType.Oracle)
                //    DBAccess.RunSQL("ALTER TABLE WF_Emp Add StartFlows BLOB");

                //if (dbtype == DBType.MySQL)
                //    DBAccess.RunSQL("ALTER TABLE WF_Emp modify StartFlows longtext ");

                //if (dbtype == DBType.MSSQL)
                //    DBAccess.RunSQL(" ALTER TABLE WF_Emp ALTER column  StartFlows nvarchar(4000) null");

                #endregion 更新wf_emp 的字段类型.

                BP.Sys.FrmRB rb = new FrmRB();
                rb.CheckPhysicsTable();

                CC ccEn = new CC();
                ccEn.CheckPhysicsTable();

                MapDtlExt dtlExt = new MapDtlExt();
                dtlExt.CheckPhysicsTable();

                //删除枚举.
                DBAccess.RunSQL("DELETE FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey IN ('SelectorModel','CtrlWayAth')");

                //SysEnum se = new SysEnum("FrmType", 1);//NOTE:此处升级时报错，2017-06-13，liuxc

                //2017.5.19 打印模板字段修复
                FrmPrintTemplate bt = new FrmPrintTemplate();
                bt.CheckPhysicsTable();
                if (DBAccess.IsExitsTableCol("Sys_FrmPrintTemplate", "url") == true)
                    DBAccess.RunSQL("UPDATE Sys_FrmPrintTemplate SET TempFilePath = Url WHERE TempFilePath IS null");

                //规范升级根目录.
                DataTable dttree = DBAccess.RunSQLReturnTable("SELECT No FROM Sys_FormTree WHERE ParentNo='-1' ");
                if (dttree.Rows.Count == 1)
                {
                    DBAccess.RunSQL("UPDATE Sys_FormTree SET ParentNo='1' WHERE ParentNo='0' ");
                    DBAccess.RunSQL("UPDATE Sys_FormTree SET No='1' WHERE No='0'  ");
                    DBAccess.RunSQL("UPDATE Sys_FormTree SET ParentNo='0' WHERE No='1'");
                }

                //删除垃圾数据.
                BP.Sys.MapExt.DeleteDB();

                //升级傻瓜表单.
                MapFrmFool mff = new MapFrmFool();
                mff.CheckPhysicsTable();

                #region 表单方案中的不可编辑, 旧版本如果包含了这个列.
                if (DBAccess.IsExitsTableCol("WF_FrmNode", "IsEdit") == true)
                {
                    /*如果存在这个列,就查询出来=0的设置，就让其设置为不可以编辑的。*/
                    sql = "UPDATE WF_FrmNode SET FrmSln=1 WHERE IsEdit=0 ";
                    DBAccess.RunSQL(sql);

                    sql = "UPDATE WF_FrmNode SET IsEdit=100000";
                    DBAccess.RunSQL(sql);
                }
                #endregion

                //执行升级 2016.04.08 
                Cond cnd = new Cond();
                cnd.CheckPhysicsTable();

                #region  增加week字段,方便按周统计.
                BP.WF.GenerWorkFlow gwf = new GenerWorkFlow();
                gwf.CheckPhysicsTable();
                sql = "SELECT WorkID,RDT FROM WF_GenerWorkFlow WHERE WeekNum=0 or WeekNum is null ";
                DataTable dt = DBAccess.RunSQLReturnTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    sql = "UPDATE WF_GenerWorkFlow SET WeekNum=" + DataType.CurrentWeekGetWeekByDay(dr[1].ToString().Replace("+", " ")) + " WHERE WorkID=" + dr[0].ToString();
                    DBAccess.RunSQL(sql);
                }

                //查询.
                BP.WF.Data.CH ch = new CH();
                ch.CheckPhysicsTable();

                sql = "SELECT MyPK,DTFrom FROM WF_CH WHERE WeekNum=0 or WeekNum is null ";
                dt = DBAccess.RunSQLReturnTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    sql = "UPDATE WF_CH SET WeekNum=" + DataType.CurrentWeekGetWeekByDay(dr[1].ToString()) + " WHERE MyPK='" + dr[0].ToString() + "'";
                    DBAccess.RunSQL(sql);
                }
                #endregion  增加week字段.

                #region 检查数据源.
                SFDBSrc src = new SFDBSrc();
                src.No = "local";
                src.Name = "本机数据源(默认)";
                if (src.RetrieveFromDBSources() == 0)
                    src.Insert();
                #endregion 检查数据源.

                #region 20170613.增加审核组件配置项“是否显示退回的审核信息”对应字段 by:liuxianchen
                try
                {
                    if (DBAccess.IsExitsTableCol("WF_Node", "FWCIsShowReturnMsg") == false)
                    {
                        switch (src.HisDBType)
                        {
                            case DBType.MSSQL:
                                DBAccess.RunSQL("ALTER TABLE WF_Node ADD FWCIsShowReturnMsg INT NULL");
                                break;
                            case DBType.Oracle:
                            case DBType.Informix:
                            case DBType.PostgreSQL:
                            case DBType.UX:
                            case DBType.KingBaseR3:
                            case DBType.KingBaseR6:
                                DBAccess.RunSQL("ALTER TABLE WF_Node ADD FWCIsShowReturnMsg INTEGER NULL");
                                break;
                            case DBType.MySQL:
                                DBAccess.RunSQL("ALTER TABLE WF_Node ADD FWCIsShowReturnMsg INT NULL");
                                break;
                            default:
                                break;
                        }

                        DBAccess.RunSQL("UPDATE WF_Node SET FWCIsShowReturnMsg = 0");
                    }
                }
                catch
                {
                }
                #endregion

                #region 20170522.增加SL表单设计器中对单选/复选按钮进行字体大小调节的功能 by:liuxianchen

                FrmRB frmRB = new FrmRB();
                frmRB.CheckPhysicsTable();

                try
                {
                    DataTable columns = src.GetColumns("Sys_FrmRB");
                    if (columns.Select("No='AtPara'").Length == 0)
                    {
                        switch (src.HisDBType)
                        {
                            case DBType.MSSQL:
                                DBAccess.RunSQL("ALTER TABLE Sys_FrmRB ADD AtPara NVARCHAR(1000) NULL");
                                break;
                            case DBType.Oracle:
                            case DBType.KingBaseR3:
                            case DBType.KingBaseR6:
                                DBAccess.RunSQL("ALTER TABLE Sys_FrmRB ADD AtPara NVARCHAR2(1000) NULL");
                                break;
                            case DBType.PostgreSQL:
                            case DBType.UX:
                                DBAccess.RunSQL("ALTER TABLE Sys_FrmRB ADD AtPara VARCHAR2(1000) NULL");
                                break;
                            case DBType.MySQL:
                            case DBType.Informix:
                                DBAccess.RunSQL("ALTER TABLE Sys_FrmRB ADD AtPara TEXT NULL");
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch
                {
                }
                #endregion

                #region 其他.
                // 更新 PassRate.
                sql = "UPDATE WF_Node SET PassRate=100 WHERE PassRate IS NULL";
                DBAccess.RunSQL(sql);
                #endregion 其他.

                #region 升级统一规则.
                #region 检查必要的升级。
                NodeWorkCheck fwc = new NodeWorkCheck();
                fwc.CheckPhysicsTable();

                Flow myfl = new Flow();
                myfl.CheckPhysicsTable();

                Node nd = new Node();
                nd.CheckPhysicsTable();

                //Sys_SFDBSrc
                SFDBSrc sfDBSrc = new SFDBSrc();
                sfDBSrc.CheckPhysicsTable();
                #endregion 检查必要的升级。
                MapExt mapExt = new MapExt();
                mapExt.CheckPhysicsTable();

                try
                {
                    string sqls = "";

                    if (BP.Difference.SystemConfig.AppCenterDBType == DBType.Oracle || BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL || BP.Difference.SystemConfig.AppCenterDBType == DBType.UX || BP.Difference.SystemConfig.AppCenterDBType == DBType.KingBaseR3 || BP.Difference.SystemConfig.AppCenterDBType == DBType.KingBaseR6)
                    {
                        sqls += "UPDATE Sys_MapExt SET MyPK= ExtType||'_'||FK_Mapdata||'_'||AttrOfOper WHERE ExtType='" + MapExtXmlList.TBFullCtrl + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK= ExtType||'_'||FK_Mapdata||'_'||AttrOfOper WHERE ExtType='" + MapExtXmlList.PopVal + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK= ExtType||'_'||FK_Mapdata||'_'||AttrOfOper WHERE ExtType='" + MapExtXmlList.DDLFullCtrl + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK= ExtType||'_'||FK_Mapdata||'_'||AttrsOfActive WHERE ExtType='" + MapExtXmlList.ActiveDDL + "'";
                    }
                    else if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL)
                    {
                        sqls += "UPDATE Sys_MapExt SET MyPK=CONCAT(ExtType,'_',FK_Mapdata,'_',AttrOfOper) WHERE ExtType='" + MapExtXmlList.TBFullCtrl + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK=CONCAT(ExtType,'_',FK_Mapdata,'_',AttrOfOper) WHERE ExtType='" + MapExtXmlList.PopVal + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK=CONCAT(ExtType,'_',FK_Mapdata,'_',AttrOfOper) WHERE ExtType='" + MapExtXmlList.DDLFullCtrl + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK=CONCAT(ExtType,'_',FK_Mapdata,'_',AttrOfOper) WHERE ExtType='" + MapExtXmlList.ActiveDDL + "'";
                    }
                    else
                    {
                        sqls += "UPDATE Sys_MapExt SET MyPK= ExtType+'_'+FK_Mapdata+'_'+AttrOfOper WHERE ExtType='" + MapExtXmlList.TBFullCtrl + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK= ExtType+'_'+FK_Mapdata+'_'+AttrOfOper WHERE ExtType='" + MapExtXmlList.PopVal + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK= ExtType+'_'+FK_Mapdata+'_'+AttrOfOper WHERE ExtType='" + MapExtXmlList.DDLFullCtrl + "'";
                        sqls += "@UPDATE Sys_MapExt SET MyPK= ExtType+'_'+FK_Mapdata+'_'+AttrsOfActive WHERE ExtType='" + MapExtXmlList.ActiveDDL + "'";

                    }

                    DBAccess.RunSQLs(sqls);
                }
                catch (Exception ex)
                {
                    BP.DA.Log.DebugWriteError(ex.Message);
                }
                #endregion

                //#region  更新CA签名(2015-03-03)。
                //BP.Tools.WFSealData sealData = new Tools.WFSealData();
                //sealData.CheckPhysicsTable();
                //sealData.UpdateColumn();
                //#endregion  更新CA签名.

                #region 其他.
                //升级表单树. 2015.10.05
                SysFormTree sft = new SysFormTree();
                sft.CheckPhysicsTable();


                //表单信息表.
                MapDataExt mapext = new MapDataExt();
                mapext.CheckPhysicsTable();

                TransferCustom tc = new TransferCustom();
                tc.CheckPhysicsTable();

                //增加部门字段。
                CCList cc = new CCList();
                cc.CheckPhysicsTable();
                #endregion 其他.

                #region 升级sys_sftable
                //升级
                BP.Sys.SFTable tab = new BP.Sys.SFTable();
                tab.CheckPhysicsTable();
                #endregion

                #region 基础数据更新.
                Node wf_Node = new Node();
                wf_Node.CheckPhysicsTable();
                // 设置节点ICON.
                sql = "UPDATE WF_Node SET ICON='审核.png' WHERE ICON IS NULL";
                DBAccess.RunSQL(sql);

                BP.WF.Template.NodeSheet nodeSheet = new BP.WF.Template.NodeSheet();
                nodeSheet.CheckPhysicsTable();

                #endregion 基础数据更新.

                #region 把节点的toolbarExcel, word 信息放入mapdata 
                //BP.WF.Template.NodeSheets nss = new NodeSheets();
                //nss.RetrieveAll();
                //foreach (BP.WF.Template.NodeSheet ns in nss)
                //{
                //    ToolbarExcel te = new ToolbarExcel();
                //    te.No = "ND" + ns.NodeID;
                //    te.RetrieveFromDBSources();
                //}
                #endregion

                #region 升级SelectAccper

                SelectAccper selectAccper = new SelectAccper();
                selectAccper.CheckPhysicsTable();
                #endregion

                #region  升级 NodeToolbar
                FrmField ff = new FrmField();
                ff.CheckPhysicsTable();




                SysFormTree ssft = new SysFormTree();
                ssft.CheckPhysicsTable();

                BP.Sys.FrmAttachment myath = new FrmAttachment();
                myath.CheckPhysicsTable();

                FrmField ffs = new FrmField();
                ffs.CheckPhysicsTable();
                #endregion

                #region 执行sql．
                DBAccess.RunSQL("delete  from " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey in ('PrintFileType','EventDoType','FormType','BatchRole','StartGuideWay','NodeFormType')");
                DBAccess.RunSQL("UPDATE Sys_FrmSln SET FK_Flow =(SELECT FK_FLOW FROM WF_Node WHERE NODEID=Sys_FrmSln.FK_Node) WHERE FK_Flow IS NULL");

                if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MSSQL)
                    DBAccess.RunSQL("UPDATE WF_FrmNode SET MyPK=FK_Frm+'_'+convert(varchar,FK_Node )+'_'+FK_Flow");

                if (BP.Difference.SystemConfig.AppCenterDBType == DBType.Oracle || BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL || BP.Difference.SystemConfig.AppCenterDBType == DBType.UX || BP.Difference.SystemConfig.AppCenterDBType == DBType.KingBaseR3 || BP.Difference.SystemConfig.AppCenterDBType == DBType.KingBaseR6)
                    DBAccess.RunSQL("UPDATE WF_FrmNode SET MyPK=FK_Frm||'_'||FK_Node||'_'||FK_Flow");

                if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL)
                    DBAccess.RunSQL("UPDATE WF_FrmNode SET MyPK=CONCAT(FK_Frm,'_',FK_Node,'_',FK_Flow)");

                #endregion

                #region 执行更新.wf_node
                sql = "UPDATE WF_Node SET FWCType=0 WHERE FWCType IS NULL";
                sql += "@UPDATE WF_Node SET FWC_H=0 WHERE FWC_H IS NULL";
                DBAccess.RunSQLs(sql);


                sql = "UPDATE WF_Node SET SFSta=0 WHERE SFSta IS NULL";
                sql += "@UPDATE WF_Node SET SF_H=0 WHERE SF_H IS NULL";
                DBAccess.RunSQLs(sql);
                #endregion 执行更新.

                #region 升级站内消息表 2013-10-20
                BP.WF.SMS sms = new SMS();
                sms.CheckPhysicsTable();
                #endregion

                #region 重新生成view WF_EmpWorks,  2013-08-06.
                if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.Single)
                {
                    if (DBAccess.IsExitsObject("WF_EmpWorks") == true)
                        DBAccess.RunSQL("DROP VIEW WF_EmpWorks");

                    if (DBAccess.IsExitsObject("V_FlowStarterBPM") == true)
                        DBAccess.RunSQL("DROP VIEW V_FlowStarterBPM");

                    if (DBAccess.IsExitsObject("V_TOTALCH") == true)
                        DBAccess.RunSQL("DROP VIEW V_TOTALCH");

                    if (DBAccess.IsExitsObject("V_TOTALCHYF") == true)
                        DBAccess.RunSQL("DROP VIEW V_TOTALCHYF");

                    if (DBAccess.IsExitsObject("V_TotalCHWeek") == true)
                        DBAccess.RunSQL("DROP VIEW V_TotalCHWeek");

                    if (DBAccess.IsExitsObject("V_WF_Delay") == true)
                        DBAccess.RunSQL("DROP VIEW V_WF_Delay");

                    string sqlscript = "";
                    //执行必须的sql.
                    switch (BP.Difference.SystemConfig.AppCenterDBType)
                    {
                        case DBType.Oracle:
                        case DBType.KingBaseR3:
                        case DBType.KingBaseR6:
                            sqlscript = BP.Difference.SystemConfig.PathOfData + "Install/SQLScript/InitView_Ora.sql";
                            break;
                        case DBType.MSSQL:
                        case DBType.Informix:
                            sqlscript = BP.Difference.SystemConfig.PathOfData + "Install/SQLScript/InitView_SQL.sql";
                            break;
                        case DBType.MySQL:
                            sqlscript = BP.Difference.SystemConfig.PathOfData + "Install/SQLScript/InitView_MySQL.sql";
                            break;
                        case DBType.PostgreSQL:
                        case DBType.UX:
                            sqlscript = BP.Difference.SystemConfig.PathOfData + "Install/SQLScript/InitView_PostgreSQL.sql";
                            break;
                        default:
                            break;
                    }

                    DBAccess.RunSQLScript(sqlscript);
                }

                #endregion

                #region 升级Img
                FrmImg img = new FrmImg();
                img.CheckPhysicsTable();

                ExtImg myimg = new ExtImg();
                myimg.CheckPhysicsTable();
                if (DBAccess.IsExitsTableCol("Sys_FrmImg", "SrcType") == true)
                {
                    DBAccess.RunSQL("UPDATE Sys_FrmImg SET ImgSrcType = SrcType WHERE ImgSrcType IS NULL");
                    DBAccess.RunSQL("UPDATE Sys_FrmImg SET ImgSrcType = 0 WHERE ImgSrcType IS NULL");
                }
                #endregion

                #region 修复 mapattr UIHeight, UIWidth 类型错误.
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        msg = "@Sys_MapAttr 修改字段";
                        break;
                    case DBType.MSSQL:
                        msg = "@修改sql server控件高度和宽度字段。";
                        DBAccess.RunSQL("ALTER TABLE Sys_MapAttr ALTER COLUMN UIWidth float");
                        DBAccess.RunSQL("ALTER TABLE Sys_MapAttr ALTER COLUMN UIHeight float");
                        break;
                    default:
                        break;
                }
                #endregion

                #region 升级常用词汇
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        int i = DBAccess.RunSQLReturnCOUNT("SELECT * FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'SYS_DEFVAL' AND COLUMN_NAME = 'PARENTNO'");
                        if (i == 0)
                        {
                            if (DBAccess.IsExitsObject("SYS_DEFVAL") == true)
                                DBAccess.RunSQL("drop table Sys_DefVal");

                            DefVal dv = new DefVal();
                            dv.CheckPhysicsTable();
                        }
                        msg = "@Sys_DefVal 修改字段";
                        break;
                    case DBType.MSSQL:
                        msg = "@修改 Sys_DefVal。";
                        i = DBAccess.RunSQLReturnCOUNT("SELECT * FROM SYSCOLUMNS WHERE ID=OBJECT_ID('Sys_DefVal') AND NAME='ParentNo'");
                        if (i == 0)
                        {
                            if (DBAccess.IsExitsObject("Sys_DefVal") == true)
                                DBAccess.RunSQL("drop table Sys_DefVal");

                            DefVal dv = new DefVal();
                            dv.CheckPhysicsTable();
                        }
                        break;
                    default:
                        break;
                }
                #endregion

                #region 登陆更新错误
                msg = "@登陆时错误。";
                DBAccess.RunSQL("DELETE FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey IN ('DeliveryWay','RunModel','OutTimeDeal','FlowAppType')");
                #endregion 登陆更新错误

                #region 升级表单树
                // 首先检查是否升级过.
                //sql = "SELECT * FROM Sys_FormTree WHERE No = '1'";
                //DataTable formTree_dt = DBAccess.RunSQLReturnTable(sql);
                //if (formTree_dt.Rows.Count == 0)
                //{
                //    /*没有升级过.增加根节点*/
                //    SysFormTree formTree = new SysFormTree();
                //    formTree.No = "1";
                //    formTree.Name = "表单库";
                //    formTree.ParentNo = "0";
                //    // formTree.TreeNo = "0";
                //    formTree.Idx = 0;
                //    formTree.IsDir = true;
                //        formTree.DirectInsert();

                //    //将表单库中的数据转入表单树
                //    SysFormTrees formSorts = new SysFormTrees();
                //    formSorts.RetrieveAll();

                //    foreach (SysFormTree item in formSorts)
                //    {
                //        if (item.No == "0")
                //            continue;
                //        SysFormTree subFormTree = new SysFormTree();
                //        subFormTree.No = item.No;
                //        subFormTree.Name = item.Name;
                //        subFormTree.ParentNo = "0";
                //        subFormTree.Save();
                //    }
                //    //表单于表单树进行关联
                //    sql = "UPDATE Sys_MapData SET FK_FormTree=FK_FrmSort WHERE FK_FrmSort <> '' AND FK_FrmSort IS not null";
                //    DBAccess.RunSQL(sql);
                //}
                #endregion

                #region 执行admin登陆. 2012-12-25 新版本.
                Emp emp = new Emp("admin");
                if (emp.RetrieveFromDBSources() == 1)
                {
                    BP.Web.WebUser.SignInOfGener(emp);
                }
                else
                {
                    emp.SetValByKey("No", "admin");
                    emp.Name = "admin";
                    emp.FK_Dept = "01";
                    emp.Pass = "123";
                    emp.Insert();
                    BP.Web.WebUser.SignInOfGener(emp);
                    //throw new Exception("admin 用户丢失，请注意大小写。");
                }
                #endregion 执行admin登陆.

                #region 修复 Sys_FrmImg 表字段 ImgAppType Tag0
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        int i = DBAccess.RunSQLReturnCOUNT("SELECT * FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'SYS_FRMIMG' AND COLUMN_NAME = 'TAG0'");
                        if (i == 0)
                        {
                            DBAccess.RunSQL("ALTER TABLE SYS_FRMIMG ADD (ImgAppType number,TAG0 nvarchar(500))");
                        }
                        msg = "@Sys_FrmImg 修改字段";
                        break;
                    case DBType.MSSQL:
                        msg = "@修改 Sys_FrmImg。";
                        i = DBAccess.RunSQLReturnCOUNT("SELECT * FROM SYSCOLUMNS WHERE ID=OBJECT_ID('Sys_FrmImg') AND NAME='Tag0'");
                        if (i == 0)
                        {
                            DBAccess.RunSQL("ALTER TABLE Sys_FrmImg ADD ImgAppType int");
                            DBAccess.RunSQL("ALTER TABLE Sys_FrmImg ADD Tag0 nvarchar(500)");
                        }
                        break;
                    default:
                        break;
                }
                #endregion

                #region 20161101.升级表单，增加图片附件必填验证 by:liuxianchen

                BP.Sys.FrmImgAth imgAth = new BP.Sys.FrmImgAth();
                imgAth.CheckPhysicsTable();

                sql = "UPDATE Sys_FrmImgAth SET IsRequired = 0 WHERE IsRequired IS NULL";
                DBAccess.RunSQL(sql);
                #endregion

                #region 20170520.附件删除规则修复
                try
                {
                    DataTable columns = src.GetColumns("Sys_FrmAttachment");
                    if (columns.Select("No='DeleteWay'").Length > 0 && columns.Select("No='IsDelete'").Length > 0)
                    {
                        DBAccess.RunSQL("UPDATE SYS_FRMATTACHMENT SET DeleteWay=IsDelete WHERE DeleteWay IS NULL");
                    }
                }
                catch
                {
                }
                #endregion

                #region 密码加密
                if (BP.Difference.SystemConfig.IsEnablePasswordEncryption == true && DBAccess.IsView("Port_Emp", BP.Difference.SystemConfig.AppCenterDBType) == false)
                {
                    BP.Port.Emps emps = new BP.Port.Emps();
                    emps.RetrieveAllFromDBSource();
                    foreach (Emp empEn in emps)
                    {
                        if (string.IsNullOrEmpty(empEn.Pass) || empEn.Pass.Length < 30)
                        {
                            empEn.Pass = BP.Tools.Cryptography.EncryptString(empEn.Pass);
                            empEn.DirectUpdate();
                        }
                    }
                }
                #endregion

                // 最后更新版本号，然后返回.
                sql = "UPDATE Sys_Serial SET IntVal=" + Ver + " WHERE CfgKey='Ver'";
                if (DBAccess.RunSQL(sql) == 0)
                {
                    sql = "INSERT INTO Sys_Serial (CfgKey,IntVal) VALUES('Ver'," + Ver + ") ";
                    DBAccess.RunSQL(sql);
                }
                // 返回版本号.
                return "旧版本:(" + currDBVer + ")新版本:(" + Ver + ")"; // +"\t\n解决问题:" + updataNote;
            }
            catch (Exception ex)
            {
                string err = "问题出处:" + ex.Message + "<hr>" + msg + "<br>详细信息:@" + ex.StackTrace + "<br>@<a href='../DBInstall.htm' >点这里到系统升级界面。</a>";
                BP.DA.Log.DebugWriteError("系统升级错误:" + err);
                return err;
            }
        }
        /// <summary>
        /// 如果发现升级sql文件日期变化了，就自动升级.
        /// 就是说该文件如果被修改了就会自动升级.
        /// </summary>
        public static void UpdataCCFlowVerSQLScript()
        {

            string sql = "SELECT IntVal FROM Sys_Serial WHERE CfgKey='UpdataCCFlowVer'";
            string currDBVer = DBAccess.RunSQLReturnStringIsNull(sql, "");

            string sqlScript = BP.Difference.SystemConfig.PathOfData + "UpdataCCFlowVer.sql";
            System.IO.FileInfo fi = new System.IO.FileInfo(sqlScript);
            if (File.Exists(sqlScript) == false)
                return;
            string myVer = fi.LastWriteTime.ToString("yyyyMMddHH");

            //判断是否可以执行，当文件发生变化后，才执行。
            if (currDBVer == "" || int.Parse(currDBVer) < int.Parse(myVer))
            {
                DBAccess.RunSQLScript(sqlScript);
                sql = "UPDATE Sys_Serial SET IntVal=" + myVer + " WHERE CfgKey='UpdataCCFlowVer'";

                if (DBAccess.RunSQL(sql) == 0)
                {
                    sql = "INSERT INTO Sys_Serial (CfgKey,IntVal) VALUES('UpdataCCFlowVer'," + myVer + ") ";
                    DBAccess.RunSQL(sql);
                }
            }
        }
        /// <summary>
        /// CCFlowAppPath
        /// </summary>
        public static string CCFlowAppPath
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("CCFlowAppPath", "/");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static bool IsEnableHuiQianList
        {
            get
            {
                if (BP.Difference.SystemConfig.CustomerNo == "TianYe")
                    return true;

                return BP.Difference.SystemConfig.GetValByKeyBoolen("IsEnableHuiQianList", false);
            }
        }
        /// <summary>
        /// 检查是否可以安装驰骋BPM系统
        /// </summary>
        /// <returns></returns>
        public static bool IsCanInstall()
        {

            if (BP.Difference.SystemConfig.CCBPMRunModel != CCBPMRunModel.Single)
                throw new Exception("err@初次安装必须设置 CCBPMRunModel=0 为单机版，安装后在修改集团或者saas版.");

            string sql = "";
            string errInfo = "";
            try
            {
                errInfo = " 当前用户没有[查询系统表]的权限. ";

                if (DBAccess.IsExitsObject("AA"))
                {
                    errInfo = " 当前用户没有[删除表]的权限. ";
                    sql = "DROP TABLE AA";
                    DBAccess.RunSQL(sql);
                }

                errInfo = " 当前用户没有[创建表]的权限. ";
                sql = "CREATE TABLE AA (OID int NOT NULL)"; //检查是否可以创建表.
                DBAccess.RunSQL(sql);

                errInfo = " 当前用户没有[插入数据]的权限. ";
                sql = "INSERT INTO AA (OID) VALUES(100)";
                DBAccess.RunSQL(sql);

                try
                {
                    //检查是否可以批量执行sql.
                    sql = "UPDATE AA SET OID=0 WHERE OID=1;UPDATE AA SET OID=0 WHERE OID=1;";
                    DBAccess.RunSQL(sql);
                }
                catch
                {
                    throw new Exception("err@需要让数据库链接支持批量执行SQL语句，请修改数据库链接配置：&allowMultiQueries=true");
                }


                errInfo = " 当前用户没有[update 表数据]的权限. ";
                sql = "UPDATE AA SET OID=101";
                DBAccess.RunSQL(sql);

                errInfo = " 当前用户没有[delete 表数据]的权限. ";
                sql = "DELETE FROM AA";
                DBAccess.RunSQL(sql);

                errInfo = " 当前用户没有[创建表主键]的权限. ";
                DBAccess.CreatePK("AA", "OID", BP.Difference.SystemConfig.AppCenterDBType);

                errInfo = " 当前用户没有[创建索引]的权限. ";
                DBAccess.CreatIndex("AA", "OID"); //可否创建索引.

                errInfo = " 当前用户没有[查询数据表]的权限. ";
                sql = "select * from AA "; //检查是否有查询的权限.
                DBAccess.RunSQLReturnTable(sql);

                errInfo = " 当前数据库设置区分了大小写，不能对大小写敏感，如果是mysql数据库请参考 https://blog.csdn.net/ccflow/article/details/100079825 ";
                sql = "select * from aa "; //检查是否区分大小写.
                DBAccess.RunSQLReturnTable(sql);

                if (DBAccess.IsExitsObject("AAVIEW"))
                {
                    errInfo = " 当前用户没有[删除视图]的权限. ";
                    sql = "DROP VIEW AAVIEW";
                    DBAccess.RunSQL(sql);
                }

                errInfo = " 当前用户没有[创建视图]的权限.";
                sql = "CREATE VIEW AAVIEW AS SELECT * FROM AA "; //检查是否可以创建视图.
                DBAccess.RunSQL(sql);

                errInfo = " 当前用户没有[删除视图]的权限.";
                sql = "DROP VIEW AAVIEW"; //检查是否可以删除视图.
                DBAccess.RunSQL(sql);

                errInfo = " 当前用户没有[删除表]的权限.";
                sql = "DROP TABLE AA"; //检查是否可以删除表.
                DBAccess.RunSQL(sql);

                if (BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL)
                {
                    sql = " set @@global.sql_mode ='STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';";
                    DBAccess.RunSQL(sql);
                }

                if (BP.Difference.SystemConfig.AppCenterDBDatabase.Contains("-") == true)
                    throw new Exception("err@数据库名称不能包含 '-' 号，您可以使用 '_' .");




                return true;
            }
            catch (Exception ex)
            {
                if (DBAccess.IsExitsObject("AA") == true)
                {
                    sql = "DROP TABLE AA";
                    DBAccess.RunSQL(sql);
                }

                if (DBAccess.IsExitsObject("AAVIEW") == true)
                {
                    sql = "DROP VIEW AAVIEW";
                    DBAccess.RunSQL(sql);
                }

                string info = "驰骋工作流引擎 - 检查数据库安装权限出现错误:";
                info += "\t\n1. 当前登录的数据库帐号，必须有创建、删除视图或者table的权限。";
                info += "\t\n2. 必须对表有增、删、改、查的权限。 ";
                info += "\t\n3. 必须有删除创建索引主键的权限。 ";
                info += "\t\n4. 我们建议您设置当前数据库连接用户为管理员权限。 ";
                info += "\t\n ccbpm检查出来的信息如下：" + errInfo;
                info += "\t\n etc: 数据库测试异常信息:" + ex.Message;

                throw new Exception("err@" + info);
                //  return false;
            }
            return true;
        }

        /// <summary>
        /// 安装包
        /// </summary>
        /// <param name="lang">语言</param>
        /// <param name="demoType">0安装Demo, 1 不安装Demo.</param>
        public static void DoInstallDataBase(string lang, int demoType)
        {
            bool isInstallFlowDemo = true;
            if (demoType == 2)
                isInstallFlowDemo = false;

            #region 检查是否是空白的数据库。
            //if (DBAccess.IsExitsObject("WF_Emp")
            //     || DBAccess.IsExitsObject("WF_Flow")
            //     || DBAccess.IsExitsObject("Port_Emp")
            //    || DBAccess.IsExitsObject("CN_City"))
            //{
            //    throw new Exception("@当前的数据库好像是一个安装执行失败的数据库，里面包含了一些cc的表，所以您需要删除这个数据库然后执行重新安装。");
            //}
            #endregion 检查是否是空白的数据库。

            #region 首先创建Port类型的表, 让admin登录.

            FrmRB rb = new FrmRB();
            rb.CheckPhysicsTable();

            BP.Port.Emp portEmp = new BP.Port.Emp();
            portEmp.CheckPhysicsTable();

            BP.Port.Emp myemp = new BP.Port.Emp();
            myemp.CheckPhysicsTable();

            BP.Port.Dept mydept = new BP.Port.Dept();
            mydept.CheckPhysicsTable();

            Station mySta = new Station();
            mySta.CheckPhysicsTable();

            StationType myStaType = new StationType();
            myStaType.CheckPhysicsTable();

            BP.Port.DeptEmp myde = new BP.Port.DeptEmp();
            myde.CheckPhysicsTable();

            BP.Port.DeptEmpStation mydes = new BP.Port.DeptEmpStation();
            mydes.CheckPhysicsTable();


            BP.Sys.FrmRB myrb = new BP.Sys.FrmRB();
            myrb.CheckPhysicsTable();

            BP.WF.Port.WFEmp wfemp = new BP.WF.Port.WFEmp();
            wfemp.CheckPhysicsTable();

            if (DBAccess.IsExitsTableCol("WF_Emp", "StartFlows") == false)
            {
                string sql = "";
                //增加StartFlows这个字段
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.MSSQL:
                        sql = "ALTER TABLE WF_Emp ADD StartFlows Text DEFAULT  NULL";
                        break;
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        sql = "ALTER TABLE  WF_EMP add StartFlows BLOB";
                        break;
                    case DBType.MySQL:
                        sql = "ALTER TABLE WF_Emp ADD StartFlows TEXT COMMENT '可以发起的流程'";
                        break;
                    case DBType.Informix:
                        sql = "ALTER TABLE WF_Emp ADD StartFlows VARCHAR(4000) DEFAULT  NULL";
                        break;
                    case DBType.PostgreSQL:
                    case DBType.UX:
                        sql = "ALTER TABLE WF_Emp ADD StartFlows Text DEFAULT  NULL";
                        break;
                    default:
                        throw new Exception("@没有涉及到的数据库类型");
                }
                DBAccess.RunSQL(sql);
            }
            #endregion 首先创建Port类型的表.

            #region 3, 执行基本的 sql
            string sqlscript = "";

            BP.Port.Emp empGPM = new BP.Port.Emp();
            empGPM.CheckPhysicsTable();

            BP.Port.DeptEmp de = new BP.Port.DeptEmp();
            de.CheckPhysicsTable();

            DBAccess.RunSQL("ALTER TABLE Port_Emp ADD Pass NVARCHAR(90) DEFAULT '' NULL ");

            sqlscript = BP.Difference.SystemConfig.CCFlowAppPath + "WF/Data/Install/SQLScript/Port_Inc_CH_BPM.sql";
            DBAccess.RunSQLScript(sqlscript);

            BP.Port.Emp empAdmin = new Emp("admin");
            BP.Web.WebUser.SignInOfGener(empAdmin);
            #endregion 执行基本的 sql

            ArrayList al = null;
            string info = "BP.En.Entity";
            al = BP.En.ClassFactory.GetObjects(info);


            #region 先创建表，否则列的顺序就会变化.
            FlowExt fe = new FlowExt();
            fe.CheckPhysicsTable();

            NodeExt ne = new NodeExt();
            ne.CheckPhysicsTable();

            MapDtl mapdtl = new MapDtl();
            mapdtl.CheckPhysicsTable();

            MapData mapData = new MapData();
            mapData.CheckPhysicsTable();

            SysEnum sysenum = new SysEnum();
            sysenum.CheckPhysicsTable();

            CC cc = new CC();
            cc.CheckPhysicsTable();

            BP.WF.Template.NodeWorkCheck fwc = new NodeWorkCheck();
            fwc.CheckPhysicsTable();

            MapAttr attr = new MapAttr();
            attr.CheckPhysicsTable();

            GenerWorkFlow gwf = new GenerWorkFlow();
            gwf.CheckPhysicsTable();

            BP.WF.Data.CH ch = new CH();
            ch.CheckPhysicsTable();

            #endregion 先创建表，否则列的顺序就会变化.

            #region 1, 创建or修复表
            foreach (Object obj in al)
            {
                Entity en = null;
                en = obj as Entity;
                if (en == null)
                    continue;

                //获得类名.
                string clsName = en.ToString();
                if (DataType.IsNullOrEmpty(clsName) == true)
                    continue;

                if (clsName.Contains("FlowSheet") == true)
                    continue;
                if (clsName.Contains("NodeSheet") == true)
                    continue;
                if (clsName.Contains("FlowFormTree") == true)
                    continue;

                //不安装CCIM的表.
                if (clsName.Contains("BP.CCIM"))
                    continue;

                //抽象的类不允许创建表.
                switch (clsName.ToUpper())
                {
                    case "BP.WF.STARTWORK":
                    case "BP.WF.WORK":
                    case "BP.WF.GESTARTWORK":
                    case "BP.EN.GENONAME":
                    case "BP.EN.GETREE":
                    case "BP.WF.GERpt":
                    case "BP.WF.GEENTITY":
                    case "BP.SYS.TSENTITYNONAME":
                        continue;
                    default:
                        break;
                }

                if (isInstallFlowDemo == false)
                {
                    /*如果不安装demo.*/
                    if (clsName.Contains("BP.CN")
                        || clsName.Contains("BP.Demo"))
                        continue;
                }


                string table = null;
                try
                {
                    table = en.EnMap.PhysicsTable;
                    if (table == null)
                        continue;
                }
                catch
                {
                    continue;
                }

                try
                {
                    switch (table)
                    {
                        case "WF_EmpWorks":
                        case "WF_GenerEmpWorkDtls":
                        case "WF_GenerEmpWorks":
                            continue;
                        case "Sys_Enum":
                            en.CheckPhysicsTable();
                            break;
                        default:
                            en.CheckPhysicsTable();
                            break;
                    }
                    en.CheckPhysicsTable();
                }
                catch
                {
                }
            }
            #endregion 修复

            #region 2, 注册枚举类型 SQL
            // 2, 注册枚举类型。
            BP.Sys.XML.EnumInfoXmls xmls = new BP.Sys.XML.EnumInfoXmls();
            xmls.RetrieveAll();
            foreach (BP.Sys.XML.EnumInfoXml xml in xmls)
            {
                BP.Sys.SysEnums ses = new BP.Sys.SysEnums();
                int count = ses.RetrieveByAttr(SysEnumAttr.EnumKey, xml.Key);
                if (count > 0)
                    continue;
                ses.RegIt(xml.Key, xml.Vals);
            }
            #endregion 注册枚举类型

            #region 2.1 重新构建视图. //删除视图.
            if (DBAccess.IsExitsObject("WF_EmpWorks") == true)
                DBAccess.RunSQL("DROP VIEW WF_EmpWorks");

            if (DBAccess.IsExitsObject("V_WF_Delay") == true)
                DBAccess.RunSQL("DROP VIEW V_WF_Delay");

            if (DBAccess.IsExitsObject("V_FlowStarter") == true)
                DBAccess.RunSQL("DROP VIEW V_FlowStarter");

            if (DBAccess.IsExitsObject("V_FlowStarterBPM") == true)
                DBAccess.RunSQL("DROP VIEW V_FlowStarterBPM");

            if (DBAccess.IsExitsObject("V_TOTALCH") == true)
                DBAccess.RunSQL("DROP VIEW V_TOTALCH");

            if (DBAccess.IsExitsObject("V_TOTALCHYF") == true)
                DBAccess.RunSQL("DROP VIEW V_TOTALCHYF");

            if (DBAccess.IsExitsObject("V_TotalCHWeek") == true)
                DBAccess.RunSQL("DROP VIEW V_TotalCHWeek");
            #endregion 重新构建视图. //删除视图.


            #region 4, 创建视图与数据.
            //执行必须的sql.

            sqlscript = "";
            //执行必须的sql.
            switch (BP.Difference.SystemConfig.AppCenterDBType)
            {
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    sqlscript = BP.Difference.SystemConfig.CCFlowAppPath + "WF/Data/Install/SQLScript/InitView_Ora.sql";
                    break;
                case DBType.MSSQL:
                case DBType.Informix:
                    sqlscript = BP.Difference.SystemConfig.CCFlowAppPath + "WF/Data/Install/SQLScript/InitView_SQL.sql";
                    break;
                case DBType.MySQL:
                    sqlscript = BP.Difference.SystemConfig.CCFlowAppPath + "WF/Data/Install/SQLScript/InitView_MySQL.sql";
                    break;
                case DBType.PostgreSQL:
                case DBType.UX:
                    sqlscript = BP.Difference.SystemConfig.CCFlowAppPath + "WF/Data/Install/SQLScript/InitView_PostgreSQL.sql";
                    break;
                default:
                    break;
            }
            DBAccess.RunSQLScript(sqlscript, false);
            #endregion 创建视图与数据

            #region 5, 初始化数据.
            if (isInstallFlowDemo)
            {
                // sqlscript =  BP.Difference.SystemConfig.PathOfData + "Install/SQLScript/InitPublicData.sql";
                // DBAccess.RunSQLScript(sqlscript);
            }
            // else
            // {
            // FlowSort fs = new FlowSort();
            // fs.No = "1";
            // fs.ParentNo = "0";
            // fs.Name = "流程树";
            // fs.DirectInsert();

            // }
            #endregion 初始化数据

            #region 6, 生成临时的 wf_emp 数据。
            if (isInstallFlowDemo == true)
            {
                BP.Port.Emps emps = new BP.Port.Emps();
                emps.RetrieveAllFromDBSource();
                int i = 0;
                foreach (BP.Port.Emp emp in emps)
                {
                    i++;
                    BP.WF.Port.WFEmp wfEmp = new BP.WF.Port.WFEmp();
                    wfEmp.Copy(emp);
                    wfEmp.No = emp.UserID;

                    /* if (wfEmp.Email.Length == 0)
                         wfEmp.Email = emp.UserID + "@citydo.com.cn";*/

                    if (wfEmp.Tel.Length == 0)
                        wfEmp.Tel = "82374939-6" + i.ToString().PadLeft(2, '0');

                    if (wfEmp.IsExits)
                        wfEmp.Update();
                    else
                        wfEmp.Insert();
                }

                // 生成简历数据.
                foreach (BP.Port.Emp emp in emps)
                {
                    for (int myIdx = 0; myIdx < 6; myIdx++)
                    {
                        string sql = "";
                        sql = "INSERT INTO Demo_Resume (OID,RefPK,NianYue,GongZuoDanWei,ZhengMingRen,BeiZhu,QT) ";
                        sql += "VALUES(" + DBAccess.GenerOID("Demo_Resume") + ",'" + emp.UserID + "','200" + myIdx + "-01','成都.驰骋" + myIdx + "公司','张三','表现良好','其他-" + myIdx + "无')";
                        DBAccess.RunSQL(sql);
                    }
                }

                DataTable dtStudent = DBAccess.RunSQLReturnTable("SELECT No FROM Demo_Student");
                foreach (DataRow dr in dtStudent.Rows)
                {
                    string no = dr[0].ToString();
                    for (int myIdx = 0; myIdx < 6; myIdx++)
                    {
                        string sql = "";
                        sql = "INSERT INTO Demo_Resume (OID,RefPK,NianYue,GongZuoDanWei,ZhengMingRen,BeiZhu,QT) ";
                        sql += "VALUES(" + DBAccess.GenerOID("Demo_Resume") + ",'" + no + "','200" + myIdx + "-01','成都.驰骋" + myIdx + "公司','张三','表现良好','其他-" + myIdx + "无')";
                        DBAccess.RunSQL(sql);
                    }
                }
                GenerWorkFlowViewNY ny = new GenerWorkFlowViewNY();
                ny.CheckPhysicsTable();
                // 生成年度月份数据.
                string sqls = "";
                DateTime dtNow = DateTime.Now;
                for (int num = 0; num < 12; num++)
                {
                    sqls = "@ INSERT INTO Pub_NY (No,Name) VALUES ('" + dtNow.ToString("yyyy-MM") + "','" + dtNow.ToString("yyyy-MM") + "')  ";
                    dtNow = dtNow.AddMonths(1);
                }
                DBAccess.RunSQLs(sqls);
            }
            #endregion 初始化数据

            #region 7, 装载 demo.flow
            if (isInstallFlowDemo == true)
            {
                BP.Port.Emp emp = new BP.Port.Emp("admin");
                BP.Web.WebUser.SignInOfGener(emp);
                BP.Sys.Base.Glo.WriteLineInfo("开始装载模板...");
                string msg = "";

                //装载数据模版.
                BP.WF.DTS.LoadTemplete l = new BP.WF.DTS.LoadTemplete();
                msg = l.Do() as string;


                BP.Sys.Base.Glo.WriteLineInfo("装载模板完成。开始修复视图...");


                BP.Sys.Base.Glo.WriteLineInfo("视图修复完成。");
            }

            if (isInstallFlowDemo == false)
            {
                //创建一个空白的流程，不然，整个结构就出问题。
                //    FlowSorts fss = new FlowSorts();
                // fss.RetrieveAll();
                //  fss.Delete();
                DBAccess.RunSQL("DELETE FROM WF_FlowSort ");
                FlowSort fs = new FlowSort();
                fs.Name = "流程树";
                fs.No = "1";
                fs.ParentNo = "0";
                fs.Insert();

                fs = new FlowSort();
                fs.No = "101";
                fs.ParentNo = "1";
                fs.Name = "日常办公类";
                fs.Insert();

                //加载一个模版,不然用户不知道如何新建流程.
                BP.WF.Template.TemplateGlo.LoadFlowTemplate(fs.No, BP.Difference.SystemConfig.PathOfData + "Install/QingJiaFlowDemoInit.xml",
                    ImpFlowTempleteModel.AsTempleteFlowNo);

                Flow fl = new Flow("001");
                fl.DoCheck(); //做一下检查.

                fs.No = "102";
                fs.ParentNo = "1";
                fs.Name = "财务类";
                fs.Insert();

                fs.No = "103";
                fs.ParentNo = "1";
                fs.Name = "人力资源类";
                fs.Insert();


                //创建一个空白的流程，不然，整个结构就出问题。
                //BP.Sys.FrmTrees frmTrees = new FrmTrees();
                //frmTrees.RetrieveAll();
                //frmTrees.Delete();

                DBAccess.RunSQL("DELETE FROM Sys_FormTree ");
                SysFormTree ftree = new SysFormTree();
                ftree.Name = "表单树";
                ftree.No = "1";
                ftree.ParentNo = "0";
                ftree.Insert();

                SysFormTree subFrmTree = new SysFormTree();
                subFrmTree.Name = "流程独立表单";
                subFrmTree.ParentNo = "1";
                subFrmTree.No = "101";
                subFrmTree.Insert();

                subFrmTree = new SysFormTree(); ;
                subFrmTree.No = "102";
                subFrmTree.Name = "常用信息管理";
                subFrmTree.ParentNo = "1";
                subFrmTree.Insert();

                subFrmTree = new SysFormTree(); ;
                subFrmTree.Name = "常用单据";
                subFrmTree.No = "103";
                subFrmTree.ParentNo = "1";
                subFrmTree.Insert();
            }
            #endregion 装载demo.flow

            #region 增加图片签名
            if (isInstallFlowDemo == true)
            {
                try
                {
                    //增加图片签名
                    BP.WF.DTS.GenerSiganture gs = new BP.WF.DTS.GenerSiganture();
                    gs.Do();
                }
                catch
                {
                }
            }
            #endregion 增加图片签名.

            //生成拼音，以方便关键字查找.
            BP.WF.DTS.GenerPinYinForEmp pinyin = new BP.WF.DTS.GenerPinYinForEmp();
            pinyin.Do();

            #region 执行补充的sql, 让外键的字段长度都设置成100.
            DBAccess.RunSQL("UPDATE Sys_MapAttr SET maxlen=100 WHERE LGType=2 AND MaxLen<100");
            DBAccess.RunSQL("UPDATE Sys_MapAttr SET maxlen=100 WHERE KeyOfEn='FK_Dept'");

            //Nodes nds = new Nodes();
            //nds.RetrieveAll();
            //foreach (Node nd in nds)
            //    nd.HisWork.CheckPhysicsTable();
            #endregion 执行补充的sql, 让外键的字段长度都设置成100.

            #region 如果是第一次运行，就执行检查。
            if (isInstallFlowDemo == true)
            {
                Flows fls = new Flows();
                fls.RetrieveAllFromDBSource();
                foreach (Flow fl in fls)
                {
                    try
                    {
                        fl.DoCheck();
                    }
                    catch (Exception ex)
                    {
                        BP.DA.Log.DebugWriteError(ex.Message);
                    }
                }
            }
            #endregion 如果是第一次运行，就执行检查。

            #region 增加大文本字段列.
            try
            {
                DBAccess.GetBigTextFromDB("Sys_MapData", "No", "001", "HtmlTemplateFile");
            }
            catch
            {
            }
            #endregion 增加大文本字段列.
        }
        /// <summary>
        /// 检查树结构是否符合需求
        /// </summary>
        /// <returns></returns>
        public static bool CheckTreeRoot()
        {
            // 流程树根节点校验
            string tmp = "SELECT Name FROM WF_FlowSort WHERE ParentNo='0'";
            tmp = DBAccess.RunSQLReturnString(tmp);
            if (string.IsNullOrEmpty(tmp))
            {
                tmp = "INSERT INTO WF_FlowSort(No,Name,ParentNo,TreeNo,idx,IsDir) values('1','流程树',0,'',0,0)";
                DBAccess.RunSQLReturnString(tmp);
            }

            // 表单树根节点校验
            tmp = "SELECT Name FROM Sys_FormTree WHERE ParentNo = '0' ";
            tmp = DBAccess.RunSQLReturnString(tmp);
            if (string.IsNullOrEmpty(tmp))
            {
                tmp = "INSERT INTO Sys_FormTree(No,Name,ParentNo,Idx,IsDir) values('1','表单树',0,0,0)";
                DBAccess.RunSQLReturnString(tmp);
            }

            //检查组织结构是否正确.
            string sql = "SELECT * FROM Port_Dept WHERE ParentNo='0' ";
            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            if (dt.Rows.Count == 0)
            {
                BP.Port.Dept rootDept = new BP.Port.Dept();
                rootDept.Name = "总部";
                rootDept.ParentNo = "0";
                try
                {
                    rootDept.Insert();
                }
                catch (Exception ex)
                {
                    BP.DA.Log.DebugWriteError("@尝试向port_dept插入数据失败，应该是视图问题. 技术信息:" + ex.Message);
                }
                throw new Exception("@没有找到部门树为0个根节点, 有可能是因为您在集成cc的时候，没有遵守cc的规则，部门树的根节点必须是ParentNo=0。");
            }
            return true;
        }

        public static void KillProcess(string processName) //杀掉进程的方法
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process pro in processes)
            {
                string name = pro.ProcessName + ".exe";
                if (name.ToLower() == processName.ToLower())
                    pro.Kill();
            }
        }
        /// <summary>
        /// 产生新的编号
        /// </summary>
        /// <param name="rptKey"></param>
        /// <returns></returns>
        public static string GenerFlowNo(string rptKey)
        {
            rptKey = rptKey.Replace("ND", "");
            rptKey = rptKey.Replace("Rpt", "");
            switch (rptKey.Length)
            {
                case 0:
                    return "001";
                case 1:
                    return "00" + rptKey;
                case 2:
                    return "0" + rptKey;
                case 3:
                    return rptKey;
                default:
                    return "001";
            }
            return rptKey;
        }
        /// <summary>
        /// 
        /// </summary>
        public static bool IsShowFlowNum
        {
            get
            {
                switch (BP.Difference.SystemConfig.AppSettings["IsShowFlowNum"])
                {
                    case "1":
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// 产生word文档.
        /// </summary>
        /// <param name="wk"></param>
        public static void GenerWord(object filename, Work wk)
        {
            BP.WF.Glo.KillProcess("WINWORD.EXE");
            string enName = wk.EnMap.PhysicsTable;
            try
            {
                RegistryKey delKey = Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Shared Tools\Text Converters\Import\",
                    true);
                delKey.DeleteValue("MSWord6.wpc");
                delKey.Close();
            }
            catch
            {
            }

            GroupField currGF = new GroupField();
            MapAttrs mattrs = new MapAttrs(enName);
            GroupFields gfs = new GroupFields(enName);
            MapDtls dtls = new MapDtls(enName);
            foreach (MapDtl dtl in dtls)
                dtl.IsUse = false;

            // 计算出来单元格的行数。
            int rowNum = 0;
            foreach (GroupField gf in gfs)
            {
                rowNum++;
                bool isLeft = true;
                foreach (MapAttr attr in mattrs)
                {
                    if (attr.UIVisible == false)
                        continue;

                    if (attr.GroupID != gf.OID)
                        continue;

                    if (attr.UIIsLine)
                    {
                        rowNum++;
                        isLeft = true;
                        continue;
                    }

                    if (isLeft == false)
                        rowNum++;
                    isLeft = !isLeft;
                }
            }

            rowNum = rowNum + 2 + dtls.Count;

            // 创建Word文档
            string CheckedInfo = "";
            string message = "";
            Object Nothing = System.Reflection.Missing.Value;

            #region 没用代码
            //  object filename = fileName;

            //Word.Application WordApp = new Word.ApplicationClass();
            //Word.Document WordDoc = WordApp.Documents.Add(ref  Nothing, ref  Nothing, ref  Nothing, ref  Nothing);
            //try
            //{
            //    WordApp.ActiveWindow.View.Type = Word.WdViewType.wdOutlineView;
            //    WordApp.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekPrimaryHeader;

            //    #region 增加页眉
            //    // 添加页眉 插入图片
            //    string pict =  BP.Difference.SystemConfig.PathOfDataUser + "log.jpg"; // 图片所在路径
            //    if (System.IO.File.Exists(pict))
            //    {
            //        System.Drawing.Image img = System.Drawing.Image.FromFile(pict);
            //        object LinkToFile = false;
            //        object SaveWithDocument = true;
            //        object Anchor = WordDoc.Application.Selection.Range;
            //        WordDoc.Application.ActiveDocument.InlineShapes.AddPicture(pict, ref  LinkToFile,
            //            ref  SaveWithDocument, ref  Anchor);
            //        //    WordDoc.Application.ActiveDocument.InlineShapes[1].Width = img.Width; // 图片宽度
            //        //    WordDoc.Application.ActiveDocument.InlineShapes[1].Height = img.Height; // 图片高度
            //    }
            //    WordApp.ActiveWindow.ActivePane.Selection.InsertAfter("[驰骋业务流程管理系统 http://citydo.com.cn]");
            //    WordApp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft; // 设置右对齐
            //    WordApp.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekMainDocument; // 跳出页眉设置
            //    WordApp.Selection.ParagraphFormat.LineSpacing = 15f; // 设置文档的行间距
            //    #endregion

            //    // 移动焦点并换行
            //    object count = 14;
            //    object WdLine = Word.WdUnits.wdLine; // 换一行;
            //    WordApp.Selection.MoveDown(ref  WdLine, ref  count, ref  Nothing); // 移动焦点
            //    WordApp.Selection.TypeParagraph(); // 插入段落

            //    // 文档中创建表格
            //    Word.Table newTable = WordDoc.Tables.Add(WordApp.Selection.Range, rowNum, 4, ref  Nothing, ref  Nothing);

            //    // 设置表格样式
            //    newTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleThickThinLargeGap;
            //    newTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

            //    newTable.Columns[1].Width = 100f;
            //    newTable.Columns[2].Width = 100f;
            //    newTable.Columns[3].Width = 100f;
            //    newTable.Columns[4].Width = 100f;

            //    // 填充表格内容
            //    newTable.Cell(1, 1).Range.Text = wk.EnDesc;
            //    newTable.Cell(1, 1).Range.Bold = 2; // 设置单元格中字体为粗体

            //    // 合并单元格
            //    newTable.Cell(1, 1).Merge(newTable.Cell(1, 4));
            //    WordApp.Selection.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter; // 垂直居中
            //    WordApp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter; // 水平居中

            //    int groupIdx = 1;
            //    foreach (GroupField gf in gfs)
            //    {
            //        groupIdx++;
            //        // 填充表格内容
            //        newTable.Cell(groupIdx, 1).Range.Text = gf.Lab;
            //        newTable.Cell(groupIdx, 1).Range.Font.Color = Word.WdColor.wdColorDarkBlue; // 设置单元格内字体颜色
            //        newTable.Cell(groupIdx, 1).Shading.BackgroundPatternColor = Word.WdColor.wdColorGray25;
            //        // 合并单元格
            //        newTable.Cell(groupIdx, 1).Merge(newTable.Cell(groupIdx, 4));
            //        WordApp.Selection.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

            //        groupIdx++;

            //        bool isLeft = true;
            //        bool isColumns2 = false;
            //        int currColumnIndex = 0;
            //        foreach (MapAttr attr in mattrs)
            //        {
            //            if (attr.UIVisible == false)
            //                continue;

            //            if (attr.GroupID != gf.OID)
            //                continue;

            //            if (newTable.Rows.Count < groupIdx)
            //                continue;

            //            #region 增加从表
            //            foreach (MapDtl dtl in dtls)
            //            {
            //                if (dtl.IsUse)
            //                    continue;

            //                if (dtl.RowIdx != groupIdx - 3)
            //                    continue;

            //                if (gf.OID != dtl.GroupID)
            //                    continue;

            //                GEDtls dtlsDB = new GEDtls(dtl.No);
            //                QueryObject qo = new QueryObject(dtlsDB);
            //                switch (dtl.DtlOpenType)
            //                {
            //                    case DtlOpenType.ForEmp:
            //                        qo.AddWhere(GEDtlAttr.RefPK, wk.OID);
            //                        break;
            //                    case DtlOpenType.ForWorkID:
            //                        qo.AddWhere(GEDtlAttr.RefPK, wk.OID);
            //                        break;
            //                    case DtlOpenType.ForFID:
            //                        qo.AddWhere(GEDtlAttr.FID, wk.OID);
            //                        break;
            //                }
            //                qo.DoQuery();

            //                newTable.Rows[groupIdx].SetHeight(100f, Word.WdRowHeightRule.wdRowHeightAtLeast);
            //                newTable.Cell(groupIdx, 1).Merge(newTable.Cell(groupIdx, 4));

            //                Attrs dtlAttrs = dtl.GenerMap().Attrs;
            //                int colNum = 0;
            //                foreach (Attr attrDtl in dtlAttrs)
            //                {
            //                    if (attrDtl.UIVisible == false)
            //                        continue;
            //                    colNum++;
            //                }

            //                newTable.Cell(groupIdx, 1).Select();
            //                WordApp.Selection.Delete(ref Nothing, ref Nothing);
            //                Word.Table newTableDtl = WordDoc.Tables.Add(WordApp.Selection.Range, dtlsDB.Count + 1, colNum,
            //                    ref Nothing, ref Nothing);

            //                newTableDtl.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            //                newTableDtl.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

            //                int colIdx = 1;
            //                foreach (Attr attrDtl in dtlAttrs)
            //                {
            //                    if (attrDtl.UIVisible == false)
            //                        continue;
            //                    newTableDtl.Cell(1, colIdx).Range.Text = attrDtl.Desc;
            //                    colIdx++;
            //                }

            //                int idxRow = 1;
            //                foreach (GEDtl item in dtlsDB)
            //                {
            //                    idxRow++;
            //                    int columIdx = 0;
            //                    foreach (Attr attrDtl in dtlAttrs)
            //                    {
            //                        if (attrDtl.UIVisible == false)
            //                            continue;
            //                        columIdx++;

            //                        if (attrDtl.IsFKorEnum)
            //                            newTableDtl.Cell(idxRow, columIdx).Range.Text = item.GetValRefTextByKey(attrDtl.Key);
            //                        else
            //                        {
            //                            if (attrDtl.MyDataType == DataType.AppMoney)
            //                                newTableDtl.Cell(idxRow, columIdx).Range.Text = item.GetValMoneyByKey(attrDtl.Key).ToString("0.00");
            //                            else
            //                                newTableDtl.Cell(idxRow, columIdx).Range.Text = item.GetValStrByKey(attrDtl.Key);

            //                            if (attrDtl.IsNum)
            //                                newTableDtl.Cell(idxRow, columIdx).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;
            //                        }
            //                    }
            //                }

            //                groupIdx++;
            //                isLeft = true;
            //            }
            //            #endregion 增加从表

            //            if (attr.UIIsLine)
            //            {
            //                currColumnIndex = 0;
            //                isLeft = true;
            //                if (attr.IsBigDoc)
            //                {
            //                    newTable.Rows[groupIdx].SetHeight(100f, Word.WdRowHeightRule.wdRowHeightAtLeast);
            //                    newTable.Cell(groupIdx, 1).Merge(newTable.Cell(groupIdx, 4));
            //                    newTable.Cell(groupIdx, 1).Range.Text = attr.Name + ":\r\n" + wk.GetValStrByKey(attr.KeyOfEn);
            //                }
            //                else
            //                {
            //                    newTable.Cell(groupIdx, 2).Merge(newTable.Cell(groupIdx, 4));
            //                    newTable.Cell(groupIdx, 1).Range.Text = attr.Name;
            //                    newTable.Cell(groupIdx, 2).Range.Text = wk.GetValStrByKey(attr.KeyOfEn);
            //                }
            //                groupIdx++;
            //                continue;
            //            }
            //            else
            //            {
            //                if (attr.IsBigDoc)
            //                {
            //                    if (currColumnIndex == 2)
            //                    {
            //                        currColumnIndex = 0;
            //                    }

            //                    newTable.Rows[groupIdx].SetHeight(100f, Word.WdRowHeightRule.wdRowHeightAtLeast);
            //                    if (currColumnIndex == 0)
            //                    {
            //                        newTable.Cell(groupIdx, 1).Merge(newTable.Cell(groupIdx, 2));
            //                        newTable.Cell(groupIdx, 1).Range.Text = attr.Name + ":\r\n" + wk.GetValStrByKey(attr.KeyOfEn);
            //                        currColumnIndex = 3;
            //                        continue;
            //                    }
            //                    else if (currColumnIndex == 3)
            //                    {
            //                        newTable.Cell(groupIdx, 2).Merge(newTable.Cell(groupIdx, 3));
            //                        newTable.Cell(groupIdx, 2).Range.Text = attr.Name + ":\r\n" + wk.GetValStrByKey(attr.KeyOfEn);
            //                        currColumnIndex = 0;
            //                        groupIdx++;
            //                        continue;
            //                    }
            //                    else
            //                    {
            //                        continue;
            //                    }
            //                }
            //                else
            //                {
            //                    string s = "";
            //                    if (attr.LGType == FieldTypeS.Normal)
            //                    {
            //                        if (attr.MyDataType == DataType.AppMoney)
            //                            s = wk.GetValDecimalByKey(attr.KeyOfEn).ToString("0.00");
            //                        else
            //                            s = wk.GetValStrByKey(attr.KeyOfEn);
            //                    }
            //                    else
            //                    {
            //                        s = wk.GetValRefTextByKey(attr.KeyOfEn);
            //                    }

            //                    switch (currColumnIndex)
            //                    {
            //                        case 0:
            //                            newTable.Cell(groupIdx, 1).Range.Text = attr.Name;
            //                            if (attr.IsSigan)
            //                            {
            //                                string path =  BP.Difference.SystemConfig.PathOfDataUser + "/Siganture/" + s + ".jpg";
            //                                if (System.IO.File.Exists(path))
            //                                {
            //                                    System.Drawing.Image img = System.Drawing.Image.FromFile(path);
            //                                    object LinkToFile = false;
            //                                    object SaveWithDocument = true;
            //                                    //object Anchor = WordDoc.Application.Selection.Range;
            //                                    object Anchor = newTable.Cell(groupIdx, 2).Range;

            //                                    WordDoc.Application.ActiveDocument.InlineShapes.AddPicture(path, ref  LinkToFile,
            //                                        ref  SaveWithDocument, ref  Anchor);
            //                                    //    WordDoc.Application.ActiveDocument.InlineShapes[1].Width = img.Width; // 图片宽度
            //                                    //    WordDoc.Application.ActiveDocument.InlineShapes[1].Height = img.Height; // 图片高度
            //                                }
            //                                else
            //                                {
            //                                    newTable.Cell(groupIdx, 2).Range.Text = s;
            //                                }
            //                            }
            //                            else
            //                            {
            //                                if (attr.IsNum)
            //                                {
            //                                    newTable.Cell(groupIdx, 2).Range.Text = s;
            //                                    newTable.Cell(groupIdx, 2).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;
            //                                }
            //                                else
            //                                {
            //                                    newTable.Cell(groupIdx, 2).Range.Text = s;
            //                                }
            //                            }
            //                            currColumnIndex = 1;
            //                            continue;
            //                            break;
            //                        case 1:
            //                            newTable.Cell(groupIdx, 3).Range.Text = attr.Name;
            //                            if (attr.IsSigan)
            //                            {
            //                                string path =  BP.Difference.SystemConfig.PathOfDataUser + "/Siganture/" + s + ".jpg";
            //                                if (System.IO.File.Exists(path))
            //                                {
            //                                    System.Drawing.Image img = System.Drawing.Image.FromFile(path);
            //                                    object LinkToFile = false;
            //                                    object SaveWithDocument = true;
            //                                    object Anchor = newTable.Cell(groupIdx, 4).Range;
            //                                    WordDoc.Application.ActiveDocument.InlineShapes.AddPicture(path, ref  LinkToFile,
            //                                        ref  SaveWithDocument, ref  Anchor);
            //                                }
            //                                else
            //                                {
            //                                    newTable.Cell(groupIdx, 4).Range.Text = s;
            //                                }
            //                            }
            //                            else
            //                            {
            //                                if (attr.IsNum)
            //                                {
            //                                    newTable.Cell(groupIdx, 4).Range.Text = s;
            //                                    newTable.Cell(groupIdx, 4).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;
            //                                }
            //                                else
            //                                {
            //                                    newTable.Cell(groupIdx, 4).Range.Text = s;
            //                                }
            //                            }
            //                            currColumnIndex = 0;
            //                            groupIdx++;
            //                            continue;
            //                            break;
            //                        default:
            //                            break;
            //                    }
            //                }
            //            }
            //        }
            //    }  //结束循环

            //    #region 添加页脚
            //    WordApp.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekPrimaryFooter;
            //    WordApp.ActiveWindow.ActivePane.Selection.InsertAfter("模板由ccflow自动生成，严谨转载。此流程的详细内容请访问 http://doc.citydo.com.cn。 建造流程管理系统请致电: 0531-82374939  ");
            //    WordApp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            //    #endregion

            //    // 文件保存
            //    WordDoc.SaveAs(ref  filename, ref  Nothing, ref  Nothing, ref  Nothing,
            //        ref  Nothing, ref  Nothing, ref  Nothing, ref  Nothing,
            //        ref  Nothing, ref  Nothing, ref  Nothing, ref  Nothing, ref  Nothing,
            //        ref  Nothing, ref  Nothing, ref  Nothing);

            //    WordDoc.Close(ref  Nothing, ref  Nothing, ref  Nothing);
            //    WordApp.Quit(ref  Nothing, ref  Nothing, ref  Nothing);
            //    try
            //    {
            //        string docFile = filename.ToString();
            //        string pdfFile = docFile.Replace(".doc", ".pdf");
            //        Glo.Rtf2PDF(docFile, pdfFile);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.DebugWriteInfo("@生成pdf失败." + ex.Message);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //    // WordApp.Quit(ref  Nothing, ref  Nothing, ref  Nothing);
            //    WordDoc.Close(ref  Nothing, ref  Nothing, ref  Nothing);
            //    WordApp.Quit(ref  Nothing, ref  Nothing, ref  Nothing);
            //}
            #endregion
        }
        #endregion 执行安装.

        #region 流程模版的ftp服务器配置.
        public static string TemplateFTPHost
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("TemplateFTPHost", "116.239.32.14");
            }
        }
        public static int TemplateFTPPort
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyInt("TemplateFTPPort", 9997);
            }
        }
        public static string TemplateFTPUser
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("TemplateFTPUser", "oa");
            }
        }
        public static string TemplateFTPPassword
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("TemplateFTPPassword", "Jszx1234");
            }
        }
        #endregion 流程模版的ftp服务器配置.


        #region 全局的方法处理
        /// <summary>
        /// 流程数据表系统字段,中间用,分开.
        /// </summary>
        public static string FlowFields
        {
            get
            {
                string str = "";
                str += GERptAttr.OID + ",";
                str += GERptAttr.AtPara + ",";
                str += GERptAttr.BillNo + ",";
                //  str += GERptAttr.CFlowNo + ",";
                //  str += GERptAttr.CWorkID + ",";
                str += GERptAttr.FID + ",";
                str += GERptAttr.FK_Dept + ",";
                str += GERptAttr.FK_DeptName + ",";
                str += GERptAttr.FK_NY + ",";
                str += GERptAttr.FlowDaySpan + ",";
                str += GERptAttr.FlowEmps + ",";
                str += GERptAttr.FlowEnder + ",";
                str += GERptAttr.FlowEnderRDT + ",";
                str += GERptAttr.FlowEndNode + ",";
                str += GERptAttr.FlowStarter + ",";
                str += GERptAttr.FlowStartRDT + ",";
                str += GERptAttr.GuestName + ",";
                str += GERptAttr.GuestNo + ",";
                str += GERptAttr.GUID + ",";
                str += GERptAttr.PEmp + ",";
                str += GERptAttr.PFID + ",";
                str += GERptAttr.PFlowNo + ",";
                str += GERptAttr.PNodeID + ",";
                str += GERptAttr.PrjName + ",";
                str += GERptAttr.PrjNo + ",";
                str += GERptAttr.PWorkID + ",";
                str += GERptAttr.Title + ",";
                str += GERptAttr.WFSta + ",";
                str += GERptAttr.WFState + ",";
                str += "Rec,";
                str += "CDT,RDT,WFStateText";
                return str;
                // return typeof(GERptAttr).GetFields().Select(o => o.Name).ToList();
            }
        }



        #region 与流程事件实体相关.
        private static Hashtable Htable_FlowFEE = null;
        /// <summary>
        /// 获得节点事件实体
        /// </summary>
        /// <param name="enName">实例名称</param>
        /// <returns>获得节点事件实体,如果没有就返回为空.</returns>
        public static FlowEventBase GetFlowEventEntityByEnName(string enName)
        {
            if (Htable_FlowFEE == null || Htable_FlowFEE.Count == 0)
            {
                Htable_FlowFEE = new Hashtable();

                string name = "BP.WF.FlowEventBase";

                ArrayList al = BP.En.ClassFactory.GetObjects(name);
                foreach (FlowEventBase en in al)
                {
                    if (Htable_FlowFEE.ContainsKey(en.ToString()) == true)
                        continue;
                    Htable_FlowFEE.Add(en.ToString(), en);
                }
            }
            FlowEventBase myen = Htable_FlowFEE[enName] as FlowEventBase;
            if (myen == null)
            {
                //throw new Exception("@根据类名称获取流程事件实体实例出现错误:" + enName + ",没有找到该类的实体.");
                BP.DA.Log.DebugWriteError("@根据类名称获取流程事件实体实例出现错误:" + enName + ",没有找到该类的实体.");
                return null;
            }
            return myen;
        }
        /// <summary>
        /// 获得事件实体String，根据编号或者流程标记
        /// </summary>
        /// <param name="flowMark">流程标记</param>
        /// <param name="flowNo">流程编号</param>
        /// <returns>null, 或者流程实体.</returns>
        public static string GetFlowEventEntityStringByFlowMark(string flowMark)
        {
            FlowEventBase en = GetFlowEventEntityByFlowMark(flowMark);
            if (en == null)
                return "";
            return en.ToString();
        }
        /// <summary>
        /// 获得事件实体，根据编号或者流程标记.
        /// </summary>
        /// <param name="flowMark">流程标记</param>
        /// <param name="flowNo">流程编号</param>
        /// <returns>null, 或者流程实体.</returns>
        public static FlowEventBase GetFlowEventEntityByFlowMark(string flowMark)
        {

            if (Htable_FlowFEE == null || Htable_FlowFEE.Count == 0)
            {
                Htable_FlowFEE = new Hashtable();

                string name = "";
                name = "BP.WF.FlowEventBase";

                ArrayList al = BP.En.ClassFactory.GetObjects(name);
                Htable_FlowFEE.Clear();
                foreach (FlowEventBase en in al)
                {
                    if (Htable_FlowFEE.Contains(en.ToString()) == true)
                        continue;
                    Htable_FlowFEE.Add(en.ToString(), en);
                }
            }

            foreach (string key in Htable_FlowFEE.Keys)
            {
                FlowEventBase fee = Htable_FlowFEE[key] as FlowEventBase;

                string mark = "," + fee.FlowMark + ",";
                if (mark.Contains("," + flowMark + ",") == true)
                    return fee;
            }
            return null;
        }
        #endregion 与流程事件实体相关.

        /// <summary>
        /// 执行发送工作后处理的业务逻辑
        /// 用于流程发送后事件调用.
        /// 如果处理失败，就会抛出异常.
        /// </summary>
        public static void DealBuinessAfterSendWork(string fk_flow, Int64 workid,
            string doFunc, string WorkIDs)
        {
            if (doFunc == "SetParentFlow")
            {
                /* 如果需要设置子父流程信息.
                 * 应用于合并审批,当多个子流程合并审批,审批后发起一个父流程.
                 */

                GenerWorkFlow gwfParent = new GenerWorkFlow(workid);

                string[] workids = WorkIDs.Split(',');
                string okworkids = ""; //成功发送后的workids.
                GenerWorkFlow gwfSubFlow = new GenerWorkFlow();
                foreach (string id in workids)
                {
                    if (string.IsNullOrEmpty(id))
                        continue;

                    // 把数据copy到里面,让子流程也可以得到父流程的数据。
                    Int64 workidC = Int64.Parse(id);
                    gwfSubFlow.WorkID = workidC;
                    gwfSubFlow.RetrieveFromDBSources();

                    //设置当前流程的ID
                    BP.WF.Dev2Interface.SetParentInfo(gwfSubFlow.FK_Flow, workidC, gwfParent.WorkID);

                    // 是否可以执行？
                    if (BP.WF.Dev2Interface.Flow_IsCanDoCurrentWork(workidC, WebUser.No) == true)
                    {
                        //执行向下发送.
                        try
                        {
                            BP.WF.Dev2Interface.Node_SendWork(gwfSubFlow.FK_Flow, workidC);
                            okworkids += workidC;
                        }
                        catch (Exception ex)
                        {
                            #region 如果有一个发送失败，就撤销子流程与父流程.
                            //首先把主流程撤销发送.
                            BP.WF.Dev2Interface.Flow_DoUnSend(fk_flow, workid);

                            //把已经发送成功的子流程撤销发送.
                            string[] myokwokid = okworkids.Split(',');
                            foreach (string okwokid in myokwokid)
                            {
                                if (string.IsNullOrEmpty(id))
                                    continue;

                                // 把数据copy到里面,让子流程也可以得到父流程的数据。
                                workidC = Int64.Parse(id);
                                BP.WF.Dev2Interface.Flow_DoUnSend(gwfSubFlow.FK_Flow, gwfSubFlow.WorkID);
                            }
                            #endregion 如果有一个发送失败，就撤销子流程与父流程.
                            throw new Exception("@在执行子流程(" + gwfSubFlow.Title + ")发送时出现如下错误:" + ex.Message);
                        }
                    }
                }
            }

        }
        #endregion 全局的方法处理

        #region web.config 属性.
        /// <summary>
        /// 是否admin
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["adminers"];
                if (string.IsNullOrEmpty(s))
                    s = "admin,";
                return s.Contains(BP.Web.WebUser.No);
            }
        }
        public static bool IsEnableTrackRec
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["IsEnableTrackRec"];
                if (string.IsNullOrEmpty(s))
                    return false;
                if (s == "0")
                    return false;

                return true;
            }
        }
        /// <summary>
        /// 获取mapdata字段查询Like。
        /// </summary>
        /// <param name="flowNo">流程编号</param>
        /// <param name="colName">列编号</param>
        public static string MapDataLikeKeyV1(string flowNo, string colName)
        {
            flowNo = int.Parse(flowNo).ToString();
            string len = BP.Difference.SystemConfig.AppCenterDBLengthStr;
            if (flowNo.Length == 1)
                return " " + colName + " LIKE 'ND" + flowNo + "%' AND " + len + "(" + colName + ")=5";
            if (flowNo.Length == 2)
                return " " + colName + " LIKE 'ND" + flowNo + "%' AND " + len + "(" + colName + ")=6";
            if (flowNo.Length == 3)
                return " " + colName + " LIKE 'ND" + flowNo + "%' AND " + len + "(" + colName + ")=7";

            return " " + colName + " LIKE 'ND" + flowNo + "%' AND " + len + "(" + colName + ")=8";
        }
        public static string MapDataLikeKey(string flowNo, string colName)
        {
            flowNo = int.Parse(flowNo).ToString();
            string len = BP.Difference.SystemConfig.AppCenterDBLengthStr;

            //edited by liuxc,2016-02-22,合并逻辑，原来分流程编号的位数，现在统一处理
            return " (" + colName + " LIKE 'ND" + flowNo + "%' AND " + len + "(" + colName + ")=" +
                   ("ND".Length + flowNo.Length + 2) + ") OR (" + colName +
                   " = 'ND" + flowNo + "Rpt' ) OR (" + colName + " LIKE 'ND" + flowNo + "__Dtl%' AND " + len + "(" +
                   colName + ")>" + ("ND".Length + flowNo.Length + 2 + "Dtl".Length) + ")";
        }
        /// <summary>
        /// 短信时间发送从
        /// 默认从 8 点开始.
        /// </summary>
        public static int SMSSendTimeFromHour
        {
            get
            {
                try
                {
                    return int.Parse(BP.Difference.SystemConfig.AppSettings["SMSSendTimeFromHour"]);
                }
                catch
                {
                    return 8;
                }
            }
        }


        /// <summary>
        /// 短信时间发送到
        /// 默认到 20 点结束.
        /// </summary>
        public static int SMSSendTimeToHour
        {
            get
            {
                try
                {
                    return int.Parse(BP.Difference.SystemConfig.AppSettings["SMSSendTimeToHour"]);
                }
                catch
                {
                    return 8;
                }
            }
        }
        #endregion webconfig属性.

        #region 常用方法
        private static string html = "";
        private static ArrayList htmlArr = new ArrayList();
        private static string backHtml = "";
        private static Int64 workid = 0;
        /// <summary>
        /// 模拟运行
        /// </summary>
        /// <param name="flowNo">流程编号</param>
        /// <param name="empNo">要执行的人员.</param>
        /// <returns>执行信息.</returns>
        public static string Simulation_RunOne(string flowNo, string empNo, string paras)
        {
            backHtml = "";//需要重新赋空值
            Hashtable ht = null;
            if (string.IsNullOrEmpty(paras) == false)
            {
                AtPara ap = new AtPara(paras);
                ht = ap.HisHT;
            }

            Emp emp = new Emp(empNo);
            backHtml += " **** 开始使用:" + Glo.GenerUserImgSmallerHtml(emp.UserID, emp.Name) + "登录模拟执行工作流程";
            BP.WF.Dev2Interface.Port_Login(empNo);

            workid = BP.WF.Dev2Interface.Node_CreateBlankWork(flowNo, ht, null, emp.UserID, null);
            SendReturnObjs objs = BP.WF.Dev2Interface.Node_SendWork(flowNo, workid, ht);
            backHtml += objs.ToMsgOfHtml().Replace("@", "<br>@");  //记录消息.


            string[] accepters = objs.VarAcceptersID.Split(',');


            foreach (string acce in accepters)
            {
                if (string.IsNullOrEmpty(acce) == true)
                    continue;

                // 执行发送.
                Simulation_Run_S1(flowNo, workid, acce, ht, empNo);
                break;
            }
            //return html;
            //return htmlArr;
            return backHtml;
        }
        private static bool isAdd = true;
        private static void Simulation_Run_S1(string flowNo, Int64 workid, string empNo, Hashtable ht, string beginEmp)
        {
            //htmlArr.Add(html);
            Emp emp = new Emp(empNo);
            //html = "";
            backHtml += "empNo" + beginEmp;
            backHtml += "<br> **** 让:" + Glo.GenerUserImgSmallerHtml(emp.UserID, emp.Name) + "执行模拟登录. ";
            // 让其登录.
            BP.WF.Dev2Interface.Port_Login(empNo);

            //执行发送.
            SendReturnObjs objs = BP.WF.Dev2Interface.Node_SendWork(flowNo, workid, ht);
            backHtml += "<br>" + objs.ToMsgOfHtml().Replace("@", "<br>@");

            if (objs.VarAcceptersID == null)
            {
                isAdd = false;
                backHtml += " <br> **** 流程结束,查看<a href='/WF/WFRpt.htm?WorkID=" + workid + "&FK_Flow=" + flowNo + "' target=_blank >流程轨迹</a> ====";
                //htmlArr.Add(html);
                //backHtml += "nextEmpNo";
                return;
            }

            if (string.IsNullOrEmpty(objs.VarAcceptersID))//此处添加为空判断，跳过下面方法的执行，否则出错。
            {
                return;
            }
            string[] accepters = objs.VarAcceptersID.Split(',');

            foreach (string acce in accepters)
            {
                if (string.IsNullOrEmpty(acce) == true)
                    continue;

                //执行发送.
                Simulation_Run_S1(flowNo, workid, acce, ht, beginEmp);
                break; //就不让其执行了.
            }
        }
        /// <summary>
        /// 是否手机访问?
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
            if (BP.Difference.SystemConfig.IsBSsystem == false)
                return false;
            string agent = (HttpContextHelper.RequestUserAgent + "").ToLower().Trim();
            if (agent == "" || agent.IndexOf("mozilla") != -1 || agent.IndexOf("opera") != -1)
                return false;
            return true;
        }
        /// <summary>
        /// 加入track
        /// </summary>
        /// <param name="at">事件类型</param>
        /// <param name="flowNo">流程编号</param>
        /// <param name="workID">工作ID</param>
        /// <param name="fid">流程ID</param>
        /// <param name="fromNodeID">从节点编号</param>
        /// <param name="fromNodeName">从节点名称</param>
        /// <param name="fromEmpID">从人员ID</param>
        /// <param name="fromEmpName">从人员名称</param>
        /// <param name="toNodeID">到节点编号</param>
        /// <param name="toNodeName">到节点名称</param>
        /// <param name="toEmpID">到人员ID</param>
        /// <param name="toEmpName">到人员名称</param>
        /// <param name="note">消息</param>
        /// <param name="tag">参数用@分开</param>
        public static string AddToTrack(ActionType at, string flowNo, Int64 workID, Int64 fid, int fromNodeID, string fromNodeName, string fromEmpID, string fromEmpName,
            int toNodeID, string toNodeName, string toEmpID, string toEmpName, string note, string tag)
        {
            if (toNodeID == 0)
            {
                toNodeID = fromNodeID;
                toNodeName = fromNodeName;
            }

            Track t = new Track();
            t.WorkID = workID;
            t.FID = fid;
            t.RDT = DataType.CurrentDateTimess;
            t.HisActionType = at;

            t.NDFrom = fromNodeID;
            t.NDFromT = fromNodeName;

            t.EmpFrom = fromEmpID;
            t.EmpFromT = fromEmpName;
            t.FK_Flow = flowNo;

            t.NDTo = toNodeID;
            t.NDToT = toNodeName;

            string[] empNos = toEmpID.Split(',');
            if (empNos.Length <= 100)
            {
                t.EmpTo = toEmpID;
                t.EmpToT = toEmpName;
            }
            else
            {
                string[] empNames = toEmpName.Split('、');
                //获取
                t.EmpTo = string.Join(",", empNos.Take(100)) + "..." + empNos[empNos.Length - 1];
                t.EmpToT = string.Join("'、", empNames.Take(100)) + "..." + empNames[empNames.Length - 1];
            }

            t.Msg = note;

            //参数.
            if (tag != null)
                t.Tag = tag;

            try
            {
                t.Insert();
            }
            catch
            {
                t.CheckPhysicsTable();
                t.Insert();
            }
            return t.MyPK;
        }
        /// <summary>
        /// 计算表达式是否通过(或者是否正确.)
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <param name="en">实体</param>
        /// <returns>true/false</returns>
        public static bool ExeExp(string exp, Entity en)
        {
            exp = exp.Replace("@WebUser.No", WebUser.No);
            exp = exp.Replace("@WebUser.Name", WebUser.Name);
            exp = exp.Replace("@WebUser.FK_DeptNameOfFull", WebUser.FK_DeptNameOfFull);
            exp = exp.Replace("@WebUser.FK_DeptName", WebUser.FK_DeptName);
            exp = exp.Replace("@WebUser.FK_Dept", WebUser.FK_Dept);

            exp = exp.Replace("@RDT", DataType.CurrentDate);
            exp = exp.Replace("@DateTime", DataType.CurrentDateTime);


            string[] strs = exp.Split(' ');
            bool isPass = false;

            string key = strs[0].Trim();
            string oper = strs[1].Trim();
            string val = strs[2].Trim();
            val = val.Replace("'", "");
            val = val.Replace("%", "");
            val = val.Replace("~", "");
            BP.En.Row row = en.Row;
            foreach (string item in row.Keys)
            {
                if (key != item.Trim())
                    continue;

                string valPara = row[key].ToString();
                if (oper == "=")
                {
                    if (valPara == val)
                        return true;
                }

                if (oper.ToUpper() == "LIKE")
                {
                    if (valPara.Contains(val))
                        return true;
                }

                if (oper == ">")
                {
                    if (float.Parse(valPara) > float.Parse(val))
                        return true;
                }
                if (oper == ">=")
                {
                    if (float.Parse(valPara) >= float.Parse(val))
                        return true;
                }
                if (oper == "<")
                {
                    if (float.Parse(valPara) < float.Parse(val))
                        return true;
                }
                if (oper == "<=")
                {
                    if (float.Parse(valPara) <= float.Parse(val))
                        return true;
                }

                if (oper == "!=")
                {
                    if (float.Parse(valPara) != float.Parse(val))
                        return true;
                }

                throw new Exception("@参数格式错误:" + exp + " Key=" + key + " oper=" + oper + " Val=" + val);
            }

            return false;
        }
        /// <summary>
        /// 前置导航导入表单数据
        /// </summary>
        /// <param name="WorkID"></param>
        /// <param name="FK_Flow"></param>
        /// <param name="FK_Node"></param>
        /// <param name="sKey">选中的No</param>
        public static DataTable StartGuidEnties(long WorkID, string FK_Flow, int FK_Node, string sKey)
        {
            Flow fl = new Flow(FK_Flow);
            switch (fl.StartGuideWay)
            {
                case StartGuideWay.SubFlowGuide:
                case StartGuideWay.BySQLOne:
                    string sql = "";
                    sql = fl.StartGuidePara3.Clone() as string;  //@李国文.
                    if (DataType.IsNullOrEmpty(sql) == false)
                    {
                        sql = sql.Replace("@Key", sKey);
                        sql = sql.Replace("@key", sKey);
                        sql = sql.Replace("~", "'");
                    }
                    else
                    {
                        sql = fl.StartGuidePara2.Clone() as string;
                    }

                    //sql = " SELECT * FROM (" + sql + ") T WHERE T.NO='" + sKey + "' ";

                    //替换变量
                    sql = sql.Replace("@WebUser.No", WebUser.No);
                    sql = sql.Replace("@WebUser.Name", WebUser.Name);
                    sql = sql.Replace("@WebUser.FK_DeptName", WebUser.FK_DeptName);
                    sql = sql.Replace("@WebUser.FK_Dept", WebUser.FK_Dept);


                    DataTable dt = DBAccess.RunSQLReturnTable(sql);
                    if (dt.Rows.Count == 0)
                        throw new Exception("err@没有找到那一行数据." + sql);

                    Hashtable ht = new Hashtable();
                    //转换成ht表
                    DataRow row = dt.Rows[0];
                    for (int i = 0; i < row.Table.Columns.Count; i++)
                    {
                        switch (row.Table.Columns[i].ColumnName.ToLower())
                        {
                            //去除关键字
                            case "no":
                            case "name":
                            case "workid":
                            case "fk_flow":
                            case "fk_node":
                            case "fid":
                            case "oid":
                            case "mypk":
                            case "title":
                            case "pworkid":
                                break;
                            default:
                                if (ht.ContainsKey(row.Table.Columns[i].ColumnName) == true)
                                    ht[row.Table.Columns[i].ColumnName] = row[i];  //@李国文.
                                else
                                    ht.Add(row.Table.Columns[i].ColumnName, row[i]);
                                break;
                        }
                    }
                    //保存
                    BP.WF.Dev2Interface.Node_SaveWork(FK_Flow, FK_Node, WorkID, ht);
                    return dt;
                case StartGuideWay.SubFlowGuideEntity:
                case StartGuideWay.BySystemUrlOneEntity:
                    break;
                default:
                    break;
            }
            return null;
        }
        /// <summary>
        /// 执行PageLoad装载数据
        /// </summary>
        /// <param name="item"></param>
        /// <param name="en"></param>
        /// <param name="mattrs"></param>
        /// <param name="dtls"></param>
        /// <returns></returns>
        public static Entity DealPageLoadFull(Entity en, MapExt item, MapAttrs mattrs, MapDtls dtls, bool isSelf = false, int nodeID = 0, long workID = 0)
        {
            if (item == null)
                return en;

            DataTable dt = null;
            string sql = item.Doc;
            /* 如果有填充主表的sql  */
            sql = Glo.DealExp(sql, en, null);
            string fk_dbSrc = item.FK_DBSrc;
            //填充方式，0=sql，1=url,2=CCFromRef.js , 3=webapi
            int doWay = item.DoWay;

            SFDBSrc sfdb = null;
            //如果是sql方式填充
            if (doWay == 0)
            {
                if (DataType.IsNullOrEmpty(fk_dbSrc) == false && fk_dbSrc.Equals("local") == false)
                    sfdb = new SFDBSrc(fk_dbSrc);
                if (string.IsNullOrEmpty(sql) == false)
                {
                    if (string.IsNullOrEmpty(sql) == false)
                    {
                        int num = Regex.Matches(sql.ToUpper(), "WHERE").Count;
                        if (num == 1)
                        {
                            string sqlext = sql.Substring(0, sql.ToUpper().IndexOf("WHERE"));
                            sqlext = sql.Substring(sqlext.Length + 1);
                            if (sqlext.Contains("@"))
                                throw new Exception("设置的sql有错误可能有没有替换的变量:" + sql);
                        }
                        if (num > 1 && sql.Contains("@"))
                            throw new Exception("设置的sql有错误可能有没有替换的变量:" + sql);
                        if (sfdb != null)
                            dt = sfdb.RunSQLReturnTable(sql);
                        else
                            dt = DBAccess.RunSQLReturnTable(sql);

                        Attrs attrs = en.EnMap.Attrs;

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            foreach (DataColumn dc in dt.Columns)
                            {
                                //去掉一些不需要copy的字段.
                                switch (dc.ColumnName)
                                {
                                    case WorkAttr.OID:
                                    case WorkAttr.FID:
                                    case WorkAttr.Rec:
                                    case WorkAttr.MD5:
                                    case GERptAttr.FlowEnder:
                                    case GERptAttr.FlowEnderRDT:
                                    case GERptAttr.AtPara:
                                    case GERptAttr.PFlowNo:
                                    case GERptAttr.PWorkID:
                                    case GERptAttr.PNodeID:
                                    case GERptAttr.BillNo:
                                    case GERptAttr.FlowDaySpan:
                                    case "RefPK":
                                    case WorkAttr.RecText:
                                        continue;
                                    default:
                                        break;
                                }

                                //如果不包含数据库.
                                if (attrs.Contains(dc.ColumnName) == false)
                                    continue;

                                //开始赋值.
                                if (string.IsNullOrEmpty(en.GetValStringByKey(dc.ColumnName)) || en.GetValStringByKey(dc.ColumnName) == "0" || en.GetValStringByKey(dc.ColumnName).Contains("0.0"))
                                {
                                    en.SetValByKey(dc.ColumnName, dr[dc.ColumnName].ToString());
                                    continue;
                                }

                                //获取attr
                                Entity entity = mattrs.GetEntityByKey("KeyOfEn", dc.ColumnName);
                                if (entity != null)
                                {
                                    MapAttr attr = (MapAttr)entity;
                                    if (attr.LGType == FieldTypeS.Enum && en.GetValStringByKey(dc.ColumnName).Equals("-1"))
                                    {
                                        en.SetValByKey(dc.ColumnName, dr[dc.ColumnName].ToString());
                                        continue;
                                    }
                                    continue;
                                }

                            }
                        }
                    }
                }
            }
            //如果是webapi方式填充
            else if (doWay == 3)
            {
                //请求地址
                string apiUrl = sql;
                //设置请求头
                Hashtable headerMap = new Hashtable();

                //设置返回值格式
                headerMap.Add("Content-Type", "application/json");
                //设置token，用于接口校验
                headerMap.Add("Authorization", WebUser.Token);

                try
                {
                    //post方式请求数据
                    string postData = HttpPostConnect(apiUrl, headerMap, "");
                    //数据序列化
                    var jsonData = postData.ToJObject();
                    //code=200，表示请求成功，否则失败
                    if (!jsonData["code"].ToString().Equals("200"))
                        return en;

                    //获取返回的数据
                    var data = jsonData["data"].ToString().ToJObject();
                    //获取主表数据
                    string mainTable = data["mainTable"].ToString();
                    dt = Json.ToDataTable(mainTable);

                    //获取全部附件数据
                    var athsJSON = jsonData["aths"].ToString().ToJObject();
                    for (int i = 0; i < athsJSON.Count; i++)
                    {
                        //获取附件
                        var athDatas = athsJSON[i];
                        //获取附件组件ID
                        string FK_FrmAttachment = athDatas["attachmentid"].ToString();
                        //获取当前组件中的附件数据
                        var athArryData = athDatas["attachmentdbs"].ToString().ToJObject();
                        //填充附件数据
                        for (int k = 0; k < athArryData.Count; k++)
                        {
                            var athData = athArryData[k];
                            //生成mypk主键值
                            string guid = DBAccess.GenerGUID();
                            FrmAttachment attachment = new FrmAttachment(FK_FrmAttachment);

                            //是否要先删除掉原有附件？根据实际需求，再做调整
                            //FrmAttachmentDBs attachmentDBs = new FrmAttachmentDBs();
                            //attachmentDBs.Retrieve(FrmAttachmentDBAttr.RefPKVal, workID, FrmAttachmentDBAttr.FK_MapData, attachment.FK_MapData);
                            //attachmentDBs.Delete();

                            //插入数据
                            FrmAttachmentDB attachmentDB = new FrmAttachmentDB();
                            attachmentDB.setMyPK(guid);
                            attachmentDB.FK_FrmAttachment = FK_FrmAttachment;
                            attachmentDB.setFK_MapData(attachment.FK_MapData);
                            attachmentDB.RefPKVal = workID.ToString();
                            attachmentDB.FID = 0;//先默认为0
                            attachmentDB.Rec = athData["rec"].ToString();//执行人
                            attachmentDB.FileFullName = athData["fileFullName"].ToString();//附件全路径
                            attachmentDB.FileName = athData["fileName"].ToString();//附件名称
                            attachmentDB.FileExts = athData["fileExts"].ToString();//文件类型
                            attachmentDB.Sort = athData["sort"].ToString();//附件类型
                            attachmentDB.FK_Dept = athData["fk_dept"].ToString();//上传人所在部门
                            attachmentDB.FK_DeptName = athData["fk_deptName"].ToString();//上传人所在部门名称
                            attachmentDB.RecName = athData["recName"].ToString();//上传人名称
                            attachmentDB.RDT = athData["rdt"].ToString();//上传时间
                            attachmentDB.UploadGUID = guid;
                            attachment.Insert();
                        }
                    }

                    //获取从表数据
                    var dtlJSON = jsonData["dtls"].ToString().ToJObject();
                    for (int i = 0; i < dtlJSON.Count; i++)
                    {
                        var dtlDatas = dtlJSON[i];
                        //获取从表编号
                        string dtlNo = dtlDatas["dtlNo"].ToString();
                        //定义map
                        MapDtl dtl = new MapDtl(dtlNo);
                        //插入之前判断
                        GEDtls gedtls = null;
                        try
                        {
                            gedtls = new GEDtls(dtl.No);
                            if (dtl.DtlOpenType == DtlOpenType.ForFID)
                            {
                                if (gedtls.RetrieveByAttr(GEDtlAttr.RefPK, workID) > 0)
                                    continue;
                            }
                            else
                            {
                                //如果存在数据，默认先删除
                                if (gedtls.RetrieveByAttr(GEDtlAttr.RefPK, en.PKVal) > 0)
                                    gedtls.Delete(GEDtlAttr.RefPK, en.PKVal);
                            }
                        }
                        catch (Exception ex)
                        {
                            (gedtls.GetNewEntity as GEDtl).CheckPhysicsTable();
                        }
                        //获取从表数据
                        var dtlArryData = dtlDatas["dtl"].ToString().ToJObject();
                        for (int k = 0; k < dtlArryData.Count; k++)
                        {
                            //获取一条数据
                            var dtlData = dtlArryData[k];
                            //从表数据
                            string dtlDataStr = dtlData["dtlData"].ToString();
                            //从表附件数据
                            var dtlAthData = dtlData["dtlAths"].ToString().ToJObject();
                            //从表数据字符串，转换成datatable
                            DataTable dtlDt = Json.ToDataTable(dtlDataStr);
                            //执行数据插入
                            foreach (DataRow dr in dtlDt.Rows)
                            {
                                GEDtl gedtl = gedtls.GetNewEntity as GEDtl;
                                foreach (DataColumn dc in dt.Columns)
                                {
                                    gedtl.SetValByKey(dc.ColumnName, dr[dc.ColumnName].ToString());
                                }

                                switch (dtl.DtlOpenType)
                                {
                                    case DtlOpenType.ForEmp:  // 按人员来控制.
                                        gedtl.RefPK = en.PKVal.ToString();
                                        gedtl.FID = long.Parse(en.PKVal.ToString());
                                        break;
                                    case DtlOpenType.ForWorkID: // 按工作ID来控制
                                        gedtl.RefPK = en.PKVal.ToString();
                                        gedtl.FID = long.Parse(en.PKVal.ToString());
                                        break;
                                    case DtlOpenType.ForFID: // 按流程ID来控制.
                                        gedtl.RefPK = workID.ToString();
                                        gedtl.FID = long.Parse(en.PKVal.ToString());
                                        break;
                                }
                                gedtl.RDT = DataType.CurrentDateTime;
                                gedtl.Rec = WebUser.No;
                                gedtl.Insert();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("接口请求失败,message:" + ex.Message.ToString());
                }
            }
            if (string.IsNullOrEmpty(item.Tag1)
                || item.Tag1.Length < 15)
                return en;

            // 填充从表.
            foreach (MapDtl dtl in dtls)
            {
                //如果有数据，就不要填充了.

                string[] sqls = item.Tag1.Split('$');
                foreach (string mysql in sqls)
                {
                    if (string.IsNullOrEmpty(mysql))
                        continue;
                    if (mysql.Contains(dtl.No + ":") == false)
                        continue;
                    if (mysql.Equals(dtl.No + ":") == true)
                        continue;

                    #region 处理sql.
                    sql = Glo.DealSQLExp(mysql.Replace(dtl.No + ":", "").ToString(), en, null);
                    #endregion 处理sql.

                    if (string.IsNullOrEmpty(sql))
                        continue;

                    int num = Regex.Matches(sql.ToUpper(), "WHERE").Count;
                    if (num == 1)
                    {
                        string sqlext = sql.Substring(0, sql.ToUpper().IndexOf("WHERE"));
                        sqlext = sql.Substring(sqlext.Length + 1);
                        if (sqlext.Contains("@"))
                            throw new Exception("设置的sql有错误可能有没有替换的变量:" + sql);
                    }
                    if (num > 1 && sql.Contains("@"))
                        throw new Exception("设置的sql有错误可能有没有替换的变量:" + sql);

                    if (isSelf == true)
                    {
                        MapDtl mdtlSln = new MapDtl();
                        mdtlSln.No = dtl.No + "_" + nodeID;
                        int result = mdtlSln.RetrieveFromDBSources();
                        if (result != 0)
                        {
                            dtl.DtlOpenType = mdtlSln.DtlOpenType;
                        }
                    }



                    GEDtls gedtls = null;

                    try
                    {
                        gedtls = new GEDtls(dtl.No);
                        if (dtl.DtlOpenType == DtlOpenType.ForFID)
                        {
                            if (gedtls.RetrieveByAttr(GEDtlAttr.RefPK, workID) > 0)
                                continue;
                        }
                        else
                        {
                            if (gedtls.RetrieveByAttr(GEDtlAttr.RefPK, en.PKVal) > 0)
                                continue;
                        }


                        //gedtls.Delete(GEDtlAttr.RefPK, en.PKVal);
                    }
                    catch (Exception ex)
                    {
                        (gedtls.GetNewEntity as GEDtl).CheckPhysicsTable();
                    }

                    sql = sql.StartsWith(dtl.No + "=") ? sql.Substring((dtl.No + "=").Length) : sql;
                    if (sfdb != null)
                        dt = sfdb.RunSQLReturnTable(sql);
                    else
                        dt = DBAccess.RunSQLReturnTable(sql);

                    foreach (DataRow dr in dt.Rows)
                    {
                        GEDtl gedtl = gedtls.GetNewEntity as GEDtl;
                        foreach (DataColumn dc in dt.Columns)
                        {
                            gedtl.SetValByKey(dc.ColumnName, dr[dc.ColumnName].ToString());
                        }

                        switch (dtl.DtlOpenType)
                        {
                            case DtlOpenType.ForEmp:  // 按人员来控制.
                                gedtl.RefPK = en.PKVal.ToString();
                                gedtl.FID = long.Parse(en.PKVal.ToString());
                                break;
                            case DtlOpenType.ForWorkID: // 按工作ID来控制
                                gedtl.RefPK = en.PKVal.ToString();
                                gedtl.FID = long.Parse(en.PKVal.ToString());
                                break;
                            case DtlOpenType.ForFID: // 按流程ID来控制.
                                gedtl.RefPK = workID.ToString();
                                gedtl.FID = long.Parse(en.PKVal.ToString());
                                break;
                        }
                        gedtl.RDT = DataType.CurrentDateTime;
                        gedtl.Rec = WebUser.No;
                        gedtl.Insert();
                    }
                }
            }
            return en;
        }
        /// <summary>
        /// SQL表达式是否正确
        /// </summary>
        /// <param name="sqlExp"></param>
        /// <param name="ht"></param>
        /// <returns></returns>
        public static bool CondExpSQL(string sqlExp, Hashtable ht, Int64 myWorkID)
        {
            string sql = sqlExp;
            sql = sql.Replace("~", "'");
            sql = sql.Replace("@WebUser.No", BP.Web.WebUser.No);
            sql = sql.Replace("@WebUser.Name", BP.Web.WebUser.Name);
            sql = sql.Replace("@WebUser.FK_Dept", BP.Web.WebUser.FK_Dept);

            foreach (string key in ht.Keys)
            {
                if (key == "OID")
                {
                    sql = sql.Replace("@WorkID", ht["OID"].ToString());
                    sql = sql.Replace("@OID", ht["OID"].ToString());
                    continue;
                }
                sql = sql.Replace("@" + key, ht[key].ToString());
            }

            //从工作流参数里面替换
            if (sql.Contains("@") == true && myWorkID != 0)
            {
                GenerWorkFlow gwf = new GenerWorkFlow(myWorkID);
                AtPara ap = gwf.atPara;
                foreach (string str in ap.HisHT.Keys)
                {
                    sql = sql.Replace("@" + str, ap.GetValStrByKey(str));
                }
            }

            int result = DBAccess.RunSQLReturnValInt(sql, -1);
            if (result <= 0)
                return false;
            return true;
        }
        /// <summary>
        /// 判断表达式是否成立
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <param name="en">变量</param>
        /// <returns>是否成立</returns>
        public static bool CondExpPara(string exp, Hashtable ht, Int64 myWorkID)
        {
            try
            {
                string[] strs = exp.Trim().Split(' ');

                string key = strs[0].Trim();
                string oper = strs[1].Trim();
                string val = strs[2].Trim();

                val = val.Replace("'", "");
                val = val.Replace("%", "");
                val = val.Replace("~", "");

                string valPara = null;
                if (ht.ContainsKey(key) == false)
                {

                    bool isHave = false;
                    if (myWorkID != 0)
                    {
                        //把外部传来的参数传入到 rptGE 让其做方向条件的判断.
                        GenerWorkFlow gwf = new GenerWorkFlow(myWorkID);
                        AtPara at = gwf.atPara;
                        foreach (string str in at.HisHT.Keys)
                        {
                            if (key.Equals(str) == false)
                                continue;

                            valPara = at.GetValStrByKey(key);
                            isHave = true;
                            break;
                        }
                    }

                    if (isHave == false)
                    {
                        try
                        {
                            if (BP.Difference.SystemConfig.IsBSsystem == true && HttpContextHelper.RequestParamKeys.Contains(key) == true)
                                valPara = HttpContextHelper.RequestParams(key);
                            else
                                throw new Exception("@判断条件时错误,请确认参数是否拼写错误,没有找到对应的表达式:" + exp + " Key=(" + key + ") oper=(" + oper + ")Val=(" + val + ")");
                        }
                        catch
                        {
                            //有可能是常量. 
                            valPara = key;
                        }
                    }
                }
                else
                {
                    valPara = ht[key].ToString().Trim();
                }

                #region 开始执行判断.
                if (oper == "=")
                {
                    if (valPara == val)
                        return true;
                    else
                        return false;
                }

                if (oper.ToUpper() == "LIKE")
                {
                    if (valPara.Contains(val))
                        return true;
                    else
                        return false;
                }
                if (oper == "!=")
                {
                    if (valPara != val)
                        return true;
                    else
                        return false;
                }


                if (DataType.IsNumStr(valPara) == false)
                    throw new Exception("err@表达式错误:[" + exp + "]没有找到参数[" + valPara + "]的值，导致无法计算。");

                if (oper == ">")
                {
                    if (float.Parse(valPara) > float.Parse(val))
                        return true;
                    else
                        return false;
                }
                if (oper == ">=")
                {
                    if (float.Parse(valPara) >= float.Parse(val))
                        return true;
                    else
                        return false;
                }
                if (oper == "<")
                {
                    if (float.Parse(valPara) < float.Parse(val))
                        return true;
                    else
                        return false;
                }
                if (oper == "<=")
                {
                    if (float.Parse(valPara) <= float.Parse(val))
                        return true;
                    else
                        return false;
                }

                if (oper == "!=")
                {
                    if (float.Parse(valPara) != float.Parse(val))
                        return true;
                    else
                        return false;
                }
                throw new Exception("@参数格式错误:" + exp + " Key=" + key + " oper=" + oper + " Val=" + val);
                #endregion 开始执行判断.

            }
            catch (Exception ex)
            {
                throw new Exception("计算参数的时候出现错误:" + ex.Message);
            }
        }
        /// <summary>
        /// 表达式替换
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string DealExp(string exp, Entity en, string errInfo = "")
        {
            //替换字符
            exp = exp.Replace("~", "'");

            if (exp.Contains("@") == false)
                return exp;

            //首先替换加; 的。
            exp = exp.Replace("@WebUser.No;", WebUser.No);
            exp = exp.Replace("@WebUser.Name;", WebUser.Name);
            exp = exp.Replace("@WebUser.FK_DeptName;", WebUser.FK_DeptName);
            exp = exp.Replace("@WebUser.FK_Dept;", WebUser.FK_Dept);

            // 替换没有 ; 的 .
            exp = exp.Replace("@WebUser.No", WebUser.No);
            exp = exp.Replace("@WebUser.Name", WebUser.Name);
            exp = exp.Replace("@WebUser.FK_DeptName", WebUser.FK_DeptName);
            exp = exp.Replace("@WebUser.FK_Dept", WebUser.FK_Dept);
            exp = exp.Replace("@WebUser.OrgNo", WebUser.OrgNo);

            exp = exp.Replace("@RDT", DataType.CurrentDateTime);

            if (exp.Contains("@") == false)
                return exp;

            //增加对新规则的支持. @MyField; 格式.
            if (en != null)
            {
                Attrs attrs = en.EnMap.Attrs;
                Row row = en.Row;
                //特殊判断.
                if (row.ContainsKey("OID") == true)
                    exp = exp.Replace("@WorkID", row["OID"].ToString());

                if (exp.Contains("@") == false)
                    return exp;


                bool isHaveFenHao = exp.Contains(';');


                foreach (string key in row.Keys)
                {
                    //值为空或者null不替换
                    if (row[key] == null)
                        continue;
                    if (exp.Contains("@" + key + ";"))
                    {
                        //先替换有单引号的.
                        exp = exp.Replace("'@" + key + ";'", "'" + row[key].ToString() + "'");
                        //在更新没有单引号的.
                        exp = exp.Replace("@" + key + ";", row[key].ToString());
                    }
                    if (exp.Contains("@" + key))
                    {
                        //先替换有单引号的.
                        exp = exp.Replace("'@" + key + "'", "'" + row[key].ToString() + "'");
                        //在更新没有单引号的.
                        exp = exp.Replace("@" + key, row[key].ToString());
                    }
                    //不包含@则返回SQL语句
                    if (exp.Contains("@") == false)
                        return exp;
                }
            }

            if (exp.Contains("@") && BP.Difference.SystemConfig.IsBSsystem == true)
            {
                /*如果是bs*/
                foreach (string key in HttpContextHelper.RequestParamKeys)
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    exp = exp.Replace("@" + key, HttpContextHelper.RequestParams(key));
                }

            }

            exp = exp.Replace("~", "'");
            return exp;
        }
        //
        /// <summary>
        /// 处理表达式
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <param name="en">数据源</param>
        /// <param name="errInfo">错误</param>
        /// <returns></returns>
        public static string DealSQLExp(string exp, Entity en, string errInfo)
        {
            //替换字符
            exp = exp.Replace("~", "'");

            //替换我们只处理WHERE 后面的内容
            //需要判断SQL 中含有几个WHERE字符
            int num = Regex.Matches(exp.ToUpper(), "WHERE").Count;
            if (num == 0)
                return exp;
            //我们暂时处理含有一个WHERE的情况
            string expFrom = "";
            if (num == 1)
            {
                expFrom = exp.Substring(0, exp.ToUpper().IndexOf("WHERE"));
                exp = exp.Substring(expFrom.Length);
            }

            string expWhere = "";

            if (exp.Contains("@") == false)
                return expFrom + exp;

            //首先替换加; 的。
            exp = exp.Replace("@WebUser.No;", WebUser.No);
            exp = exp.Replace("@WebUser.Name;", WebUser.Name);
            exp = exp.Replace("@WebUser.FK_DeptName;", WebUser.FK_DeptName);
            exp = exp.Replace("@WebUser.FK_Dept;", WebUser.FK_Dept);


            // 替换没有 ; 的 .
            exp = exp.Replace("@WebUser.No", WebUser.No);
            exp = exp.Replace("@WebUser.Name", WebUser.Name);
            exp = exp.Replace("@WebUser.FK_DeptName", WebUser.FK_DeptName);
            exp = exp.Replace("@WebUser.FK_Dept", WebUser.FK_Dept);

            if (Glo.CCBPMRunModel != CCBPMRunModel.Single)
                exp = exp.Replace("@WebUser.OrgNo", WebUser.OrgNo);

            if (exp.Contains("@") == false)
                return expFrom + exp;

            //增加对新规则的支持. @MyField; 格式.
            if (en != null)
            {
                Row row = en.Row;
                //特殊判断.
                if (row.ContainsKey("OID") == true)
                    exp = exp.Replace("@WorkID", row["OID"].ToString());

                if (exp.Contains("@") == false)
                    return expFrom + exp;

                foreach (string key in row.Keys)
                {
                    //值为空或者null不替换
                    if (row[key] == null || row[key].Equals("") == true)
                        continue;

                    if (exp.Contains("@" + key + ";"))
                        exp = exp.Replace("@" + key + ";", row[key].ToString());

                    //不包含@则返回SQL语句
                    if (exp.Contains("@") == false)
                        return expFrom + exp;
                }


                #region 解决排序问题.
                Attrs attrs = en.EnMap.Attrs;
                string mystrs = "";
                foreach (Attr attr in attrs)
                {
                    if (attr.MyDataType == DataType.AppString)
                        mystrs += "@" + attr.Key + ",";
                    else
                        mystrs += "@" + attr.Key;
                }
                string[] strs = mystrs.Split('@');
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("No", typeof(string)));
                foreach (string str in strs)
                {
                    if (string.IsNullOrEmpty(str))
                        continue;

                    DataRow dr = dt.NewRow();
                    dr[0] = str;
                    dt.Rows.Add(dr);
                }
                DataView dv = dt.DefaultView;
                dv.Sort = "No DESC";
                DataTable dtNew = dv.Table;
                #endregion  解决排序问题.

                #region 替换变量.
                foreach (DataRow dr in dtNew.Rows)
                {
                    string key = dr[0].ToString();
                    bool isStr = key.Contains(",");
                    if (isStr == true)
                        key = key.Replace(",", "");

                    if (DataType.IsNullOrEmpty(en.GetValStrByKey(key)))
                        continue;

                    exp = exp.Replace("@" + key, en.GetValStrByKey(key));
                }
                #endregion

                if (exp.Contains("@") == false)
                    return expFrom + exp;
            }

            if (exp.Contains("@") && BP.Difference.SystemConfig.IsBSsystem == true)
            {
                /*如果是bs*/
                foreach (string key in HttpContextHelper.RequestParamKeys)
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    exp = exp.Replace("@" + key, HttpContextHelper.RequestParams(key));
                }
                /*如果是bs*/
                //foreach (string key in System.Web.HttpContext.Current.Request.Form.AllKeys)
                //{
                //    if (string.IsNullOrEmpty(key))
                //        continue;
                //    exp = exp.Replace("@" + key, System.Web.HttpContext.Current.Request.Form[key]);
                //}

            }

            exp = exp.Replace("~", "'");
            exp = exp.Replace("\r", "");
            exp = exp.Replace("\n", "");
            return expFrom + exp;
        }
        /// <summary>
        /// 加密MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GenerMD5(BP.WF.Work wk)
        {
            string s = null;
            foreach (Attr attr in wk.EnMap.Attrs)
            {
                switch (attr.Key)
                {
                    case WorkAttr.MD5:
                    case WorkAttr.Rec:
                    case GERptAttr.Title:
                    // case GERptAttr.Emps:
                    case GERptAttr.FK_Dept:
                    //case GERptAttr.PRI:
                    case GERptAttr.FID:
                        continue;
                    default:
                        break;
                }

                string obj = attr.DefaultVal as string;
                //if (obj == null)
                //    continue;
                if (obj != null && obj.Contains("@"))
                    continue;

                s += wk.GetValStrByKey(attr.Key);
            }
            s += "ccflow";

            return GetMD5Hash(s);
            //return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(s, "MD5").ToLower();
        }

        /// <summary>
        /// 取得MD5加密串
        /// </summary>
        /// <param name="input">源明文字符串</param>
        /// <returns>密文字符串</returns>
        public static string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = md5.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        /// <summary>
        /// 装载流程数据 
        /// </summary>
        /// <param name="xlsFile"></param>
        public static string LoadFlowDataWithToSpecNode(string xlsFile)
        {
            DataTable dt = DBLoad.ReadExcelFileToDataTable(xlsFile);
            string err = "";
            string info = "";

            foreach (DataRow dr in dt.Rows)
            {
                string flowPK = dr["FlowPK"].ToString();
                string starter = dr["Starter"].ToString();
                string executer = dr["Executer"].ToString();
                int toNode = int.Parse(dr["ToNodeID"].ToString().Replace("ND", ""));
                Node nd = new Node();
                nd.NodeID = toNode;
                if (nd.RetrieveFromDBSources() == 0)
                {
                    err += "节点ID错误:" + toNode;
                    continue;
                }
                string sql = "SELECT count(*) as Num FROM ND" + int.Parse(nd.FK_Flow) + "01 WHERE FlowPK='" + flowPK + "'";
                int i = DBAccess.RunSQLReturnValInt(sql);
                if (i == 1)
                    continue; // 此数据已经调度了。

                #region 检查数据是否完整。
                BP.Port.Emp emp = new BP.Port.Emp();
                emp.UserID = executer;
                if (emp.RetrieveFromDBSources() == 0)
                {
                    err += "@账号:" + starter + ",不存在。";
                    continue;
                }
                if (string.IsNullOrEmpty(emp.FK_Dept))
                {
                    err += "@账号:" + starter + ",没有部门。";
                    continue;
                }

                emp.UserID = starter;
                if (emp.RetrieveFromDBSources() == 0)
                {
                    err += "@账号:" + executer + ",不存在。";
                    continue;
                }
                if (string.IsNullOrEmpty(emp.FK_Dept))
                {
                    err += "@账号:" + executer + ",没有部门。";
                    continue;
                }
                #endregion 检查数据是否完整。

                BP.Web.WebUser.SignInOfGener(emp);
                Flow fl = nd.HisFlow;
                Work wk = fl.NewWork();

                Attrs attrs = wk.EnMap.Attrs;
                //foreach (Attr attr in wk.EnMap.Attrs)
                //{
                //}

                foreach (DataColumn dc in dt.Columns)
                {
                    Attr attr = attrs.GetAttrByKey(dc.ColumnName.Trim());
                    if (attr == null)
                        continue;

                    string val = dr[dc.ColumnName].ToString().Trim();
                    switch (attr.MyDataType)
                    {
                        case DataType.AppString:
                        case DataType.AppDate:
                        case DataType.AppDateTime:
                            wk.SetValByKey(attr.Key, val);
                            break;
                        case DataType.AppInt:
                        case DataType.AppBoolean:
                            wk.SetValByKey(attr.Key, int.Parse(val));
                            break;
                        case DataType.AppMoney:
                        case DataType.AppDouble:
                        case DataType.AppFloat:
                            wk.SetValByKey(attr.Key, decimal.Parse(val));
                            break;
                        default:
                            wk.SetValByKey(attr.Key, val);
                            break;
                    }
                }

                wk.SetValByKey(WorkAttr.Rec, BP.Web.WebUser.No);
                //  wk.SetValByKey(GERptAttr.FK_Dept, BP.Web.WebUser.FK_Dept);
                // wk.SetValByKey("FK_NY", DataType.CurrentYearMonth);
                wk.Update();

                Node ndStart = nd.HisFlow.HisStartNode;
                WorkNode wn = new WorkNode(wk, ndStart);
                try
                {
                    info += "<hr>" + wn.NodeSend(nd, executer).ToMsgOfHtml();
                }
                catch (Exception ex)
                {
                    err += "<hr>" + ex.Message;
                    WorkFlow wf = new WorkFlow(fl, wk.OID);
                    wf.DoDeleteWorkFlowByReal(true);
                    continue;
                }

                #region 更新 下一个节点数据。
                Work wkNext = nd.HisWork;
                wkNext.OID = wk.OID;
                wkNext.RetrieveFromDBSources();
                attrs = wkNext.EnMap.Attrs;
                foreach (DataColumn dc in dt.Columns)
                {
                    Attr attr = attrs.GetAttrByKey(dc.ColumnName.Trim());
                    if (attr == null)
                        continue;

                    string val = dr[dc.ColumnName].ToString().Trim();
                    switch (attr.MyDataType)
                    {
                        case DataType.AppString:
                        case DataType.AppDate:
                        case DataType.AppDateTime:
                            wkNext.SetValByKey(attr.Key, val);
                            break;
                        case DataType.AppInt:
                        case DataType.AppBoolean:
                            wkNext.SetValByKey(attr.Key, int.Parse(val));
                            break;
                        case DataType.AppMoney:
                        case DataType.AppDouble:
                        case DataType.AppFloat:
                            wkNext.SetValByKey(attr.Key, decimal.Parse(val));
                            break;
                        default:
                            wkNext.SetValByKey(attr.Key, val);
                            break;
                    }
                }

                wkNext.DirectUpdate();

                GERpt rtp = fl.HisGERpt;
                rtp.SetValByKey("OID", wkNext.OID);
                rtp.RetrieveFromDBSources();
                rtp.Copy(wkNext);
                rtp.DirectUpdate();

                #endregion 更新 下一个节点数据。
            }
            return info + err;
        }
        public static string LoadFlowDataWithToSpecEndNode(string xlsFile)
        {
            DataTable dt = DBLoad.ReadExcelFileToDataTable(xlsFile);
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ds.WriteXml("C:/已完成.xml");

            string err = "";
            string info = "";
            int idx = 0;
            foreach (DataRow dr in dt.Rows)
            {
                string flowPK = dr["FlowPK"].ToString().Trim();
                if (string.IsNullOrEmpty(flowPK))
                    continue;

                string starter = dr["Starter"].ToString();
                string executer = dr["Executer"].ToString();
                int toNode = int.Parse(dr["ToNodeID"].ToString().Replace("ND", ""));
                Node ndOfEnd = new Node();
                ndOfEnd.NodeID = toNode;
                if (ndOfEnd.RetrieveFromDBSources() == 0)
                {
                    err += "节点ID错误:" + toNode;
                    continue;
                }

                if (ndOfEnd.IsEndNode == false)
                {
                    err += "节点ID错误:" + toNode + ", 非结束节点。";
                    continue;
                }

                string sql = "SELECT count(*) as Num FROM ND" + int.Parse(ndOfEnd.FK_Flow) + "01 WHERE FlowPK='" + flowPK + "'";
                int i = DBAccess.RunSQLReturnValInt(sql);
                if (i == 1)
                    continue; // 此数据已经调度了。

                #region 检查数据是否完整。
                //发起人发起。
                BP.Port.Emp emp = new BP.Port.Emp();
                emp.UserID = executer;
                if (emp.RetrieveFromDBSources() == 0)
                {
                    err += "@账号:" + starter + ",不存在。";
                    continue;
                }

                if (string.IsNullOrEmpty(emp.FK_Dept))
                {
                    err += "@账号:" + starter + ",没有设置部门。";
                    continue;
                }

                emp = new BP.Port.Emp();
                emp.UserID = starter;
                if (emp.RetrieveFromDBSources() == 0)
                {
                    err += "@账号:" + starter + ",不存在。";
                    continue;
                }
                else
                {
                    emp.RetrieveFromDBSources();
                    if (string.IsNullOrEmpty(emp.FK_Dept))
                    {
                        err += "@账号:" + starter + ",没有设置部门。";
                        continue;
                    }
                }
                #endregion 检查数据是否完整。


                BP.Web.WebUser.SignInOfGener(emp);
                Flow fl = ndOfEnd.HisFlow;
                Work wk = fl.NewWork();
                foreach (DataColumn dc in dt.Columns)
                    wk.SetValByKey(dc.ColumnName.Trim(), dr[dc.ColumnName].ToString().Trim());

                wk.SetValByKey(WorkAttr.Rec, BP.Web.WebUser.No);
                //wk.SetValByKey(GERptAttr.FK_Dept, BP.Web.WebUser.FK_Dept);
                //wk.SetValByKey("FK_NY", DataType.CurrentYearMonth);
                //wk.SetValByKey(WorkAttr.MyNum, 1);
                wk.Update();

                Node ndStart = fl.HisStartNode;
                WorkNode wn = new WorkNode(wk, ndStart);
                try
                {
                    info += "<hr>" + wn.NodeSend(ndOfEnd, executer).ToMsgOfHtml();
                }
                catch (Exception ex)
                {
                    err += "<hr>启动错误:" + ex.Message;
                    DBAccess.RunSQL("DELETE FROM ND" + int.Parse(ndOfEnd.FK_Flow) + "01 WHERE FlowPK='" + flowPK + "'");
                    WorkFlow wf = new WorkFlow(fl, wk.OID);
                    wf.DoDeleteWorkFlowByReal(true);
                    continue;
                }

                //结束点结束。
                emp = new BP.Port.Emp(executer);
                BP.Web.WebUser.SignInOfGener(emp);

                Work wkEnd = ndOfEnd.GetWork(wk.OID);
                foreach (DataColumn dc in dt.Columns)
                    wkEnd.SetValByKey(dc.ColumnName.Trim(), dr[dc.ColumnName].ToString().Trim());

                wkEnd.SetValByKey(WorkAttr.Rec, BP.Web.WebUser.No);
                //wkEnd.SetValByKey(GERptAttr.FK_Dept, BP.Web.WebUser.FK_Dept);
                //wkEnd.SetValByKey("FK_NY", DataType.CurrentYearMonth);
                //wkEnd.SetValByKey(WorkAttr.MyNum, 1);
                wkEnd.Update();

                try
                {
                    WorkNode wnEnd = new WorkNode(wkEnd, ndOfEnd);
                    //  wnEnd.AfterNodeSave();
                    info += "<hr>" + wnEnd.NodeSend().ToMsgOfHtml();
                }
                catch (Exception ex)
                {
                    err += "<hr>结束错误(系统直接删除它):" + ex.Message;
                    WorkFlow wf = new WorkFlow(fl, wk.OID);
                    wf.DoDeleteWorkFlowByReal(true);
                    continue;
                }
            }
            return info + err;
        }
        /// <summary>
        /// 判断是否登陆当前UserNo
        /// </summary>
        /// <param name="userNo"></param>
        public static void IsSingleUser(string userNo)
        {
            if (string.IsNullOrEmpty(WebUser.No) || WebUser.No != userNo)
            {
                if (!string.IsNullOrEmpty(userNo))
                {
                    BP.WF.Dev2Interface.Port_Login(userNo);
                }
            }
        }
        //public static void ResetFlowView()
        //{
        //    string sql = "DROP VIEW V_WF_Data ";
        //    try
        //    {
        //        DBAccess.RunSQL(sql);
        //    }
        //    catch
        //    {
        //    }

        //    Flows fls = new Flows();
        //    fls.RetrieveAll();
        //    sql = "CREATE VIEW V_WF_Data AS ";
        //    foreach (Flow fl in fls)
        //    {
        //        fl.CheckRpt();
        //        sql += "\t\n SELECT '" + fl.No + "' as FK_Flow, '" + fl.Name + "' AS FlowName, '" + fl.FK_FlowSort + "' as FK_FlowSort,CDT,Emps,FID,FK_Dept,FK_NY,";
        //        sql += "MyNum,OID,RDT,Rec,Title,WFState,FlowEmps,";
        //        sql += "FlowStarter,FlowStartRDT,FlowEnder,FlowEnderRDT,FlowDaySpan FROM ND" + int.Parse(fl.No) + "Rpt";
        //        sql += "\t\n  UNION";
        //    }
        //    sql = sql.Substring(0, sql.Length - 6);
        //    sql += "\t\n GO";
        //    DBAccess.RunSQL(sql);
        //}
        public static void Rtf2PDF(object pathOfRtf, object pathOfPDF)
        {
            //        Object Nothing = System.Reflection.Missing.Value;
            //        //创建一个名为WordApp的组件对象    
            //        Microsoft.Office.Interop.Word.Application wordApp =
            //new Microsoft.Office.Interop.Word.ApplicationClass();
            //        //创建一个名为WordDoc的文档对象并打开    
            //        Microsoft.Office.Interop.Word.Document doc = wordApp.Documents.Open(ref pathOfRtf, ref Nothing, ref Nothing, ref Nothing, ref Nothing,
            // ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing,
            //ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing);

            //        //设置保存的格式    
            //        object filefarmat = Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF;

            //        //保存为PDF    
            //        doc.SaveAs(ref pathOfPDF, ref filefarmat, ref Nothing, ref Nothing, ref Nothing, ref Nothing,
            //ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing,
            // ref Nothing, ref Nothing, ref Nothing);
            //        //关闭文档对象    
            //        doc.Close(ref Nothing, ref Nothing, ref Nothing);
            //        //推出组建    
            //        wordApp.Quit(ref Nothing, ref Nothing, ref Nothing);
            //        GC.Collect();
        }
        #endregion 常用方法

        #region 属性
        /// <summary>
        /// 消息
        /// </summary>
        public static string SessionMsg
        {
            get
            {
                Paras ps = new Paras();
                ps.SQL = "SELECT Msg FROM WF_Emp where No=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "FK_Emp";
                ps.Add("FK_Emp", BP.Web.WebUser.No);

                return DBAccess.RunSQLReturnString(ps);
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                    return;

                if (BP.Difference.SystemConfig.CCBPMRunModel == CCBPMRunModel.SAAS)
                {
                    string mypk = WebUser.OrgNo + "_" + WebUser.No;

                    Paras myps = new Paras();
                    myps.SQL = "UPDATE WF_Emp SET Msg=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "Msg WHERE No=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "No";
                    myps.Add("Msg", value);
                    myps.Add("No", mypk);
                    if (DBAccess.RunSQL(myps) != 0)
                        return;

                    /*如果没有更新到.*/
                    BP.WF.Port.WFEmp myemp = new BP.WF.Port.WFEmp();
                    //myemp.No = mypk;

                    myemp.SetValByKey("No", mypk);

                    myemp.Name = BP.Web.WebUser.Name;
                    myemp.FK_Dept = BP.Web.WebUser.FK_Dept;

                    //SAAS 需要判断.
                    //BP.Port.Emp emp = new BP.Port.Emp();
                    //emp.Email = DBAccess.RunSQLReturnString("SELECT Eamil FROM Port_Emp WHERE "); // new BP.Port.Emp(WebUser.No).Email;
                    myemp.Insert();
                    DBAccess.RunSQL(myps);
                    return;
                }


                Paras ps = new Paras();
                ps.SQL = "UPDATE WF_Emp SET Msg=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "v WHERE No=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "No";
                ps.Add("v", value);
                ps.Add("No", WebUser.No);
                if (DBAccess.RunSQL(ps) == 1)
                    return;

                /*如果没有更新到.*/
                BP.WF.Port.WFEmp emp = new BP.WF.Port.WFEmp();
                emp.No = BP.Web.WebUser.No;
                emp.Name = BP.Web.WebUser.Name;
                emp.FK_Dept = BP.Web.WebUser.FK_Dept;

                //SAAS 需要判断.
                //BP.Port.Emp emp = new BP.Port.Emp();
                //emp.Email = DBAccess.RunSQLReturnString("SELECT Eamil FROM Port_Emp WHERE "); // new BP.Port.Emp(WebUser.No).Email;
                emp.Insert();
                DBAccess.RunSQL(ps);
            }
        }
        private static Hashtable _SendHTOfTemp = null;
        /// <summary>
        /// 临时的发送传输变量.
        /// </summary>
        //public static Hashtable SendHTOfTemp
        //{
        //    //get
        //    //{
        //    //    if (_SendHTOfTemp == null)
        //    //        _SendHTOfTemp = new Hashtable();
        //    //    return _SendHTOfTemp[BP.Web.WebUser.No] as Hashtable;
        //    //}
        //    //set
        //    //{
        //    //    if (value == null)
        //    //        return;

        //    //    if (_SendHTOfTemp == null)
        //    //        _SendHTOfTemp = new Hashtable();
        //    //    _SendHTOfTemp[BP.Web.WebUser.No] = value;
        //    //}
        //}
        /// <summary>
        /// 报表属性集合
        /// </summary>
        private static Attrs _AttrsOfRpt = null;
        /// <summary>
        /// 报表属性集合
        /// </summary>
        public static Attrs AttrsOfRpt
        {
            get
            {
                if (_AttrsOfRpt == null)
                {
                    _AttrsOfRpt = new Attrs();
                    _AttrsOfRpt.AddTBInt(GERptAttr.OID, 0, "WorkID", true, true);
                    _AttrsOfRpt.AddTBInt(GERptAttr.FID, 0, "FlowID", false, false);

                    _AttrsOfRpt.AddTBString(GERptAttr.Title, null, "标题", true, false, 0, 10, 10);
                    _AttrsOfRpt.AddTBString(GERptAttr.FlowStarter, null, "发起人", true, false, 0, 10, 10);
                    _AttrsOfRpt.AddTBString(GERptAttr.FlowStartRDT, null, "发起时间", true, false, 0, 10, 10);
                    _AttrsOfRpt.AddTBString(GERptAttr.WFState, null, "状态", true, false, 0, 10, 10);

                    //Attr attr = new Attr();
                    //attr.Desc = "流程状态";
                    //attr.Key = "WFState";
                    //attr.MyFieldType = FieldType.Enum;
                    //attr.UIBindKey = "WFState";
                    //attr.UITag = "@0=进行中@1=已经完成";

                    _AttrsOfRpt.AddDDLSysEnum(GERptAttr.WFState, 0, "流程状态", true, true, GERptAttr.WFState);
                    _AttrsOfRpt.AddTBString(GERptAttr.FlowEmps, null, "参与人", true, false, 0, 10, 10);
                    _AttrsOfRpt.AddTBString(GERptAttr.FlowEnder, null, "结束人", true, false, 0, 10, 10);
                    _AttrsOfRpt.AddTBString(GERptAttr.FlowEnderRDT, null, "最后处理时间", true, false, 0, 10, 10);
                    _AttrsOfRpt.AddTBInt(GERptAttr.FlowEndNode, 0, "停留节点", true, false);
                    _AttrsOfRpt.AddTBDecimal(GERptAttr.FlowDaySpan, 0, "流程时长(天)", true, false);
                    //_AttrsOfRpt.AddTBString(GERptAttr.FK_NY, null, "隶属月份", true, false, 0, 10, 10);
                }
                return _AttrsOfRpt;
            }
        }
        #endregion 属性

        #region 其他配置.

        /// <summary>
        /// 帮助
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        public static string GenerHelpCCForm(string text, string id1, string id2)
        {
            if (id1 == null)
                return "<div style='float:right' ><a href='http://ccform.mydoc.io' target=_blank><img src='/WF/Img/Help.gif'>" + text + "</a></div>";
            else
                return "<div style='float:right' ><a href='" + id1 + "' target=_blank><img src='/WF/Img/Help.gif'>" + text + "</a></div>";
        }
        public static string GenerHelpCCFlow(string text, string id1, string id2)
        {
            return "<div style='float:right' ><a href='" + id1 + "' target=_blank><img src='/WF/Img/Help.gif'>" + text + "</a></div>";
        }
        public static string NodeImagePath
        {
            get
            {
                return Glo.IntallPath + "Data/Node/";
            }
        }


        public static void ClearDBData()
        {
            string sql = "DELETE FROM WF_GenerWorkFlow WHERE fk_flow not in (select no from wf_flow )";
            DBAccess.RunSQL(sql);

            sql = "DELETE FROM WF_GenerWorkerlist WHERE fk_flow not in (select no from wf_flow )";
            DBAccess.RunSQL(sql);
        }
        public static string OEM_Flag = "CCS";
        public static string FlowFileBill
        {
            get { return Glo.IntallPath + "DataUser/Bill/"; }
        }
        private static string _IntallPath = null;
        public static string IntallPath
        {
            get
            {
                if (_IntallPath == null)
                {
                    if (BP.Difference.SystemConfig.IsBSsystem == true)
                        _IntallPath = BP.Difference.SystemConfig.PathOfWebApp;
                }

                if (_IntallPath == null)
                    throw new Exception("@没有实现如何获得 cs 下的根目录.");

                return _IntallPath;
            }
            set
            {
                _IntallPath = value;
            }
        }
        private static string _ServerIP = null;
        public static string ServerIP
        {
            get
            {
                if (_ServerIP == null)
                {
                    string ip = "127.0.0.1";
                    System.Net.IPAddress[] addressList = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                    if (addressList.Length > 1)
                        _ServerIP = addressList[1].ToString();
                    else
                        _ServerIP = addressList[0].ToString();
                }
                return _ServerIP;
            }
            set
            {
                _ServerIP = value;
            }
        }
        /// <summary>
        /// 全局的安全验证码
        /// </summary>
        public static string GloSID
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["GloSID"] as string;
                if (DataType.IsNullOrEmpty(s))
                    s = "sdfq2erre-2342-234sdf23423-323";
                return s;
            }
        }
        /// <summary>
        /// 是否启用检查用户的状态?
        /// 如果启用了:在MyFlow.htm中每次都会检查当前的用户状态是否被禁
        /// 用，如果禁用了就不能执行任何操作了。启用后，就意味着每次都要
        /// 访问数据库。
        /// </summary>
        public static bool IsEnableCheckUseSta
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["IsEnableCheckUseSta"] as string;
                if (s == null || s == "0")
                    return false;
                return true;
            }
        }
        /// <summary>
        /// 是否启用显示节点名称
        /// </summary>
        public static bool IsEnableMyNodeName
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["IsEnableMyNodeName"] as string;
                if (s == null || s == "0")
                    return false;
                return true;
            }
        }
        /// <summary>
        /// 检查一下当前的用户是否仍旧有效使用？
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsEnableWFEmp()
        {
            Paras ps = new Paras();
            ps.SQL = "SELECT UseSta FROM WF_Emp WHERE No=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "FK_Emp";
            ps.Add("FK_Emp", WebUser.No);

            string s = DBAccess.RunSQLReturnStringIsNull(ps, "1");
            if (s == null || s.Equals("1") == true)
                return true;
            return false;
        }
        /// <summary>
        /// 语言
        /// </summary>
        public static string Language = "CH";
        public static bool IsQL
        {
            get
            {
                string s = BP.Difference.SystemConfig.AppSettings["IsQL"];
                if (s == null || s == "0")
                    return false;
                return true;
            }
        }
        /// <summary>
        /// 是否启用共享任务池？
        /// </summary>
        public static bool IsEnableTaskPool
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyBoolen("IsEnableTaskPool", false);
            }
        }
        /// <summary>
        /// 是否显示标题
        /// </summary>
        public static bool IsShowTitle
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyBoolen("IsShowTitle", false);
            }
        }

        /// <summary>
        /// 用户信息显示格式
        /// </summary>
        public static UserInfoShowModel UserInfoShowModel
        {
            get
            {
                return (UserInfoShowModel)SystemConfig.GetValByKeyInt("UserInfoShowModel", 0);
            }
        }
        /// <summary>
        /// 产生用户数字签名
        /// </summary>
        /// <returns></returns>
        public static string GenerUserSigantureHtml(string userNo, string userName)
        {
            return "<img src='" + CCFlowAppPath + "DataUser/Siganture/" + userNo + ".jpg' title='" + userName + "' border=0 onerror=\"src='" + CCFlowAppPath + "DataUser/UserIcon/DefaultSmaller.png'\" />";
        }
        /// <summary>
        /// 产生用户小图片
        /// </summary>
        /// <returns></returns>
        public static string GenerUserImgSmallerHtml(string userNo, string userName)
        {
            return "<img src='" + CCFlowAppPath + "DataUser/UserIcon/" + userNo + "Smaller.png' border=0  style='height:15px;width:15px;padding-right:5px;vertical-align:middle;'  onerror=\"src='" + CCFlowAppPath + "DataUser/UserIcon/DefaultSmaller.png'\" />" + userName;
        }
        /// <summary>
        /// 产生用户大图片
        /// </summary>
        /// <returns></returns>
        public static string GenerUserImgHtml(string userNo, string userName)
        {
            return "<img src='" + CCFlowAppPath + "DataUser/UserIcon/" + userNo + ".png'  style='padding-right:5px;width:60px;height:80px;border:0px;text-align:middle' onerror=\"src='" + CCFlowAppPath + "DataUser/UserIcon/Default.png'\" /><br>" + userName;
        }
        /// <summary>
        /// 更新主表的SQL
        /// </summary>
        public static string UpdataMainDeptSQL
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("UpdataMainDeptSQL", "UPDATE Port_Emp SET FK_Dept=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "FK_Dept WHERE No=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "No");
            }
        }
        /// <summary>
        /// 更新SID的SQL
        /// </summary>
        public static string UpdataSID
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("UpdataSID", "UPDATE Port_Emp SET SID=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "SID WHERE No=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "No");
            }
        }
        /// <summary>
        /// 处理显示格式
        /// </summary>
        /// <param name="no"></param>
        /// <param name="name"></param>
        /// <returns>现实格式</returns>
        public static string DealUserInfoShowModel(string no, string name)
        {
            switch (BP.WF.Glo.UserInfoShowModel)
            {
                case UserInfoShowModel.UserIDOnly:
                    return no;
                case UserInfoShowModel.UserIDUserName:
                    // return "(" + no + "," + name + ")";
                    return no + "," + name;
                case UserInfoShowModel.UserNameOnly:
                    //return "(" + name + ")";
                    return name;
                default:
                    throw new Exception("@没有判断的格式类型.");
                    break;
            }
        }

        /// <summary>
        /// 处理人员显示格式
        /// <para>added by liuxc,2017-4-27</para>
        /// </summary>
        /// <param name="emps">人员字符串，类似“duqinglian,杜清莲;wangyihan,王一涵;”</param>
        /// <param name="idBefore">是否用户id在前面、用户name在后面</param>
        /// <returns></returns>
        public static string DealUserInfoShowModel(string emps, bool idBefore = true)
        {
            if (string.IsNullOrWhiteSpace(emps))
                return emps;

            bool haveKH = emps.StartsWith("(");

            if (haveKH)
                emps = emps.Replace("(", "").Replace(")", "");

            string[] es = emps.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string newEmps = haveKH ? "(" : string.Empty;
            string[] ess = null;

            switch (BP.WF.Glo.UserInfoShowModel)
            {
                case UserInfoShowModel.UserIDOnly:
                    foreach (string e in es)
                    {
                        ess = e.Split(',');

                        if (ess.Length == 1)
                        {
                            newEmps += ess[0] + ";";
                            continue;
                        }

                        newEmps += (idBefore ? ess[0] : ess[1]) + ";";
                    }

                    return haveKH ? (newEmps + ")") : newEmps;
                case UserInfoShowModel.UserNameOnly:
                    foreach (string e in es)
                    {
                        ess = e.Split(',');

                        if (ess.Length == 1)
                        {
                            newEmps += ess[0] + ";";
                            continue;
                        }

                        newEmps += (idBefore ? ess[1] : ess[0]) + ";";
                    }

                    return haveKH ? (newEmps + ")") : newEmps;
                default:
                    return emps;
            }
        }
        /// <summary>
        /// 钉钉是否启用
        /// </summary>
        public static bool IsEnable_DingDing
        {
            get
            {
                //如果两个参数都不为空说明启用
                string corpid = BP.Difference.SystemConfig.Ding_CorpID;
                string corpsecret = BP.Difference.SystemConfig.Ding_CorpSecret;
                if (string.IsNullOrEmpty(corpid) || string.IsNullOrEmpty(corpsecret))
                    return false;

                return true;
            }
        }
        /// <summary>
        /// 微信是否启用
        /// </summary>
        public static bool IsEnable_WeiXin
        {
            get
            {
                //如果两个参数都不为空说明启用
                string corpid = BP.Difference.SystemConfig.WX_CorpID;
                string corpsecret = BP.Difference.SystemConfig.WX_AppSecret;
                if (string.IsNullOrEmpty(corpid) || string.IsNullOrEmpty(corpsecret))
                    return false;

                return true;
            }
        }
        /// <summary>
        /// 是否检查表单树字段填写是否为空
        /// </summary>
        public static bool IsEnableCheckFrmTreeIsNull
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyBoolen("IsEnableCheckFrmTreeIsNull", true);
            }
        }
        /// <summary>
        /// 是否启用消息系统消息。
        /// </summary>
        public static bool IsEnableSysMessage
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyBoolen("IsEnableSysMessage", true);
            }
        }
        /// <summary>
        /// 与ccflow流程服务相关的配置: 执行自动任务节点，间隔的时间，以分钟计算，默认为2分钟。
        /// </summary>
        public static int AutoNodeDTSTimeSpanMinutes
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyInt("AutoNodeDTSTimeSpanMinutes", 60);
            }
        }
        /// <summary>
        /// ccim集成的数据库.
        /// 是为了向ccim写入消息.
        /// </summary>
        public static string CCIMDBName
        {
            get
            {
                string baseUrl = BP.Difference.SystemConfig.AppSettings["CCIMDBName"];
                if (string.IsNullOrEmpty(baseUrl) == true)
                    baseUrl = "ccPort.dbo";
                return baseUrl;
            }
        }
        /// <summary>
        /// 主机
        /// </summary>
        public static string HostURL
        {
            get
            {
                if (BP.Difference.SystemConfig.IsBSsystem)
                {
                    /* 如果是BS 就要求 路径.*/
                }

                string baseUrl = BP.Difference.SystemConfig.AppSettings["HostURL"];
                if (string.IsNullOrEmpty(baseUrl) == true)
                    baseUrl = "http://127.0.0.1/";

                if (baseUrl.Substring(baseUrl.Length - 1) != "/")
                    baseUrl = baseUrl + "/";
                return baseUrl;
            }
        }
        /// <summary>
        /// 移动端主机
        /// </summary>
        public static string MobileURL
        {
            get
            {
                if (BP.Difference.SystemConfig.IsBSsystem)
                {
                    /* 如果是BS 就要求 路径.*/
                }

                string baseUrl = BP.Difference.SystemConfig.AppSettings["BpmMobileAddress"];
                if (string.IsNullOrEmpty(baseUrl) == true)
                    baseUrl = "http://127.0.0.1/";

                if (baseUrl.Substring(baseUrl.Length - 1) != "/")
                    baseUrl = baseUrl + "/";
                return baseUrl;
            }
        }
        #endregion

        #region 时间计算.
        /// <summary>
        /// 设置成工作时间
        /// </summary>
        /// <param name="DateTime"></param>
        /// <returns></returns>
        public static DateTime SetToWorkTime(DateTime dt)
        {
            if (BP.Sys.GloVar.Holidays.Contains(dt.ToString("MM-dd")))
            {
                dt = dt.AddDays(1);
                /*如果当前是节假日，就要从下一个有效期计算。*/
                while (true)
                {
                    if (BP.Sys.GloVar.Holidays.Contains(dt.ToString("MM-dd")) == false)
                        break;
                    dt = dt.AddDays(1);
                }

                //从下一个上班时间计算.
                dt = DataType.ParseSysDate2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.AMFrom);
                return dt;
            }

            int timeInt = int.Parse(dt.ToString("HHmm"));

            //判断是否在A区间, 如果是，就返回A区间的时间点.
            if (Glo.AMFromInt >= timeInt)
                return DataType.ParseSysDate2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.AMFrom);


            //判断是否在E区间, 如果是就返回第2天的上班时间点.
            if (Glo.PMToInt <= timeInt)
            {
                return DataType.ParseSysDate2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMTo);
            }

            //如果在午休时间点中间.
            if (Glo.AMToInt <= timeInt && Glo.PMFromInt > timeInt)
            {
                return DataType.ParseSysDate2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMFrom);
            }
            return dt;
        }
        /// <summary>
        /// 在指定的日期上增加小时数。
        /// 1，扣除午休。
        /// 2，扣除节假日。
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="hours"></param>
        /// <returns></returns>
        private static DateTime AddMinutes(DateTime dt, int hh, int minutes)
        {
            if (1 == 1)
            {
                dt = dt.AddHours(hh);
                dt = dt.AddMinutes(minutes);
                return dt;
            }

            //如果没有设置,就返回.
            if (minutes == 0 && hh == 0)
            {
                return dt;
            }

            //设置成工作时间.
            dt = SetToWorkTime(dt);

            //首先判断是否是在一天整的时间完成.
            if (minutes == Glo.AMPMHours * 60)
            {
                /*如果需要在一天完成*/
                dt = DataType.AddDays(dt, 1, TWay.Holiday);
                return dt;
            }

            //判断是否是AM.
            bool isAM = false;
            int timeInt = int.Parse(dt.ToString("HHmm"));
            if (Glo.AMToInt > timeInt)
                isAM = true;

            #region 如果是当天的情况.
            //如果规定的时间在 1天之内.
            if (minutes / 60 / Glo.AMPMHours < 1)
            {
                if (isAM == true)
                {
                    /*如果是中午, 中午到中午休息之间的时间. */

                    TimeSpan ts = DataType.ParseSysDateTime2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.AMTo) - dt;
                    if (ts.TotalMinutes >= minutes)
                    {
                        /*如果剩余的分钟大于 要增加的分钟数，就是说+上分钟后，仍然在中午，就直接增加上这个分钟，让其返回。*/
                        return dt.AddMinutes(minutes);
                    }
                    else
                    {
                        // 求出到下班时间的分钟数。
                        TimeSpan myts = DataType.ParseSysDateTime2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMTo) - dt;

                        // 扣除午休的时间.
                        int leftMuit = (int)(myts.TotalMinutes - Glo.AMPMTimeSpan * 60);
                        if (leftMuit - minutes >= 0)
                        {
                            /*说明还是在当天的时间内.*/
                            DateTime mydt = DataType.ParseSysDateTime2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMTo);
                            return mydt.AddMinutes(minutes - leftMuit);
                        }

                        //说明要跨到第2天上去了.
                        dt = DataType.AddDays(dt, 1, TWay.Holiday);
                        return Glo.AddMinutes(dt.ToString("yyyy-MM-dd") + " " + Glo.AMFrom, minutes - leftMuit);
                    }

                    // 把当前的时间加上去.
                    dt = dt.AddMinutes(minutes);

                    //判断是否是中午.
                    bool isInAM = false;
                    timeInt = int.Parse(dt.ToString("HHmm"));
                    if (Glo.AMToInt >= timeInt)
                        isInAM = true;

                    if (isInAM == true)
                    {
                        // 加上时间后仍然是中午就返回.
                        return dt;
                    }

                    //延迟一个午休时间.
                    dt = dt.AddHours(Glo.AMPMTimeSpan);

                    //判断时间点是否落入了E区间.
                    timeInt = int.Parse(dt.ToString("HHmm"));
                    if (Glo.PMToInt <= timeInt)
                    {
                        /*如果落入了E区间.*/

                        // 求出来时间点到，下班之间的分钟数.
                        TimeSpan tsE = dt - DataType.ParseSysDate2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMTo);

                        //从次日的上班时间计算+ 这个时间差. 
                        dt = DataType.ParseSysDate2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMTo);
                        return dt.AddMinutes(tsE.TotalMinutes);
                    }
                    else
                    {
                        /*过了第2天的情况很少，就不考虑了.*/
                        return dt;
                    }
                }
                else
                {
                    /*如果是下午, 计算出来到下午下班还需多少分钟，与增加的分钟数据相比较. */
                    TimeSpan ts = DataType.ParseSysDateTime2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.PMTo) - dt;
                    if (ts.TotalMinutes >= minutes)
                    {
                        /*如果剩余的分钟大于 要增加的分钟数，就直接增加上这个分钟，让其返回。*/
                        return dt.AddMinutes(minutes);
                    }
                    else
                    {

                        //剩余的分钟数 = 总分钟数 - 今天下午剩余的分钟数.
                        int leftMin = minutes - (int)ts.TotalMinutes;

                        /*否则要计算到第2天上去了， 计算时间要从下一个有效的工作日上班时间开始. */
                        dt = DataType.AddDays(DataType.ParseSysDateTime2DateTime(dt.ToString("yyyy-MM-dd") + " " + Glo.AMFrom), 1, TWay.Holiday);

                        //递归调用,让其在次日的上班时间在增加，分钟数。
                        return Glo.AddMinutes(dt, 0, leftMin);
                    }

                }
            }
            #endregion 如果是当天的情况.

            return dt;
        }
        /// <summary>
        /// 增加分钟数.
        /// </summary>
        /// <param name="sysdt"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static DateTime AddMinutes(string sysdt, int minutes)
        {
            DateTime dt = DataType.ParseSysDate2DateTime(sysdt);
            return AddMinutes(dt, 0, minutes);
        }
        /// <summary>
        /// 在指定的日期上增加n天n小时，并考虑节假日
        /// </summary>
        /// <param name="sysdt">指定的日期</param>
        /// <param name="day">天数</param>
        /// <param name="minutes">分钟数</param>
        /// <returns>返回计算后的日期</returns>
        public static DateTime AddDayHoursSpan(string specDT, int day, int hh, int minutes, TWay tway)
        {
            DateTime mydt = DataType.AddDays(specDT, day, tway);
            return Glo.AddMinutes(mydt, hh, minutes);
        }
        /// <summary>
        /// 在指定的日期上增加n天n小时，并考虑节假日
        /// </summary>
        /// <param name="sysdt">指定的日期</param>
        /// <param name="day">天数</param>
        /// <param name="minutes">分钟数</param>
        /// <returns>返回计算后的日期</returns>
        public static DateTime AddDayHoursSpan(DateTime specDT, int day, int hh, int minutes, TWay tway)
        {
            DateTime mydt = DataType.AddDays(specDT, day, tway);
            mydt = mydt.AddHours(hh); //加小时.
            mydt = mydt.AddMinutes(minutes); //加分钟.
            return mydt;
            //return Glo.AddMinutes(mydt, minutes);
        }

        #endregion ssxxx.

        #region 与考核相关.
        /// <summary>
        /// 当流程发送下去以后，就开始执行考核。
        /// </summary>
        /// <param name="fl"></param>
        /// <param name="nd"></param>
        /// <param name="workid"></param>
        /// <param name="fid"></param>
        /// <param name="title"></param>
        public static void InitCH(Flow fl, Node nd, Int64 workid, Int64 fid, string title, GenerWorkerList gwl = null)
        {
            InitCH2017(fl, nd, workid, fid, title, null, null, DateTime.Now, gwl);
        }
        /// <summary>
        /// 执行考核
        /// </summary>
        /// <param name="fl">流程</param>
        /// <param name="nd">节点</param>
        /// <param name="workid">工作ID</param>
        /// <param name="fid">FID</param>
        /// <param name="title">标题</param>
        /// <param name="prvRDT">上一个时间点</param>
        /// <param name="sdt">应完成日期</param>
        /// <param name="dtNow">当前日期</param>
        private static void InitCH2017(Flow fl, Node nd, Int64 workid, Int64 fid, string title, string prvRDT, string sdt,
            DateTime dtNow, GenerWorkerList gwl)
        {

            // 开始节点不考核.
            if (nd.IsStartNode || nd.HisCHWay == CHWay.None)
                return;

            //如果设置为0,则不考核.
            if (nd.TimeLimit == 0 && nd.TimeLimitHH == 0 && nd.TimeLimitMM == 0)
                return;

            if (dtNow == null)
                dtNow = DateTime.Now;

            #region 求参与人员 todoEmps ，应完成日期 sdt ，与工作派发日期 prvRDT.
            //参与人员.
            string todoEmps = "";
            string dbstr = BP.Difference.SystemConfig.AppCenterDBVarStr;
            if (nd.IsEndNode == true && gwl == null)
            {
                /* 如果是最后一个节点，可以使用这样的方式来求人员信息 , */

                #region 求应完成日期，与参与的人集合.
                Paras ps = new Paras();
                switch (BP.Difference.SystemConfig.AppCenterDBType)
                {
                    case DBType.MSSQL:
                        ps.SQL = "SELECT TOP 1 SDTOfNode, TodoEmps FROM WF_GenerWorkFlow  WHERE WorkID=" + dbstr + "WorkID ";
                        break;
                    case DBType.Oracle:
                    case DBType.KingBaseR3:
                    case DBType.KingBaseR6:
                        ps.SQL = "SELECT SDTOfNode, TodoEmps FROM WF_GenerWorkFlow  WHERE WorkID=" + dbstr + "WorkID  ";
                        break;
                    case DBType.MySQL:
                        ps.SQL = "SELECT SDTOfNode, TodoEmps FROM WF_GenerWorkFlow  WHERE WorkID=" + dbstr + "WorkID  ";
                        break;
                    case DBType.PostgreSQL:
                    case DBType.UX:
                        ps.SQL = "SELECT SDTOfNode, TodoEmps FROM WF_GenerWorkFlow  WHERE WorkID=" + dbstr + "WorkID  ";
                        break;
                    default:
                        throw new Exception("err@没有判断的数据库类型.");
                }

                ps.Add("WorkID", workid);
                DataTable dt = DBAccess.RunSQLReturnTable(ps);
                if (dt.Rows.Count == 0)
                    return;
                sdt = dt.Rows[0]["SDTOfNode"].ToString(); //应完成日期.
                todoEmps = dt.Rows[0]["TodoEmps"].ToString(); //参与人员.
                #endregion 求应完成日期，与参与的人集合.

                #region 求上一个节点的日期.
                dt = Dev2Interface.Flow_GetPreviousNodeTrack(workid, nd.NodeID);
                if (dt.Rows.Count == 0)
                    return;
                //上一个节点的活动日期.
                prvRDT = dt.Rows[0]["RDT"].ToString();
                #endregion
            }


            if (nd.IsEndNode == false)
            {
                if (gwl == null)
                {
                    gwl = new GenerWorkerList();
                    gwl.Retrieve(GenerWorkerListAttr.WorkID, workid,
                        GenerWorkerListAttr.FK_Node, nd.NodeID,
                        GenerWorkerListAttr.FK_Emp, WebUser.No);
                }

                prvRDT = gwl.RDT; // dt.Rows[0]["RDT"].ToString(); //上一个时间点的记录日期.
                sdt = gwl.SDT; //  dt.Rows[0]["SDT"].ToString(); //应完成日期.
                todoEmps = WebUser.No + "," + WebUser.Name + ";";
            }
            #endregion 求参与人员，应完成日期，与工作派发日期.

            #region 求 preSender上一个发送人，preSenderText 发送人姓名
            string preSender = "";
            string preSenderText = "";
            DataTable dt_Sender = Dev2Interface.Flow_GetPreviousNodeTrack(workid, nd.NodeID);
            if (dt_Sender.Rows.Count > 0)
            {
                preSender = dt_Sender.Rows[0]["EmpFrom"].ToString();
                preSenderText = dt_Sender.Rows[0]["EmpFromT"].ToString();
            }
            #endregion

            #region 初始化基础数据.
            BP.WF.Data.CH ch = new CH();
            ch.WorkID = workid;
            ch.FID = fid;
            ch.Title = title;

            //记录当时设定的值.
            ch.TimeLimit = nd.TimeLimit;

            ch.FK_NY = dtNow.ToString("yyyy-MM");

            ch.DTFrom = prvRDT;  //任务下达时间.
            ch.DTTo = dtNow.ToString("yyyy-MM-dd HH:mm:ss"); //时间到.

            ch.SDT = sdt; //应该完成时间.

            ch.FK_Flow = nd.FK_Flow; //流程信息.
            ch.FK_FlowT = nd.FlowName;

            ch.FK_Node = nd.NodeID; //节点.
            ch.FK_NodeT = nd.Name;

            ch.FK_Dept = WebUser.FK_Dept; //部门.
            ch.FK_DeptT = WebUser.FK_DeptName;

            ch.FK_Emp = WebUser.No;//当事人.
            ch.FK_EmpT = WebUser.Name;

            // 处理相关联的当事人.
            ch.GroupEmpsNames = todoEmps;
            //上一步发送人
            ch.Sender = preSender;
            ch.SenderT = preSenderText;
            //考核状态
            ch.DTSWay = (int)nd.HisCHWay;

            //求参与人员数量.
            string[] strs = todoEmps.Split(';');
            ch.GroupEmpsNum = strs.Length - 1; //个数.

            //求参与人的ids.
            string empids = ",";
            foreach (string str in strs)
            {
                if (string.IsNullOrEmpty(str))
                    continue;

                string[] mystr = str.Split(',');
                empids += mystr[0] + ",";
            }
            ch.GroupEmps = empids;

            // mypk.
            ch.setMyPK(nd.NodeID + "_" + workid + "_" + fid + "_" + WebUser.No);
            #endregion 初始化基础数据.

            #region 求计算属性.
            //求出是第几个周.
            System.Globalization.CultureInfo myCI =
                        new System.Globalization.CultureInfo("zh-CN");
            ch.WeekNum = myCI.Calendar.GetWeekOfYear(dtNow, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);

            // UseDays . 求出实际使用天数.
            DateTime dtFrom = DataType.ParseSysDate2DateTime(ch.DTFrom);
            DateTime dtTo = DataType.ParseSysDate2DateTime(ch.DTTo);

            TimeSpan ts = dtTo - dtFrom;
            ch.UseDays = ts.Days;//用时，天数
            ch.UseMinutes = ts.Minutes;//用时，分钟
            //int hour = ts.Hours;
            //ch.UseDays += ts.Hours / 8; //使用的天数.
            if (DataType.IsNullOrEmpty(ch.SDT) == false && ch.SDT.Equals("无") == false)
            {
                // OverDays . 求出 逾期天 数.
                DateTime sdtOfDT = DataType.ParseSysDate2DateTime(ch.SDT);

                TimeSpan myts = dtTo - sdtOfDT;
                ch.OverDays = myts.Days; //逾期的天数.
                ch.OverMinutes = myts.Minutes;//逾期的分钟数
                if (sdtOfDT >= dtTo)
                {
                    /* 正常完成 */
                    ch.CHSta = CHSta.AnQi; //按期完成.
                    ch.Points = 0;
                }
                else
                {
                    /*逾期完成.*/
                    ch.CHSta = CHSta.YuQi; //逾期完成.
                    ch.Points = float.Parse((ch.OverDays * nd.TCent).ToString("0.00"));
                }
            }
            else
            {
                /* 正常完成 */
                ch.CHSta = CHSta.AnQi; //按期完成.
                ch.Points = 0;
            }

            #endregion 求计算属性.
            if (SystemConfig.CCBPMRunModel != CCBPMRunModel.Single)
                ch.SetValByKey(CHAttr.OrgNo, WebUser.OrgNo);
            //执行保存.
            try
            {
                ch.DirectInsert();
            }
            catch
            {
                if (ch.IsExits == true)
                {
                    ch.Update();
                }
                else
                {
                    //如果遇到退回的情况就可能涉及到主键重复的问题.
                    ch.setMyPK(DBAccess.GenerGUID());
                    ch.Insert();
                }
            }
        }
        /// <summary>
        /// 中午时间从
        /// </summary>
        public static string AMFrom
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("AMFrom", "08:30");
            }
        }
        /// <summary>
        /// 中午时间从
        /// </summary>
        public static int AMFromInt
        {
            get
            {
                return int.Parse(Glo.AMFrom.Replace(":", ""));
            }
        }
        /// <summary>
        /// 一天有效的工作小时数
        /// 是中午工作小时+下午工作小时.
        /// </summary>
        public static float AMPMHours
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyFloat("AMPMHours", 8);
            }
        }
        /// <summary>
        /// 中午间隔的小时数
        /// </summary>
        public static float AMPMTimeSpan
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKeyFloat("AMPMTimeSpan", 1);
            }
        }
        /// <summary>
        /// 中午时间到
        /// </summary>
        public static string AMTo
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("AMTo", "11:30");
            }
        }
        /// <summary>
        /// 中午时间到
        /// </summary>
        public static int AMToInt
        {
            get
            {
                return int.Parse(Glo.AMTo.Replace(":", ""));
            }
        }
        /// <summary>
        /// 下午时间从
        /// </summary>
        public static string PMFrom
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("PMFrom", "13:30");
            }
        }
        /// <summary>
        /// 到
        /// </summary>
        public static int PMFromInt
        {
            get
            {
                return int.Parse(Glo.PMFrom.Replace(":", ""));
            }
        }
        /// <summary>
        /// 到
        /// </summary>
        public static string PMTo
        {
            get
            {
                return BP.Difference.SystemConfig.GetValByKey("PMTo", "17:30");
            }
        }
        /// <summary>
        /// 到
        /// </summary>
        public static int PMToInt
        {
            get
            {
                return int.Parse(Glo.PMTo.Replace(":", ""));
            }
        }
        #endregion 与考核相关.

        #region 其他方法。

        /// <summary>
        /// 删除临时文件
        /// </summary>
        public static void DeleteTempFiles()
        {
            try
            {
                //删除目录.
                string temp = BP.Difference.SystemConfig.PathOfTemp;
                System.IO.Directory.Delete(temp, true);

                //创建目录.
                System.IO.Directory.CreateDirectory(temp);

                //删除pdf 目录.
                temp = BP.Difference.SystemConfig.PathOfDataUser + "InstancePacketOfData/";
                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(temp);
                System.IO.DirectoryInfo[] dirs = info.GetDirectories();
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    if (dir.Name.IndexOf("ND") == 0)
                        dir.Delete(true);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static BP.Sys.FrmAttachmentDBs GenerFrmAttachmentDBs(FrmAttachment athDesc, string pkval, string FK_FrmAttachment,
            Int64 workid = 0, Int64 fid = 0, Int64 pworkid = 0, bool isContantSelf = true, int fk_node = 0, string fk_mapData = null)
        {
            if (pkval == null)
                pkval = "0"; //解决预览的时候的错误.

            BP.Sys.FrmAttachmentDBs dbs = new BP.Sys.FrmAttachmentDBs();
            //查询使用的workId
            string ctrlWayId = "";
            if (FK_FrmAttachment.Contains("AthMDtl") == true || athDesc.GetParaBoolen("IsDtlAth")== true)
                ctrlWayId = pkval;
            else
            {
                MapData mapData = new MapData(athDesc.FK_MapData);
                if (mapData.EntityType == EntityType.FrmDict || mapData.EntityType == EntityType.FrmBill)
                    ctrlWayId = pkval;
                else
                    ctrlWayId = BP.WF.Dev2Interface.GetAthRefPKVal(workid, pworkid, fid, fk_node, fk_mapData, athDesc);
            }


            //如果是空的，就返回空数据结构. @lizhen.
            if (ctrlWayId.Equals("0") == true)
                return dbs;

            BP.En.QueryObject qo = new BP.En.QueryObject(dbs);
            //从表附件
            if (FK_FrmAttachment.Contains("AthMDtl") || athDesc.GetParaBoolen("IsDtlAth") == true)
            {
                /*如果是一个明细表的多附件，就直接按照传递过来的PK来查询.*/
                qo.AddWhere(FrmAttachmentDBAttr.RefPKVal, pkval);
                qo.addAnd();
                qo.AddWhere(FrmAttachmentDBAttr.NoOfObj, athDesc.NoOfObj);
                qo.DoQuery();
                return dbs;
            }
            if (athDesc.HisCtrlWay == AthCtrlWay.MySelfOnly || athDesc.HisCtrlWay == AthCtrlWay.PK)
            {
                qo.AddWhere(FrmAttachmentDBAttr.RefPKVal, pkval);
                qo.addAnd();
                qo.AddWhere(FrmAttachmentDBAttr.FK_FrmAttachment, FK_FrmAttachment);
                if (isContantSelf == false)
                {
                    qo.addAnd();
                    qo.AddWhere(FrmAttachmentDBAttr.Rec, "!=", WebUser.No);
                }
                qo.addOrderBy("Idx,RDT");
                qo.DoQuery();
                return dbs;
            }

            /* 继承模式 */
            if (athDesc.AthUploadWay == AthUploadWay.Interwork)
                qo.AddWhere(FrmAttachmentDBAttr.RefPKVal, ctrlWayId);
            else
                qo.AddWhereIn(FrmAttachmentDBAttr.RefPKVal, "('" + ctrlWayId + "','" + pkval + "')");

            qo.addAnd();
            qo.AddWhere(FrmAttachmentDBAttr.NoOfObj, athDesc.NoOfObj);

            if (isContantSelf == false)
            {
                qo.addAnd();
                qo.AddWhere(FrmAttachmentDBAttr.Rec, "!=", WebUser.No);
            }
            qo.addOrderBy("Idx,RDT");
            qo.DoQuery();
            return dbs;
        }
        /// <summary>
        /// 获得一个表单的动态附件字段
        /// </summary>
        /// <param name="exts">扩展</param>
        /// <param name="nd">节点</param>
        /// <param name="en">实体</param>
        /// <param name="md">map</param>
        /// <param name="attrs">属性集合</param>
        /// <returns>附件的主键</returns>
        public static string GenerActiveAths(MapExts exts, Node nd, Entity en, MapData md, MapAttrs attrs)
        {
            string strs = "";
            foreach (MapExt me in exts)
            {
                if (me.ExtType != MapExtXmlList.SepcAthSepcUsers)
                    continue;

                bool isCando = false;
                if (me.Tag1 != "")
                {
                    string tag1 = me.Tag1 + ",";
                    if (tag1.Contains(BP.Web.WebUser.No + ","))
                    {
                        //根据设置的人员计算.
                        isCando = true;
                    }
                }

                if (me.Tag2 != "")
                {
                    //根据sql判断.
                    string sql = me.Tag2.Clone() as string;
                    sql = BP.WF.Glo.DealExp(sql, en);
                    if (DBAccess.RunSQLReturnValFloat(sql) > 0)
                        isCando = true;
                }

                if (me.Tag3 != "" && BP.Web.WebUser.FK_Dept == me.Tag3)
                {
                    //根据部门编号判断.
                    isCando = true;
                }

                if (isCando == false)
                    continue;
                strs += me.Doc;
            }
            return strs;
        }
        /// <summary>
        /// 获得一个表单的动态权限字段
        /// </summary>
        /// <param name="exts"></param>
        /// <param name="nd"></param>
        /// <param name="en"></param>
        /// <param name="md"></param>
        /// <param name="attrs"></param>
        /// <returns></returns>
        public static string GenerActiveFiels(MapExts exts, Node nd, Entity en, MapData md, MapAttrs attrs)
        {
            string strs = "";
            foreach (MapExt me in exts)
            {
                if (me.ExtType != MapExtXmlList.SepcFiledsSepcUsers)
                    continue;

                bool isCando = false;
                if (me.Tag1 != "")
                {
                    string tag1 = me.Tag1 + ",";
                    if (tag1.Contains(BP.Web.WebUser.No + ","))
                    {
                        //根据设置的人员计算.
                        isCando = true;
                    }
                }

                if (me.Tag2 != "")
                {
                    //根据sql判断.
                    string sql = me.Tag2.Clone() as string;
                    sql = BP.WF.Glo.DealExp(sql, en);
                    if (DBAccess.RunSQLReturnValFloat(sql) > 0)
                        isCando = true;
                }

                if (me.Tag3 != "" && BP.Web.WebUser.FK_Dept == me.Tag3)
                {
                    //根据部门编号判断.
                    isCando = true;
                }

                if (isCando == false)
                    continue;
                strs += me.Doc;
            }
            return strs;
        }
        /// <summary>
        /// 检查流程发起限制
        /// </summary>
        /// <param name="flow">流程</param>
        /// <param name="wk">开始节点工作</param>
        /// <returns></returns>
        public static bool CheckIsCanStartFlow_InitStartFlow(Flow flow)
        {
            StartLimitRole role = flow.StartLimitRole;
            if (role == StartLimitRole.None)
                return true;

            string sql = "";
            string ptable = flow.PTable;

            try
            {
                #region 按照时间的必须是，在表单加载后判断, 不管用户设置是否正确.
                DateTime dtNow = DateTime.Now;
                if (role == StartLimitRole.Day)
                {
                    /* 仅允许一天发起一次 */
                    sql = "SELECT COUNT(*) as Num FROM " + ptable + " WHERE RDT LIKE '" + DataType.CurrentDate + "%' AND WFState NOT IN(0,1) AND FlowStarter='" + WebUser.No + "'";
                    if (DBAccess.RunSQLReturnValInt(sql, 0) == 0)
                    {
                        if (DataType.IsNullOrEmpty(flow.StartLimitPara))
                            return true;

                        //判断时间是否在设置的发起范围内. 配置的格式为 @11:00-12:00@15:00-13:45
                        string[] strs = flow.StartLimitPara.Split('@');
                        foreach (string str in strs)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;
                            string[] timeStrs = str.Split('-');
                            string tFrom = DateTime.Now.ToString("yyyy-MM-dd") + " " + timeStrs[0].Trim();
                            string tTo = DateTime.Now.ToString("yyyy-MM-dd") + " " + timeStrs[1].Trim();
                            if (DataType.ParseSysDateTime2DateTime(tFrom) <= dtNow && DataType.ParseSysDateTime2DateTime(tTo) >= dtNow)
                                return true;
                        }
                        return false;
                    }
                    else
                        return false;
                }

                if (role == StartLimitRole.Week)
                {
                    /*
                     * 1, 找出周1 与周日分别是第几日.
                     * 2, 按照这个范围去查询,如果查询到结果，就说明已经启动了。
                     */
                    sql = "SELECT COUNT(*) as Num FROM " + ptable + " WHERE RDT >= '" + DataType.WeekOfMonday(dtNow) + "' AND WFState NOT IN(0,1) AND FlowStarter='" + WebUser.No + "'";
                    if (DBAccess.RunSQLReturnValInt(sql, 0) == 0)
                    {
                        if (DataType.IsNullOrEmpty(flow.StartLimitPara))
                            return true; /*如果没有时间的限制.*/

                        //判断时间是否在设置的发起范围内. 
                        // 配置的格式为 @Sunday,11:00-12:00@Monday,15:00-13:45, 意思是.周日，周一的指定的时间点范围内可以启动流程.

                        string[] strs = flow.StartLimitPara.Split('@');
                        foreach (string str in strs)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;

                            string weekStr = DateTime.Now.DayOfWeek.ToString().ToLower();
                            if (str.ToLower().Contains(weekStr) == false)
                                continue; // 判断是否当前的周.

                            string[] timeStrs = str.Split(',');
                            string tFrom = DateTime.Now.ToString("yyyy-MM-dd") + " " + timeStrs[0].Trim();
                            string tTo = DateTime.Now.ToString("yyyy-MM-dd") + " " + timeStrs[1].Trim();
                            if (DataType.ParseSysDateTime2DateTime(tFrom) <= dtNow && dtNow >= DataType.ParseSysDateTime2DateTime(tTo))
                                return true;
                        }
                        return false;
                    }
                    else
                        return false;
                }

                // #warning 没有考虑到周的如何处理.

                if (role == StartLimitRole.Month)
                {
                    sql = "SELECT COUNT(*) as Num FROM " + ptable + " WHERE FK_NY = '" + DataType.CurrentYearMonth + "' AND WFState NOT IN(0,1) AND FlowStarter='" + WebUser.No + "'";
                    if (DBAccess.RunSQLReturnValInt(sql, 0) == 0)
                    {
                        if (DataType.IsNullOrEmpty(flow.StartLimitPara))
                            return true;

                        //判断时间是否在设置的发起范围内. 配置格式: @-01 12:00-13:11@-15 12:00-13:11 , 意思是：在每月的1号,15号 12:00-13:11可以启动流程.
                        string[] strs = flow.StartLimitPara.Split('@');
                        foreach (string str in strs)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;
                            string[] timeStrs = str.Split('-');
                            string tFrom = DateTime.Now.ToString("yyyy-MM-") + " " + timeStrs[0].Trim();
                            string tTo = DateTime.Now.ToString("yyyy-MM-") + " " + timeStrs[1].Trim();
                            if (DataType.ParseSysDateTime2DateTime(tFrom) <= dtNow && dtNow >= DataType.ParseSysDateTime2DateTime(tTo))
                                return true;
                        }
                        return false;
                    }
                    else
                        return false;
                }

                if (role == StartLimitRole.JD)
                {
                    sql = "SELECT COUNT(*) as Num FROM " + ptable + " WHERE FK_NY = '" + DataType.CurrentAPOfJD + "' AND WFState NOT IN(0,1) AND FlowStarter='" + WebUser.No + "'";
                    if (DBAccess.RunSQLReturnValInt(sql, 0) == 0)
                    {
                        if (DataType.IsNullOrEmpty(flow.StartLimitPara))
                            return true;

                        //判断时间是否在设置的发起范围内.
                        string[] strs = flow.StartLimitPara.Split('@');
                        foreach (string str in strs)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;
                            string[] timeStrs = str.Split('-');
                            string tFrom = DateTime.Now.ToString("yyyy-") + " " + timeStrs[0].Trim();
                            string tTo = DateTime.Now.ToString("yyyy-") + " " + timeStrs[1].Trim();
                            if (DataType.ParseSysDateTime2DateTime(tFrom) <= dtNow && dtNow >= DataType.ParseSysDateTime2DateTime(tTo))
                                return true;
                        }
                        return false;
                    }
                    else
                        return false;
                }

                if (role == StartLimitRole.Year)
                {
                    sql = "SELECT COUNT(*) as Num FROM " + ptable + " WHERE FK_NY LIKE '" + DataType.CurrentYear + "%' AND WFState NOT IN(0,1) AND FlowStarter='" + WebUser.No + "'";
                    if (DBAccess.RunSQLReturnValInt(sql, 0) == 0)
                    {
                        if (DataType.IsNullOrEmpty(flow.StartLimitPara))
                            return true;

                        //判断时间是否在设置的发起范围内.
                        string[] strs = flow.StartLimitPara.Split('@');
                        foreach (string str in strs)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;
                            string[] timeStrs = str.Split('-');
                            string tFrom = DateTime.Now.ToString("yyyy-") + " " + timeStrs[0].Trim();
                            string tTo = DateTime.Now.ToString("yyyy-") + " " + timeStrs[1].Trim();
                            if (DataType.ParseSysDateTime2DateTime(tFrom) <= dtNow && dtNow >= DataType.ParseSysDateTime2DateTime(tTo))
                                return true;
                        }
                        return false;
                    }
                    else
                        return false;
                }
                #endregion 按照时间的必须是，在表单加载后判断, 不管用户设置是否正确.


                //为子流程的时候，该子流程只能被调用一次.
                if (role == StartLimitRole.OnlyOneSubFlow)
                {

                    if (BP.Difference.SystemConfig.IsBSsystem == true)
                    {

                        string pflowNo = HttpContextHelper.RequestParams("PFlowNo");
                        string pworkid = HttpContextHelper.RequestParams("PWorkID");

                        if (pworkid == null)
                            return true;

                        sql = "SELECT Starter, RDT FROM WF_GenerWorkFlow WHERE PWorkID=" + pworkid + " AND FK_Flow='" + flow.No + "'";
                        DataTable dt = DBAccess.RunSQLReturnTable(sql);
                        if (dt.Rows.Count == 0 || dt.Rows.Count == 1)
                            return true;

                        //  string title = dt.Rows[0]["Title"].ToString();
                        string starter = dt.Rows[0]["Starter"].ToString();
                        string rdt = dt.Rows[0]["RDT"].ToString();
                        return false;
                        //throw new Exception(flow.StartLimitAlert + "@该子流程已经被[" + starter + "], 在[" + rdt + "]发起，系统只允许发起一次。");
                    }
                }

                // 配置的sql,执行后,返回结果是 0 .
                if (role == StartLimitRole.ResultIsZero)
                {
                    sql = BP.WF.Glo.DealExp(flow.StartLimitPara, null);
                    if (DBAccess.RunSQLReturnValInt(sql, 0) == 0)
                        return true;
                    else
                        return false;
                }

                // 配置的sql,执行后,返回结果是 <> 0 .
                if (role == StartLimitRole.ResultIsNotZero)
                {
                    if (DataType.IsNullOrEmpty(flow.StartLimitPara) == true)
                        return true;

                    sql = BP.WF.Glo.DealExp(flow.StartLimitPara, null);
                    if (DBAccess.RunSQLReturnValInt(sql, 0) != 0)
                        return true;
                    else
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("err@发起限制规则[" + role.ToString() + "]出现错误:" + ex.Message);
            }
        }

        /// <summary>
        /// 复制表单权限-从一个节点到另一个节点.
        /// </summary>
        /// <param name="fk_flow">流程编号</param>
        /// <param name="frmID">表单ID</param>
        /// <param name="currNodeID">当前节点</param>
        /// <param name="fromNodeID">从节点</param>
        public static void CopyFrmSlnFromNodeToNode(string fk_flow, string frmID, int currNodeID, int fromNodeID)
        {
            #region 处理字段.
            //删除现有的.
            FrmFields frms = new FrmFields();
            frms.Delete(FrmFieldAttr.FK_Node, currNodeID, FrmFieldAttr.FK_MapData, frmID);

            //查询出来,指定的权限方案.
            frms.Retrieve(FrmFieldAttr.FK_Node, fromNodeID, FrmFieldAttr.FK_MapData, frmID);

            //开始复制.
            foreach (FrmField item in frms)
            {
                item.setMyPK(frmID + "_" + fk_flow + "_" + currNodeID + "_" + item.KeyOfEn);
                item.FK_Node = currNodeID;
                item.Insert(); // 插入数据库.
            }
            #endregion 处理字段.

            //没有考虑到附件的权限 20161020 hzm
            #region 附件权限

            FrmAttachments fas = new FrmAttachments();
            //删除现有节点的附件权限
            fas.Delete(FrmAttachmentAttr.FK_Node, currNodeID, FrmAttachmentAttr.FK_MapData, frmID);
            //查询出 现在表单上是否有附件的情况
            fas.Retrieve(FrmAttachmentAttr.FK_Node, fromNodeID, FrmAttachmentAttr.FK_MapData, frmID);

            //复制权限
            foreach (FrmAttachment fa in fas)
            {
                fa.setMyPK(fa.FK_MapData + "_" + fa.NoOfObj + "_" + currNodeID);
                fa.FK_Node = currNodeID;
                fa.Insert();
            }

            #endregion

        }
        /// <summary>
        /// 产生消息,senderEmpNo是为了保证写入消息的唯一性，receiveid才是真正的接收者.
        /// 如果插入失败.
        /// </summary>
        /// <param name="fromEmpNo">发送人</param>
        /// <param name="now">发送时间</param>
        /// <param name="msg">消息内容</param>
        /// <param name="sendToEmpNo">接受人</param>
        public static void SendMessageToCCIM(string fromEmpNo, string sendToEmpNo, string msg, string now)
        {
            //周朋.
            return;  //暂停支持.

            if (fromEmpNo == null)
                fromEmpNo = "";

            if (DataType.IsNullOrEmpty(sendToEmpNo))
                return;

            // throw new Exception("@接受人不能为空");

            string dbStr = BP.Difference.SystemConfig.AppCenterDBVarStr;
            //保存系统通知消息
            StringBuilder strHql1 = new StringBuilder();
            //加密处理
            msg = BP.Tools.SecurityDES.Encrypt(msg);

            Paras ps = new Paras();
            string sql = "INSERT INTO CCIM_RecordMsg (OID,SendID,MsgDateTime,MsgContent,ImageInfo,FontName,FontSize,FontBold,FontColor,InfoClass,GroupID,SendUserID) VALUES (";
            sql += dbStr + "OID,";
            sql += "'SYSTEM',";
            sql += dbStr + "MsgDateTime,";
            sql += dbStr + "MsgContent,";
            sql += dbStr + "ImageInfo,";
            sql += dbStr + "FontName,";
            sql += dbStr + "FontSize,";
            sql += dbStr + "FontBold,";
            sql += dbStr + "FontColor,";
            sql += dbStr + "InfoClass,";
            sql += dbStr + "GroupID,";
            sql += dbStr + "SendUserID)";
            ps.SQL = sql;

            Int64 messgeID = DBAccess.GenerOID("RecordMsgUser");

            ps.Add("OID", messgeID);
            ps.Add("MsgDateTime", now);
            ps.Add("MsgContent", msg);
            ps.Add("ImageInfo", "");
            ps.Add("FontName", "宋体");
            ps.Add("FontSize", 10);
            ps.Add("FontBold", 0);
            ps.Add("FontColor", -16777216);
            ps.Add("InfoClass", 15);
            ps.Add("GroupID", -1);
            ps.Add("SendUserID", fromEmpNo);
            DBAccess.RunSQL(ps);

            //保存消息发送对象,这个是消息的接收人表.
            ps = new Paras();
            ps.SQL = "INSERT INTO CCIM_RecordMsgUser (OID,MsgId,ReceiveID) VALUES ( ";
            ps.SQL += dbStr + "OID,";
            ps.SQL += dbStr + "MsgId,";
            ps.SQL += dbStr + "ReceiveID)";

            ps.Add("OID", messgeID);
            ps.Add("MsgId", messgeID);
            ps.Add("ReceiveID", sendToEmpNo);
            DBAccess.RunSQL(ps);
        }

        private const string StrRegex = @"-|;|,|/|(|)|[|]|}|{|%|@|*|!|'|`|~|#|$|^|&|.|?";
        private const string StrKeyWord = @"select|insert|delete|from|count(|drop table|update|truncate|asc(|mid(|char(|xp_cmdshell|exec master|netlocalgroup administrators|:|net user|""|or|and";
        /// <summary>
        /// 检查KeyWord是否包涵特殊字符
        /// </summary>
        /// <param name="_sWord">需要检查的字符串</param>
        /// <returns></returns>
        public static string CheckKeyWord(string KeyWord)
        {
            //特殊符号
            string[] strRegx = StrRegex.Split('|');
            //特殊符号 的注入情况
            foreach (string key in strRegx)
            {
                if (KeyWord.IndexOf(key) >= 0)
                {
                    //替换掉特殊字符
                    KeyWord = KeyWord.Replace(key, "");
                }
            }
            return KeyWord;
        }
        /// <summary>
        /// 检查_sword是否包涵SQL关键字
        /// </summary>
        /// <param name="_sWord">需要检查的字符串</param>
        /// <returns>存在SQL注入关键字时返回 true，否则返回 false</returns>
        public static bool CheckKeyWordInSql(string _sWord)
        {
            bool result = false;
            //Sql注入de可能关键字
            string[] patten1 = StrKeyWord.Split('|');
            //Sql注入的可能关键字 的注入情况
            foreach (string sqlKey in patten1)
            {
                if (_sWord.IndexOf(" " + sqlKey) >= 0 || _sWord.IndexOf(sqlKey + " ") >= 0)
                {
                    //只要存在一个可能出现Sql注入的参数,则直接退出
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion 其他方法。

        #region http请求
        /// <summary>
        /// Http Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpGet(string url)
        {
            try
            {
                HttpWebRequest request;
                // 创建一个HTTP请求
                request = (HttpWebRequest)WebRequest.Create(url);
                // request.Method="get";
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string responseText = myreader.ReadToEnd();
                myreader.Close();
                response.Close();
                return responseText;
            }
            catch (Exception ex)
            {
                //url请求失败
                return ex.Message;
            }
        }
        /// <summary>
        /// httppost方式发送数据
        /// </summary>
        /// <param name="url">要提交的url</param>
        /// <param name="postDataStr"></param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="encode">text code.</param>
        /// <returns>成功：返回读取内容；失败：0</returns>
        public static string HttpPostConnect(string serverUrl, string postData)
        {
            var dataArray = Encoding.UTF8.GetBytes(postData);
            //创建请求
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUrl);
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            //设置上传服务的数据格式  设置之后不好使
            //request.ContentType = "application/x-www-form-urlencoded";
            //请求的身份验证信息为默认
            request.Credentials = CredentialCache.DefaultCredentials;
            // request.ContentType = "application/x-www-form-urlencoded";
            request.ContentType = "application/json";
            //  request.ContentType = "application/x-www-form-urlencoded";

            //请求超时时间
            request.Timeout = 10000;
            //创建输入流
            Stream dataStream;
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception)
            {
                return "0";//连接服务器失败
            }
            //发送请求
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();

            HttpWebResponse res;

            res = (HttpWebResponse)request.GetResponse();

            //}
            //catch (WebException ex)
            //{
            //    throw new Exception(ex.Message);
            //    //res = (HttpWebResponse)ex.Response;
            //}
            StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            //读取返回消息
            string data = sr.ReadToEnd();
            sr.Close();
            return data;
        }
        /// <summary>
        /// httppost方式发送数据
        /// </summary>
        /// <param name="url">要提交的url</param>
        /// <param name="headerMap">自定义的请求头</param>
        /// <param name="postDataStr"></param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="encode">text code.</param>
        /// <returns>成功：返回读取内容；失败：0</returns>
        public static string HttpPostConnect(string serverUrl, Hashtable headerMap, string postData)
        {
            var dataArray = Encoding.UTF8.GetBytes(postData);
            //创建请求
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUrl);
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            //设置上传服务的数据格式  设置之后不好使
            //request.ContentType = "application/x-www-form-urlencoded";
            //请求的身份验证信息为默认
            request.Credentials = CredentialCache.DefaultCredentials;
            // request.ContentType = "application/x-www-form-urlencoded";
            request.ContentType = "application/json";
            //  request.ContentType = "application/x-www-form-urlencoded";

            //设置请求头
            if (headerMap.Count > 0)
            {
                foreach (string key in headerMap.Keys)
                {
                    request.Headers.Add(key, headerMap[key].ToString());
                }
            }

            //请求超时时间
            request.Timeout = 10000;
            //创建输入流
            Stream dataStream;
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception)
            {
                return "0";//连接服务器失败
            }
            //发送请求
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();

            HttpWebResponse res;

            res = (HttpWebResponse)request.GetResponse();

            //}
            //catch (WebException ex)
            //{
            //    throw new Exception(ex.Message);
            //    //res = (HttpWebResponse)ex.Response;
            //}
            StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            //读取返回消息
            string data = sr.ReadToEnd();
            sr.Close();
            return data;
        }
        public static string FormatMoney(string money)
        {
            //处理带有负号情况
            int negNumber = money.IndexOf("-");
            string prefix = string.Empty;
            if (negNumber != -1)
            {
                prefix = "-";
                money = money.Substring(1);
            }
            //处理有小数位情况
            int decNumber = money.IndexOf(".");
            string postfix = string.Empty;
            if (decNumber != -1)
            {
                postfix = money.Substring(decNumber);
                money = money.Substring(0, decNumber - 1);
            }
            else
            {
                postfix = ".00";
            }
            //开始添加”,“号
            if (money.Length > 3)
            {
                string str1 = money.Substring(0, money.Length - 3);
                string str2 = money.Substring(money.Length - 3, 3);
                if (str1.Length > 3)
                {
                    return prefix + FormatMoney(str1) + "," + str2 + postfix;
                }
                else
                {
                    return prefix + str1 + "," + str2 + postfix;
                }
            }
            else
            {
                return prefix + money + postfix;
            }
        }
    }
    #endregion http请求
}
