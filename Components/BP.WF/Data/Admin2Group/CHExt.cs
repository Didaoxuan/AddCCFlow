﻿using System;
using BP.En;


namespace BP.WF.Data.Admin2Group
{
    /// <summary>
    /// 时效考核
    /// </summary> 
    public class CHExt : EntityMyPK
    {
        #region 基本属性
        /// <summary>
        /// 考核状态
        /// </summary>
        public CHSta CHSta
        {
            get
            {
                return (CHSta)this.GetValIntByKey(CHAttr.CHSta);
            }
            set
            {
                this.SetValByKey(CHAttr.CHSta, (int)value);
            }
        }
        /// <summary>
        /// 时间到
        /// </summary>
        public string DTTo
        {
            get
            {
                return this.GetValStringByKey(CHAttr.DTTo);
            }
            set
            {
                this.SetValByKey(CHAttr.DTTo, value);
            }
        }
        /// <summary>
        /// 时间从
        /// </summary>
        public string DTFrom
        {
            get
            {
                return this.GetValStringByKey(CHAttr.DTFrom);
            }
            set
            {
                this.SetValByKey(CHAttr.DTFrom, value);
            }
        }
        /// <summary>
        /// 应完成日期
        /// </summary>
        public string SDT
        {
            get
            {
                return this.GetValStringByKey(CHAttr.SDT);
            }
            set
            {
                this.SetValByKey(CHAttr.SDT, value);
            }
        }
        /// <summary>
        /// 流程标题
        /// </summary>
        public string Title
        {
            get
            {
                return this.GetValStringByKey(CHAttr.Title);
            }
            set
            {
                this.SetValByKey(CHAttr.Title, value);
            }
        }
        /// <summary>
        /// 流程编号
        /// </summary>
        public string FK_Flow
        {
            get
            {
                return this.GetValStringByKey(CHAttr.FK_Flow);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_Flow, value);
            }
        }
        /// <summary>
        /// 流程
        /// </summary>
        public string FK_FlowT
        {
            get
            {
                return this.GetValStringByKey(CHAttr.FK_FlowT);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_FlowT, value);
            }
        }
        /// <summary>
        /// 限期
        /// </summary>
        public int TimeLimit
        {
            get
            {
                return this.GetValIntByKey(CHAttr.TimeLimit);
            }
            set
            {
                this.SetValByKey(CHAttr.TimeLimit, value);
            }
        }
        /// <summary>
        /// 操作人员
        /// </summary>
        public string FK_Emp
        {
            get
            {
                return this.GetValStringByKey(CHAttr.FK_Emp);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_Emp, value);
            }
        }
        /// <summary>
        /// 人员
        /// </summary>
        public string FK_EmpT
        {
            get
            {
                return this.GetValStringByKey(CHAttr.FK_EmpT);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_EmpT, value);
            }
        }
        /// <summary>
        /// 部门
        /// </summary>
        public string FK_Dept
        {
            get
            {
                return this.GetValStrByKey(CHAttr.FK_Dept);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_Dept, value);
            }
        }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string FK_DeptT
        {
            get
            {
                return this.GetValStrByKey(CHAttr.FK_DeptT);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_DeptT, value);
            }
        }
        /// <summary>
        /// 年月
        /// </summary>
        public string FK_NY
        {
            get
            {
                return this.GetValStrByKey(CHAttr.FK_NY);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_NY, value);
            }
        }
        /// <summary>
        /// 工作ID
        /// </summary>
        public Int64 WorkID
        {
            get
            {
                return this.GetValInt64ByKey(CHAttr.WorkID);
            }
            set
            {
                this.SetValByKey(CHAttr.WorkID, value);
            }
        }
        /// <summary>
        /// 流程ID
        /// </summary>
        public Int64 FID
        {
            get
            {
                return this.GetValInt64ByKey(CHAttr.FID);
            }
            set
            {
                this.SetValByKey(CHAttr.FID, value);
            }
        }
        /// <summary>
        /// 节点ID
        /// </summary>
        public int FK_Node
        {
            get
            {
                return this.GetValIntByKey(CHAttr.FK_Node);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_Node, value);
            }
        }
        /// <summary>
        /// 节点名称
        /// </summary>
        public string FK_NodeT
        {
            get
            {
                return this.GetValStrByKey(CHAttr.FK_NodeT);
            }
            set
            {
                this.SetValByKey(CHAttr.FK_NodeT, value);
            }
        }
        #endregion

        #region 构造方法
        /// <summary>
        /// UI界面上的访问控制
        /// </summary>
        public override UAC HisUAC
        {
            get
            {
                UAC uac = new UAC();
                uac.IsDelete = false;
                uac.IsInsert = false;
                uac.IsUpdate = false;
                uac.IsView = true;
                return uac;
            }
        }
        /// <summary>
        /// 时效考核
        /// </summary>
        public CHExt() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pk"></param>
        public CHExt(string pk)
            : base(pk)
        {
        }
        #endregion

        #region Map
        /// <summary>
        /// EnMap
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;

                Map map = new Map("WF_CH", "时效考核");
                map.AddMyPK();

                map.AddTBString(CHAttr.FK_DeptT, null, "部门", true, true, 0, 900, 5, true);
                map.AddDDLEntities(CHAttr.FK_Emp, null, "当事人", new BP.Port.Emps(), false);

                map.AddDDLEntities(CHAttr.FK_Flow, null, "流程", new FlowSimples(), false);
                map.AddTBString(CHAttr.Title, null, "标题", true, true, 0, 900, 5, true);
                map.AddTBString(CHAttr.FK_NodeT, null, "节点名称", true, true, 0, 50, 5);

                map.AddTBString(CHAttr.DTFrom, null, "时间从", true, true, 0, 50, 5);
                map.AddTBString(CHAttr.DTTo, null, "到", true, true, 0, 50, 5);
                map.AddTBString(CHAttr.SDT, null, "应完成日期", true, true, 0, 50, 5);

                map.AddTBString(CHAttr.TimeLimit, null, "限期", true, true, 0, 50, 5);

                map.AddTBFloat(CHAttr.UseDays, 0, "实际使用天", false, true);
                map.AddTBFloat(CHAttr.OverDays, 0, "逾期天", false, true);

                map.AddDDLSysEnum(CHAttr.CHSta, 0, "状态", true, true, CHAttr.CHSta,
                    "@0=及时完成@1=按期完成@2=逾期完成@3=超期完成");

                map.AddTBString(CHAttr.FK_NY, null, "月份", true, true, 0, 50, 5);
                map.AddTBInt(CHAttr.WorkID, 0, "工作ID", true, true);
                map.AddTBString(CHAttr.OrgNo, null, "OrgNo", false, false, 0, 50, 5);

                //查询条件.
                map.AddSearchAttr(CHAttr.CHSta);
                map.AddSearchAttr(CHAttr.FK_Flow);
                map.AddHidden("OrgNo", " = ", "@WebUser.OrgNo"); //查询本组织的.

                //方法.
                RefMethod rm = new RefMethod();
                rm.Title = "详情";
                rm.ClassMethodName = this.ToString() + ".DoOpen";
                rm.RefMethodType = RefMethodType.LinkeWinOpen;
                rm.IsForEns = false;
                map.AddRefMethod(rm);


                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion

        public string DoOpen()
        {
            return "../../MyView.htm?FK_Flow" + this.FK_Flow + "&WorkID=" + this.WorkID + "&OID=" + this.WorkID;
        }
    }
    /// <summary>
    /// 时效考核s
    /// </summary>
    public class CHExts : Entities
    {
        #region 构造方法属性
        /// <summary>
        /// 时效考核s
        /// </summary>
        public CHExts() { }
        #endregion

        #region 属性
        /// <summary>
        /// 时效考核
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new CHExt();
            }
        }
        #endregion

        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<CHExt> ToJavaList()
        {
            return (System.Collections.Generic.IList<CHExt>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<CHExt> Tolist()
        {
            System.Collections.Generic.List<CHExt> list = new System.Collections.Generic.List<CHExt>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((CHExt)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
