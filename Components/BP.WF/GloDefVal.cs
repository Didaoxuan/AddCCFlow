﻿using BP.Difference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP.WF
{
    /// <summary>
    /// 默认值
    /// </summary>
    public class GloDefVal
    {
        public static string SQL
        {
            get
            {
                string sql = "";
                switch(SystemConfig.AppCenterDBType)
                {
                    case DA.DBType.MSSQL:
                        return sql;
                    default:
                        throw new Exception("err@没有判断的类型.");
                }
                return sql;
            }
        }
    }
}
