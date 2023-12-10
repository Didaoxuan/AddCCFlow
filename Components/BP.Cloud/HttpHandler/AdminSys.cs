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
using BP.Difference;


namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class AdminSys : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AdminSys()
        {

        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public string Login_Submit()
        {
            try
            {
                string userNo = this.GetRequestVal("TB_No");
                if (userNo == null)
                    userNo = this.GetRequestVal("TB_UserNo");

                string pass = this.GetRequestVal("TB_PW");
                if (pass == null)
                    pass = this.GetRequestVal("TB_Pass");

                if (userNo.Equals("admin") == true)
                {
                    userNo = "ccs_" + userNo;
                }
                else
                {
                    if (BP.DA.DataType.IsMobile(userNo) == false)
                        return "err@请使用手机号登录.";
                }

                //从数据库里查询.
                BP.Port.Emp emp = new BP.Port.Emp();
                emp.No = userNo;
                if (emp.RetrieveFromDBSources() == 0)
                    return "err@用户名或者密码错误.";

                //if (emp.CheckPass(pass) == false || 1==1 )
                //    return "err@用户名或者密码错误.";

                BP.Cloud.Emp empCloud = new Emp();
                empCloud.No = userNo;
                empCloud.Retrieve();

                //调用登录方法.
                BP.Cloud.Dev2Interface.Port_Login(empCloud);

                return "登陆成功";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
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

        #region xxx 界面 .
        #endregion xxx 界面方法.

    }
}
