﻿using System;
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
using BP.Difference;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class DataUser_AppCoder : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DataUser_AppCoder()
        {
        }

        #region 执行父类的重写方法.
        /// <summary>
        /// 默认执行的方法
        /// </summary>
        /// <returns></returns>
        protected override string DoDefaultMethod()
        {
            switch (this.DoType)
            {
                case "DtlFieldUp": //字段上移
                    return "执行成功.";
                default:
                    break;
            }

            //找不不到标记就抛出异常.
            throw new Exception("@标记[" + this.DoType + "]，没有找到. @RowURL:" + HttpContextHelper.RequestRawUrl);
        }
        #endregion 执行父类的重写方法.

        #region 欢迎页面初始化.
        /// <summary>
        /// 欢迎页面初始化-获得数量.
        /// </summary>
        /// <returns></returns>
        public string FlowDesignerWelcome_Init()
        {
            Hashtable ht = new Hashtable();
          
            ht.Add("FlowNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(No) FROM WF_Flow where OrgNo='" + WebUser.FK_Dept + "'")); //流程数

            ht.Add("NodeNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(NodeID) FROM WF_Node t,WF_Flow t1 where t.FK_Flow = t1.no and t1.OrgNo='" + WebUser.FK_Dept + "'")); //节点数据
            //表单数.
            ht.Add("FromNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(No) FROM Sys_MapData  WHERE FK_FormTree !='' AND FK_FormTree IS NOT NULL AND  OrgNo='" + WebUser.FK_Dept + "'")); //表单数

            //所有的实例数量.
            ht.Add("FlowInstaceNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(WorkID) FROM WF_GenerWorkFlow WHERE WFState > 1  AND  OrgNo='" + WebUser.FK_Dept + "'")); //实例数.

            //所有的待办数量.
            ht.Add("TodolistNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(WorkID) FROM WF_GenerWorkFlow WHERE WFState=2  AND  OrgNo='" + WebUser.FK_Dept + "'"));

            //退回数.
            ht.Add("ReturnNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(WorkID) FROM WF_GenerWorkFlow WHERE WFState=5  AND  OrgNo='" + WebUser.FK_Dept + "'"));

            //说有逾期的数量. 应该根据 WF_GenerWorkerList的 SDT 字段来求.
            ht.Add("OverTimeNum", DBAccess.RunSQLReturnValInt("SELECT COUNT(*) FROM WF_EMPWORKS where convert(varchar(100),SDT,120) < CONVERT(varchar(100), GETDATE(), 120)   AND  OrgNo='" + WebUser.FK_Dept + "'"));
            
            return BP.Tools.Json.ToJson(ht);
        }
        /// <summary>
        /// 获得数量  流程饼图，部门柱状图，月份折线图.
        /// </summary>
        /// <returns></returns>
        public string FlowDesignerWelcome_DataSet()
        {
            DataSet ds = new DataSet();

            #region  实例分析
            //月份分组.
            string sql = "SELECT FK_NY, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState >1 AND  OrgNo='" + WebUser.FK_Dept + "' GROUP BY FK_NY ";
            DataTable FlowsByNY = DBAccess.RunSQLReturnTable(sql);
            FlowsByNY.TableName = "FlowsByNY";
            ds.Tables.Add(FlowsByNY);

            //部门分组.
            sql = "SELECT DeptName, count(WorkID) as Num FROM WF_GenerWorkFlow WHERE WFState >1 AND  OrgNo='" + WebUser.FK_Dept + "' GROUP BY DeptName ";
            DataTable FlowsByDept = DBAccess.RunSQLReturnTable(sql);
            FlowsByDept.TableName = "FlowsByDept";
            ds.Tables.Add(FlowsByDept);
            #endregion 实例分析。


            #region 待办 分析
            //待办 - 部门分组.
            sql = "SELECT DeptName, count(WorkID) as Num FROM WF_EmpWorks WHERE WFState >1 AND  OrgNo='" + WebUser.FK_Dept + "' GROUP BY DeptName";
            DataTable TodolistByDept = DBAccess.RunSQLReturnTable(sql);
            TodolistByDept.TableName = "TodolistByDept";
            ds.Tables.Add(TodolistByDept);

            //待办的 - 流程分组.
            sql = "SELECT FlowName as name, count(WorkID) as value FROM WF_EmpWorks WHERE WFState >1 AND  OrgNo='" + WebUser.FK_Dept + "' GROUP BY FlowName";
            DataTable TodolistByFlow = DBAccess.RunSQLReturnTable(sql);
            TodolistByFlow.TableName = "TodolistByFlow";
            ds.Tables.Add(TodolistByFlow);
            #endregion 待办。


            #region 逾期 分析.
            //逾期的 - 流程分组.
           // sql = "SELECT FlowName as name, count(WorkID) as value FROM WF_EmpWorks WHERE WFState >1 and convert(varchar(100),SDT,120) < CONVERT(varchar(100), GETDATE(), 120) GROUP BY FlowName";
            
            sql = "SELECT FlowName as name, count(WorkID) as value FROM WF_EmpWorks WHERE WFState >1 AND  OrgNo='" + WebUser.FK_Dept + "' GROUP BY FlowName";
            DataTable OverTimeByFlow = DBAccess.RunSQLReturnTable(sql);
            OverTimeByFlow.TableName = "OverTimeByFlow";
            ds.Tables.Add(OverTimeByFlow);

            //逾期的 - 部门分组.
           sql = "SELECT DeptName, count(WorkID) as Num FROM WF_EmpWorks WHERE WFState >1 and convert(varchar(100),SDT,120) < CONVERT(varchar(100), GETDATE(), 120) AND  OrgNo='" + WebUser.FK_Dept + "' GROUP BY DeptName";
            
            //sql = "SELECT DeptName, count(WorkID) as Num FROM WF_EmpWorks WHERE WFState >1 GROUP BY DeptName";
            DataTable OverTimeByDept = DBAccess.RunSQLReturnTable(sql);
            OverTimeByDept.TableName = "OverTimeByDept";
            ds.Tables.Add(OverTimeByDept);
            #endregion 逾期。


            return BP.Tools.Json.ToJson(ds);
        }
        #endregion 欢迎页面初始化.

    }
}
