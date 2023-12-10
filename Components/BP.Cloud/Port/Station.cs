using System;
using System.Data;
using System.Collections;
using BP.DA;
using BP.En;
using BP.Sys;

namespace BP.Cloud
{
    /// <summary>
    /// 角色属性
    /// </summary>
    public class StationAttr : EntityNoNameAttr
    {
        /// <summary>
        /// 角色类型
        /// </summary>
        public const string FK_StationType = "FK_StationType";
        /// <summary>
        /// OrgNo
        /// </summary>
        public const string OrgNo = "OrgNo";
    }
    /// <summary>
    /// 角色
    /// </summary>
    public class Station : EntityNoName
    {
        #region 实现基本的方法
        public override UAC HisUAC
        {
            get
            {
                UAC uac = new UAC();
                uac.OpenForSysAdmin();
                return uac;
            }
        }
        /// <summary>
        /// Idx.
        /// </summary>
        public int Idx
        {
            get
            {
                return this.GetValIntByKey(EmpAttr.Idx);
            }
            set
            {
                this.SetValByKey(EmpAttr.Idx, value);
            }
        }
        /// <summary>
        /// 组织编号.
        /// </summary>
        public string OrgNo
        {
            get
            {
                return this.GetValStrByKey(EmpAttr.OrgNo);
            }
            set
            {
                this.SetValByKey(EmpAttr.OrgNo, value);
            }
        }
        /// <summary>
        /// 角色类型
        /// </summary>
        public string FK_StationType
        {
            get
            {
                return this.GetValStrByKey(StationAttr.FK_StationType);
            }
            set
            {
                this.SetValByKey(StationAttr.FK_StationType, value);
            }
        }
        #endregion

        #region 构造方法
        /// <summary>
        /// 角色
        /// </summary> 
        public Station()
        {
        }
        /// <summary>
        /// 角色
        /// </summary>
        /// <param name="no">角色编号</param>
        public Station(string no)
        {
            this.No = no.Trim();
            if (this.No.Length == 0)
                throw new Exception("@要查询的角色编号为空。");

            this.Retrieve();
        }
        /// <summary>
        /// EnMap
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;

                Map map = new Map("Port_Station", "角色");


                map.AddTBStringPK(EmpAttr.No, null, "编号", true, false, 1, 40, 4);
                map.AddTBString(EmpAttr.Name, null, "名称", true, false, 0, 100, 100);

                map.AddTBString(StationAttr.FK_StationType, null, "类型", true, false,
                    0, 100, 100);
                map.AddTBString(StationAttr.OrgNo, null, "隶属组织编号", true, true, 0, 100, 100);

                map.AddTBInt(EmpAttr.Idx, 0, "Idx", true, false);

                //增加隐藏查询条件.
                map.AddHidden(StationTypeAttr.OrgNo, "=", BP.Web.WebUser.OrgNo);

                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion

        protected override bool beforeDelete()
        {
            DeptEmpStations ensD = new DeptEmpStations();
            ensD.Retrieve(DeptEmpStationAttr.FK_Station, this.No);
            if (ensD.Count > 0)
                throw new Exception("err@删除角色错误，该角色下有人员。");

            return base.beforeDelete();
        }

    }
    /// <summary>
    /// 角色s
    /// </summary>
    public class Stations : EntitiesNoName
    {
        /// <summary>
        /// 角色
        /// </summary>
        public Stations() { }
        /// <summary>
        /// 得到它的 Entity
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new Station();
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
        public System.Collections.Generic.IList<Station> ToJavaList()
        {
            return (System.Collections.Generic.IList<Station>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<Station> Tolist()
        {
            System.Collections.Generic.List<Station> list = new System.Collections.Generic.List<Station>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((Station)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
