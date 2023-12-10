using System;
using System.Collections;
using BP.DA;
using BP.En;

namespace BP.FrmTemplate
{
    /// <summary>
    /// 表单类别2
    /// </summary>
    public class SortFuncAttr : EntityNoNameAttr
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
    ///  表单类别2
    /// </summary>
    public class SortFunc : EntityNoName
    {
        #region 属性
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
        /// 表单类别2
        /// </summary>
        public SortFunc()
        {
        }
        /// <summary>
        /// 表单类别2
        /// </summary>
        /// <param name="_No"></param>
        public SortFunc(string _No) : base(_No) { }
        #endregion

        /// <summary>
        /// 表单类别2Map
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;

                Map map = new Map("Frm_SortFunc", "表单类别2");
                map.CodeStruct = "2";

                map.AddTBStringPK(SortFuncAttr.No, null, "编号", false, false, 1, 40, 42);
                map.AddTBString(SortFuncAttr.Name, null, "名称", true, false, 1, 50, 20);
                map.AddTBInt(SortFuncAttr.Idx, 0, "顺序", false, false);

                this._enMap = map;
                return this._enMap;
            }
        }


    }
    /// <summary>
    /// 表单类别2
    /// </summary>
    public class SortFuncs : EntitiesNoName
    {
        /// <summary>
        /// 表单类别2s
        /// </summary>
        public SortFuncs() { }
        /// <summary>
        /// 得到它的 Entity 
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new SortFunc();
            }
        }


        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<SortFunc> ToJavaList()
        {
            return (System.Collections.Generic.IList<SortFunc>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<SortFunc> Tolist()
        {
            System.Collections.Generic.List<SortFunc> list = new System.Collections.Generic.List<SortFunc>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((SortFunc)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
