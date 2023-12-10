using BP.DA;
using BP.En;
using BP.Sys;
using BP.Web;
using BP.WF;
using BP.WF.Template;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections;
using System.Data;
using System.Net.Http;

namespace CCFlow.DataUser.API.Controllers
{
    [Route("WF/[controller]/[Action]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="userNo"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Portal_Login(string privateKey, string userNo)
        {
            try
            {
                if (DataType.IsNullOrEmpty(userNo) == true)
                    return "err@账号不能为空";

                string localKey = BP.Difference.SystemConfig.GetValByKey("PrivateKey", "");
                if (DataType.IsNullOrEmpty(localKey) == true)
                    localKey = "DiGuaDiGua,IamCCBPM";
                if (localKey.Equals(privateKey) == false)
                    return "err@私约错误，请检查全局文件中配置 PrivateKey";


                //执行本地登录.
                BP.WF.Dev2Interface.Port_Login(userNo);
                string token = BP.WF.Dev2Interface.Port_GenerToken();

                Hashtable ht = new Hashtable();
                ht.Add("No", WebUser.No);
                ht.Add("Name", WebUser.Name);
                ht.Add("FK_Dept", WebUser.FK_Dept);
                ht.Add("FK_DeptName", WebUser.FK_DeptName);
                ht.Add("OrgNo", WebUser.OrgNo);
                ht.Add("OrgName", WebUser.OrgName);
                ht.Add("Token", token);
                return BP.Tools.Json.ToJson(ht);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Portal_LoginOut()
        {
            try
            {
                BP.WF.Dev2Interface.Port_SigOut();
                return "退出成功";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }

        }
        #region 组织人员岗位维护
        /// <summary>
        /// 人员信息保存
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orgNo"></param>
        /// <param name="userNo"></param>
        /// <param name="userName"></param>
        /// <param name="deptNo"></param>
        /// <param name="kvs"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Port_Emp_Save(string token, string orgNo, string userNo, string userName, string deptNo, string kvs, string stats)
        {
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能维护人员信息";
            string msg =  BP.Port.OrganizationAPI.Port_Emp_Save(orgNo, userNo, userName, deptNo, kvs, stats);
            return msg;

        }
        /// <summary>
        /// 人员删除
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="orgNo">组织编号</param>
        /// <param name="userNo">人员编号</param>
        /// <returns>reutrn 1=成功,  其他的标识异常.</returns>
        [HttpGet, HttpPost]
        public string Port_Emp_Delete(string token, string userNo, string orgNo ="" )
        {
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能删除人员信息";
            return BP.Port.OrganizationAPI.Port_Emp_Delete(orgNo, userNo);
        }
        /// <returns>return 1 增加成功，其他的增加失败.</returns>

        /// <summary>
        /// 集团模式下同步组织以及管理员信息
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="orgNo">组织编号</param>
        /// <param name="name">组织名称</param>
        /// <param name="adminer">管理员账号</param>
        /// <param name="adminerName">管理员名字</param>
        /// <param name="keyVals">其他的值:@Leaer=zhangsan@Tel=12233333@Idx=1</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Port_Org_Save(string token, string orgNo, string name, string adminer,string adminerName, string keyVals="")
        {
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能维护组织信息";
            return BP.Port.OrganizationAPI.Port_Org_Save(orgNo, name, adminer, adminerName,keyVals);
        }

        /// <summary>
        /// 保存部门,如果有此数据则修改,无此数据则增加.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="no">部门编号</param>
        /// <param name="name">名称</param>
        /// <param name="parentNo">父节点编号</param>
        /// <param name="orgNo">组织编号</param>
        /// <param name="keyVals">其他的值:@Leaer=zhangsan@Tel=18660153393@Idx=1</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Port_Dept_Save(string token, string no, string name, string parentNo, string orgNo="", string keyVals="")
        {
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能维护部门信息";
            return BP.Port.OrganizationAPI.Port_Dept_Save(orgNo, no, name, parentNo, keyVals);

        }

        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="no">要删除的部门编号</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Port_Dept_Delete(string token, string no)
        {
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能删除部门信息";
              return BP.Port.OrganizationAPI.Port_Dept_Delete(no);
        }

        /// <summary>
        /// 保存岗位, 如果有此数据则修改，无此数据则增加.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="orgNo">组织编号</param>
        /// <param name="no">岗位编号</param>
        /// <param name="name">岗位名称</param>
        /// <param name="keyVals">其他值</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Port_Station_Save(string token, string orgNo, string no, string name, string keyVals)
        {
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能维护岗位信息";
            return BP.Port.OrganizationAPI.Port_Station_Save(orgNo, no, name, keyVals);
        }
        /// <summary>
        /// 删除岗位.
        /// </summary>
        /// <param name="no">删除指定的岗位编号</param>
        /// <returns></returns>
         [HttpGet,HttpPost]
        public string Port_Station_Delete(string token, string no)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (WebUser.IsAdmin == false)
                return "err@[" + BP.Web.WebUser.Name + "]不是管理员不能删除岗位信息";
            return BP.Port.OrganizationAPI.Port_Station_Delete(no);
        }
        #endregion  组织人员岗位维护
        /// <summary>
        /// 可以发起的流程
        /// </summary>
        /// <param name="token"></param>
        /// <param name="domain">流程所属的域</param>
        /// <returns></returns>
        [HttpGet,HttpPost]
        public string DB_Start(string token,string domain)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            //获取可以发起的列表
            DataTable dt = BP.WF.Dev2Interface.DB_StarFlows(BP.Web.WebUser.No, domain);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 待办
        /// </summary>
        /// <param name="token"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string DB_Todolist(string token, string domain)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            //获取可以发起的列表
            DataTable dt = BP.WF.Dev2Interface.DB_GenerEmpWorksOfDataTable(BP.Web.WebUser.No, 0, null, domain, null, null);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 在途
        /// </summary>
        /// <param name="token"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string DB_Runing(string token, string domain)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            //获取可以发起的列表
            DataTable dt = BP.WF.Dev2Interface.DB_GenerRuning(BP.Web.WebUser.No, false, domain); ;
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 草稿
        /// </summary>
        /// <param name="token"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string DB_Draft(string token, string domain)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            //获取可以发起的列表
            DataTable dt = BP.WF.Dev2Interface.DB_GenerDraftDataTable(null, domain);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 打开的表单
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID">工作实例WorkID</param>
        /// <param name="flowNo">流程编号</param>
        /// <param name="nodeID">节点ID</param>
        /// <param name="fid">父WorkID</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string GenerFrmUrl(string token, Int64 workID,string flowNo,int nodeID,Int64 fid)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            /*
             * 发起的url需要在该流程的开始节点的表单方案中，使用SDK表单，并把表单的url设置到里面去.
             * 设置步骤:
             * 1. 打开流程设计器.
             * 2. 在开始节点上右键，选择表单方案.
             * 3. 选择SDK表单，把url配置到文本框里去.
             * 比如: /App/F027QingJia.htm
             */
            if (nodeID == 0)
                nodeID = int.Parse(flowNo + "01");

            if (workID == 0)
                workID = BP.WF.Dev2Interface.Node_CreateBlankWork(flowNo, BP.Web.WebUser.No);

            string url = "";
            Node nd = new Node(nodeID);
            if (nd.FormType == NodeFormType.SDKForm || nd.FormType == NodeFormType.SelfForm)
            {
                //.
                url = nd.FormUrl;
                if (url.Contains("?") == true)
                    url += "&FK_Flow=" + flowNo + "&FK_Node=" + nodeID + "&WorkID=" + workID + "&Token=" + token + "&UserNo=" + BP.Web.WebUser.No;
                else
                    url += "?FK_Flow=" + flowNo + "&FK_Node=" + nodeID + "&WorkID=" + workID + "&Token=" + token + "&UserNo=" + BP.Web.WebUser.No;
            }
            else
            {
                url = "/WF/MyFlow.htm?FK_Flow=" + flowNo + "&FK_Node=" + nodeID + "&WorkID=" + workID + "&Token=" + token;
            }
            return url;
        }
        /// <summary>
        /// 创建WorkID
        /// </summary>
        /// <param name="token"></param>
        /// <param name="flowNo">流程编号</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_CreateBlankWorkID(string token, string flowNo)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                Int64 workid = Dev2Interface.Node_CreateBlankWork(flowNo, BP.Web.WebUser.No);
                return workid.ToString();
            }catch(Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 是否可以处理当前的工作
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_IsCanDealWork(string token, Int64 workID)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                GenerWorkFlow gwf = new GenerWorkFlow(workID);
                string todoEmps = gwf.TodoEmps;
                bool isCanDo = false;
                if (gwf.FK_Node.ToString().EndsWith("01") == true)
                {
                    if (gwf.Starter.Equals(BP.Web.WebUser.No) == false)
                        isCanDo = false; //处理开始节点发送后，撤销的情况，第2个节点打开了，第1个节点撤销了,造成第2个节点也可以发送下去.
                    else
                        isCanDo = true; // 开始节点不判断权限.
                }
                else
                {
                    isCanDo = todoEmps.Contains(";" + WebUser.No + ",");
                    if (isCanDo == false)
                        isCanDo = Dev2Interface.Flow_IsCanDoCurrentWork(workID, BP.Web.WebUser.No);
                }
                return isCanDo == true ? "1" : "0";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 设置草稿
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <param name="flowNo"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_SetDraft(string token, Int64 workID, string flowNo)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                BP.WF.Dev2Interface.Node_SetDraft(flowNo, workID);
                return "设置草稿成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 把当前工作移交给指定的人员
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <param name="toEmpNo"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_Shift(string token, Int64 workID, string toEmpNo,string msg)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                msg  = BP.WF.Dev2Interface.Node_Shift(workID, toEmpNo, msg);
                return msg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 给当前的工作增加处理人
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_AddTodolist(string token, Int64 workID, string empNo)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                BP.WF.Dev2Interface.Node_AddTodolist(workID, empNo);
                return "增加人员成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 根据ID获取当前的流程实例信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_GenerWorkFlow(string token, Int64 workID)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                GenerWorkFlow gwf = new GenerWorkFlow(workID);
                return gwf.ToJson();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 保存参数到WF_GenerWorkFlow,用于方向条件的判断
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_SaveParas(string token, Int64 workID, string paras)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                BP.WF.Dev2Interface.Flow_SaveParas(workID, paras);
                return "参数保存成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_SetTitle(string token,Int64 workID, string title)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                BP.WF.Dev2Interface.Flow_SetFlowTitle(null, workID, title);
                return "标题设置成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 发送接口
        /// </summary>
        /// <param name="token"></param>
        /// <param name="flowNo">流程编号</param>
        /// <param name="workID">工作实例WorkID</param>
        /// <param name="toNodeID">到达的下一个节点</param>
        /// <param name="toEmps">下一个节点的接收人</param>
        /// <param name="paras">参数，保存到WF_GenerWorkFlow</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_SendWork(string token,  Int64 workID, string flowNo="", int toNodeID=0,string toEmps="",string paras="")
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if(DataType.IsNullOrEmpty(paras)==false)
                BP.WF.Dev2Interface.Flow_SaveParas(workID, paras);
            //执行发送.
            Hashtable ht = new Hashtable();
            ht.Add("FK_Flow", flowNo);
            ht.Add("WorkID", workID);
            try
            {
                if (DataType.IsNullOrEmpty(flowNo) == true)
                    flowNo = DBAccess.RunSQLReturnString("SELECT FK_Flow FROM WF_GenerWorkFlow WHERE WorkID=" + workID);

                //执行发送.
                SendReturnObjs objs = Dev2Interface.Node_SendWork(flowNo, workID, ht, null, toNodeID, toEmps);
                string msg = objs.ToMsgOfText();
                return msg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 根据流程编号获取流程实例
        /// </summary>
        /// <param name="token"></param>
        /// <param name="flowNo"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string DB_GenerWorkFlow(string token, string flowNo)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                GenerWorkFlows gwfs = new GenerWorkFlows();
                QueryObject qo = new QueryObject(gwfs);
                qo.AddWhere("FK_Flow", flowNo);
                qo.addAnd();
                qo.AddWhere("WFState", ">", 1);
                qo.addOrderBy("RDT");
                qo.DoQuery();
                return gwfs.ToJson();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取当前节点可以退回到的节点集合
        /// </summary>
        /// <param name="token"></param>
        /// <param name="nodeID">节点ID</param>
        /// <param name="workID">工作实例WorkID</param>
        /// <param name="fid">父WorkID</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string DB_GenerWillReturnNodes(string token, int nodeID,Int64 workID,Int64 fid)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                DataTable dt = Dev2Interface.DB_GenerWillReturnNodes(nodeID, workID, fid);
                return BP.Tools.Json.ToJson(dt);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }
        /// <summary>
        /// 当前节点执行退回操作
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workID">工作实例WorkID</param>
        /// <param name="nodeID">当前节点ID</param>
        /// <param name="returnToNodeID">退回到的节点</param>
        /// <param name="returnToEmp">退回到的接收人</param>
        /// <param name="msg">退回原因</param>
        /// <param name="isBackToThisNode">是否原路返回到当前节点</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Node_ReturnWork(string token, Int64 workID, int nodeID,int returnToNodeID,string returnToEmp,string msg,bool isBackToThisNode)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                if (returnToNodeID == 0)
                {
                    DataTable dt = BP.WF.Dev2Interface.DB_GenerWillReturnNodes(nodeID, workID, 0);
                    if (dt.Rows.Count == 1)
                    {
                        returnToNodeID = Int32.Parse(dt.Rows[0]["No"].ToString());
                        returnToEmp = dt.Rows[0]["Rec"].ToString();

                    }
                }

                //执行退回.
                string strs = Dev2Interface.Node_ReturnWork(workID,
                    returnToNodeID, returnToEmp, msg, isBackToThisNode);
                return strs;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 催办
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workIDs">工作实例的WorkID集合</param>
        /// <param name="msg">催办信息</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_DoPress(string token, string workIDs,string msg)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@执行批量催办的WorkIDs不能为空";
            }
            string[] strs = workIDs.Split(',');

            if (msg == null)
                msg = "需要您处理待办工作.";
            try
            {
                string info = "";
                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    info += "@" + BP.WF.Dev2Interface.Flow_DoPress(int.Parse(workidStr), msg, true);
                }
                return info;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
        /// <summary>
        /// 批量审核
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workIDs"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string CC_BatchCheckOver(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@执行批量审批的WorkIDs不能为空";
            }
            try
            {
                string str = BP.WF.Dev2Interface.Node_CC_SetCheckOverBatch(workIDs);
                return str;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
           
        }
        /// <summary>
        /// 批量删除流程
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workIDs"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_BatchDeleteByFlag(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@批量删除的WorkIDs不能为空";
            }
            try
            {
                string[] strs = workIDs.Split(',');
                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    string st1r = BP.WF.Dev2Interface.Flow_DoDeleteFlowByFlag(Int64.Parse(workidStr), "删除", true);
                }
                return "删除成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workIDs">工作实例WorkIDs</param>
        /// <param name="isDeleteSubFlows">是否删除子流程</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_BatchDeleteByReal(string token, string workIDs, bool isDeleteSubFlows)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@批量删除的WorkIDs不能为空";
            }
            try
            {
                string[] strs = workIDs.Split(',');

                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    string st1r = BP.WF.Dev2Interface.Flow_DoDeleteFlowByReal(Int64.Parse(workidStr), isDeleteSubFlows);
                }
                return "删除成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
        /// <summary>
        /// 批量恢复逻辑删除的流程
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workIDs"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_BatchDeleteByFlagAndUnDone(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@批量撤销删除的WorkIDs不能为空";
            }
            try
            {
                string[] strs = workIDs.Split(',');

                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    string st1r = BP.WF.Dev2Interface.Flow_DoUnDeleteFlowByFlag(null, int.Parse(workidStr), "删除");
                }
                return "撤销删除成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
           
        }
        /// <summary>
        /// 批量撤回
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workids"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_DoUnSend(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            //获取可以发起的列表
            string[] strs = workIDs.Split(',');
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@批量撤回的WorkIDs不能为空";
            }
            try
            {
                string info = "";
                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    info += BP.WF.Dev2Interface.Flow_DoUnSend(null, Int64.Parse(workidStr), 0, 0);
                }
                return info;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
           
        }
        /// <summary>
        /// 批量删除草稿
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workids"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_DeleteDraft(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            string[] strs = workIDs.Split(',');
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@批量删除草稿的WorkIDs不能为空";
            }
            try
            {
                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    BP.WF.Dev2Interface.Node_DeleteDraft(Int64.Parse(workidStr));
                }
                return "删除成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
        /// <summary>
        /// 批量结束流程
        /// </summary>
        /// <param name="token"></param>
        /// <param name="workids"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Flow_DoFlowOver(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            string[] strs = workIDs.Split(',');
            if (DataType.IsNullOrEmpty(workIDs))
            {
                return "err@批量结束流程的WorkIDs不能为空";
            }
            try
            {
                foreach (string workidStr in strs)
                {
                    if (BP.DA.DataType.IsNullOrEmpty(workidStr) == true)
                        continue;

                    BP.WF.Dev2Interface.Flow_DoFlowOver(Int64.Parse(workidStr), "批量结束", 1);
                }

                return "执行成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
        #region 批处理相关
        /// <summary>
        /// 批量处理
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Batch_Init(string token)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;          
            try
            {
                var handle = new BP.WF.HttpHandler.WF();
                string str = handle.Batch_Init();
                return str;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        /// <summary>
        /// 根据NodeID获取节点信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string En_Node(string token,int nodeID)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;

            try
            {
                Node node = new Node(nodeID);
              
                return node.ToJson();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        /// <summary>
        /// 根据流程编号获取流程信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="flowNo"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string En_Flow(string token, string flowNo)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;

            try
            {
                Flow flow = new Flow(flowNo);

                return flow.ToJson();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        /// <summary>
        /// 根据流程编号获取流程信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string WorkCheckModel_Init(string token, int nodeID)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;

            try
            {
                DataSet ds = new DataSet();
                //获取节点信息
                BP.WF.Node nd = new BP.WF.Node(nodeID);
                Flow fl = nd.HisFlow;
                ds.Tables.Add(nd.ToDataTableField("WF_Node"));
                string sql = "";
                if (nd.IsSubThread == true)
                {
                    sql = "SELECT a.*, b.Starter,b.StarterName,b.ADT,b.WorkID FROM " + fl.PTable
                              + " a , WF_EmpWorks b WHERE a.OID=B.FID AND b.WFState Not IN (7) AND b.FK_Node=" + nd.NodeID
                              + " AND b.FK_Emp='" + WebUser.No + "'";
                }
                else
                {
                    sql = "SELECT a.*, b.Starter,b.StarterName,b.ADT,b.WorkID FROM " + fl.PTable
                            + " a , WF_EmpWorks b WHERE a.OID=B.WorkID AND b.WFState Not IN (7) AND b.FK_Node=" + nd.NodeID
                            + " AND b.FK_Emp='" + WebUser.No + "'";
                }

                //获取待审批的流程信息集合
                DataTable dt = DBAccess.RunSQLReturnTable(sql);
                dt.TableName = "Works";
                ds.Tables.Add(dt);
                //获取按钮权限
                BtnLab btnLab = new BtnLab(nodeID);
                ds.Tables.Add(btnLab.ToDataTableField("Sys_BtnLab"));
                //获取字段属性
                MapAttrs attrs = new MapAttrs("ND" + nodeID);
                //获取实际中需要展示的列.
                string batchParas = nd.GetParaString("BatchFields");
                MapAttrs realAttr = new MapAttrs();
                if (DataType.IsNullOrEmpty(batchParas) == false)
                {
                    string[] strs = batchParas.Split(',');
                    foreach (string str in strs)
                    {
                        if (string.IsNullOrEmpty(str)
                            || str.Contains("@PFlowNo") == true)
                            continue;

                        foreach (MapAttr attr in attrs)
                        {
                            if (str != attr.KeyOfEn)
                                continue;

                            realAttr.AddEntity(attr);
                        }
                    }
                }
                ds.Tables.Add(realAttr.ToDataTableField("Sys_MapAttr"));
                return BP.Tools.Json.ToJson(ds);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        [HttpGet, HttpPost]
        public string Batch_InitDDL(string token, int nodeID)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;

            try
            {
                GenerWorkFlow gwf = new GenerWorkFlow();
                Node nd = new Node(nodeID);
                gwf.TodoEmps = WebUser.No + ",";
                DataTable mydt = BP.WF.Dev2Interface.Node_GenerDTOfToNodes(gwf, nd);
                return BP.Tools.Json.ToJson(mydt);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        [HttpGet, HttpPost]
        public string WorkCheckModel_Send(string token, string flowNo)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;

            try
            {
                var handle = new BP.WF.HttpHandler.WF_WorkOpt_Batch();
                string str = handle.WorkCheckModel_Send();
                return str;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        [HttpGet, HttpPost]
        public string Batch_Delete(string token, string workIDs)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;

            try
            {
                if (DataType.IsNullOrEmpty(workIDs) == true)
                    return "err@没有选择需要处理的工作";
                string msg = "";
                GenerWorkFlows gwfs = new GenerWorkFlows();
                gwfs.RetrieveIn("WorkID", workIDs);
                foreach (GenerWorkFlow gwf in gwfs)
                {
                    msg += "@对工作(" + gwf.Title + ")处理情况如下。<br>";
                    string mes = BP.WF.Dev2Interface.Flow_DoDeleteFlowByFlag(gwf.WorkID, "批量删除", true);
                    msg += mes;
                    msg += "<hr>";
                }
                return "批量删除成功" + msg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion 批处理相关
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="dtFrom">日期从</param>
        /// <param name="dtTo">日期到</param>
        /// <param name="scop">范围</param>
        /// <param name="pageIdx">分页</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Search_Init(string token,string key,string dtFrom,string dtTo,string scop,int pageIdx)
        {
            //根据token登录
            string result = Port_GenerToken(token);
            if (result != null)
                return result;
            try
            {
                GenerWorkFlows gwfs = new GenerWorkFlows();
                //创建查询对象.
                QueryObject qo = new QueryObject(gwfs);
                if (DataType.IsNullOrEmpty(key) == false)
                {
                    qo.AddWhere(GenerWorkFlowAttr.Title, " LIKE ", "%" + key + "%");
                    qo.addAnd();
                }

                //我参与的.
                if (scop.Equals("0") == true)
                    qo.AddWhere(GenerWorkFlowAttr.Emps, "LIKE", "%@" + WebUser.No + ",%");

                //我发起的.
                if (scop.Equals("1") == true)
                    qo.AddWhere(GenerWorkFlowAttr.Starter, "=", WebUser.No);

                //我部门发起的.
                if (scop.Equals("2") == true)
                    qo.AddWhere(GenerWorkFlowAttr.FK_Dept, "=", WebUser.FK_Dept);


                //任何一个为空.
                if (DataType.IsNullOrEmpty(dtFrom) == true || DataType.IsNullOrEmpty(dtTo) == true)
                {

                }
                else
                {
                    qo.addAnd();
                    qo.AddWhere(GenerWorkFlowAttr.RDT, ">=", dtFrom);
                    qo.addAnd();
                    qo.AddWhere(GenerWorkFlowAttr.RDT, "<=", dtTo);
                }

                var count = qo.GetCount(); //获得总数.

                qo.DoQuery("WorkID", 20, pageIdx);
                //   qo.DoQuery(); // "WorkID", 20, pageIdx);


                DataTable dt = gwfs.ToDataTableField("gwls");

                //创建容器.
                DataSet ds = new DataSet();
                ds.Tables.Add(dt); //增加查询对象.

                //增加数量.
                DataTable mydt = new DataTable();
                mydt.TableName = "count";
                mydt.Columns.Add("CC");
                DataRow dr = mydt.NewRow();
                dr[0] = count.ToString(); //把数量加进去.
                mydt.Rows.Add(dr);
                ds.Tables.Add(mydt);
                return BP.Tools.Json.ToJson(ds);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
           
        }
       
      

        private string Port_GenerToken(string token)
        {
            //根据token登录
            try
            {
                BP.WF.Dev2Interface.Port_LoginByToken(token);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }
    }

}
