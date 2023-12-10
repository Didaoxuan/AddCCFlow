using System;
using System.Collections;
using BP.DA;
using BP.En;

namespace BP.FrmTemplate
{
    /// <summary>
    /// 表单树类别
    /// </summary>
    public class Sort1Attr : EntityNoNameAttr
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
    ///  表单树类别
    /// </summary>
    public class Sort1 : EntityNoName
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
        /// 表单树类别
        /// </summary>
        public Sort1()
        {
        }
        /// <summary>
        /// 表单树类别
        /// </summary>
        /// <param name="_No"></param>
        public Sort1(string _No) : base(_No) { }
        #endregion

        /// <summary>
        /// 表单树类别Map
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;
                Map map = new Map("Frm_Sort1", "表单树(行业)");
                map.CodeStruct = "2";

                map.AddTBStringPK(Sort1Attr.No, null, "编号", false, false, 1, 40, 42);
                map.AddTBString(Sort1Attr.Name, null, "名称", true, false, 1, 50, 20);
                map.AddTBInt(Sort1Attr.Idx, 0, "顺序", false, false);

                this._enMap = map;
                return this._enMap;
            }
        }
    }
    /// <summary>
    /// 表单树类别
    /// </summary>
    public class Sort1s : EntitiesNoName
    {
        /// <summary>
        /// 表单树类别s
        /// </summary>
        public Sort1s() { }
        /// <summary>
        /// 得到它的 Entity 
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new Sort1();
            }
        }

        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<Sort1> ToJavaList()
        {
            return (System.Collections.Generic.IList<Sort1>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<Sort1> Tolist()
        {
            System.Collections.Generic.List<Sort1> list = new System.Collections.Generic.List<Sort1>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((Sort1)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
