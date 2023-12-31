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
    public class Admin_Org : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Admin_Org()
        {
        }

        /// <summary>
        /// 保存微信名字的方法.
        /// </summary>
        /// <returns></returns>
        public string Organization_SaveEmpsByWX()
        {
            foreach (string key in HttpContextHelper.Request.Form.Keys)
            {
                if (DataType.IsNullOrEmpty(key) == true)
                    continue;
                if (key.Contains("TB_") == false)
                    continue;

                string empNo = key.Replace("TB_", "");

                //获得名字.
                string name = this.GetRequestVal(key);

                BP.Port.Emp emp = new BP.Port.Emp(empNo);
                if (emp.Name.Equals(name) == true)
                    continue;

                emp.Name = name;
                emp.Update();
            }
            return "保存成功.";
        }
        /// <summary>
        /// 获得该部门下的所有人员。
        /// </summary>
        /// <returns></returns>
        public string Organization_GenerEmpsByDeptNo()
        {
            try
            {
                string sql = "SELECT a.No,a.Name,a.UserID, a.Email,a.PinYin,b.IsMainDept,Tel,1 as UseSta FROM Port_Emp A, Port_DeptEmp B ";
                sql += " WHERE A.UserID=B.FK_Emp AND  B.FK_Dept='" + this.FK_Dept + "'  ";
                sql += "  AND A.OrgNo='" + BP.Web.WebUser.OrgNo + "'";
                sql += "  ORDER BY A.Idx ";
                DataTable dt = DBAccess.RunSQLReturnTable(sql);

                //加上禁用的用户WF_Emp
                sql = " Select No,Name,UserID,Email,PinYin,1 as IsMainDept,Tel,UseSta FROM WF_Emp Where FK_Dept='" + this.FK_Dept + "' AND OrgNo='" + BP.Web.WebUser.OrgNo + "' AND UseSta=0";
                sql += "  ORDER BY Idx ";
                DataTable wfdt = DBAccess.RunSQLReturnTable(sql);


                //查询出来主部门的数据.
                Emps emps = new Emps();
                emps.Retrieve(EmpAttr.FK_Dept, this.FK_Dept);

                //遍历人员.
                bool isIsert = false;
                foreach (Emp myemp in emps)
                {
                    if (DataType.IsNullOrEmpty(myemp.UserID) == true)
                    {
                        myemp.Delete();
                        continue;
                    }

                    bool isHave = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string empNo = dr[0].ToString();
                        if (myemp.No.Equals(empNo) == true)
                        {
                            isHave = true;
                            break;
                        }
                       
                    }
                    if (isHave == true)
                        continue;

                    DeptEmp de = new DeptEmp();
                    de.setMyPK(myemp.OrgNo + "_" + myemp.UserID);
                    de.FK_Dept = myemp.FK_Dept;
                    de.FK_Emp = myemp.UserID;
                    de.IsMainDept = true;
                    de.OrgNo = WebUser.OrgNo;
                    de.Save();
                    isIsert = true;
                }

                if (isIsert == true)
                    dt = DBAccess.RunSQLReturnTable(sql);
                DataRow mydr;
               foreach(DataRow wfdr in wfdt.Rows)
                {
                    mydr = dt.NewRow();
                    mydr.ItemArray = wfdr.ItemArray;
                    dt.Rows.Add(mydr);
                }

                return BP.Tools.Json.ToJson(dt);
            }catch(Exception e)
            {
                BP.WF.Port.WFEmp wfEmp = new BP.WF.Port.WFEmp();
                wfEmp.CheckPhysicsTable();
                throw new Exception(e.Message);
            }
           
        }

        #region 签名.
        /// <summary>
        /// 图片签名初始化
        /// </summary>
        /// <returns></returns>
        public string Siganture_Init()
        {
            if (BP.Web.WebUser.NoOfRel == null)
                return "err@登录信息丢失";
            Hashtable ht = new Hashtable();
            ht.Add("No", BP.Web.WebUser.No);
            ht.Add("Name", BP.Web.WebUser.Name);
            ht.Add("FK_Dept", BP.Web.WebUser.FK_Dept);
            ht.Add("FK_DeptName", BP.Web.WebUser.FK_DeptName);
            return BP.Tools.Json.ToJson(ht);
        }
        /// <summary>
        /// 签名保存
        /// </summary>
        /// <returns></returns>
        public string Siganture_Save()
        {
            var f = HttpContextHelper.RequestFiles(0);
            string empNo = this.GetRequestVal("EmpNo");
            if (DataType.IsNullOrEmpty(empNo) == true)
                empNo = WebUser.No;

            //判断文件类型.
            string fileExt = ",bpm,jpg,jpeg,png,gif,";
            string ext = f.FileName.Substring(f.FileName.LastIndexOf('.') + 1).ToLower();
            if (fileExt.IndexOf(ext + ",") == -1)
            {
                return "err@上传的文件必须是以图片格式:" + fileExt + "类型, 现在类型是:" + ext;
            }

            try
            {
                string tempFile = BP.Difference.SystemConfig.PathOfWebApp + "DataUser/Siganture/" + WebUser.OrgNo + "/";
                if (System.IO.Directory.Exists(tempFile) == false)
                    System.IO.Directory.CreateDirectory(tempFile);
                tempFile = tempFile + empNo.Replace(WebUser.OrgNo+"_","") + ".jpg";
                if (System.IO.File.Exists(tempFile) == true)
                    System.IO.File.Delete(tempFile);

                //f.SaveAs(tempFile);
                HttpContextHelper.UploadFile(f, tempFile);

                System.Drawing.Image img = System.Drawing.Image.FromFile(tempFile);
                img.Dispose();
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }

            //f.SaveAs(BP.Difference.SystemConfig.PathOfWebApp + "/DataUser/Siganture/" + this.FK_Emp + ".jpg");
            HttpContextHelper.UploadFile(f, BP.Difference.SystemConfig.PathOfWebApp + "DataUser/Siganture/" + this.FK_Emp + ".jpg");

            // f.SaveAs(BP.Difference.SystemConfig.PathOfWebApp + "/DataUser/Siganture/" + WebUser.Name + ".jpg");

            //f.PostedFile.InputStream.Close();
            //f.PostedFile.InputStream.Dispose();
            //f.Dispose();

            //   this.Response.Redirect(this.Request.RawUrl, true);
            return "上传成功！";
        }
        #endregion


        #region 组织结构维护.
        /// <summary>
        /// 初始化组织结构部门表维护.
        /// </summary>
        /// <returns></returns>
        public string Organization_Init()
        {
            BP.Cloud.Depts depts = new BP.Cloud.Depts();
            depts.Retrieve("OrgNo", WebUser.OrgNo);

            //depts.AddEntity(new Dept(WebUser.FK_Dept));
            return depts.ToJson();
        }

        /// <summary>
        /// 获取该部门的所有人员
        /// </summary>
        /// <returns></returns>
        /// 
        public string LoadDatagridDeptEmp_Init()
        {
            string deptNo = this.GetRequestVal("deptNo");
            if (string.IsNullOrEmpty(deptNo))
            {
                return "{ total: 0, rows: [] }";
            }
            string orderBy = this.GetRequestVal("orderBy");


            string searchText = this.GetRequestVal("searchText");
            if (!DataType.IsNullOrEmpty(searchText))
            {
                searchText.Trim();
            }
            string addQue = "";
            if (!string.IsNullOrEmpty(searchText))
            {
                addQue = "  AND (pe.No like '%" + searchText + "%' or pe.Name like '%" + searchText + "%') ";
            }

            string pageNumber = this.GetRequestVal("pageNumber");
            int iPageNumber = string.IsNullOrEmpty(pageNumber) ? 1 : Convert.ToInt32(pageNumber);
            //每页多少行
            string pageSize = this.GetRequestVal("pageSize");
            int iPageSize = string.IsNullOrEmpty(pageSize) ? 9999 : Convert.ToInt32(pageSize);

            string sql = "(select pe.*,pd.name FK_DutyText from port_emp pe left join port_duty pd on pd.no=pe.fk_duty where pe.no in (select fk_emp from Port_DeptEmp where fk_dept='" + deptNo + "') "
                + addQue + " ) dbSo ";

            return DBPaging(sql, iPageNumber, iPageSize, "No", orderBy);
        }
        /// <summary>
        /// 以下算法只包含 oracle mysql sqlserver 三种类型的数据库 qin
        /// </summary>
        /// <param name="dataSource">表名</param>
        /// <param name="pageNumber">当前页</param>
        /// <param name="pageSize">当前页数据条数</param>
        /// <param name="key">计算总行数需要</param>
        /// <param name="orderKey">排序字段</param>
        /// <returns></returns>
        public string DBPaging(string dataSource, int pageNumber, int pageSize, string key, string orderKey)
        {
            string sql = "";
            string orderByStr = "";

            if (!string.IsNullOrEmpty(orderKey))
                orderByStr = " ORDER BY " + orderKey;

            switch (DBAccess.AppCenterDBType)
            {
                case DBType.Oracle:
                    int beginIndex = (pageNumber - 1) * pageSize + 1;
                    int endIndex = pageNumber * pageSize;

                    sql = "SELECT * FROM ( SELECT A.*, ROWNUM RN " +
                        "FROM (SELECT * FROM  " + dataSource + orderByStr + ") A WHERE ROWNUM <= " + endIndex + " ) WHERE RN >=" + beginIndex;
                    break;
                case DBType.MSSQL:
                    sql = "SELECT TOP " + pageSize + " * FROM " + dataSource + " WHERE " + key + " NOT IN  ("
                    + "SELECT TOP (" + pageSize + "*(" + pageNumber + "-1)) " + key + " FROM " + dataSource + " )" + orderByStr;
                    break;
                case DBType.MySQL:
                    pageNumber -= 1;
                    sql = "select * from  " + dataSource + orderByStr + " limit " + pageNumber + "," + pageSize;
                    break;
                default:
                    throw new Exception("暂不支持您的数据库类型.");
            }

            DataTable DTable = DBAccess.RunSQLReturnTable(sql);

            int totalCount = DBAccess.RunSQLReturnCOUNT("select " + key + " from " + dataSource);

            return DataTableConvertJson.DataTable2Json(DTable, totalCount);
        }
        #endregion


        public string StationToDeptEmp_Init()
        {
            return "";
        }

        /// <summary>
        /// 处理系统编辑菜单.
        /// </summary>
        /// <returns></returns>
        public string AppMenu_Init()
        {
            //BP.GPM.App app = new BP.GPM.App();
            //app.No = "CCFlowBPM";
            //if (app.RetrieveFromDBSources() == 0)
            //{
            //    BP.GPM.App.InitBPMMenu();
            //    app.Retrieve();
            //}
            return "";
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
