using System;
using System.Data;
using System.Collections;
using System.Reflection;
using BP.Port;
using BP.DA;
using BP.En;
using BP.Sys;

namespace BP.Cloud
{
    /// <summary>
    /// 生成考核数据
    /// </summary>
    public class DTSClearTestData : Method
    {
        /// <summary>
        /// 生成考核数据
        /// </summary>
        public DTSClearTestData()
        {
            this.Title = "删除测试数据（admin管理员用）";
            this.Help = "admin管理员用";
            this.GroupName = "系统运维";

        }
        /// <summary>
        /// 设置执行变量
        /// </summary>
        /// <returns></returns>
        public override void Init()
        {
        }
        /// <summary>
        /// 当前的操纵员是否可以执行这个方法
        /// </summary>
        public override bool IsCanDo
        {
            get
            {
                if (BP.Web.WebUser.No.Equals("admin") == true)
                    return true;
                return false;
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns>返回执行结果</returns>
        public override object Do()
        {
            //删除所有的流程.
            BP.WF.Flows fls = new BP.WF.Flows();
            fls.RetrieveAll();
            foreach (BP.WF.Flow item in fls)
            {
                if (DBAccess.IsExitsObject(item.PTable) == true)
                    DBAccess.RunSQL("DROP TABLE " + item.PTable);

                string track = "ND" + int.Parse(item.No) + "Track";
                if (DBAccess.IsExitsObject(track) == true)
                    DBAccess.RunSQL("DROP TABLE " + track);
            }

            //删除从表.
            MapDtls dtls = new MapDtls();
            dtls.RetrieveAll();
            foreach (MapDtl item in dtls)
                item.Delete();

            string path = @"D:\CCFlowCloud\CCFlow\AdminSys\CCFlowCloud删除测试数据.sql";
            BP.DA.DBAccess.RunSQLScript(path);
            return "执行成功.";
        }
    }
}
