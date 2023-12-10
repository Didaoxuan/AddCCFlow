using System;
using System.Data;
using System.Collections;
using BP.DA;
using BP.En;
using BP.Sys;

namespace BP.FrmTemplate
{
    /// <summary>
    /// 表单属性
    /// </summary>
    public class FrmCloudAttr : BP.Sys.MapDataAttr
    {
        /// <summary>
        /// 表单类型
        /// </summary>
        public const string FK_FormTree = "FK_FormTree";
        /// <summary>
        /// OrgNo
        /// </summary>
        public const string FK_SortFunc = "FK_SortFunc";
        /// <summary>
        /// 单据BIll类型.
        /// </summary>
        public const string BillEntityType = "BillEntityType";
        /// <summary>
        /// 是否是推荐
        /// </summary>
        public const string IsTuiJian = "IsTuiJian";
    }
    /// <summary>
    /// 表单
    /// </summary>
    public class FrmCloud : EntityNoName
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
        /// 表单类型
        /// </summary>
        public string FK_FormTree
        {
            get
            {
                return this.GetValStrByKey(FrmCloudAttr.FK_FormTree);
            }
            set
            {
                this.SetValByKey(FrmCloudAttr.FK_FormTree, value);
            }
        }
        #endregion

        #region 构造方法
        /// <summary>
        /// 表单
        /// </summary> 
        public FrmCloud()
        {
        }
        /// <summary>
        /// 表单
        /// </summary>
        /// <param name="no">表单编号</param>
        public FrmCloud(string no)
        {
            this.No = no.Trim();
            if (this.No.Length == 0)
                throw new Exception("@要查询的表单编号为空。");

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

                Map map = new Map("Frm_Template", "表单");


                map.AddTBStringPK(FrmCloudAttr.No, null, "编号", true, false, 4, 4, 4);
                map.AddTBString(FrmCloudAttr.Name, null, "名称", true, false, 0, 100, 100);

                map.AddTBString(FrmCloudAttr.FK_FormTree, null, "名称", true, false, 0, 100, 100);
                map.AddTBString(FrmCloudAttr.FK_SortFunc, null, "名称", true, false, 0, 100, 100);
                map.AddTBInt(FrmCloudAttr.BillEntityType, 0, "单据实体?", true, true);
                map.AddBoolean(FrmCloudAttr.IsTuiJian, false, "是否是推荐", true, true);
                map.AddTBStringDoc(FrmCloudAttr.Note, null, "备注", true, false);


                //map.AddTBString(FrmCloudAttr.FK_SortFunc, null, "名称", true, false, 0, 100, 100);
                //map.AddDDLEntities(FrmCloudAttr.FK_FormTree, null, "表单树类别", new Sort1s(), true);
                //map.AddDDLEntities(FrmCloudAttr.FK_SortFunc, null, "功能用途", new SortFuncs(), true);
                //map.AddDDLSysEnum(FrmCloudAttr.BillEntityType, 0, "单据实体?", true, true, FrmCloudAttr.BillEntityType, "@0=单据@1=实体", false);


                //类别不是空.
                map.AddHidden(FrmCloudAttr.FK_FormTree, "!=", "");

                //    map.AddDDLSysEnum()

                map.AddSearchAttr(FrmCloudAttr.FK_FormTree);
                map.AddSearchAttr(FrmCloudAttr.FK_SortFunc);
                map.AddSearchAttr(FrmCloudAttr.BillEntityType);


                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion

    }
    /// <summary>
    /// 表单s
    /// </summary>
    public class FrmClouds : EntitiesNoName
    {
        /// <summary>
        /// 表单
        /// </summary>
        public FrmClouds() { }
        /// <summary>
        /// 得到它的 Entity
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new FrmCloud();
            }
        }

        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<FrmCloud> ToJavaList()
        {
            return (System.Collections.Generic.IList<FrmCloud>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<FrmCloud> Tolist()
        {
            System.Collections.Generic.List<FrmCloud> list = new System.Collections.Generic.List<FrmCloud>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((FrmCloud)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
