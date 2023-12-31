﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.Services;
using BP.WF;
using BP.WF.Template;
using BP.WF.Data;
using System.Net;
using System.IO;
using System.Text;
using BP.Cloud;
using System.Security.Cryptography;
using BP.DA;

namespace CCFlow.DataUser
{
    /// <summary>
    /// LocalWS 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    [System.Web.Script.Services.ScriptService]
    public class LocalWS : System.Web.Services.WebService
    {
        //微信开发者appid和secret_key
        //第一版小程序
        private static string appid = "wxd4893788d8f6088b";
        private static string secret = "3f0850c81baf10847c05d01b769d2990";
        /// <summary>
        /// 获得工作进度-用于展示流程的进度图
        /// </summary>
        /// <param name="workID">workID</param>
        /// <returns>返回进度数据</returns>
        public string DB_JobSchedule(Int64 workID)
        {
            DataSet ds = BP.WF.Dev2Interface.DB_JobSchedule(workID);
            return BP.Tools.Json.ToJson(ds);
        }
        /// <summary>
        /// 获得待办
        /// </summary>
        /// <param name="userNo">用户编号</param>
        /// <param name="sysNo">系统编号,为空时返回平台所有数据。</param>
        /// <returns>返回待办</returns>
        public string DB_Todolist(string userNo, string sysNo = null)
        {
            return null;
            Paras ps = new Paras();
            string sql = "";
            if (sysNo == null)
            {
                ps.SQL = "SELECT * FROM WF_EmpWorks WHERE FK_Emp=" + BP.DA.DBAccess.AppCenterDBType + "FK_Emp";
                ps.Add(userNo);
                ps.Add(sysNo);
            }
            else
            {
                ps.SQL = "SELECT * FROM WF_EmpWorks WHERE Domain='" + sysNo + "' AND FK_Emp='" + userNo + "'";
                ps.Add(userNo);
                ps.Add(sysNo);
            }
            DataTable dt = BP.DA.DBAccess.RunSQLReturnTable(ps);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 获得在途
        /// </summary>
        /// <param name="userNo">用户编号</param>
        /// <param name="sysNo">系统编号，为空时返回平台所有数据。</param>
        /// <returns></returns>
        [WebMethod]
        public string DB_Runing(string userNo, string sysNo = null)
        {
            DataTable dt = BP.WF.Dev2Interface.DB_GenerRuning(userNo, null, false, sysNo);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 获得我可以发起的流程.
        /// </summary>
        /// <param name="userNo">用户编号</param>
        /// <param name="sysNo">系统编号，为空时返回平台所有数据。</param>
        /// <returns>返回我可以发起的流程列表.</returns>
        [WebMethod]
        public string DB_StarFlows(string userNo, string domain = null)
        {
            DataTable dt = BP.WF.Dev2Interface.DB_StarFlows(userNo, domain);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 我发起的流程实例
        /// </summary>
        /// <param name="userNo">用户编号</param>
        /// <param name="sysNo">子系统编号</param>
        /// <returns>我发起的流程列表.</returns>
        [WebMethod]
        public string DB_MyStartFlowInstance(string userNo, string domain = null, int pageSize = 0, int pageIdx = 0)
        {
            string sql = "";
            if (domain == null)
                sql = "SELECT * FROM WF_GenerWorkFlow WHERE Starter='" + userNo + "'";
            else
                sql = "SELECT * FROM WF_GenerWorkFlow WHERE Domain='" + domain + "' AND Starter='" + userNo + "'";

            DataTable dt = BP.DA.DBAccess.RunSQLReturnTable(sql);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 运行一个sql，返回一个json.
        /// </summary>
        /// <param name="sqlOfSelect">要运行的SQL,查询</param>
        /// <param name="password">密码,双方约定的密码</param>
        /// <returns>json</returns>
        [WebMethod]
        public string DB_RunSQLReturnJSON(string sqlOfSelect, string password)
        {
            // if ( password.Equals("xxxxxx") == false)
            //  return "err@密码错误";
            // DataTable dt = BP.DA.DBAccess.RunSQLReturnTable(sqlOfSelect);
            // return BP.Tools.Json.ToJson(dt);
            throw new Exception("err@请实现该方法,密码部分是双方约定的,不对外公开的.");
        }
        /// <summary>
        /// 创建WorkID
        /// </summary>
        /// <param name="flowNo">流程编号</param>
        /// <param name="userNo">工作人员编号</param>
        /// <returns>一个长整型的工作流程实例.</returns>
        [WebMethod]
        public Int64 CreateWorkID(string userNo, string flowNo, string starterNo)
        {
            BP.WF.Dev2Interface.Port_Login(userNo);
            return BP.WF.Dev2Interface.Node_CreateBlankWork(flowNo, userNo);
        }
        /// <summary>
        /// 执行发送
        /// </summary>
        /// <param name="flowNo">流的程模版ID</param>
        /// <param name="workid">工作ID</param>
        /// <param name="atParas">参数: @Field1=Val1@Field2=Val2</param>
        /// <param name="toNodeID">到达的节点ID.如果让系统自动计算就传入0</param>
        /// <param name="toEmps">到达的人员IDs,比如:zhangsan,lisi,wangwu. 如果为Null就标识让系统自动计算.</param>
        /// <returns>发送的结果信息.</returns>
        [WebMethod]
        public string SendWork(string flowNo, Int64 workid, string atParas, int toNodeID, string toEmps)
        {

            BP.DA.AtPara ap = new BP.DA.AtPara(atParas);

            BP.WF.SendReturnObjs objs = BP.WF.Dev2Interface.Node_SendWork(flowNo, workid, ap.HisHT, toNodeID, toEmps);

            string msg = objs.ToMsgOfText();

            Hashtable myht = new Hashtable();
            myht.Add("Message", msg);
            myht.Add("IsStopFlow", objs.IsStopFlow);
            myht.Add("VarAcceptersID", objs.VarAcceptersID);
            myht.Add("VarAcceptersName", objs.VarAcceptersName);
            myht.Add("VarToNodeID", objs.VarToNodeID);
            myht.Add("VarToNodeName", objs.VarToNodeName);

            return BP.Tools.Json.ToJson(myht);
        }
        [WebMethod]
        public void SaveAth(int nodeid, string flowNo, Int64 workid, string athNo, string frmID, byte[] byteFile, string fileName, string fileExt, string userNo, string sort = null, Int32 fid = 0, Int32 pworkid = 0)
        {
            //把byte文件保存到临时文件中
            string tempPath = BP.Difference.SystemConfig.PathOfTemp + "\\" + DBAccess.GenerGUID() + "." + fileExt;
            FileInfo fi = new System.IO.FileInfo(tempPath);
            FileStream fs = fi.OpenWrite();
            fs.Write(byteFile, 0, byteFile.Length);
            fs.Close();
            fs.Dispose();
            BP.WF.Dev2Interface.Port_Login(userNo);
            BP.WF.Dev2Interface.CCForm_AddAth(nodeid, flowNo, workid, athNo, frmID, tempPath, fileName + "." + fileExt, sort, fid, pworkid);
        }
        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="userNo"></param>
        [WebMethod]
        public void Port_Login(string userNo, string orgNo)
        {
            BP.WF.Dev2Interface.Port_Login(userNo, orgNo);
        }
        /// <summary>
        /// 保存参数
        /// </summary>
        /// <param name="workid">工作ID</param>
        /// <param name="paras">用于控制流程运转的参数，比如方向条件. 格式为:@JinE=1000@QingJaiTianShu=100</param>
        [WebMethod]
        public void SaveParas(Int64 workid, string paras)
        {
            BP.WF.Dev2Interface.Flow_SaveParas(workid, paras);
        }
        /// <summary>
        /// 获得下一个节点信息
        /// </summary>
        /// <param name="flowNo">流程编号</param>
        /// <param name="workid">流程实例</param>
        /// <param name="paras">方向条件所需要的参数，可以为空。</param>
        /// <returns>下一个节点的JSON.</returns>
        [WebMethod]
        public string GenerNextStepNode(string flowNo, Int64 workid, string paras = null)
        {
            if (paras != null)
                BP.WF.Dev2Interface.Flow_SaveParas(workid, paras);

            int nodeID = BP.WF.Dev2Interface.Node_GetNextStepNode(flowNo, workid);
            BP.WF.Node nd = new BP.WF.Node(nodeID);

            //如果字段 DeliveryWay = 4 就表示到达的接点是由当前节点发送人选择接收人.
            //自定义参数的字段是 SelfParas, DeliveryWay 
            // CondModel = 方向条件计算规则.
            return nd.ToJson();
        }
        /// <summary>
        /// 获得下一步节点的接收人
        /// </summary>
        /// <param name="toNodeID">节点ID</param>
        /// <param name="workid">工作事例ID</param>
        /// <returns>返回两个结果集一个是分组的Depts(No,Name)，另外一个是人员的Emps(No, Name, FK_Dept),接受后，用于构造人员选择器.</returns>
        [WebMethod]
        public string GenerNextStepNodeEmps(string flowNo, int toNodeID, int workid)
        {
            Selector select = new Selector(toNodeID);
            Node nd = new Node(toNodeID);

            GERpt rpt = new GERpt("ND" + int.Parse(flowNo) + "Rpt", workid);
            DataSet ds = select.GenerDataSet(toNodeID, rpt);
            return BP.Tools.Json.ToJson(ds);
        }
        /// <summary>
        /// 将要达到的节点
        /// </summary>
        /// <param name="currNodeID">当前节点ID</param>
        /// <returns>返回节点集合的json.</returns>
        [WebMethod]
        public string WillToNodes(int currNodeID)
        {
            Node nd = new Node(currNodeID);
            if (nd.CondModel != DirCondModel.ByLineCond)
                return "err@当前节点是由选择的.";

            Directions dirs = new Directions();
            Nodes nds = dirs.GetHisToNodes(currNodeID, false);
            return nds.ToJson();
        }
        /// <summary>
        /// 退回
        /// </summary>
        /// <param name="workid">工作ID</param>
        /// <param name="returnToNodeID">退回到nodeID</param>
        /// <param name="returnMsg">退回信息</param>
        /// <returns></returns>
        [WebMethod]
        public string Node_ReturnWork(Int64 workid, int returnToNodeID, string returnMsg)
        {
            //GenerWorkFlow gwf = new GenerWorkFlow(workid);
            //return BP.WF.Dev2Interface.Node_ReturnWork(gwf.FK_Flow, gwf.WorkID, gwf.FID, gwf.FK_Node, returnToNodeID, returnMsg);
            return "";
        }
        /// <summary>
        /// 写入审核信息
        /// </summary>
        /// <param name="workid">workID</param>
        /// <param name="msg">审核信息</param>
        [WebMethod]
        public void Node_WriteWorkCheck(Int64 workid, string msg)
        {
            GenerWorkFlow gwf = new GenerWorkFlow(workid);
            BP.WF.Dev2Interface.WriteTrackWorkCheck(gwf.FK_Flow, gwf.FK_Node, gwf.WorkID, gwf.FID, msg, "审核", null);
        }
        /// <summary>
        /// 是否可以查看该工作
        /// </summary>
        /// <param name="flowNo">流程No</param>
        /// <param name="workid">工作ID</param>
        /// <param name="userNo">人员ID</param>
        /// <returns>true,false</returns>
        [WebMethod]
        public bool Flow_IsCanView(string flowNo, Int64 workid, string userNo)
        {
            return BP.WF.Dev2Interface.Flow_IsCanViewTruck(flowNo, workid, userNo);
        }
        /// <summary>
        /// 是否可以处理当前工作.
        /// </summary>
        /// <param name="workid">当前工作ID</param>
        /// <param name="workid">处理人员ID</param>
        /// <returns>true,false</returns>
        [WebMethod]
        public bool Flow_IsCanDoCurrentWork(Int64 workid, string userNo)
        {
            return BP.WF.Dev2Interface.Flow_IsCanDoCurrentWork(workid, userNo);
        }
        /// <summary>
        /// 获得当前节点信息.
        /// </summary>
        /// <param name="currNodeID">节点ID.</param>
        /// <returns>当前节点信息</returns>
        [WebMethod]
        public string CurrNodeInfo(int currNodeID)
        {
            Node nd = new Node(currNodeID);
            return nd.ToJson();
        }
        /// <summary>
        /// 获得当前流程信息.
        /// </summary>
        /// <param name="flowNo">流程ID.</param>
        /// <returns>当前节点信息</returns>
        [WebMethod]
        public string CurrFlowInfo(string flowNo)
        {

            Flow fl = new Flow(flowNo);
            return fl.ToJson();
        }
        /// <summary>
        /// 获得当前流程信息.
        /// </summary>
        /// <param name="flowNo">流程ID.</param>
        /// <returns>当前节点信息</returns>
        [WebMethod]
        public string CurrGenerWorkFlowInfo(Int64 workID)
        {

            GenerWorkFlow gwf = new GenerWorkFlow(workID);
            return gwf.ToJson();
        }
        /// <summary>
        /// 授权后获取小程序用户的OpenID
        /// </summary>
        [WebMethod]
        public string ASCGetUserInfo(string code)
        {
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid=" + appid + "&secret=" + secret + "&js_code=" + code + "&grant_type=client_credential";
            string serviceAddress = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);
            request.Method = "GET";
            request.ContentType = "textml;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
        /// <summary>
        /// 解密微信授权的手机号
        /// </summary>
        /// <param name="encryptedDataStr"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        [WebMethod]
        public string AES_Decrypt(string encryptedDataStr, string key, string iv)
        {

            RijndaelManaged rijalg = new RijndaelManaged();
            //-----------------    
            //设置 cipher 格式 AES-128-CBC    

            rijalg.KeySize = 128;

            rijalg.Padding = PaddingMode.PKCS7;
            rijalg.Mode = CipherMode.CBC;

            rijalg.Key = Convert.FromBase64String(key);
            rijalg.IV = Convert.FromBase64String(iv);


            byte[] encryptedData = Convert.FromBase64String(encryptedDataStr);
            //解密    
            ICryptoTransform decryptor = rijalg.CreateDecryptor(rijalg.Key, rijalg.IV);

            string result;

            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        result = srDecrypt.ReadToEnd();
                    }
                }
            }

            return result;
        }
        /// <summary>
        ///BPM登录
        /// </summary>
        /// <param name="openID"></param>
        /// <returns></returns>
        [WebMethod]
        public string LoadOrgInfo(string openID)
        {
            //先从Port_User 去查找 openID, 如果没有记录，就转到注册页面上去
            //如果有就列出此人所有注册过的公司，选择其一登录
            User user = new User();
            int i = user.Retrieve(UserAttr.SOpenID, openID);
            if (i == 1)
            {
                Emps emps = new Emps();
                emps.Retrieve(EmpAttr.OpenID, openID);
                return emps.ToJson();
            }
            return "info@zhuce";
        }
        /// <summary>
        /// 注册企业
        /// </summary>
        /// <param name="orgName"></param>
        /// <param name="orgShortName"></param>
        /// <param name="openid">小程序ID</param>
        /// <param name="userName"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        [WebMethod]
        public string RegByXiaoChengXu(string orgName, string orgShortName,
            string openid, string userName, string tel)
        {

            //注册企业.
            BP.Cloud.Org org = new BP.Cloud.Org();
            org.No = BP.DA.DBAccess.GenerGUID(4, "Port_Org", "No");
            org.Name = orgShortName;
            org.NameFull = orgName;
            org.Adminer = openid;
            org.AdminerName = userName;
            org.Insert();


            //增加这个人员.
            Emp emp = new Emp();
            emp.No = org.No + "_" + openid;
            emp.Name = userName;
            emp.Pass = "123";
            emp.OrgNo = org.No;
            emp.OrgName = org.NameFull;
            // 设置ID.
            emp.UserID = openid;
            emp.OpenID = openid;

            emp.FK_Dept = org.No;
            emp.Tel = tel;
            emp.Insert();

            BP.Cloud.User user = new User();
            int i = user.Retrieve(UserAttr.SOpenID, openid);
            if (i == 0)
            {
                user.Copy(emp);
                user.No = emp.OpenID;
                user.OrgNo = emp.OrgNo;
                user.SOpenID = emp.OpenID;
                user.Insert();
            }
            else
            {
                user.Copy(emp);
                user.No = emp.OpenID;
                user.OrgNo = emp.OrgNo;
                user.SOpenID = emp.OpenID;
                user.Update();
            }

            ////初始化部门.
            //BP.Cloud.Dept dept = new Dept();
            //dept.ParentNo = "100";
            //dept.No = org.No;
            //dept.Name = org.Name;
            //dept.OrgNo = org.No;
            //dept.Insert();

            //dept.ParentNo = org.No;
            //dept.No = BP.DA.DBAccess.GenerGUID(5, "Port_Dept", "No");
            //dept.Name = "办公室";
            //dept.OrgNo = org.No;
            //dept.Insert();

            //dept.ParentNo = org.No;
            //dept.No = BP.DA.DBAccess.GenerGUID(5, "Port_Dept", "No");
            //dept.Name = "财务部";
            //dept.OrgNo = org.No;
            //dept.Insert();

            BP.Web.WebUser.OrgNo = org.No;
            //生成其他的信息.(@lizhenerr 有报错？)
            org.Init_OrgDatas();

            //管理员登录
            BP.WF.Dev2Interface.Port_Login(openid, org.No);

            ////初始化岗位.
            //BP.Cloud.Station sta = new Station();
            //sta.No = BP.DA.DBAccess.GenerGUID();
            //sta.Name = "办公室主任";
            //sta.OrgNo = org.No;
            //sta.Insert();

            //sta = new Station();
            //sta.No = BP.DA.DBAccess.GenerGUID();
            //sta.Name = "财务部主任";
            //sta.OrgNo = org.No;
            //sta.Insert();

            return org.ToJson();
        }
        /// <summary>
        /// 检查此人是否加入
        /// </summary>
        [WebMethod]
        public string CheckJoin(string openID, string orgNo)
        {
            ////让管理员登录.
            //this.LetUserLogin("admin", "ccs");

            BP.Cloud.HttpHandler.App_Portal apl = new BP.Cloud.HttpHandler.App_Portal();
            return apl.Invited_CheckIsExit(openID, orgNo);

            ////让管理员退出。
            //BP.Web.WebUser.Exit();

            //return "加入成功！";
        }
        /// <summary>
        /// 扫码增加人员
        /// </summary>
        [WebMethod]
        public string CreateEmp(string openID, string orgNo, string userNo, string tel, string empName, string deptNo)
        {
            ////让管理员登录.
            //this.LetUserLogin("admin", "ccs");

            BP.Cloud.HttpHandler.App_Portal apl = new BP.Cloud.HttpHandler.App_Portal();
            return apl.Invited_AddEmp(openID, orgNo, userNo, tel, empName, deptNo);

            ////让管理员退出。
            //BP.Web.WebUser.Exit();

            //return "加入成功！";
        }
        /// <summary>
        /// 获取单位该单位的部门列表
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="orgNo"></param>
        [WebMethod]
        public string ASCLoadDepts(string orgNo)
        {
            string sql = "SELECT * FROM Port_Dept WHERE OrgNo='" + orgNo + "' ORDER BY Idx";
            DataTable dt = new DataTable();
            dt = DBAccess.RunSQLReturnTable(sql);
            return BP.Tools.Json.ToJson(dt);
        }

        #region 关于组织结构的接口.
        /// <summary>
        /// 保存用户数据, 如果有此数据则修改，无此数据则增加.
        /// </summary>
        /// <param name="orgNo">组织编号</param>
        /// <param name="userNo">用户编号,如果是saas版本就是orgNo_userID</param>
        /// <param name="userName">用户名称</param>
        /// <param name="deptNo">部门编号</param>
        /// <param name="kvs">属性值，比如: @Name=张三@Tel=18778882345@Pass=123, 如果是saas模式：就必须有@UserID=xxxx </param>
        /// <param name="stats">岗位编号：比如:001,002,003,</param>
        /// <returns>reutrn 1=成功,  其他的标识异常.</returns>
        [WebMethod]
        public string Port_Emp_Save(string orgNo, string userNo, string userName, string deptNo, string kvs, string stats)
        {
            if (BP.Web.WebUser.IsAdmin == false)
                return "0";

            if (BP.Difference.SystemConfig.CCBPMRunModel == BP.Sys.CCBPMRunModel.SAAS)
            {
                if (kvs == null || kvs.IndexOf("@UserID=") == -1)
                    return "err@saas模式下，需要在kvs参数里，增加@UserID=xxxx 属性.";
            }

            if (BP.Difference.SystemConfig.CCBPMRunModel != BP.Sys.CCBPMRunModel.Single)
            {
                if (DataType.IsNullOrEmpty(orgNo) == true)
                    return "err@组织编号不能为空.";

                BP.WF.Admin.Org org = new BP.WF.Admin.Org();
                org.No = orgNo;
                if (org.RetrieveFromDBSources() == 0)
                    return "err@组织编号错误:" + orgNo;
            }
            else
            {
                orgNo = "";
            }

            if (DataType.IsNullOrEmpty(userNo) || DataType.IsNullOrEmpty(userName) || DataType.IsNullOrEmpty(deptNo) == true)
                throw new Exception("err@用户编号，名称，部门不能为空.");

            BP.Port.Dept dept = new BP.Port.Dept();
            dept.No = deptNo;
            if (dept.RetrieveFromDBSources() == 0)
                throw new Exception("err@部门编号错误:" + deptNo);

            try
            {
                //增加人员信息.
                BP.Port.Emp emp = new BP.Port.Emp();
                emp.No = userNo;
                if (emp.RetrieveFromDBSources() == 0)
                {
                    emp.Name = userName;
                    emp.FK_Dept = deptNo;
                    emp.Insert();
                }

                BP.DA.AtPara ap = new AtPara(kvs);
                foreach (string key in ap.HisHT.Keys)
                {
                    if (DataType.IsNullOrEmpty(key) == true)
                        continue;
                    emp.SetValByKey(key, ap.GetValStrByKey(key));
                }
                emp.FK_Dept = deptNo;
                emp.Name = userName;
                emp.OrgNo = orgNo;
                emp.Update();

                if (BP.Difference.SystemConfig.CCBPMRunModel == BP.Sys.CCBPMRunModel.Single)
                {
                    BP.DA.DBAccess.RunSQL("DELETE FROM Port_DeptEmp WHERE FK_Emp='" + userNo + "'");
                    BP.DA.DBAccess.RunSQL("DELETE FROM Port_DeptEmpStation WHERE FK_Emp='" + userNo + "'");
                }
                else
                {
                    BP.DA.DBAccess.RunSQL("DELETE FROM Port_DeptEmp WHERE FK_Emp='" + userNo + "' AND OrgNo='" + orgNo + "'");
                    BP.DA.DBAccess.RunSQL("DELETE FROM Port_DeptEmpStation WHERE FK_Emp='" + userNo + "' AND OrgNo='" + orgNo + "'");
                }

                //插入部门.
                BP.Port.DeptEmp de = new BP.Port.DeptEmp();
                de.FK_Dept = deptNo;
                de.FK_Emp = userNo;
                de.OrgNo = orgNo;
                //    de.IsMainDept = true;
                de.MyPK = de.FK_Dept + "_" + userNo;
                de.DirectInsert();

                //更新岗位.
                if (stats == null)
                    stats = "";
                string[] strs = stats.Split(',');
                for (int i = 0; i < strs.Length; i++)
                {
                    string str = strs[i];
                    if (DataType.IsNullOrEmpty(str))
                        continue;

                    Station st = new Station();
                    st.No = str;
                    if (st.RetrieveFromDBSources() == 0)
                        throw new Exception("err@岗位编号错误." + str);

                    //插入部门.
                    DeptEmpStation des = new DeptEmpStation();
                    des.FK_Dept = deptNo;
                    des.FK_Emp = userNo;
                    des.FK_Station = str;
                    des.OrgNo = orgNo;
                    des.MyPK = de.FK_Dept + "_" + des.FK_Emp + "_" + des.FK_Station;
                    des.DirectInsert();
                }
                return "1";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 保存岗位
        /// </summary>
        /// <param name="userNo"></param>
        /// <param name="stas">岗位用逗号分开</param>
        /// <returns>reutrn 1=成功,  其他的标识异常.</returns>
        [WebMethod]
        public string Port_Emp_Delete(string orgNo, string userNo)
        {
            if (BP.Web.WebUser.IsAdmin == false)
                return "0";

            try
            {
                //增加人员信息.
                BP.Port.Emp emp = new BP.Port.Emp();
                emp.No = userNo;
                emp.OrgNo = orgNo;
                if (emp.RetrieveFromDBSources() == 0)
                    return "err@该用户【" + userNo + "】不存在.";

                //删除岗位.
                BP.DA.DBAccess.RunSQL("delete from port_deptemp where fk_emp='" + userNo + "' AND OrgNo='" + orgNo + "'");
                BP.DA.DBAccess.RunSQL("delete from port_deptempStation where fk_emp='" + userNo + "' AND OrgNo='" + orgNo + "'");
                emp.Delete();
                return "1";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 保存部门, 如果有此数据则修改，无此数据则增加.
        /// </summary>
        /// <param name="orgNo">组织编号</param>
        /// <param name="no">部门编号</param>
        /// <param name="name">名称</param>
        /// <param name="parntNo">父节点编号</param>
        /// <param name="keyval">比如：@Leaer=zhangsan@Tel=12233333@Idx=1</param>
        /// <returns>return 1 增加成功，其他的增加失败.</returns>
        [WebMethod]
        public string Port_Dept_Save(string orgNo, string no, string name, string parntNo, string keyVals)
        {
            if (BP.Web.WebUser.IsAdmin == false)
                return "0";


            if (BP.Difference.SystemConfig.CCBPMRunModel != BP.Sys.CCBPMRunModel.Single)
            {
                if (DataType.IsNullOrEmpty(orgNo) == true)
                    return "err@组织编号不能为空.";

                BP.WF.Admin.Org org = new BP.WF.Admin.Org();
                org.No = orgNo;
                if (org.RetrieveFromDBSources() == 0)
                    return "err@组织编号错误:" + orgNo;
            }


            try
            {
                //增加人员信息.
                BP.Port.Dept deptP = new BP.Port.Dept(parntNo);
                AtPara ap = new AtPara(keyVals);
                //增加部门.
                BP.Port.Dept dept = new BP.Port.Dept();
                dept.No = no;
                if (dept.RetrieveFromDBSources() == 0)
                {
                    dept.Name = name;
                    dept.ParentNo = parntNo;
                    dept.OrgNo = orgNo;

                    foreach (string key in ap.HisHT.Keys)
                    {
                        if (DataType.IsNullOrEmpty(key) == true)
                            continue;
                        dept.SetValByKey(key, ap.GetValStrByKey(key));
                    }
                    dept.Insert();
                }
                else
                {
                    dept.Name = name;
                    dept.ParentNo = parntNo;
                    dept.OrgNo = orgNo;

                    foreach (string key in ap.HisHT.Keys)
                    {
                        if (DataType.IsNullOrEmpty(key) == true)
                            continue;
                        dept.SetValByKey(key, ap.GetValStrByKey(key));
                    }

                    dept.Update();
                }

                return "1";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 删除部门.
        /// </summary>
        /// <param name="no">删除指定的部门编号</param>
        /// <returns></returns>
        [WebMethod]
        public string Port_Dept_Delete(string no)
        {
            if (BP.Web.WebUser.IsAdmin == false)
                return "0";

            try
            {
                //删除部门.
                BP.Port.Dept dept = new BP.Port.Dept(no);
                dept.Delete();

                return "1";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 保存岗位, 如果有此数据则修改，无此数据则增加.
        /// </summary>
        /// <param name="orgNo">组织编号</param>
        /// <param name="no">编号</param>
        /// <param name="name">名称</param>
        /// <returns>return 1 增加成功，其他的增加失败.</returns>
        [WebMethod]
        public string Port_Station_Save(string orgNo, string no, string name, string keyVals)
        {
            if (BP.Web.WebUser.IsAdmin == false)
                return "0";

            if (BP.Difference.SystemConfig.CCBPMRunModel != BP.Sys.CCBPMRunModel.Single)
            {
                if (DataType.IsNullOrEmpty(orgNo) == true)
                    return "err@组织编号不能为空.";

                BP.WF.Admin.Org org = new BP.WF.Admin.Org();
                org.No = orgNo;
                if (org.RetrieveFromDBSources() == 0)
                    return "err@组织编号错误:" + orgNo;
            }

            try
            {
                AtPara ap = new AtPara(keyVals);

                //增加部门.
                BP.Port.Station en = new BP.Port.Station();
                en.No = no;
                if (en.RetrieveFromDBSources() == 0)
                {
                    en.Name = name;
                    en.OrgNo = orgNo;
                    en.Insert();
                }
                foreach (string item in ap.HisHT.Keys)
                {
                    if (DataType.IsNullOrEmpty(item) == true)
                        continue;
                    en.SetValByKey(item, ap.GetValStrByKey(item));
                }
                en.Name = name;
                en.OrgNo = orgNo;
                en.Update();

                return "1";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 删除部门.
        /// </summary>
        /// <param name="no">删除指定的部门编号</param>
        /// <returns></returns>
        [WebMethod]
        public string Port_Station_Delete(string no)
        {
            try
            {
                if (BP.Web.WebUser.IsAdmin == false)
                    return "0";
                //删除部门.
                BP.Port.Station dept = new BP.Port.Station(no);
                dept.Delete();

                return "1";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }

        #endregion 关于组织的接口.


    }
}
