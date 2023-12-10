using System;
using System.Data;
using BP.DA;
using BP.Web;
using BP.WF.Template;
using BP.WF.XML;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class Admin_Portal : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Admin_Portal()
        {
        }

        /// <summary>
        /// 查询可以登录的企业.
        /// </summary>
        /// <returns></returns>
        public string SelectOneInc_Init()
        {
            Depts depts = new Depts();
            depts.Retrieve(DeptAttr.Adminer, WebUser.No);
            return depts.ToJson();
        }

        public string SelectOneInc_SelectIt()
        {
            string no = this.GetRequestVal("No");
            Emp emp = new Emp(WebUser.No);
            emp.FK_Dept = no;
            emp.Update();
            return "url@Admin/Portal/Default.htm?Token=" + BP.Web.WebUser.Token + "&UserNo=" + BP.Web.WebUser.No;
        }
        /// <summary>
        ///  要返回的数据.
        /// </summary>
        /// <returns></returns>
        public string Default_Init()
        {
            return "";
        }
        /// <summary>
        /// 流程树
        /// </summary>
        /// <returns></returns>
        public string Default_FlowsTree()
        {
            //组织数据源.
            string sql = "SELECT * FROM (SELECT 'F'+No as NO,'F'+ParentNo PARENTNO, NAME, IDX, 1 ISPARENT,'FLOWTYPE' TTYPE, -1 DTYPE FROM WF_FlowSort WHERE OrgNo='" + WebUser.FK_Dept +
                          "' union SELECT NO, 'F'+FK_FlowSort as PARENTNO,NAME as NAME,IDX,0 ISPARENT,'FLOW' TTYPE, 0 as DTYPE FROM WF_Flow WHERE OrgNo='" + WebUser.FK_Dept + "') A  ORDER BY DTYPE, IDX ";

            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            //判断是否为空，如果为空，则创建一个流程根结点，added by liuxc,2016-01-24
            if (dt.Rows.Count == 0)
            {
                FlowSort fs = new FlowSort();
                fs.No = "99";
                fs.ParentNo = "0";
                fs.Name = "流程树";
                // fs.Insert();

                dt.Rows.Add("F99", "F0", "流程树", 0, 1, "FLOWTYPE", -1);
            }
            else
            {
                DataRow[] drs = dt.Select("NAME='流程树'");
                if (drs.Length > 0 && !Equals(drs[0]["PARENTNO"], "F0"))
                    drs[0]["PARENTNO"] = "F0";
            }

            String str = BP.Tools.Json.ToJson(dt);
            return str;
        }
        /// <summary>
        /// 查询表单树
        /// </summary>
        /// <returns></returns>
        public string Default_FrmTree()
        {
            //组织数据源.
            string sqls = "";
            /* sqls = "SELECT No,ParentNo,Name, Idx FROM Sys_FormTree WHERE OrgNo=" + WebUser.FK_Dept + " ORDER BY Idx ASC ";
             sqls += "SELECT No, FK_FormTree as ParentNo,Name,Idx,0 IsParent  FROM Sys_MapData    ";*/
            sqls = "SELECT No,ParentNo,Name, Idx, 1 IsParent, 'FORMTYPE' TType FROM Sys_FormTree WHERE OrgNo='" + WebUser.OrgNo + "' ORDER BY Idx ASC ; ";
            sqls += "SELECT No, FK_FormTree as ParentNo,Name,Idx,0 IsParent, 'FORM' TType FROM Sys_MapData  WHERE AppType=0 AND FK_FormTree IN (SELECT No FROM Sys_FormTree WHERE OrgNo='" + WebUser.OrgNo + "') ORDER BY Idx ASC";

            DataSet ds = DBAccess.RunSQLReturnDataSet(sqls);
            DataTable dtSort = ds.Tables[0]; //类别表.
            DataTable dtForm = ds.Tables[1]; //表单表,这个是最终返回的数据.
            //增加顶级目录.
            DataRow[] rowsOfSort = dtSort.Select("ParentNo='0'");
            DataRow drFormRoot = dtForm.NewRow();
            if (rowsOfSort.Length != 0)
            {
                drFormRoot[0] = rowsOfSort[0]["No"];
                drFormRoot[1] = "0";
                drFormRoot[2] = rowsOfSort[0]["Name"];
                drFormRoot[3] = rowsOfSort[0]["Idx"];
                drFormRoot[4] = rowsOfSort[0]["IsParent"];
                drFormRoot[5] = rowsOfSort[0]["TType"];
                dtForm.Rows.Add(drFormRoot); //增加顶级类别..
            }
           

            //把类别数据组装到form数据里.
            foreach (DataRow dr in dtSort.Rows)
            {
                DataRow drForm = dtForm.NewRow();
                drForm[0] = dr["No"];
                drForm[1] = dr["ParentNo"];
                drForm[2] = dr["Name"];
                drForm[3] = dr["Idx"];
                drForm[4] = dr["IsParent"];
                drForm[5] = dr["TType"];
                dtForm.Rows.Add(drForm); //类别.
            }

            foreach (DataRow row in ds.Tables[1].Rows)
            {
                dtForm.Rows.Add(row.ItemArray);
            }

            String str = BP.Tools.Json.ToJson(dtForm);
            return str;
        }
        /// <summary>
        /// 获取设计器 - 系统维护菜单数据
        /// 系统维护管理员菜单 需要翻译
        /// </summary>
        /// <returns></returns>
        public string Default_AdminMenu()
        {
            //查询全部.
            AdminMenus groups = new AdminMenus();
            groups.RetrieveAll();
             
            return BP.Tools.Json.ToJson(groups.ToDataTable());
        }
      
    
    }
}
