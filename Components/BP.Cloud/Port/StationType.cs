﻿using System;
using System.Collections;
using BP.DA;
using BP.En;

namespace BP.Cloud
{
    /// <summary>
    /// 角色类型
    /// </summary>
    public class StationTypeAttr : EntityNoNameAttr
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public const string Idx = "Idx";
        /// <summary>
        /// 组织机构编号
        /// </summary>
        public const string OrgNo = "OrgNo";
    }
    /// <summary>
    ///  角色类型
    /// </summary>
    public class StationType : EntityNoName
    {
        #region 属性
        public string OrgNo
        {
            get
            {
                return this.GetValStrByKey(StationAttr.OrgNo);
            }
            set
            {
                this.SetValByKey(StationAttr.OrgNo, value);
            }
        }
        #endregion

        #region 实现基本的方方法
        public override UAC HisUAC
        {
            get
            {
                UAC uac = new UAC();
                uac.OpenForSysAdmin();
                return uac;
            }
        }
        #endregion

        #region 构造方法
        /// <summary>
        /// 角色类型
        /// </summary>
        public StationType()
        {
        }
        /// <summary>
        /// 角色类型
        /// </summary>
        /// <param name="_No"></param>
        public StationType(string _No) : base(_No) { }
        #endregion

        /// <summary>
        /// 角色类型Map
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;
                Map map = new Map("Port_StationType", "角色类型");
                map.CodeStruct="2";

                map.AddTBStringPK(StationTypeAttr.No, null, "编号", false, false, 1, 40, 40);
                map.AddTBString(StationTypeAttr.Name, null, "名称", true, false, 1, 50, 40);

                map.AddTBInt(StationTypeAttr.Idx, 0, "顺序", false, false);
                map.AddTBString(StationTypeAttr.OrgNo, null, "组织机构编号", false, false, 0, 50, 40);

                //增加隐藏查询条件.
                map.AddHidden(StationTypeAttr.OrgNo, "=", BP.Web.WebUser.OrgNo);


                this._enMap = map;
                return this._enMap;
            }
        }

        protected override bool beforeInsert()
        {
            this.OrgNo = BP.Web.WebUser.OrgNo;
            return base.beforeInsert();
        }

        protected override bool beforeDelete()
        {
            Stations ens = new Stations();
            ens.Retrieve(StationAttr.FK_StationType, this.No);
            if (ens.Count > 0)
                throw new Exception("err@删除角色类型错误，该类型下有角色。");

            return base.beforeDelete();
        }

    }
    /// <summary>
    /// 角色类型
    /// </summary>
    public class StationTypes : EntitiesNoName
    {
        /// <summary>
        /// 角色类型s
        /// </summary>
        public StationTypes() { }
        /// <summary>
        /// 得到它的 Entity 
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new StationType();
            }
        }
        public override int RetrieveAll()
        {
            return this.Retrieve(EmpAttr.OrgNo, BP.Web.WebUser.OrgNo);
        }


        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<StationType> ToJavaList()
        {
            return (System.Collections.Generic.IList<StationType>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<StationType> Tolist()
        {
            System.Collections.Generic.List<StationType> list = new System.Collections.Generic.List<StationType>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((StationType)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
