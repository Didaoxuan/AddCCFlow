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
    public class FrmAttr : BP.Sys.MapDataAttr
    {
        /// <summary>
        /// 功能编号
        /// </summary>
        public const string FK_SortFunc = "FK_SortFunc";
        /// <summary>
        /// 工作模式.
        /// </summary>
        public const string WorkModel = "WorkModel";
        /// <summary>
        /// 是否是推荐
        /// </summary>
        public const string IsTuiJian = "IsTuiJian";
        /// <summary>
        /// 为流程时，是否导入指定的流程模版?
        /// </summary>
        public const string IsImpFlowTemplate = "IsImpFlowTemplate";
        /// <summary>
        /// 指定的流程编号
        /// </summary>
        public const string SpecFlowNo = "SpecFlowNo";
        /// <summary>
        /// 为实体表单是，是否导入其他单据模版?
        /// </summary>
        public const string IsImpFrmsTemplate = "IsImpFrmsTemplate";
        /// <summary>
        /// 指定的单据表单编号
        /// </summary>
        public const string RefDictSpecFrmsNo = "RefDictSpecFrmsNo";
        /// <summary>
        /// 关联子流程的编号
        /// </summary>
        public const string RefDictSpecSubFlowsNo = "RefDictSpecSubFlowsNo";
        /// <summary>
        /// ICON
        /// </summary>
        public const string Icon = "Icon";
    }
    /// <summary>
    /// 表单
    /// </summary>
    public class Frm : EntityNoName
    {
        #region 实现基本的方法
        public override UAC HisUAC
        {
            get
            {
                UAC uac = new UAC();
                uac.OpenForSysAdmin();
                uac.IsDelete = false;
                uac.IsInsert = false;
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
                return this.GetValStrByKey(FrmAttr.FK_FormTree);
            }
            set
            {
                this.SetValByKey(FrmAttr.FK_FormTree, value);
            }
        }
        /// <summary>
        /// 工作模式
        /// </summary>
        public int WorkModel
        {
            get
            {
                return this.GetValIntByKey(FrmAttr.WorkModel);
            }
            set
            {
                this.SetValByKey(FrmAttr.WorkModel, value);
            }
        }
        /// <summary>
        /// 指定的流程编号
        /// </summary>
        public string SpecFlowNo
        {
            get
            {
                return this.GetValStrByKey(FrmAttr.SpecFlowNo);
            }
            set
            {
                this.SetValByKey(FrmAttr.SpecFlowNo, value);
            }
        }
        /// <summary>
        /// 表单IDs
        /// </summary>
        public string RefDictSpecFrmsNo
        {
            get
            {
                return this.GetValStrByKey(FrmAttr.RefDictSpecFrmsNo);
            }
            set
            {
                this.SetValByKey(FrmAttr.RefDictSpecFrmsNo, value);
            }
        }
        #endregion

        #region 构造方法
        /// <summary>
        /// 表单
        /// </summary> 
        public Frm()
        {
        }
        /// <summary>
        /// 表单
        /// </summary>
        /// <param name="no">表单编号</param>
        public Frm(string no)
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

                Map map = new Map("Sys_MapData", "表单");

                #region 表单基本信息.
                map.AddTBStringPK(FrmAttr.No, null, "编号", true, false, 0, 50, 4);
                map.AddTBString(FrmAttr.Name, null, "名称", true, false, 0, 100, 100);

                map.AddDDLEntities(FrmAttr.FK_FormTree, null, "表单树类别", new Sort1s(), true);
                map.AddDDLEntities(FrmAttr.FK_SortFunc, null, "功能用途", new SortFuncs(), true);
                map.AddDDLSysEnum(FrmAttr.WorkModel, 0, "工作模式", true, true,
                    FrmAttr.WorkModel, "@0=内部流程@1=外部流程@2=实体台账@3=业务表单,单据", false);

                map.AddBoolean(FrmAttr.IsTuiJian, false, "是否是推荐", true, true);
                map.AddTBString(FrmAttr.Icon, null, "图标", true, false, 0, 200, 4);
                #endregion 表单基本信息.

                #region 流程属性.
                map.AddBoolean(FrmAttr.IsImpFlowTemplate, false, "为流程时，是否导入指定的流程模版?", true, true,true);
                map.AddTBString(FrmAttr.SpecFlowNo, null, "指定的流程编号", true, false, 0, 20, 4, true);
                #endregion 流程属性.

                #region 实体单据属性.
                map.AddBoolean(FrmAttr.IsImpFrmsTemplate, false, "为实体台账时，是否关联导入其他单据流程?", true, true, true);
                map.AddTBString(FrmAttr.RefDictSpecFrmsNo, null, "指定的单据编号", true, false, 0, 200, 4,true);
                map.AddTBString(FrmAttr.RefDictSpecSubFlowsNo, null, "关联子流程编号", true, false, 0, 100, 4, true);
                #endregion 实体单据属性.

                map.AddTBStringDoc(FrmAttr.Note, null, "备注", true, false);

                //类别不是空.
                map.AddHidden(FrmAttr.FK_FormTree, "!=", "");

                //查询条件.
                map.AddSearchAttr(FrmAttr.FK_FormTree);
                map.AddSearchAttr(FrmAttr.FK_SortFunc);
                map.AddSearchAttr(FrmAttr.WorkModel);

                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion
    }
    /// <summary>
    /// 表单s
    /// </summary>
    public class Frms : EntitiesNoName
    {
        /// <summary>
        /// 表单
        /// </summary>
        public Frms() { }
        /// <summary>
        /// 得到它的 Entity
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new Frm();
            }
        }

        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<Frm> ToJavaList()
        {
            return (System.Collections.Generic.IList<Frm>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<Frm> Tolist()
        {
            System.Collections.Generic.List<Frm> list = new System.Collections.Generic.List<Frm>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((Frm)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
