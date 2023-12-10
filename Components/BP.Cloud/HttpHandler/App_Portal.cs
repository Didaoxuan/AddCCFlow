using System;
using System.Collections;
using System.Data;
using BP.DA;
using BP.Web;
using BP.En;
using BP.WF;
using BP.Difference;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class App_Portal : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App_Portal()
        {
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        public string PasswordReset_Save()
        {
            string pass = this.GetRequestVal("Password");
            string sid = this.GetRequestVal("Token");
            BP.Cloud.Emp emp = new Emp();
            emp.Retrieve("Token", sid);
            emp.DoResetpassword(pass, pass);

            return "重设成功，请您登录. http://" + BP.Cloud.Glo.SaasHost + "/index.htm";
        }
        public string PasswordReq_Save()
        {
            string val = this.GetRequestVal("Email");

            BP.Cloud.Emp emp = new Emp();
            QueryObject qo = new QueryObject(emp);
            qo.AddWhere("No", val);
            qo.addOr();
            qo.AddWhere("Email", val);
            int i = qo.DoQuery();
            if (i == 0)
                return "err@用户名或者E-mail输入错误.";

            emp.Update();

            string url = "http://" + BP.Cloud.Glo.SaasHost + "/App/Portal/PasswordReset.htm?Token=" +WebUser.Token;

            string docs = "请您重设密码:";
            docs += "，点击如下链接 <a href='" + url + "' target=_blank>" + url + "</a> , 或者将如下URL复制到您的浏览器;" + url;

            //发送邮件.
            SMS.SendEmailNowAsync(emp.Email, "科伦BPM云,密码重置.", docs);

            return "请登录到邮箱[" + emp.Email + "],重设密码.";

        }
        /// <summary>
        /// 获得菜单
        /// </summary>
        /// <returns></returns>
        public string Home_Menu()
        {
            if (WebUser.No == null)
                return "err@/App/index.htm?err=no_session";

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
            dr["Name"] = "流程中心";
            dr["ICON"] = "FlowCenter.png";
            dtSort.Rows.Add(dr);

            dr = dtSort.NewRow();
            dr["No"] = "02";
            dr["Name"] = "综合查询";
            dr["ICON"] = "FlowSearch.png";
            dtSort.Rows.Add(dr);

            if (WebUser.IsAdmin)
            {
                dr = dtSort.NewRow();
                dr["No"] = "03";
                dr["Name"] = "系统管理";
                dr["ICON"] = "System.png";
                dtSort.Rows.Add(dr);
            }
            #endregion 类别.

            #region 流程中心-菜单.
            dr = dtMenu.NewRow();
            dr["No"] = "Start";
            dr["Name"] = "发起";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/Start.htm";
            dr["ICON"] = "Start.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "Todolist";
            dr["Name"] = "待办";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/Todolist.htm";
            dr["ICON"] = "Todolist.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "Runing";
            dr["Name"] = "未完成";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/Runing.htm";
            dr["ICON"] = "Runing.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "CC";
            dr["Name"] = "抄送";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/CC.htm";
            dr["ICON"] = "CC.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "OneFlow";
            dr["Name"] = "流程应用";
            dr["SortNo"] = "01";
            dr["Url"] = "/App/Apps.htm";
            dr["ICON"] = "FlowApp.png";
            dtMenu.Rows.Add(dr);
            #endregion 流程中心-菜单.

            #region 流程查询-菜单.
            dr = dtMenu.NewRow();
            dr["No"] = "MyStartFlows";
            dr["Name"] = "我发起的";
            dr["SortNo"] = "02";
            dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.WF.Data.MyStartFlows";
            dr["ICON"] = "SearchMy.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "MyDeptFlows";
            dr["Name"] = "我部门发起的";
            dr["SortNo"] = "02";
            dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.WF.Data.MyDeptFlows";
            dr["ICON"] = "SearchMyDept.png";
            dtMenu.Rows.Add(dr);

            dr = dtMenu.NewRow();
            dr["No"] = "MyJoinFlows";
            dr["Name"] = "我审批的";
            dr["SortNo"] = "02";
            dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.WF.Data.MyJoinFlows";
            dr["ICON"] = "SearchMyCheck.png";
            dtMenu.Rows.Add(dr);
            #endregion 流程查询-菜单.

            #region 系统管理-菜单.
            if (WebUser.IsAdmin == true)
            {
                dr = dtMenu.NewRow();
                dr["No"] = "Template";
                dr["Name"] = "流程模版";
                dr["SortNo"] = "03";
                //   dr["Url"] = "/App/FlowDesigner/Flows.htm";
                dr["Url"] = "/App/Apps.htm";
                dr["ICON"] = "Template.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "TemplateShare";
                dr["Name"] = "共享模版";
                dr["SortNo"] = "03";
                // dr["Url"] = "/AppTemplate/Default.htm";
                dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.FrmTemplate.FrmExts";
                dr["ICON"] = "Share.png";
                dtMenu.Rows.Add(dr);


                dr = dtMenu.NewRow();
                dr["No"] = "Org";
                dr["Name"] = "组织结构";
                dr["SortNo"] = "03";
                dr["Url"] = "/App/Org/Organization.htm";
                dr["ICON"] = "Organization.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "Emps";
                dr["Name"] = "人员台账";
                dr["SortNo"] = "03";
                dr["Url"] = "/WF/Comm/Search.htm?EnsName=BP.Cloud.EmpWebs";
                dr["ICON"] = "Emps.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "StationTypes";
                dr["Name"] = "角色类型维护";
                dr["SortNo"] = "03";
                dr["Url"] = "/WF/Comm/Ens.htm?EnsName=BP.Cloud.StationTypeExts";
                dr["ICON"] = "StationTypes.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "Stations";
                dr["Name"] = "角色维护";
                dr["SortNo"] = "03";
                dr["Url"] = "/WF/Comm/Ens.htm?EnsName=BP.Cloud.StationExts";
                dr["ICON"] = "Stations.png";
                dtMenu.Rows.Add(dr);

                dr = dtMenu.NewRow();
                dr["No"] = "OrgSetting";
                dr["Name"] = "系统设置";
                dr["SortNo"] = "03";
                dr["Url"] = "/WF/Comm/RefFunc/En.htm?EnName=BP.Cloud.OrgSetting.Org&No=" + WebUser.OrgNo;
                dr["ICON"] = "OrgSetting.png";
                dtMenu.Rows.Add(dr);

                //dr = dtMenu.NewRow();
                //dr["No"] = "Invited";
                //dr["Name"] = "邀请同事加入";
                //dr["SortNo"] = "03";
                //dr["Url"] = "/App/Portal/Invited.htm";
                //dr["ICON"] = "Share.png";
                //dtMenu.Rows.Add(dr);

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
        public string Home_Init()
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
        public string Index_Init()
        {
            Hashtable ht = new Hashtable();
            ht.Add("Todolist_Runing", BP.WF.Dev2Interface.Todolist_Runing); //运行中.
            ht.Add("Todolist_EmpWorks", BP.WF.Dev2Interface.Todolist_EmpWorks); //待办
            ht.Add("Todolist_CCWorks", BP.WF.Dev2Interface.Todolist_CCWorks); //抄送.

            //本周.
            ht.Add("TodayNum", BP.WF.Dev2Interface.Todolist_CCWorks); //抄送.

            return BP.Tools.Json.ToJsonEntityModel(ht);
        }
        /// <summary>
        /// 邀请码@lizhen 生成邀请连接与Img.
        /// </summary>
        /// <returns></returns>
        public string Invited_Init()
        {
            return "";
        }
        public string Invited_CheckIsExit(string openID, string orgNo)
        {
            Emp emp = new Emp();
            emp.No = orgNo + "_" + openID;
            if (emp.RetrieveFromDBSources() == 1)
                return "err@yijiaru";
            return "";
        }
        /// <summary>
        /// 增加人员 @lizhen.
        /// </summary>
        /// <param name="openID"></param>
        /// <param name="orgNo"></param>
        /// <param name="tel"></param>
        /// <param name="empName"></param>
        /// <param name="deptNo"></param>
        /// <returns></returns>
        public string Invited_AddEmp(string openID, string orgNo, string userNo, string tel, string empName, string deptNo)
        {
            Org org = new Org(orgNo);

            Emp emp = new Emp();
            emp.No = orgNo + "_" + openID;
            if (emp.RetrieveFromDBSources() == 1)
                return "err@该人员已经存在.";

            //工号可以为空.
            if (DataType.IsNullOrEmpty(userNo) == true)
                userNo = tel;

            emp.UserID = userNo;
            emp.OpenID = openID;
            emp.OrgNo = org.No;
            emp.OrgName = org.Name;
            emp.FK_Dept = deptNo;
            emp.Name = empName;
            emp.Tel = tel;
            emp.Insert();

            DeptEmp de = new DeptEmp();
            de.FK_Dept = deptNo;
            de.FK_Emp = emp.UserID;
            de.OrgNo = orgNo;
            de.setMyPK(FK_Dept + "_" + tel);
            de.Insert();
            return "增加成功.";
        }

        #region 登录界面.
        /// <summary>
        /// 登录.
        /// </summary>
        /// <returns></returns>
        public string Login_Submit()
        {
            try
            {
                string userNo = this.GetRequestVal("TB_Adminer");
                // string defaultpass = this.GetRequestVal("DefaultPass");
                string pass = this.GetRequestVal("TB_PassWord2");
                BP.Cloud.Emp emp = new BP.Cloud.Emp();
                emp.No = userNo;
                if (emp.RetrieveFromDBSources() == 0)
                    return "err@用户名或者密码错误.";

                ////设置默认登陆
                //if (DataType.IsNullOrEmpty(defaultpass) == false)
                //{
                //    if (emp.Pass != defaultpass)
                //    {
                //        return "err@用户名或者密码错误";
                //    }
                //}
                //else
                //{
                if (emp.CheckPass(pass) == false)
                    return "err@用户名或者密码错误.";
                //  }

                string orgNo = this.GetRequestVal("OrgNo");
                if (DataType.IsNullOrEmpty(orgNo) == true)
                    orgNo = emp.OrgNo;

                //执行登录.
                return LoginExt(emp, orgNo);
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 获取用户下的组织
        /// </summary>
        /// <returns></returns>
        public string User_OrgNos()
        {
            string userId = this.GetRequestVal("UserID");
            if (DataType.IsNullOrEmpty(userId) == true)
                return "err@请输入用户的登录账号";
            string sql = " SELECT A.OrgNo as No,B.Name,B.OrgSta From Port_Emp A ,Port_Org B Where  A.OrgNo=B.No  AND UserID=" + SystemConfig.AppCenterDBVarStr + "UserID";
            Paras ps = new Paras();
            ps.Add("UserID", userId);
            DataTable dt = DBAccess.RunSQLReturnTable(sql, ps);
            if (dt.Rows.Count == 0)
                return "err@账号输入错误或者该用户已经被禁用";
            return BP.Tools.Json.ToJson(dt);
        }

        public string User_ChangeOrg()
        {
            string userId = this.GetRequestVal("UserID");
            if (DataType.IsNullOrEmpty(userId) == true)
                return "err@请输入用户的登录账号";
            if (DataType.IsNullOrEmpty(this.OrgNo) == true)
                return "err@组织编号不能为空";
            BP.WF.Dev2Interface.Port_Login(userId, this.OrgNo);
            return "切换成功";
        }
        public string LoginExt(BP.Cloud.Emp emp, string orgNo, string deptNo = null)
        {
            #region 求出来 orgNo .
            //获取当前人员的所有部门集合.
            DeptEmps des = new DeptEmps();
            des.Retrieve(DeptEmpAttr.FK_Emp, emp.UserID);
            //如果没有组织编号.
            if (DataType.IsNullOrEmpty(orgNo) == true || orgNo == "undefined")
            {
                //没有参数，就获得当前人员有多少个OrgNo ,如果有多个，就让其选择一个部门或者一个组织.
                string sql = "SELECT DISTINCT OrgNo FROM Port_DeptEmp WHERE fk_emp='" + emp.No + "'";
                DataTable dt = DBAccess.RunSQLReturnTable(sql);
                if (dt.Rows.Count == 0)
                    return "err@登录用户没有组织信息,无法登录.";

                if (dt.Rows.Count == 1)
                    orgNo = dt.Rows[0][0].ToString();

                if (dt.Rows.Count > 1)
                {
                    //调用登录方法.
                    BP.WF.Dev2Interface.Port_Login(emp.No);
                    return "url@SelectOneOrg.html";
                }
            }
            else if (DataType.IsNullOrEmpty(orgNo) == false) //如果是传递来的组织参数.
            {
                /*检查当前登陆者是否在该组织下？*/
                bool isHave = false;
                foreach (DeptEmp de in des)
                {
                    if (de.OrgNo == orgNo)
                        isHave = true;
                }
                if (isHave == false)
                    return "err@您没权限登录[" + orgNo + "]组织里.";
            }
            #endregion 求出来 orgNo .

            #region 求出来 fk_dept.
            //求出当前人员的主部门.
            string fk_dept = null;
            if (deptNo != null)
                fk_dept = deptNo;
            if (fk_dept == null)
            {
                foreach (DeptEmp de in des)
                {
                    if (de.OrgNo == orgNo && de.IsMainDept == true)
                    {
                        fk_dept = de.FK_Dept;
                        break;
                    }
                }
                if (fk_dept == null)
                    return "err@当前人员，没有设置主部门.";
            }

            //如果当前人员的部门与求出来的部门不一致，就更新port_emp表.
            if (emp.FK_Dept != fk_dept)
            {
                emp.FK_Dept = fk_dept;
                emp.Update();
            }
            #endregion 求出来 fk_dept.

            BP.Cloud.Dev2Interface.Port_Login(emp);

            //开始执行登录.
            // BP.WF.Dev2Interface.Port_Login(emp.No);
            return "登陆成功";

        }
        /// <summary>
        /// 执行登录
        /// </summary>
        /// <returns></returns>
        public string Login_Init()
        {
            string doType = GetRequestVal("LoginType");
            if (DataType.IsNullOrEmpty(doType) == false && doType.Equals("Out") == true)
            {
                //清空cookie
                WebUser.Exit();
            }

            Hashtable ht = new Hashtable();
            ht.Add("SysName", BP.Difference.SystemConfig.SysName);
            ht.Add("ServiceTel", BP.Difference.SystemConfig.ServiceTel);
            ht.Add("CustomerName", BP.Difference.SystemConfig.CustomerName);
            if (WebUser.NoOfRel == null)
            {
                ht.Add("UserNo", "");
                ht.Add("UserName", "");
            }
            else
            {
                ht.Add("UserNo", WebUser.No);

                string name = WebUser.Name;

                if (DataType.IsNullOrEmpty(name) == true)
                    ht.Add("UserName", WebUser.No);
                else
                    ht.Add("UserName", name);
            }

            return BP.Tools.Json.ToJsonEntityModel(ht);
        }
        #endregion 登录界面.

        #region SelectOneOrg.
       
        public string SelectOneOrg_Init()
        {
            DeptEmps des = new DeptEmps();
            des.Retrieve(DeptEmpAttr.FK_Emp, WebUser.No);

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
                Dept dept = new Dept(item.FK_Dept);
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

          //  string token=BP.WF.Dev2Interface.Port_GenerToken(User.)

            var url = "/App/Portal/Home.htm?OrgNo=" + emp.OrgNo + "&UserNo=" + emp.No + "&Token=" + WebUser.Token;
            return url;
        }
        #endregion SelectOneOrg.

        /// <summary>
        /// 获取当前用户最近使用的流程表单
        /// </summary>
        /// <returns></returns>
        public string GetUseFlowByUserNo()
        {
            string sql = "";
            int top = GetRequestValInt("Top");
            if (top == 0) top = 4;

            switch (BP.Difference.SystemConfig.AppCenterDBType)
            {
                case DBType.MSSQL:
                    sql = " SELECT TOP " + top + " FK_Flow,FlowName,F.Icon FROM WF_GenerWorkFlow G ,WF_Flow F WHERE  F.No=G.FK_Flow AND Starter='" + WebUser.No + "' AND G.OrgNo='" + WebUser.OrgNo + "' GROUP BY FK_Flow,FlowName,ICON ORDER By Max(SendDT) DESC";
                    break;
                case DBType.MySQL:
                case DBType.PostgreSQL:
                case DBType.UX:
                    sql = " SELECT DISTINCT FK_Flow,FlowName,F.Icon FROM WF_GenerWorkFlow G ,WF_Flow F WHERE  F.No=G.FK_Flow AND Starter='" + WebUser.No + "'  AND G.OrgNo='" + WebUser.OrgNo + "'   Order By SendDT  limit  " + top;
                    break;
                case DBType.Oracle:
                case DBType.DM:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    sql = " SELECT * From (SELECT DISTINCT FK_Flow as \"FK_Flow\",FlowName as \"FlowName\",F.Icon ,max(SendDT) SendDT FROM WF_GenerWorkFlow G ,WF_Flow F WHERE  F.No=G.FK_Flow AND Starter='" + WebUser.No + "' AND G.OrgNo='" + WebUser.OrgNo + "'  GROUP BY FK_Flow,FlowName,ICON Order By SendDT) WHERE  rownum <=" + top;
                    break;
                default:
                    throw new Exception("err@系统暂时还未开发使用" + BP.Difference.SystemConfig.AppCenterDBType + "数据库");
            }
            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            return BP.Tools.Json.ToJson(dt);
        }


        /// <summary>
        /// 当前登陆用户的信息
        /// </summary>
        /// <returns></returns>
        public string GetUserInfo()
        {
            Hashtable ht = new Hashtable();

            string userNo = Web.WebUser.No;
            if (DataType.IsNullOrEmpty(userNo) == true)
                return "";
            ht.Add("No", WebUser.No);
            ht.Add("Name", WebUser.Name);
            ht.Add("FK_Dept", WebUser.FK_Dept);
            ht.Add("FK_DeptName", WebUser.FK_DeptName);
            ht.Add("FK_DeptNameOfFull", WebUser.FK_DeptNameOfFull);
            string sql = "SELECT S.Name From Port_DeptEmpStation D,Port_Station S Where D.FK_Dept='" + WebUser.FK_Dept + "' AND D.FK_Station=S.No";
            string stations = DBAccess.RunSQLReturnStringIsNull(sql, "");
            ht.Add("FK_Stations ", stations.Replace(",", " "));
            ht.Add("OrgName", WebUser.OrgName);
            return BP.Tools.Json.ToJson(ht);
        }

        /// <summary>
        /// Main.htm页面中待办、抄送、退回、逾期的数量
        /// </summary>
        /// <returns></returns>
        public string GetToListCount()
        {
            Hashtable ht = new Hashtable();
            //待办的总数量
            ht.Add("EmpWorks", BP.Cloud.Dev2Interface.Todolist_EmpWorks);
            //抄送数量
            ht.Add("CCs", BP.Cloud.Dev2Interface.Todolist_CCWorks);
            //退回数量
            ht.Add("ReturnWorks", BP.Cloud.Dev2Interface.Todolist_ReturnNum);
            //逾期数量
            ht.Add("OverTimeWorks", BP.Cloud.Dev2Interface.Todolist_OverWorkNum);
            return BP.Tools.Json.ToJson(ht);
        }

        /// <summary>
        /// 获取我发起未完成的待办的流程
        /// 我参与的，我发起的，本部门发起的流程分析
        /// </summary>
        /// <returns></returns>
        public string FlowAnalyse_DataSet()
        {
            DataSet ds = new DataSet();

            //当前用户发起未完成的流程 
            Paras ps = new Paras();
            string str =  BP.Difference.SystemConfig.AppCenterDBVarStr;
            //获取待办、在途、已完成
            ps.SQL = "SELECT * FROM(select WorkID,'todolist' as RunningType,E.StarterName,E.Title,E.RDT,E.FK_Node,E.FK_Flow,E.FID,E.WFState,F.ICON from WF_EmpWorks E,WF_Flow F WHERE E.FK_Flow=F.No AND"
                + " FK_Emp=" + str + "FK_Emp AND E.OrgNo=" + str + "OrgNo union SELECT DISTINCT a.WorkID,'running' as RunningType,a.StarterName,a.Title,a.RDT,a.FK_Node,a.FK_Flow,a.FID,a.WFState,F.ICON "
                + " FROM WF_GenerWorkFlow A,WF_GenerWorkerlist B,WF_Flow F WHERE A.WorkID=B.WorkID AND A.FK_Flow=F.No AND B.FK_Emp=" + str + "EMP AND A.OrgNo=" + str + "OrgNo AND "
                + " B.IsEnable=1 AND (B.IsPass=1 or B.IsPass < -1 ) AND  A.Starter=" + str + "Starter union "
                + "SELECT T.WorkID,'complete' as RunningType,T.StarterName,T.Title,T.RDT,T.FK_Node,T.FK_Flow,T.FID ,T.WFState,F.ICON FROM WF_GenerWorkFlow T,WF_Flow F WHERE T.FK_Flow=F.No AND"
                + " (T.Emps LIKE '%@" + WebUser.No + "@%' OR  T.Emps LIKE '%@" + WebUser.No + ",%') AND T.FID=0 "
                + "AND T.WFState=3  AND T.OrgNo=" + str + "OrgNo)  T  ORDER BY RDT DESC";
            //替换参数
            ps.Add("FK_Emp", WebUser.No);
            ps.Add("EMP", WebUser.No);
            ps.Add("Starter", WebUser.No);
            ps.Add("OrgNo", WebUser.OrgNo);
            //返回json
            DataTable dt = BP.DA.DBAccess.RunSQLReturnTable(ps);
            dt.TableName = "Running";
            ds.Tables.Add(dt);

            //我发起的流程
            string sql = "SELECT G.FlowName AS \"name\", count(G.WorkID) as \"value\" FROM WF_GenerWorkFlow G WHERE G.OrgNo='" + WebUser.OrgNo + "' AND G.Starter='" + WebUser.No + "' GROUP BY G.FlowName";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "MyStartFlow";
            ds.Tables.Add(dt);

            //我参与的流程
            sql = "SELECT G.FlowName AS \"name\", count(G.WorkID) as \"value\" FROM WF_GenerWorkFlow G WHERE G.OrgNo='" + WebUser.OrgNo + "' AND (G.Emps LIKE '%@" + WebUser.No + "@%' OR G.Emps LIKE '%@" + WebUser.No + ",%') GROUP BY G.FlowName";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "MyJoinFlow";
            ds.Tables.Add(dt);

            //我部门发起的流程
            sql = "SELECT G.FlowName AS \"name\", count(G.WorkID) as \"value\" FROM WF_GenerWorkFlow G WHERE G.OrgNo='" + WebUser.OrgNo + "' AND G.FK_Dept='" + WebUser.FK_Dept + "' GROUP BY G.FlowName";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "MyDeptFlow";
            ds.Tables.Add(dt);

            return BP.Tools.Json.ToJson(ds);

        }
    }
}
