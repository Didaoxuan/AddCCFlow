using BP.DA;
using BP.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace BP.Difference
{
    /// <summary>
    /// 公共的方法.
    /// </summary>
    public static class Glo
    {
        /// <summary>
        /// 获得ID地址
        /// </summary>
        public static string GetIP
        {
            get
            {
                return "127.0.0.1";
            }
        }

        public static string RequestParas
        {
            get
            {
                string urlExt = "";
                string rawUrl = "";
                if (HttpContextHelper.Request != null && HttpContextHelper.Request.QueryString.HasValue)
                    rawUrl = HttpContextHelper.Request.QueryString.Value;
                rawUrl = rawUrl.Substring(1); // 去掉开头的问号?
                string[] paras = rawUrl.Split('&');
                foreach (string para in paras)
                {
                    if (para == null
                        || para == ""
                        || para.Contains("=") == false)
                        continue;
                    urlExt += "&" + para;
                }
                return urlExt;
            }
        }
        public static string DealSQLStringEnumFormat(string cfgString)
        {
            //把这个string,转化SQL. @tuanyuan=团员@dangyuan=党员
            AtPara ap = new AtPara(cfgString);
            string sql = "";
            foreach (string item in ap.HisHT.Keys)
            {
                sql += " SELECT '" + item + "' as No, '" + ap.GetValStrByKey(item) + "' as Name FROM Port_Emp WHERE No = 'admin' UNION ";
            }
            sql = sql.Substring(0, sql.Length - 6);
            return sql;
        }
    }
}
