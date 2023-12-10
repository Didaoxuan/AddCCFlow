using System;
using System.Data;
using System.Collections;
using BP.DA;
using BP.En;
using BP.Cloud;
using BP.Sys;

namespace BP.Cloud
{
    /// <summary>
    /// 分库属性
    /// </summary>
    public class DBAttr : BP.En.EntityNoNameAttr
    {
        /// <summary>
        /// 数据库链接串
        /// </summary>
        public const string DNS = "DNS";
    }
    /// <summary>
    ///  分库
    /// </summary>
    public class DB : EntityNoName
    {
        #region 属性.
        #endregion 属性.

        #region 构造方法
        /// <summary>
        /// 分库
        /// </summary>
        public DB()
        {
        }
        /// <summary>
        /// 分库
        /// </summary>
        /// <param name="_No"></param>
        public DB(string _No) : base(_No) { }
        public override UAC HisUAC
        {
            get
            {
                UAC uac = new UAC();
                if (BP.Web.WebUser.No.Equals("admin") == true)
                    uac.OpenForSysAdmin();

                return uac;
            }
        }
        #endregion

        /// <summary>
        /// 分库Map
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;

                Map map = new Map("Port_DB", "分库");
                map.CodeStruct = "3";

                map.AddTBStringPK(DBAttr.No, null, "编号", true, true, 3, 3, 3);
                map.AddTBString(DBAttr.Name, null, "名称", true, false, 0, 200, 30, true);
                map.AddTBString(DBAttr.DNS, null, "DNS", true, false, 0, 200, 500, true);
                this._enMap = map;
                return this._enMap;
            }
        }
    }
    /// <summary>
    /// 分库
    /// </summary>
    public class DBs : EntitiesTree
    {
        /// <summary>
        /// 分库s
        /// </summary>
        public DBs() { }
        /// <summary>
        /// 得到它的 Entity 
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new DB();
            }
        }
    }
}
