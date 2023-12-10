using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;
using BP.DA;
using BP.Sys;
using BP.Web;
using BP.En;
using BP.WF;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using LitJson;
using Glo = BP.Cloud.WeXinAPI.Glo;
using BP.Difference;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class Root : BP.WF.HttpHandler.DirectoryPageBase
    {

        //微信开发者appid和secret_key
        //第一版小程序
        private static string appid = "wxd4893788d8f6088b";
        private static string secret = "3f0850c81baf10847c05d01b769d2990";

        //httppost请求
        BP.WF.HttpWebResponseUtility httpWebResponseUtility = new BP.WF.HttpWebResponseUtility();
        /// <summary>
        /// 构造函数
        /// </summary>
        public Root()
        {

        }
        //查询出来历史的记录.
        public string Print_GenerDB()
        {
            //查询出来历史的记录.  ActionType=13 是回滚操作的记录.
            string sql = "";
            sql = "SELECT * FROM ND8Track WHERE ActionType=13 AND WorkID=" + this.WorkID + " ORDER BY RDT ";
            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 开始打印
        /// </summary>
        /// <returns></returns>
        public string Print_Again()
        {
            string str = BP.WF.Dev2Interface.Flow_DoRebackWorkFlow("001",
                this.WorkID, 108, "重新打印");
            return str;
        }
        /// <summary>
        /// 获得信息  @gzx
        /// </summary>
        /// <returns></returns>
        public string RegisterByWebOfWX_Init()
        {
            string uuid = this.GetRequestVal("UUID");
            string url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoTpye=ReqUserInfoByUUID&UUID=" + uuid;
            string data = DataType.ReadURLContext(url, 99999);
            return data;
        }
        /// <summary>
        /// 注册微信用户by扫码后 @gzx
        /// </summary>
        /// <returns></returns>
        public string RegisterByWebOfWX_RegWX()
        {
            string uuid = this.GetRequestVal("UUID");
            string name = this.GetRequestVal("Name");
            string tel = this.GetRequestVal("Tel");

            //把icon存储到本地.
            string icon = "/DataUsr/UserIcon/" + tel + ".png";


            string url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoTpye=ReqUserInfoByWX";
            url += "&UUID=" + uuid;
            url += "&Name=" + name;
            url += "&Tel=" + tel;

            string data = DataType.ReadURLContext(url, 99999);
            return "微信用户注册成功.";

        }
        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public string Default_Submit()
        {
            return Login_Submit();
        }

        public string Register_Init()
        {
            return "注册页面";
        }


        /// <summary>
        /// 微信小程序登录BPM
        /// </summary>  
        public string WXLogin_In()
        {
            string userID = GetRequestVal("OpenID");
            string orgNo = GetRequestVal("OrgNo");

            BP.Web.WebUser.OrgNo = orgNo;

            BP.WF.Dev2Interface.Port_Login(userID,  orgNo);

            return "url@/App/Portal/Home.htm?UserNo=" + userID + "&OrgNo=" + orgNo;
        }


        /// <summary>
        /// 后台退出方法
        /// </summary>  
        public string Login_Exit()
        {
            BP.Web.GuestUser.Exit();
            return "退出成功";
        }
        /// <summary>
        /// 公司图标保存的方法
        /// </summary>       
        public string UPOrgIcon_Save()
        {
            string empNo = this.GetRequestVal("EmpNo");
            string dict = HttpContextHelper.Request.Path + "/DataUser/OrgIcon";
            //判断当前路径下的文件夹是否存在
            if (!Directory.Exists(dict))
            {
                // 如果没有则创建文件夹
                Directory.CreateDirectory(dict);
            }
            //获取前台name="file_upload"提交的图片
            var file = HttpContextHelper.RequestFiles(0);
            //截取文件的后缀名//file.filename是文件的全名例如123.jpg
            string subfixname = file.FileName.Substring(file.FileName.LastIndexOf("."));
            //这里统一将图片后缀名为.jpg
            string tempFile = SystemConfig.PathOfDataUser + "OrgIcon/" + empNo + ".jpg";
            if (System.IO.File.Exists(tempFile) == true)
                System.IO.File.Delete(tempFile);
            HttpContextHelper.UploadFile(file, tempFile);
            return "上传成功";
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        public string ReqPassword_ResetPass()
        {
            string email = this.GetRequestVal("TB_Email");
            if (DataType.IsNullOrEmpty(email) == false)
            {
                Emp ep = new Emp();
                int retriresults = ep.Retrieve("Email", email);
                if (retriresults != 1)
                {
                    return "err@邮箱填写有误";
                }
                if (BP.Difference.SystemConfig.IsEnablePasswordEncryption == true)
                    ep.Pass = BP.Tools.Cryptography.EncryptString(this.GetRequestVal("TB_PassWord2"));
                ep.Update();
                return "重置密码成功";
            }
            return "err@邮箱输入有误";

        }
        /// <summary>
        /// 调用163发送邮箱验证码
        /// </summary>
        /// <returns></returns>
        public string ReqPass_SendCode()
        {
            try
            {
                string email = this.GetRequestVal("TB_Email");
                Emps emps = new Emps();
                QueryObject qo = new QueryObject(emps);
                qo.AddWhere("Email", email);
                int doresults = qo.DoQuery();
                if (doresults != 1)
                    return "err@邮箱填写有误";

                MailMessage myMail = new MailMessage();
                myMail.From = new MailAddress("chichengsoftyun@163.com");
                myMail.To.Add(new MailAddress(email));
                myMail.Subject = "科伦云用户验证";
                myMail.SubjectEncoding = Encoding.UTF8;
                Random dom = new Random();
                int code = dom.Next(0, 9999);
                myMail.Body = code.ToString();
                myMail.BodyEncoding = Encoding.UTF8;
                myMail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.EnableSsl = true;//是否使用加密链接
                smtp.Host = "smtp.163.com";
                //第一个参数为163邮箱账户第二个参数为smtp协议授权码如要配置信息请登录163官网账户:chichengsoftyun密码:sadf23adsfj90s
                smtp.Credentials = new NetworkCredential("chichengsoftyun", "RJISMPAKRXLVOABX");
                smtp.Send(myMail);
                return "发送成功" + code;
            }
            catch (Exception ex)
            {
                return "err@发送失败" + ex.Message;
            }
        }
        /// <summary>
        /// 注册页面提交
        /// </summary>
        /// <returns></returns>
        public string RegisterAdminer_Submit()
        {
            string tel = this.GetRequestVal("TB_Adminer");//管理员名称拼音.
            // weixin 
            string openid = this.GetRequestVal("openid");

            string unionid = this.GetRequestVal("unionid");

            //检查手机号是否存在？
            //org.No = this.GetRequestVal("TB_OrgNo");
            //调用方法生成OrgNo.
            string url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoType=IsExitUserNo&UserNo=" + tel;
            string str = DataType.ReadURLContext(url);
            if (str.Equals("1") == true)
                return "err@该账号已经注册过了.";

            if (str.IndexOf("err@") == 0)
                return "err@判断是否注册出现错误." + str;


            //检查注册的安全性.
            string ip = BP.Difference.Glo.GetIP;
            string sql = "SELECT COUNT(*) FROM Port_Org WHERE RegIP='" + ip + "' AND DTReg LIKE '" + DataType.CurrentDate + "%'";
            int num = DBAccess.RunSQLReturnValInt(sql);
            if (num >= 4)
                return "err@系统错误，不能连续注册。";

            string adminer = this.GetRequestVal("TB_AdminerName"); //管理员名称中文.

            BP.Cloud.Emp ep = new BP.Cloud.Emp();

            #region 检查admin是否存在？当前用户表里,如果第一次安装就初始化数据.
            //检查admin是否存在？当前用户表里,如果第一次安装就初始化数据.
            ep.No = "admin";
            if (ep.IsExits == false)
            {
                string sqlscript =  BP.Difference.SystemConfig.PathOfWebApp + "InitData.sql";
                DBAccess.RunSQLScript(sqlscript);
            }
            #endregion 检查admin是否存在？当前用户表里,如果第一次安装就初始化数据.

            ep.UserID = tel;
            ep.Name = adminer;

            BP.Cloud.Org org = new Org();
            try
            {
                //admin登录.
                BP.Cloud.Dev2Interface.Port_Login("admin", "ccs");

                //org.No = this.GetRequestVal("TB_OrgNo");
                //调用方法生成OrgNo.
                url = "http://passport."+BP.Cloud.Glo.SaasHost+"/Handler.ashx?DoType=GenerNewOrgNo";
                org.No = DataType.ReadURLContext(url, 9000);

                org.Name = this.GetRequestVal("TB_OrgName");
                org.NameFull = this.GetRequestVal("TB_OrgNameFull");

                //避免其他的注册错误.
                BP.Web.WebUser.OrgNo = org.No;
                BP.Web.WebUser.OrgName = org.Name;

                org.RegFrom = 0; //0=网站.1=企业微信.
                org.Adminer = tel;
                org.AdminerName = ep.Name;
                org.DTReg = DataType.CurrentDateTime;

                //获取来源.
                string from = this.GetRequestVal("From");
                if (DataType.IsNullOrEmpty(from) == true)
                    from = "ccbpm";
                org.UrlFrom = from;
                org.DirectInsert();

                ep.No = org.No + "_" + tel;
                ep.UserID = tel;
                ep.FK_Dept = org.No;
                if (ep.RetrieveFromDBSources() == 1)
                    throw new Exception("err@该姓名用户" + tel + "已经存在.");

                ep.No = org.No + "_" + tel;
                ep.UserID = tel;
                ep.FK_Dept = org.No;

                //循环遍历 看邮箱是否唯一用户忘记密码用邮箱找回.
                string email = this.GetRequestVal("TB_Email");
                ep.Email = email;
                ep.Name = adminer;
                ep.Pass = this.GetRequestVal("TB_PassWord2");

                //密码加密。
                if (BP.Difference.SystemConfig.IsEnablePasswordEncryption == true)
                    ep.Pass = BP.Tools.Cryptography.EncryptString(ep.Pass);

                //处理拼音
                string pinyinQP = BP.DA.DataType.ParseStringToPinyin(ep.Name).ToLower();
                string pinyinJX = BP.DA.DataType.ParseStringToPinyinJianXie(ep.Name).ToLower();
                ep.PinYin = "," + pinyinQP + "," + pinyinJX + ",";

                ep.OrgName = this.GetRequestVal("TB_OrgName");
                ep.FK_Dept = org.No;
                ep.OrgNo = org.No;
                ep.No = ep.OrgNo + "_" + tel;
                ep.DirectInsert();

                //初始化Port_User信息表.
                User user = new User();
                user.Copy(ep);
                user.No = ep.UserID;
                user.Pass = ep.Pass;
                user.OpenID = openid;
                user.UnionID = unionid;
                if (user.RetrieveFromDBSources() == 0)
                    user.Insert();
                else
                    user.Update();

                //让 组织 管理员登录.
                string token = BP.Cloud.Dev2Interface.Port_Login(ep.UserID, org.No);

                //初始化数据.
                org.Adminer = ep.UserID;
                org.AdminerName = ep.Name;
                org.Init_OrgDatas();

                #region 把数据注册到用户中心.
                url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoType=RegOrg&UrlFrom=" + from + "&CurrDB=" + BP.Difference.SystemConfig.AppSettings["CurrDB"];

                url += "&UserNo=" + tel;
                url += "&UserName=" + ep.Name;
                url += "&MM=" + ep.Pass;

                string json = org.ToJson();
                string data = BP.WF.Glo.HttpPostConnect(url, json);
                if (data.IndexOf("err@") == 0)
                    throw new Exception("err@写入组织信息到中心库失败:" + data);

                //url = "http://passport."+BP.Cloud.Glo.SaasHost+"/Handler.ashx?DoType=RegUser";
                //json = user.ToJson();
                //data = BP.WF.Glo.HttpPostConnect(url, json);
                //if (data.IndexOf("err@") == 0)
                //    throw new Exception("err@写入用户信息到中心库失败:" + data);
                #endregion 把数据注册到用户中心.

                return "http://AppCenter." + BP.Cloud.Glo.SaasHost + "/App/GotoUrl.htm?OrgNo=" + org.No + "&Token=" + token + "&UserNo=" + tel;

                //调用服务，把数据传入里面去. 组织数据，人员数据就好.
                // return token;
                // var url = "/App/Portal/Home.htm?UserNo=" + WebUser.No + "&Token=" + WebUser.SID + "&OrgNo=" + WebUser.OrgNo;
                // return url;
                // return ep.No;
            }
            catch (Exception ex)
            {
                org.DoDelete();
                BP.WF.Dev2Interface.Port_SigOut();
                return "err@安装期间出现错误:" + ex.Message;
            }
            ////让其退出登录.
            //BP.Web.GuestUser.Exit();
            //BP.WF.Dev2Interface.Port_Login(ep.No);
            //string orgno = WebUser.OrgNo;
        }

        public void InitFlowSorts()
        {
            #region 流程树.
            BP.Cloud.Template.FlowSort fs = new BP.Cloud.Template.FlowSort();
            fs.No = this.No; //公司编号
            fs.Name = "流程树";
            fs.OrgNo = this.No;
            fs.ParentNo = "100"; //这里固定死了,必须是100.
            fs.DirectInsert();

            fs.No = DBAccess.GenerGUID();
            fs.ParentNo = this.No; //帐号信息.
            fs.Name = "日常办公";
            fs.OrgNo = this.No;
            fs.DirectInsert();

            fs.No = DBAccess.GenerGUID();
            fs.ParentNo = this.No; //帐号信息.
            fs.Name = "财务类";
            fs.OrgNo = this.No;
            fs.DirectInsert();

            fs.No = DBAccess.GenerGUID();
            fs.ParentNo = this.No;
            fs.Name = "人力资源类";
            fs.OrgNo = this.No;
            fs.DirectInsert();
            #endregion 开始流程树.
        }

        public string SelectOneOrg_Selected()
        {
            string deptNo = this.GetRequestVal("DeptNo");
            DeptEmp de = new DeptEmp();
            int i = de.Retrieve(DeptEmpAttr.FK_Emp, WebUser.No, DeptEmpAttr.FK_Dept, deptNo);
            if (i == 0)
                return "err@您没有此部门的权限.";

            string no = de.OrgNo + "_" + WebUser.No;
            BP.Cloud.Emp emp = new Cloud.Emp();
            emp.No = no;
            if (emp.RetrieveFromDBSources() == 0)
                return "err@人员错误:" + no;
            emp.FK_Dept = deptNo;
            emp.OrgNo = de.OrgNo;
            emp.Update();

            //执行登录.
            BP.Cloud.Dev2Interface.Port_Login(emp.UserID, emp.OrgNo);

            string host = "http://appcenter." + BP.Cloud.Glo.SaasHost + "";

            var url = host + "/App/GoToUrl.htm?OrgNo=" + emp.OrgNo + "&UserNo=" + emp.No + "&Token="+ BP.Web.WebUser.Token;
            return url;
        }
        public string SelectOneOrg_Init()
        {
            DeptEmps des = new DeptEmps();
            des.Retrieve(DeptEmpAttr.FK_Emp, WebUser.No);

            if (des.Count == 0)
                return "err@该人员没有部门信息." + WebUser.No;

            DataTable dt = new DataTable();
            dt.Columns.Add("DeptNo");
            dt.Columns.Add("DeptName");
            dt.Columns.Add("OrgNo");
            dt.Columns.Add("OrgName");
            dt.Columns.Add("IsAdmin");
            foreach (DeptEmp item in des)
            {
                DataRow dr = dt.NewRow();
                //部门信息.
                Dept dept = new Dept();
                dept.No = item.FK_Dept;
                if (dept.RetrieveFromDBSources() == 0)
                {
                    item.Delete();
                    continue;
                }

                dr["DeptNo"] = dept.No;
                dr["DeptName"] = dept.Name;

                //组织.
                Org org = new Org(dept.OrgNo);
                dr["OrgNo"] = org.No;
                dr["OrgName"] = org.Name;

                //检查是否是admin.
                BP.WF.Port.Admin2Group.OrgAdminers admins = new BP.WF.Port.Admin2Group.OrgAdminers();
                admins.Retrieve(BP.WF.Port.Admin2Group.OrgAdminerAttr.OrgNo, org.No,
                   BP.WF.Port.Admin2Group.OrgAdminerAttr.FK_Emp, WebUser.No);

                //数量.
                dr["IsAdmin"] = admins.Count;
                dt.Rows.Add(dr);
            }
            return BP.Tools.Json.ToJson(dt);
        }
        /// <summary>
        /// 登录的时候判断.
        /// </summary>
        /// <returns></returns>
        public string Login_Submit()
        {
            string isMobile = this.GetRequestVal("isMobile");
            if (DataType.IsNullOrEmpty(isMobile) == true)
                isMobile = "0";
            string usrNoRel = this.GetRequestVal("No");
            string orgNo = this.GetRequestVal("OrgNo");
            string pass = this.GetRequestVal("PassWord").Trim();

            string sid = "";

            //如果推出的，能够找到帐的，就直接在本行号进行。
            if (DataType.IsNullOrEmpty(orgNo) == false)
            {
                Emp myEmp = new Emp();
                int i = myEmp.Retrieve("UserID", usrNoRel, "OrgNo", orgNo);

                if (i == 0)
                    return "err@用户名错误.";

                if (myEmp.CheckPass(pass) == false)
                    return "err@用户名或密码错误.";

                //设置他的组织，信息.
                WebUser.No = myEmp.UserID; //登录帐号.
                WebUser.FK_Dept = myEmp.FK_Dept;
                WebUser.FK_DeptName = myEmp.FK_DeptText;

                BP.Web.WebUser.No = myEmp.UserID;

                sid = BP.Cloud.Dev2Interface.Port_Login(myEmp.UserID, myEmp.OrgNo);

                if (isMobile.Equals("1") == true)
                    return "url@Home.htm?UserNo=" + myEmp.UserID + "&OrgNo=" + myEmp.OrgNo + "&Token=" + WebUser.Token;
                else
                    return "url@/App/Portal/Home.htm?UserNo=" + myEmp.UserID + "&OrgNo=" + myEmp.OrgNo + "&Token=" + WebUser.Token;
            }

            string userNo = usrNoRel;

            Emps emps = new Emps();
            emps.Retrieve("UserID", userNo);
            if (emps.Count == 0)
                return "err@用户名或密码错误.";

            Emp emp = emps[0] as Emp;
            //设置orgNo.
            BP.Web.WebUser.OrgNo = emp.OrgNo;
            if (emp.CheckPass(pass) == false)
                return "err@用户名或密码错误.";

            //设置他的组织，信息.
            WebUser.No = emp.UserID; //登录帐号.
            WebUser.Name = emp.Name; //登录帐号.
            WebUser.FK_Dept = emp.FK_Dept;
            WebUser.FK_DeptName = emp.FK_DeptText;
            WebUser.OrgNo = emp.OrgNo;

            //设置他的userID作为No.
            BP.Web.WebUser.No = emp.UserID;
            sid = BP.Cloud.Dev2Interface.Port_Login(userNo, emp.OrgNo);

            WebUser.Token = sid; //设置SID.
         
            if (emps.Count == 1)
            {
                if (IsMobile.Equals("1") == true)
                    return "url@Home.htm?UserNo=" + emp.UserID + "&OrgNo=" + emp.OrgNo + "&Token=" + WebUser.Token;
                else
                    return "url@/App/Portal/Home.htm?UserNo=" + emp.UserID + "&OrgNo=" + emp.OrgNo + "&Token=" + WebUser.Token;
            }

            return "url@/App/Portal/SelectOneOrg.htm?Token=" + WebUser.Token + "&UserNo=" + emp.UserID + "&OrgNo=" + emp.OrgNo;
        }
        /// <summary>
        /// 注册按钮链接的生成和嵌入 ,获取注册码接口
        /// </summary>
        /// <returns></returns>
        public string RegistQiYeWeixin()
        {
            String provider_access_token = BP.Cloud.WeXinAPI.Glo.getProviderAccessToken();
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("template_id", BP.Cloud.WeXinAPI.Glo.TemplateId);

            /* parameters.Add("corp_name", BP.Cloud.WeXinAPI.Glo.CorpName);
             parameters.Add("admin_name", BP.Cloud.WeXinAPI.Glo.AdminName);
             parameters.Add("admin_mobile", BP.Cloud.WeXinAPI.Glo.AdminMobile);
             parameters.Add("state", "TestState123");
             parameters.Add("follow_user", BP.Cloud.WeXinAPI.Glo.CorpID);*/
            string url = "https://qyapi.weixin.qq.com/cgi-bin/service/get_register_code?provider_access_token=" + provider_access_token;

            //获得返回的数据.
            string res = httpWebResponseUtility.HttpResponsePost_Json(url, JsonConvert.SerializeObject(parameters));
            Dictionary<string, object> ddresS = res.Trim(new char[] { '{', '}' }).Split(',').ToDictionary(s => s.Split(':')[0].Trim('"'), s => (object)s.Split(':')[1].Trim('"'));
            string register_code = (string)ddresS["register_code"];

            //授权链接
            string registUrl = "https://open.work.weixin.qq.com/3rdservice/wework/register?register_code=" + register_code;

            return registUrl;
        }
        /// <summary>
        /// 企业微信扫描登陆返回url.
        /// </summary>
        /// <returns></returns>
        public string QiYeWeiXinSaoMa()
        {
            //回调url
            string redirect_uri = HttpUtility.UrlEncode(BP.Cloud.WeXinAPI.Glo.Domain + "Admin/WeChat/CallBack.aspx");

            //授权链接
            string oatuth2 = "https://open.work.weixin.qq.com/wwopen/sso/3rd_qrConnect?appid=" + BP.Cloud.WeXinAPI.Glo.CorpID +
                "&redirect_uri=" + redirect_uri;


            return oatuth2;
        }
        /// <summary>
        /// 企业微信扫描安装返回url.
        /// </summary>
        /// <returns></returns>
        public string QiYeWeiXinSaoMaInstall()
        {

            //获取第三方应用凭证
            string suitAccessToken = BP.Cloud.WeXinAPI.Glo.getSuitAccessToken();
            if (string.IsNullOrEmpty(suitAccessToken))
            {
                return "";
            }
            string yuUrl = "https://qyapi.weixin.qq.com/cgi-bin/service/get_pre_auth_code?suite_access_token=" + suitAccessToken;

            string res = httpWebResponseUtility.HttpResponseGet(yuUrl);
            Dictionary<string, object> dd = res.Trim(new char[] { '{', '}' }).Split(',').ToDictionary(s => s.Split(':')[0].Trim('"'), s => (object)s.Split(':')[1].Trim('"'));
            string preAuthCode = (string)dd["pre_auth_code"];
            if (string.IsNullOrEmpty(preAuthCode))
            {
                return "";
            }
            //设置授权配置,该接口可对某次授权进行配置。可支持测试模式（应用未发布时）。
            string resS = setSessionInfo(suitAccessToken, preAuthCode);
            Dictionary<string, object> ddresS = resS.Trim(new char[] { '{', '}' }).Split(',').ToDictionary(s => s.Split(':')[0].Trim('"'), s => (object)s.Split(':')[1].Trim('"'));
            string errcode = (string)ddresS["errcode"];
            if (!errcode.Equals("0"))
            {
                return "";
            }
            string redirect_uri = HttpUtility.UrlEncode(BP.Cloud.WeXinAPI.Glo.Domain + "Admin/WeChat/CallReg.aspx");

            //跳转链接中，第三方服务商需提供suite_id、预授权码、授权完成回调URI和state参数。其中redirect_uri是授权完成后的回调网址，redirect_uri需要经过一次urlencode作为参数；state可填a - zA - Z0 - 9的参数值（不超过128个字节），用于第三方自行校验session，防止跨域攻击。
            string urlInstall = "https://open.work.weixin.qq.com/3rdapp/install?suite_id=" + BP.Cloud.WeXinAPI.Glo.SuiteID +
                "&pre_auth_code=" + preAuthCode + "&redirect_uri=" + redirect_uri + "&state=1";
            return urlInstall;
        }
        ///<summary>
        ///设置授权配置
        ///该接口可对某次授权进行配置。可支持测试模式（应用未发布时）。
        ///请求方式：POST（HTTPS）
        ///请求地址： https://qyapi.weixin.qq.com/cgi-bin/service/set_session_info?suite_access_token=SUITE_ACCESS_TOKEN
        /// </summary>
        public string setSessionInfo(string suitAccessToken, string preAuthCode)
        {
            string url = "https://qyapi.weixin.qq.com/cgi-bin/service/set_session_info?suite_access_token=" + suitAccessToken;

            string parameters = "{\"pre_auth_code\":\"" + preAuthCode + "\",\"session_info\":{\"appid\":[],\"auth_type\":0}}";

            string res = httpWebResponseUtility.HttpResponsePost_Json(url, parameters);
            return res;
        }

        public string Root_Guest_Login()
        {
            //获取code
            string code = this.GetRequestVal("code");
            string state = this.GetRequestVal("state");
            if (DataType.IsNullOrEmpty(state) == false)
            {
                string[] strs = state.Split(',');
                //获取组织
                WebUser.OrgNo = strs[1].Replace("OrgNo_", "");

            }
            //获取token
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + BP.Cloud.WeXinAPI.Glo.CorpID + "&secret=SECRET&code=" + code + "&grant_type=authorization_code";
            string res = new HttpWebResponseUtility().HttpResponseGet(url);
            JsonData jd = JsonMapper.ToObject(res);
            if (res.Contains("errcode") == true)
            {
                Object errcode = jd["errcode"];
                if (errcode != null)
                {
                    return "err@网页授权access_token获取失败" + res;
                }
            }
            string access_token = jd["access_token"].ToString();
            string openid = jd["openid"].ToString();

            //根据token、openid获取用户信息
            url = "https://api.weixin.qq.com/sns/userinfo?access_token=" + access_token + "&openid=" + openid + "&lang=zh_CN";
            res = httpWebResponseUtility.HttpResponseGet(url);
            jd = JsonMapper.ToObject(res);
            if (res.Contains("errcode") == true)
            {
                Object errcode1 = jd["errcode"];
                if (errcode1 != null)
                {
                    return "err@网页授权获取用户信息失败";
                }
            }

            openid = jd["openid"].ToString();
            string name = jd["nickname"].ToString();
            Guest guest = new Guest();
            guest.No = openid;
            if (guest.RetrieveFromDBSources() == 0)
            {
                //插入客户信息
                guest.Insert();
                return "GusetNo@" + openid;
            }

            //判断当前的组织结构中是否存在Guest用

            //外部客户登陆
            BP.Cloud.Dev2InterfaceGuest.Port_Login(guest.No, guest.Name);

            return "";
        }
        /// <summary>
        /// 外部用户登陆
        /// </summary>
        public void Guest_Login()
        {
            Guest guest = new Guest();
            guest.No = this.GetRequestVal("GuestNo");
            if (guest.RetrieveFromDBSources() == 0)
                throw new Exception("err@还没有记录外部用户信息");
            guest.Name = this.GetRequestVal("GuestName");
            guest.Tel = this.GetRequestVal("Tel");
            guest.Update();
            //外部客户登陆
            BP.Cloud.Dev2InterfaceGuest.Port_Login(guest.No, guest.Name);
        }
        public static string GetFunction(string url)
        {
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
        /// 生成微信小程序的access_token
        /// </summary>
        /// <returns></returns>
        public static string JsCode2Session()
        {
            string JsCode2SessionUrl = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + secret;
            var url = string.Format(JsCode2SessionUrl, appid, secret);
            var str = GetFunction(url);
            try
            {
                LitJson.JsonData jo = LitJson.JsonMapper.ToObject(str);
                string access_token = jo["access_token"].ToString();
                return access_token;
            }
            catch (Exception ex)
            {
                return "@err生成access_token错误-" + ex.Message;
            }
        }
        /// <summary>
        /// 将数据流转为byte[]
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToBytes(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            int temp = stream.ReadByte();
            while (temp != -1)
            {
                bytes.Add((byte)temp);
                temp = stream.ReadByte();
            }
            return bytes.ToArray();
        }
        /// <summary>
        /// 返回二维码图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string CreateWeChatQrCode(string url, string param, string path, string imgName)
        {
            string strURL = url;
            HttpWebRequest request;
            try
            {
                request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                string paraUrlCoded = param;
                //byte[] payload;
                byte[] payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                request.ContentLength = payload.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                System.Net.HttpWebResponse response;
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s;
                s = response.GetResponseStream();//返回图片数据流
                byte[] tt = StreamToBytes(s);//将数据流转为byte[]

                imgName = imgName + ".jpg";
                //获取相对于应用的基目录创建目录
                //string imgPath = System.Web.Hosting.HostingEnvironment.MapPath(path);//System.AppDomain.CurrentDomain.baxxxxseDirectory + path;     //通过此对象获取文件名
                //if (Directory.Exists(imgPath) == false)
                //    Directory.CreateDirectory(imgPath);

                //System.IO.File.WriteAllBytes(HttpContextHelper.Current.Server.MapPath(path + imgName), tt);//讲byte[]存储为图片
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return path + imgName;
        }
        /// <summary>
        /// 图片转二进制流
        /// </summary>
        /// <returns></returns>
        public byte[] ImageDatabytes(string FilePath)
        {

            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read); //将图片以文件流的形式进行保存
            BinaryReader br = new BinaryReader(fs);
            byte[] imgBytesIn = br.ReadBytes((int)fs.Length); //将流读入到字节数组中
            return imgBytesIn;

        }
        /// <summary>
        /// 生成登录（注册的）二维码
        /// </summary>
        /// <param name="page"></param>
        /// <param name="scene"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public string CreateUnitWxCode()
        {

            string page = GetRequestVal("page");

            string scene = "text=1";

            string imgName = GetRequestVal("imgName");
            string path = "/ImgCode/RegUnit/";
            string width = "700";
            string ret = string.Empty;
            string access_token = JsCode2Session();
            string DataJson;
            string url = "https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token;
            DataJson = "{";
            DataJson += string.Format("\"scene\":\"{0}\",", scene);//所要传的参数用,分开
            DataJson += string.Format("\"width\":\"{0}\",", width);

            //扫码所要跳转的地址，根路径前不要填加'/'不能携带参数（参数请放在scene字段里），如果不填写这个字段，默认跳主页面
            DataJson += string.Format("\"page\":\"{0}\"", page);
            DataJson += "}";

            ret = CreateWeChatQrCode(url, DataJson, path, imgName);//返回图片地址.
            byte[] bmpBytes = ImageDatabytes(BP.Difference.SystemConfig.CCFlowAppPath + ret);
            //如果转字符串的话.
            string BmpStr = Convert.ToBase64String(bmpBytes);
            return BmpStr;
        }
        /// <summary>
        /// 生成增加人员的二维码
        /// </summary>
        /// <param name="page"></param>
        /// <param name="scene"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public string CreateAddEmpWxCode()
        {
            string page = GetRequestVal("page");
            string deptNo = GetRequestVal("DeptNo");
            string orgNo = GetRequestVal("OrgNo");
            string scene = "OrgNo=" + orgNo + ",DeptNo=" + deptNo;

            string imgName = GetRequestVal("imgName");
            string path = "/ImgCode/OrgAddEmps/" + orgNo + "-AddEmps/";
            string width = "400";
            string ret = string.Empty;
            string access_token = JsCode2Session();
            string DataJson;
            string url = "https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token;
            DataJson = "{";
            DataJson += string.Format("\"scene\":\"{0}\",", scene);//所要传的参数用,分开
            DataJson += string.Format("\"width\":\"{0}\",", width);

            //扫码所要跳转的地址，根路径前不要填加'/'不能携带参数（参数请放在scene字段里），如果不填写这个字段，默认跳主页面
            DataJson += string.Format("\"page\":\"{0}\"", page);
            DataJson += "}";

            ret = CreateWeChatQrCode(url, DataJson, path, imgName);//返回图片地址.
            byte[] bmpBytes = ImageDatabytes(BP.Difference.SystemConfig.CCFlowAppPath + ret);
            //如果转字符串的话.
            string BmpStr = Convert.ToBase64String(bmpBytes);
            return BmpStr;
        }
        /// <summary>
        /// 同步到用户中心库.
        /// </summary>
        /// <param name="orgNo">组织编号</param>
        /// <param name="urlFrom">注册人</param>
        public void DTS_OrgData(string orgNo, string urlFrom)
        {
            //定义一个数据容器.
            DataSet ds = new DataSet();

            Org org = new Org(orgNo);
            ds.Tables.Add(org.ToDataTableField("Port_Org"));

            Emps emps = new Emps();
            emps.Retrieve(DeptAttr.OrgNo, orgNo);
            ds.Tables.Add(emps.ToDataTableField("Port_Emp"));

            Depts depts = new Depts();
            depts.Retrieve(DeptAttr.OrgNo, orgNo);
            ds.Tables.Add(depts.ToDataTableField("Port_Dept"));

            //DeptEmps des = new DeptEmps();
            //des.Retrieve(DeptAttr.OrgNo, orgNo);
            //ds.Tables.Add(des.ToDataTableField("Port_DeptEmp"));

            //OrgAdminers oas = new OrgAdminers();
            //oas.Retrieve(DeptAttr.OrgNo, orgNo);
            //ds.Tables.Add(oas.ToDataTableField("Port_OrgAdminer"));

            #region 把数据注册到用户中心.
            string url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoType=QYWX_Create&CurrDB=" + BP.Difference.SystemConfig.AppSettings["CurrDB"];
            url += "&UserNo=" + BP.Web.WebUser.No;
            url += "&UrlFrom=" + urlFrom;
            url += "&UserName=" + WebUser.Name;
            url += "&MM=11";

            string json = BP.Tools.Json.ToJson(ds);
            string data = BP.WF.Glo.HttpPostConnect(url, json);
            if (data.IndexOf("err@") == 0)
                throw new Exception("err@写入组织信息到中心库失败:" + data);

            #endregion 把数据注册到用户中心.


        }
    }
}
