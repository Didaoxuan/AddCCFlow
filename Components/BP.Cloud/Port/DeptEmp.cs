using System;
using System.Collections;
using System.Data;
using BP.DA;
using BP.Web;
using BP.En;
using BP.Difference;

namespace BP.Cloud
{
    /// <summary>
    /// 部门编号人员
    /// </summary>
    public class DeptEmpAttr
    {
        /// <summary>
        /// 操作员
        /// </summary>
        public const string FK_Emp = "FK_Emp";
        /// <summary>
        /// 部门编号
        /// </summary>
        public const string FK_Dept = "FK_Dept";
        /// <summary>
        /// 组织结构编码
        /// </summary>
        public const string OrgNo = "OrgNo";
        /// <summary>
        /// 是否是主要部门
        /// </summary>
        public const string IsMainDept = "IsMainDept";
        /// <summary>
        /// 人员编号()
        /// </summary>
        public const string EmpNo = "EmpNo";
    }
    /// <summary>
    /// 部门编号人员
    /// </summary>
    public class DeptEmp : EntityMyPK
    {
        #region 属性
        /// <summary>
        /// 是否是主部门
        /// </summary>
        public bool IsMainDept
        {
            get
            {
                return this.GetValBooleanByKey(DeptEmpAttr.IsMainDept);
            }
            set
            {
                this.SetValByKey(DeptEmpAttr.IsMainDept, value);
            }
        }
        public string FK_Emp
        {
            get
            {
                return this.GetValStringByKey(DeptEmpAttr.FK_Emp);
            }
            set
            {
                this.SetValByKey(DeptEmpAttr.FK_Emp, value);
            }
        }
        /// <summary>
        /// 人员编号
        /// </summary>
        public string EmpNo
        {
            get
            {
                return this.GetValStringByKey(DeptEmpAttr.EmpNo);
            }
            set
            {
                this.SetValByKey(DeptEmpAttr.EmpNo, value);
            }
        }
        public string FK_Dept
        {
            get
            {
                return this.GetValStringByKey(DeptEmpAttr.FK_Dept);
            }
            set
            {
                this.SetValByKey(DeptEmpAttr.FK_Dept, value);
            }
        }
        /// <summary>
        /// 组织结构编码
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

        #endregion

        #region 构造方法
        /// <summary>
        /// 部门编号人员
        /// </summary>
        public DeptEmp()
        {
        }

        /// <summary>
        /// 部门编号人员
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;
                Map map = new Map("Port_DeptEmp", "部门人员");

                map.AddMyPK();
                map.AddTBString(DeptEmpAttr.FK_Dept, null, "编号", true, false, 0, 50, 20);
                map.AddTBString(DeptEmpAttr.FK_Emp, null, "人员", true, false, 0, 50, 20);
                map.AddTBString(EmpAttr.OrgNo, null, "OrgNo", false, false, 0, 50, 36);

             

                // SAAS 模式下用到.
                if (SystemConfig.CCBPMRunModel == BP.Sys.CCBPMRunModel.SAAS)
                {
                    map.AddTBString(DeptEmpAttr.EmpNo, null, "EmpNo", false, false, 0, 50, 36);
                    map.AddTBInt(DeptEmpAttr.IsMainDept, 1, "是否是主部门", false, false);
                }

                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion


        protected override bool beforeDelete()
        {

            if (DataType.IsNullOrEmpty(this.EmpNo) == false)
            {
                BP.Cloud.Emp emp = new Emp();
                emp.No = this.EmpNo;
                if (emp.RetrieveFromDBSources() == 1)
                {
                    this.FK_Emp = emp.UserID;
                    this.OrgNo = emp.OrgNo;
                }
            }

            this.setMyPK(this.FK_Dept + "_" + this.FK_Emp);

            //删除角色信息.
            DeptEmpStations des = new DeptEmpStations();
            des.Delete(DeptEmpStationAttr.FK_Emp, this.FK_Emp, DeptEmpStationAttr.FK_Dept, this.FK_Dept);

            return base.beforeDelete();
        }

        protected override bool beforeInsert()
        {
            //if (BP.Difference.SystemConfig.CCBPMRunModel!= BP.Sys.CCBPMRunModel.Single && this.EmpNo.Contains(BP.Web.WebUser.OrgNo + "_") == true)
            //{
            //    BP.Cloud.Emp emp = new BP.Cloud.Emp(this.EmpNo);
            //    this.setMyPK(this.FK_Dept + "_" + emp.UserID);
            //    this.FK_Emp = emp.UserID;
            //    this.IsMainDept = false;
            //}
            //if (DataType.IsNullOrEmpty(this.EmpNo) == true)
            //{
            //    this.EmpNo = BP.Web.WebUser.OrgNo + "_" + this.FK_Emp;
            //}

            ////组织编号.
            //this.OrgNo = BP.Web.WebUser.OrgNo;

            //当前人员所在的部门.
            //this.OrgNo = BP.Web.WebUser.FK_Dept;
            return base.beforeInsert();
        }
    }
    /// <summary>
    /// 部门编号人员s
    /// </summary>
    public class DeptEmps : EntitiesMM
    {
        #region 构造
        /// <summary>
        /// 部门编号s
        /// </summary>
        public DeptEmps()
        {
        }
        /// <summary>
        /// 得到它的 Entity
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new DeptEmp();
            }
        }
        #endregion

        public override int RetrieveAll()
        {
            return this.Retrieve(EmpAttr.OrgNo, BP.Web.WebUser.OrgNo);
        }


        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<DeptEmp> ToJavaList()
        {
            return (System.Collections.Generic.IList<DeptEmp>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<DeptEmp> Tolist()
        {
            System.Collections.Generic.List<DeptEmp> list = new System.Collections.Generic.List<DeptEmp>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((DeptEmp)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }
}
