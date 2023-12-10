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
    /// 市场属性
    /// </summary>
    public class MarketAttr : BP.En.EntityNoNameAttr
    {
        /// <summary>
        /// Email
        /// </summary>
        public const string Email = "Email";
        public const string Tel = "Tel";
        public const string Linker = "Linker";
        public const string ADUrl = "ADUrl";
        public const string GUID = "GUID";
    }
    /// <summary>
    ///  市场
    /// </summary>
    public class Market : EntityNoName
    {
        #region 属性.
        public string Linker
        {
            get
            {
                return this.GetValStringByKey(MarketAttr.Linker);
            }
            set
            {
                this.SetValByKey(MarketAttr.Linker, value);
            }
        }
        /// <summary>
        /// GUID
        /// </summary>
        public string GUID
        { 
            get
            {
                return this.GetValStringByKey(MarketAttr.GUID);
            }
            set
            {
                this.SetValByKey(MarketAttr.GUID, value);
            }
        }
        public string Email
        {
            get
            {
                return this.GetValStringByKey(MarketAttr.Email);
            }
            set
            {
                this.SetValByKey(MarketAttr.Email, value);
            }
        }
        #endregion 属性.

        #region 构造方法
        /// <summary>
        /// 市场
        /// </summary>
        public Market()
        {
        }
        /// <summary>
        /// 市场
        /// </summary>
        /// <param name="_No"></param>
        public Market(string _No) : base(_No) { }
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
        /// 市场Map
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;

                Map map = new Map("Port_Market", "市场");

                map.AddTBStringPK(MarketAttr.No, null, "编号", true, false, 0, 30, 30);
                map.AddTBString(MarketAttr.Name, null, "名称", true, false, 0, 200, 200, true);
                map.AddTBString(MarketAttr.Email, null, "Email", true, false, 0, 200, 200, true);
                map.AddTBString(MarketAttr.Tel, null, "Tel", true, false, 0, 200, 200, true);
                map.AddTBString(MarketAttr.Linker, null, "Linker", true, false, 0, 200, 200, true);
                map.AddTBString(MarketAttr.ADUrl, null, "ADUrl", true, false, 0, 200, 200, true);
                map.AddTBString(MarketAttr.GUID, null, "GUID", true, false, 0, 200, 200, true);
                this._enMap = map;
                return this._enMap;
            }
        }
    }
    /// <summary>
    /// 市场
    /// </summary>
    public class Markets : EntitiesTree
    {
        /// <summary>
        /// 市场s
        /// </summary>
        public Markets() { }
        /// <summary>
        /// 得到它的 Entity 
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new Market();
            }
        }
    }
}
