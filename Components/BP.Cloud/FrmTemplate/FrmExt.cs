using System;
using System.Data;
using System.Collections;
using BP.DA;
using BP.En;
using BP.Sys;

namespace BP.FrmTemplate
{
    /// <summary>
    /// 表单
    /// </summary>
    public class FrmExt : EntityNoName
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
        #endregion

        #region 构造方法
        /// <summary>
        /// 表单
        /// </summary> 
        public FrmExt()
        {
        }
        /// <summary>
        /// 表单
        /// </summary>
        /// <param name="no">表单编号</param>
        public FrmExt(string no)
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

                #region 表单基本信息.
                map.AddTBStringPK(FrmAttr.No, null, "编号", true, false, 0, 50, 4);
                map.AddTBString(FrmAttr.Name, null, "名称", true, false, 0, 100, 100);

                map.AddDDLEntities(FrmAttr.FK_FormTree, null, "类别", new Sort1s(), true);
                map.AddDDLEntities(FrmAttr.FK_SortFunc, null, "功能用途", new SortFuncs(), true);
                map.AddDDLSysEnum(FrmAttr.WorkModel, 0, "工作模式", true, true,
                    FrmAttr.WorkModel, "@0=内部流程@1=外部流程@2=实体台账@3=业务表单,单据", false);

                map.AddBoolean(FrmAttr.IsTuiJian, false, "是否是推荐", true, true);
                map.AddTBString(FrmAttr.Icon, null, "图标", true, false, 0, 200, 4);
                #endregion 表单基本信息.

                map.AddTBStringDoc(FrmAttr.Note, null, "备注", true, false);

                //类别不是空.
                //  map.AddHidden(FrmAttr.FK_FormTree, "!=", "");

                //查询条件.
                map.AddSearchAttr(FrmAttr.FK_FormTree);
                map.AddSearchAttr(FrmAttr.FK_SortFunc);
                map.AddSearchAttr(FrmAttr.WorkModel);

                RefMethod rm = new RefMethod();
                rm.Title = "查看";
                rm.RefMethodType = RefMethodType.LinkeWinOpen;
                rm.ClassMethodName = this.ToString() + ".DoView";
                rm.IsForEns = true;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Title = "安装";
                rm.RefMethodType = RefMethodType.RightFrameOpen;
                rm.ClassMethodName = this.ToString() + ".DoInstall";
                rm.IsForEns = true;
                map.AddRefMethod(rm);

                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion

        /// <summary>
        /// 执行查看
        /// </summary>
        /// <returns></returns>
        public string DoView()
        {
            return "http://template."+BP.Cloud.Glo.SaasHost+"/WF/CCForm/FrmGener.htm?1=2&FrmID="+this.No;
        }
        /// <summary>
        /// 执行安装
        /// </summary>
        /// <returns></returns>
        public string DoInstall()
        {
            return "/App/FlowDesigner/NewFlowByTemplateFromCloud.htm?FrmID="+this.No;
        }
    }
    /// <summary>
    /// 表单s
    /// </summary>
    public class FrmExts : EntitiesNoName
    {
        /// <summary>
        /// 表单
        /// </summary>
        public FrmExts() { }
        /// <summary>
        /// 得到它的 Entity
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new FrmExt();
            }
        }

        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<FrmExt> ToJavaList()
        {
            return (System.Collections.Generic.IList<FrmExt>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<FrmExt> Tolist()
        {
            System.Collections.Generic.List<FrmExt> list = new System.Collections.Generic.List<FrmExt>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((FrmExt)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
