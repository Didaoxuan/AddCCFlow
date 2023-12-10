using System;
using System.Collections.Generic;
using System.Data;
using BP.DA;
using BP.Web;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BP.Sys;
using System.Drawing;
using System.Net.Http;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class App_Organization : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App_Organization()
        {
        }
        #region 二维码邀请.
        public string RegUser_Init()
        {
            // if (Glo.IsEnableRegUser == false)
            // return "err@该系统尚未启动注册功能，请通知管理员把全局配置项 IsEnableRegUser 设置为1，没有该项就添加该配置项.";

            //返回部门信息，用与绑定部门.
            Depts ens = new Depts();
            ens.Retrieve("OrgNo", this.OrgNo);
            return ens.ToJson();
        }
        /// <summary>
        /// 生成二维码.
        /// </summary>
        /// <returns></returns>
        public string InvitedQRCode_Init()
        {
            // 测试时可以修改URL
            string url = "http://appcenter." + BP.Cloud.Glo.SaasHost + "/App/Organization/Invited.htm?OrgNo=" + WebUser.OrgNo + "&From=" + WebUser.No;
            var img = QRCodeHelper.GenerateQRCode(url);
            var Content = "data:image/jpeg;base64," + Convert.ToBase64String(img);
            return JsonConvert.SerializeObject(Content);
        }
        /// <summary>
        /// 邀请注册提交
        /// </summary>
        /// <returns></returns>
        public string Invited_Submit()
        {
            try
            {
                string pass1 = this.GetRequestVal("TB_Pass1");
                Org org = new Org(this.OrgNo);

                //要切换当前的分库.
                string userNo = this.GetRequestVal("TB_No");
                string userName = this.GetRequestVal("TB_Name");
                string deptNo = this.GetRequestVal("DDL_Dept");

                Emp emp = new Emp();
                DeptEmp de = new DeptEmp();

                try
                {
                    emp.No = this.OrgNo + "_" + userNo;
                    if (emp.RetrieveFromDBSources() == 1)
                        return "err@该账号[" + userNo + "]已经存在【" + org.Name + "】,如果丢失密码请重新找回.";

                    emp.UserID = userNo;
                    emp.OrgNo = org.No;
                    emp.OrgName = org.Name;
                    emp.FK_Dept = deptNo;
                    emp.Name = userName;
                    emp.Tel = userNo;
                    emp.Insert();

                    de.FK_Dept = deptNo;
                    de.FK_Emp = emp.UserID;
                    de.EmpNo = emp.No;
                    de.OrgNo = org.No;
                    de.setMyPK(FK_Dept + "_" + userNo);
                    de.DirectInsert();
                }
                catch (Exception ex)
                {
                    return "err@写入本地库错误:" + ex.Message;
                }

                string url = "";
                try
                {
                    //调用接口写入用户中心.
                    url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoType=NewEmpByInvited&OrgNo=" + org.No + "&DeptNo=" + deptNo;
                    url += "&UserName=" + userName;
                    url += "&UserNo=" + userNo;
                    url += "&Pass=" + BP.Tools.Cryptography.EncryptString(pass1);
                    url += "&OpenID=123";

                    string str = DataType.ReadURLContext(url, 9000);
                }
                catch (Exception ex)
                {
                    emp.Delete();
                    de.Delete();
                    return "err@写入中心库错误，请联系系统管理员：" + ex.Message;
                }

                //返回要登陆的url.
                string token = BP.Cloud.Dev2Interface.Port_Login(userNo, OrgNo);
                url = "http://appcenter." + BP.Cloud.Glo.SaasHost + "/App/GotoUrl.htm?OrgNo=" + this.OrgNo + "&UserNo=" + userNo + "&Token=" + token;
                return url;
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }

        /// <summary>
        /// 微信的注册
        /// </summary>
        /// <returns></returns>
        public string InvitedByWX_Submit()
        {
            try
            {
                Org org = new Org(this.OrgNo);


                //要切换当前的分库.
                string userNo = this.GetRequestVal("TB_No"); //手机号
                string userName = this.GetRequestVal("TB_Name"); //名称.
                string deptNo = this.GetRequestVal("DDL_Dept"); //所在部门.

                //登陆ID.
                string openID = this.GetRequestVal("OpenID");


                Emp emp = new Emp();
                DeptEmp de = new DeptEmp();

                try
                {
                    emp.No = this.OrgNo + "_" + userNo;
                    if (emp.RetrieveFromDBSources() == 1)
                        return "err@该账号[" + userNo + "]已经存在【" + org.Name + "】,如果丢失密码请重新找回.";

                    emp.UserID = userNo;
                    emp.OrgNo = org.No;
                    emp.OrgName = org.Name;
                    emp.FK_Dept = deptNo;
                    emp.Name = userName;
                    emp.Tel = userNo;
                    emp.OpenID = openID;
                    emp.Insert();

                    de.FK_Dept = deptNo;
                    de.FK_Emp = emp.UserID;
                    de.EmpNo = emp.No;
                    de.OrgNo = org.No;
                    de.setMyPK(FK_Dept + "_" + userNo);
                    de.DirectInsert();
                }
                catch (Exception ex)
                {
                    return "err@写入本地库错误:" + ex.Message;
                }

                string url = "";
                try
                {
                    //调用接口写入用户中心.
                    url = "http://passport." + BP.Cloud.Glo.SaasHost + "/Handler.ashx?DoType=NewEmpByInvited&OrgNo=" + org.No + "&DeptNo=" + deptNo;
                    url += "&UserName=" + userName;
                    url += "&UserNo=" + userNo;
                    url += "&Pass=" + BP.DA.DBAccess.GenerGUID();
                    url += "&OpenID=" +  openID; //


                    string str = DataType.ReadURLContext(url, 9000);
                }
                catch (Exception ex)
                {
                    emp.Delete();
                    de.Delete();
                    return "err@写入中心库错误，请联系系统管理员：" + ex.Message;
                }

                //返回要登陆的url.
                string token = BP.Cloud.Dev2Interface.Port_Login(userNo, OrgNo);
                url = "http://appcenter." + BP.Cloud.Glo.SaasHost + "/App/GotoUrl.htm?OrgNo=" + this.OrgNo + "&UserNo=" + userNo + "&Token=" + token;
                return url;
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        #endregion 二维码邀请.
    }
}
