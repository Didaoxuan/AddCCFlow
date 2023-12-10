using System;
using System.Data;
using BP.DA;
using BP.WF;
using BP.Port;
using BP.Sys;
using BP.En;

namespace BP.Cloud
{
    /// <summary>
    /// 流程管理
    /// </summary>
    public class GWFAdminAttr
    {
        #region 基本属性
        /// <summary>
        /// 工作ID
        /// </summary>
        public const string WorkID = "WorkID";
        /// <summary>
        /// 工作流
        /// </summary>
        public const string FK_Flow = "FK_Flow";
        /// <summary>
        /// 流程状态
        /// </summary>
        public const string WFState = "WFState";
        /// <summary>
        /// 流程状态(简单)
        /// </summary>
        public const string WFSta = "WFSta";
        /// <summary>
        /// TSpan
        /// </summary>
        public const string TSpan = "TSpan";
        /// <summary>
        /// 标题
        /// </summary>
        public const string Title = "Title";
        /// <summary>
        /// 发起人
        /// </summary>
        public const string Starter = "Starter";
        /// <summary>
        /// 产生时间
        /// </summary>
        public const string RDT = "RDT";
        /// <summary>
        /// 完成时间
        /// </summary>
        public const string CDT = "CDT";
        /// <summary>
        /// 得分
        /// </summary>
        public const string Cent = "Cent";
        /// <summary>
        /// 当前工作到的节点.
        /// </summary>
        public const string FK_Node = "FK_Node";
        /// <summary>
        /// 当前工作角色
        /// </summary>
        public const string FK_Station = "FK_Station";
        /// <summary>
        /// 部门
        /// </summary>
        public const string FK_Dept = "FK_Dept";
        /// <summary>
        /// 流程ID
        /// </summary>
        public const string FID = "FID";
        /// <summary>
        /// 是否启用
        /// </summary>
        public const string IsEnable = "IsEnable";
        /// <summary>
        /// 流程名称
        /// </summary>
        public const string FlowName = "FlowName";
        /// <summary>
        /// 发起人名称
        /// </summary>
        public const string StarterName = "StarterName";
        /// <summary>
        /// 节点名称
        /// </summary>
        public const string NodeName = "NodeName";
        /// <summary>
        /// 部门名称
        /// </summary>
        public const string DeptName = "DeptName";
        /// <summary>
        /// 流程类别
        /// </summary>
        public const string FK_FlowSort = "FK_FlowSort";
        /// <summary>
        /// 优先级
        /// </summary>
        public const string PRI = "PRI";
        /// <summary>
        /// 流程应完成时间
        /// </summary>
        public const string SDTOfFlow = "SDTOfFlow";
        /// <summary>
        /// 节点应完成时间
        /// </summary>
        public const string SDTOfNode = "SDTOfNode";
        /// <summary>
        /// 父流程ID
        /// </summary>
        public const string PWorkID = "PWorkID";
        /// <summary>
        /// 父流程编号
        /// </summary>
        public const string PFlowNo = "PFlowNo";
        /// <summary>
        /// 父流程节点
        /// </summary>
        public const string PNodeID = "PNodeID";
        /// <summary>
        /// 子流程的调用人.
        /// </summary>
        public const string PEmp = "PEmp";
        /// <summary>
        /// 客户编号(对于客户发起的流程有效)
        /// </summary>
        public const string GuestNo = "GuestNo";
        /// <summary>
        /// 客户名称
        /// </summary>
        public const string GuestName = "GuestName";
        /// <summary>
        /// 单据编号
        /// </summary>
        public const string BillNo = "BillNo";
        /// <summary>
        /// 备注
        /// </summary>
        public const string FlowNote = "FlowNote";
        /// <summary>
        /// 待办人员
        /// </summary>
        public const string TodoEmps = "TodoEmps";
        /// <summary>
        /// 待办人员数量
        /// </summary>
        public const string TodoEmpsNum = "TodoEmpsNum";
        /// <summary>
        /// 任务状态
        /// </summary>
        public const string TaskSta = "TaskSta";
        /// <summary>
        /// 临时存放的参数
        /// </summary>
        public const string AtPara = "AtPara";
        /// <summary>
        /// 参与人
        /// </summary>
        public const string Emps = "Emps";
        /// <summary>
        /// GUID
        /// </summary>
        public const string GUID = "GUID";
        /// <summary>
        /// 组织管理
        /// </summary>
        public const string OrgNo = "OrgNo";
        #endregion
    }
    /// <summary>
    /// 流程管理
    /// </summary>
    public class GWFAdmin : Entity
    {
        #region 基本属性
        public override UAC HisUAC
        {
            get
            {
                UAC uac = new UAC();
                uac.Readonly();
                uac.IsExp = UserRegedit.HaveRoleForExp(this.ToString());
                return uac;
            }
        }
        /// <summary>
        /// 主键
        /// </summary>
        public override string PK
        {
            get
            {
                return GWFAdminAttr.WorkID;
            }
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string FlowNote
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.FlowNote);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FlowNote, value);
            }
        }
        /// <summary>
        /// 工作流程编号
        /// </summary>
        public string FK_Flow
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.FK_Flow);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FK_Flow, value);
            }
        }
        /// <summary>
        /// BillNo
        /// </summary>
        public string BillNo
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.BillNo);
            }
            set
            {
                SetValByKey(GWFAdminAttr.BillNo, value);
            }
        }
        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.FlowName);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FlowName, value);
            }
        }
        /// <summary>
        /// 优先级
        /// </summary>
        public int PRI
        {
            get
            {
                return this.GetValIntByKey(GWFAdminAttr.PRI);
            }
            set
            {
                SetValByKey(GWFAdminAttr.PRI, value);
            }
        }
        /// <summary>
        /// 待办人员数量
        /// </summary>
        public int TodoEmpsNum
        {
            get
            {
                return this.GetValIntByKey(GWFAdminAttr.TodoEmpsNum);
            }
            set
            {
                SetValByKey(GWFAdminAttr.TodoEmpsNum, value);
            }
        }
        /// <summary>
        /// 待办人员列表
        /// </summary>
        public string TodoEmps
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.TodoEmps);
            }
            set
            {
                SetValByKey(GWFAdminAttr.TodoEmps, value);
            }
        }
        /// <summary>
        /// 参与人
        /// </summary>
        public string Emps
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.Emps);
            }
            set
            {
                SetValByKey(GWFAdminAttr.Emps, value);
            }
        }
        /// <summary>
        /// 状态
        /// </summary>
        public TaskSta TaskSta
        {
            get
            {
                return (TaskSta)this.GetValIntByKey(GWFAdminAttr.TaskSta);
            }
            set
            {
                SetValByKey(GWFAdminAttr.TaskSta, (int)value);
            }
        }
        /// <summary>
        /// 类别编号
        /// </summary>
        public string FK_FlowSort
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.FK_FlowSort);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FK_FlowSort, value);
            }
        }
        /// <summary>
        /// 部门编号
        /// </summary>
        public string FK_Dept
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.FK_Dept);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FK_Dept, value);
            }
        }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.Title);
            }
            set
            {
                SetValByKey(GWFAdminAttr.Title, value);
            }
        }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string GuestNo
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.GuestNo);
            }
            set
            {
                SetValByKey(GWFAdminAttr.GuestNo, value);
            }
        }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string GuestName
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.GuestName);
            }
            set
            {
                SetValByKey(GWFAdminAttr.GuestName, value);
            }
        }
        /// <summary>
        /// 产生时间
        /// </summary>
        public string RDT
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.RDT);
            }
            set
            {
                SetValByKey(GWFAdminAttr.RDT, value);
            }
        }
        /// <summary>
        /// 节点应完成时间
        /// </summary>
        public string SDTOfNode
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.SDTOfNode);
            }
            set
            {
                SetValByKey(GWFAdminAttr.SDTOfNode, value);
            }
        }
        /// <summary>
        /// 流程应完成时间
        /// </summary>
        public string SDTOfFlow
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.SDTOfFlow);
            }
            set
            {
                SetValByKey(GWFAdminAttr.SDTOfFlow, value);
            }
        }
        /// <summary>
        /// 流程ID
        /// </summary>
        public Int64 WorkID
        {
            get
            {
                return this.GetValInt64ByKey(GWFAdminAttr.WorkID);
            }
            set
            {
                SetValByKey(GWFAdminAttr.WorkID, value);
            }
        }
        /// <summary>
        /// 主线程ID
        /// </summary>
        public Int64 FID
        {
            get
            {
                return this.GetValInt64ByKey(GWFAdminAttr.FID);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FID, value);
            }
        }
        /// <summary>
        /// 父节点流程编号.
        /// </summary>
        public Int64 PWorkID
        {
            get
            {
                return this.GetValInt64ByKey(GWFAdminAttr.PWorkID);
            }
            set
            {
                SetValByKey(GWFAdminAttr.PWorkID, value);
            }
        }
        /// <summary>
        /// 父流程调用的节点
        /// </summary>
        public int PNodeID
        {
            get
            {
                return this.GetValIntByKey(GWFAdminAttr.PNodeID);
            }
            set
            {
                SetValByKey(GWFAdminAttr.PNodeID, value);
            }
        }
        /// <summary>
        /// PFlowNo
        /// </summary>
        public string PFlowNo
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.PFlowNo);
            }
            set
            {
                SetValByKey(GWFAdminAttr.PFlowNo, value);
            }
        }
        /// <summary>
        /// 吊起子流程的人员
        /// </summary>
        public string PEmp
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.PEmp);
            }
            set
            {
                SetValByKey(GWFAdminAttr.PEmp, value);
            }
        }
        /// <summary>
        /// 发起人
        /// </summary>
        public string Starter
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.Starter);
            }
            set
            {
                SetValByKey(GWFAdminAttr.Starter, value);
            }
        }
        /// <summary>
        /// 发起人名称
        /// </summary>
        public string StarterName
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.StarterName);
            }
            set
            {
                this.SetValByKey(GWFAdminAttr.StarterName, value);
            }
        }
        /// <summary>
        /// 发起人部门名称
        /// </summary>
        public string DeptName
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.DeptName);
            }
            set
            {
                this.SetValByKey(GWFAdminAttr.DeptName, value);
            }
        }
        /// <summary>
        /// 当前节点名称
        /// </summary>
        public string NodeName
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.NodeName);
            }
            set
            {
                this.SetValByKey(GWFAdminAttr.NodeName, value);
            }
        }
        /// <summary>
        /// 当前工作到的节点
        /// </summary>
        public int FK_Node
        {
            get
            {
                return this.GetValIntByKey(GWFAdminAttr.FK_Node);
            }
            set
            {
                SetValByKey(GWFAdminAttr.FK_Node, value);
            }
        }
        /// <summary>
        /// 工作流程状态
        /// </summary>
        public WFState WFState
        {
            get
            {
                return (WFState)this.GetValIntByKey(GWFAdminAttr.WFState);
            }
            set
            {
                if (value == WF.WFState.Complete)
                    SetValByKey(GWFAdminAttr.WFSta, (int)WFSta.Complete);
                else if (value == WF.WFState.Delete)
                    SetValByKey(GWFAdminAttr.WFSta, (int)WFSta.Etc);
                else
                    SetValByKey(GWFAdminAttr.WFSta, (int)WFSta.Runing);

                SetValByKey(GWFAdminAttr.WFState, (int)value);
            }
        }
        /// <summary>
        /// 状态(简单)
        /// </summary>
        public WFSta WFSta
        {
            get
            {
                return (WFSta)this.GetValIntByKey(GWFAdminAttr.WFSta);
            }
            set
            {
                SetValByKey(GWFAdminAttr.WFSta, (int)value);
            }
        }
        public string WFStateText
        {
            get
            {
                BP.WF.WFState ws = (WFState)this.WFState;
                switch (ws)
                {
                    case WF.WFState.Complete:
                        return "已完成";
                    case WF.WFState.Runing:
                        return "在运行";
                    case WF.WFState.Hungup:
                        return "挂起";
                    case WF.WFState.Askfor:
                        return "加签";
                    default:
                        return "未判断";
                }
            }
        }
        /// <summary>
        /// GUID
        /// </summary>
        public string GUID
        {
            get
            {
                return this.GetValStrByKey(GWFAdminAttr.GUID);
            }
            set
            {
                SetValByKey(GWFAdminAttr.GUID, value);
            }
        }
        #endregion

        #region 参数属性.
        public string Paras_ToNodes
        {

            get
            {
                return this.GetParaString("ToNodes");
            }

            set
            {
                this.SetPara("ToNodes", value);
            }
        }
        /// <summary>
        /// 加签信息
        /// </summary>
        public string Paras_AskForReply
        {

            get
            {
                return this.GetParaString("AskForReply");
            }

            set
            {
                this.SetPara("AskForReply", value);
            }
        }
        #endregion 参数属性.

        #region 构造函数
        /// <summary>
        /// 产生的工作流程
        /// </summary>
        public GWFAdmin()
        {
        }
        public GWFAdmin(Int64 workId)
        {
            QueryObject qo = new QueryObject(this);
            qo.AddWhere(GWFAdminAttr.WorkID, workId);
            if (qo.DoQuery() == 0)
                throw new Exception("工作 GWFAdmin [" + workId + "]不存在。");
        }
        /// <summary>
        /// 执行修复
        /// </summary>
        public void DoRepair()
        {
        }
        /// <summary>
        /// 重写基类方法
        /// </summary>
        public override Map EnMap
        {
            get
            {
                if (this._enMap != null)
                    return this._enMap;

                Map map = new Map("WF_GenerWorkFlow", "流程管理");
                map.setEnType(EnType.View);

                map.AddTBIntPK(GWFAdminAttr.WorkID, 0, "WorkID", false, false);
                map.AddTBString(GWFAdminAttr.Title, null, "标题", true, false, 0, 300, 200, true);

                map.AddDDLEntities(GWFAdminAttr.FK_Flow, null, "流程", new Flows(), false);

                map.AddTBString(GWFAdminAttr.BillNo, null, "单据编号", true, true, 0, 100, 50);
                map.AddTBInt(GWFAdminAttr.FK_Node, 0, "节点编号", false, false);

                map.AddDDLSysEnum(GWFAdminAttr.WFSta, 0, "状态", true, true, GWFAdminAttr.WFSta, "@0=运行中@1=已完成@2=其他");
                map.AddTBString(GWFAdminAttr.Starter, null, "发起人", false, false, 0, 100, 100);
                map.AddTBDate(GWFAdminAttr.RDT, "发起日期", true, true);

                map.AddTBString(GWFAdminAttr.NodeName, null, "停留节点", true, true, 0, 100, 100, false);
                map.AddTBString(GWFAdminAttr.TodoEmps, null, "当前处理人", true, false, 0, 100, 100, false);

                map.AddTBString(GWFAdminAttr.Emps, null, "参与人", false, false, 0, 4000, 100, true);
                //map.AddDDLSysEnum(GWFAdminAttr.TSpan, 0, "时间段", true, false,
                //    GWFAdminAttr.TSpan, "@0=本周@1=上周@2=两周以前@3=三周以前@4=更早");

                //隐藏字段.
                map.AddTBInt(GWFAdminAttr.WFState, 0, "状态", false, false);
                map.AddTBInt(GWFAdminAttr.FID, 0, "FID", false, false);
                map.AddTBInt(GWFAdminAttr.PWorkID, 0, "PWorkID", false, false);

                map.AddTBString(GWFAdminAttr.OrgNo, null, "OrgNo", false, false, 0, 4000, 100, false);

                //  map.AddSearchAttr(GWFAdminAttr.FK_Flow);
                map.AddSearchAttr(GWFAdminAttr.WFSta);

                //map.AddSearchAttr(GWFAdminAttr.TSpan,4000);
                map.AddHidden(GWFAdminAttr.FID, "=", "0");
                map.AddHidden(GWFAdminAttr.OrgNo, "=", BP.Web.WebUser.OrgNo);
                map.AddHidden(GWFAdminAttr.WFState, ">", "1");
                map.DTSearchWay = DTSearchWay.ByDate;
                map.DTSearchLabel = "发起日期";
                map.DTSearchKey = GWFAdminAttr.RDT;

                SearchNormal search = new SearchNormal(GWFAdminAttr.WFState, "流程状态",
                    GWFAdminAttr.WFState, "not in", "('0')", 0, true);
                map.SearchNormals.Add(search);

                RefMethod rm = new RefMethod();
                rm.Title = "轨迹";
                rm.ClassMethodName = this.ToString() + ".DoTrack";
                rm.RefMethodType = RefMethodType.LinkeWinOpen;
                rm.IsForEns = true;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Title = "删除";
                rm.ClassMethodName = this.ToString() + ".DoDelete";
                rm.Warning = "您确定要删除吗？";
                rm.Icon = "../../WF/Img/Btn/Delete.gif";
                rm.IsForEns = false;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Icon = "../../WF/Img/Btn/CC.gif";
                rm.Title = "移交";
                rm.ClassMethodName = this.ToString() + ".DoFlowShift";
                rm.RefMethodType = RefMethodType.LinkeWinOpen;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Icon = "../../WF/Img/Btn/Back.png";
                rm.Title = "回滚";
                rm.ClassMethodName = this.ToString() + ".DoRollback";

                rm.HisAttrs.AddDDLSQL("NodeID", "0", "回滚到节点",
                   "SELECT NodeID+'' as No,Name FROM WF_Node WHERE FK_Flow='@FK_Flow'", true);
                rm.HisAttrs.AddTBString("Note", null, "回滚原因", true, false, 0, 100, 100);
                map.AddRefMethod(rm);


                //rm = new RefMethod();
                //rm.Icon = "../../WF/Img/Btn/CC.gif";
                //rm.Title = "跳转";
                //rm.IsForEns = false;
                //rm.ClassMethodName = this.ToString() + ".DoFlowSkip";
                //rm.RefMethodType = RefMethodType.RightFrameOpen;
                //map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Icon = "../../WF/Img/Btn/CC.gif";
                rm.Title = "修复该流程数据实例";
                rm.IsForEns = false;
                rm.ClassMethodName = this.ToString() + ".RepairDataIt";
                rm.RefMethodType = RefMethodType.Func;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Title = "调整";
                rm.HisAttrs.AddTBString("RenYuan", null, "调整到人员", true, false, 0, 100, 100);
                //rm.HisAttrs.AddTBInt("shuzi", 0, "调整到节点", true, false);
                rm.HisAttrs.AddDDLSQL("nodeID", "0", "调整到节点",
                    "SELECT NodeID as No,Name FROM WF_Node WHERE FK_Flow='@FK_Flow'", true);
                rm.ClassMethodName = this.ToString() + ".DoTest";
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Title = "轨迹";
                rm.ClassMethodName = this.ToString() + ".DoTrack";
                rm.Icon = "../../WF/Img/Track.png";
                rm.IsForEns = true;
                rm.Visable = true;
                rm.RefMethodType = RefMethodType.RightFrameOpen;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Title = "修改轨迹";
                rm.IsForEns = false;
                rm.ClassMethodName = this.ToString() + ".DoEditTrack";
                rm.RefMethodType = RefMethodType.RightFrameOpen;
                map.AddRefMethod(rm);

                rm = new RefMethod();
                rm.Title = "调整数据";
                rm.IsForEns = false;
                rm.ClassMethodName = this.ToString() + ".DoEditFrm";
                rm.RefMethodType = RefMethodType.RightFrameOpen;
                map.AddRefMethod(rm);

                this._enMap = map;
                return this._enMap;
            }
        }
        #endregion


    
        /// <summary>
        /// 轨迹
        /// </summary>
        /// <returns></returns>
        public string DoTrack()
        {
            //PubClass.WinOpen(Glo.CCFlowAppPath + "WF/WFRpt.htm?WorkID=" + this.WorkID + "&FID=" + this.FID + "&FK_Flow=" + this.FK_Flow, 900, 800);
            return "/WF/WFRpt.htm?CurrTab=Truck&WorkID=" + this.WorkID + "&FID=" + this.FID + "&FK_Flow=" + this.FK_Flow + "&FK_Node=" + this.FK_Node;
        }
        /// <summary>
        /// 修改表单
        /// </summary>
        /// <returns></returns>
        public string DoEditFrm()
        {
            Node nd = new Node(this.FK_Node);
            if (nd.FormType == NodeFormType.SelfForm
                || nd.FormType == NodeFormType.SDKForm
                || nd.FormType == NodeFormType.SheetAutoTree
                || nd.FormType == NodeFormType.SheetTree
                || nd.FormType == NodeFormType.WebOffice
                || nd.FormType == NodeFormType.WordForm)
                return "err@当前节点表单类型不同.";

            string frmID = nd.NodeFrmID;
            return "/WF/Admin/AttrFlow/AdminFrmList.htm?FK_Flow=" + this.FK_Flow + "&FrmID=" + frmID + "&WorkID=" + this.WorkID;
        }
        public string DoTest(string toEmpNo, string toNodeID)
        {
            return BP.WF.Dev2Interface.Flow_ReSend(this.WorkID, int.Parse(toNodeID),
                toEmpNo, BP.Web.WebUser.Name + ":调整.");
        }
        public string RepairDataIt()
        {
            string infos = "";

            Flow fl = new Flow(this.FK_Flow);
            Node nd = new Node(int.Parse(fl.No + "01"));
            Work wk = nd.HisWork;

            string trackTable = "ND" + int.Parse(fl.No) + "Track";
            string sql = "SELECT MyPK FROM " + trackTable + " WHERE WorkID=" + this.WorkID + " AND ACTIONTYPE=1 and NDFrom=" + nd.NodeID;
            string mypk = DBAccess.RunSQLReturnString(sql);
            if (DataType.IsNullOrEmpty(mypk) == true)
                return "err@没有找到track主键。";

            wk.OID = this.WorkID;
            wk.RetrieveFromDBSources();

            
            string json = DBAccess.GetBigTextFromDB(trackTable, "MyPK", mypk, "FrmDB");
            if (DataType.IsNullOrEmpty(json) == true)
                return "";
            DataTable dtVal = BP.Tools.Json.ToDataTable(json);

            DataRow mydr = dtVal.Rows[0];

            Attrs attrs = wk.EnMap.Attrs;
            bool isHave = false;
            foreach (Attr attr in attrs)
            {
                string jsonVal = mydr[attr.Key].ToString();
                string enVal = wk.GetValStringByKey(attr.Key);
                if (DataType.IsNullOrEmpty(enVal) == true)
                {
                    wk.SetValByKey(attr.Key, jsonVal);
                    isHave = true;
                }
            }

            if (isHave == true)
            {
                wk.DirectUpdate();
                return "不需要更新数据.";
            }
            infos += "@WorkID=" + wk.OID + " =" + wk.Rec + "  被修复.";

            return infos;
        }
        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="nodeid">节点ID</param>
        /// <param name="note">回滚原因</param>
        /// <returns>回滚的结果</returns>
        public string DoRollback(string nodeID, string note)
        {
            try
            {
                return BP.WF.Dev2Interface.Flow_DoRebackWorkFlow(this.FK_Flow, this.WorkID,
                    int.Parse(nodeID), note);
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 修改轨迹
        /// </summary>
        /// <returns></returns>
        public string DoEditTrack()
        {
            return "../../Admin/AttrFlow/EditTrack.htm?WorkID=" + this.WorkID + "&FID=" + this.FID + "&FK_Flow=" + this.FK_Flow;
        }
        /// <summary>
        /// 执行移交
        /// </summary>
        /// <param name="ToEmp"></param>
        /// <param name="Note"></param>
        /// <returns></returns>
        public string DoShift(string ToEmp, string Note)
        {
            if (BP.WF.Dev2Interface.Flow_IsCanViewTruck(this.FK_Flow, this.WorkID) == false)
                return "您没有操作该流程数据的权限.";

            try
            {
                BP.WF.Dev2Interface.Node_Shift(this.WorkID, ToEmp, Note);
                return "移交成功";
            }
            catch (Exception ex)
            {
                return "移交失败@" + ex.Message;
            }
        }
        /// <summary>
        /// 执行删除
        /// </summary>
        /// <returns></returns>
        public string DoDelete()
        {
            if (BP.WF.Dev2Interface.Flow_IsCanViewTruck(this.FK_Flow, this.WorkID) == false)
                return "您没有操作该流程数据的权限.";

            try
            {
                BP.WF.Dev2Interface.Flow_DoDeleteFlowByReal(this.WorkID, true);
                return "删除成功";
            }
            catch (Exception ex)
            {
                return "删除失败@" + ex.Message;
            }
        }
        /// <summary>
        /// 移交
        /// </summary>
        /// <returns></returns>
        public string DoFlowShift()
        {
            return "../../WorkOpt/Shift.htm?WorkID=" + this.WorkID + "&FID=" + this.FID + "&FK_Flow=" + this.FK_Flow + "&FK_Node=" + this.FK_Node;
        }
        /// <summary>
        /// 回滚流程
        /// </summary>
        /// <returns></returns>
        public string Rollback()
        {

            return "../../WorkOpt/Rollback.htm?WorkID=" + this.WorkID + "&FID=" + this.FID + "&FK_Flow=" + this.FK_Flow + "&FK_Node=" + this.FK_Node;
        }
        /// <summary>
        /// 执行跳转
        /// </summary>
        /// <returns></returns>
        public string DoFlowSkip()
        {
            return "../../WorkOpt/FlowSkip.htm?WorkID=" + this.WorkID + "&FID=" + this.FID + "&FK_Flow=" + this.FK_Flow + "&FK_Node=" + this.FK_Node;
        }
    }
    /// <summary>
    /// 流程管理s
    /// </summary>
    public class GWFAdmins : Entities
    {
        #region 方法
        /// <summary>
        /// 得到它的 Entity 
        /// </summary>
        public override Entity GetNewEntity
        {
            get
            {
                return new GWFAdmin();
            }
        }
        /// <summary>
        /// 流程管理集合
        /// </summary>
        public GWFAdmins() { }
        #endregion

        #region 为了适应自动翻译成java的需要,把实体转换成List.
        /// <summary>
        /// 转化成 java list,C#不能调用.
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.IList<GWFAdmin> ToJavaList()
        {
            return (System.Collections.Generic.IList<GWFAdmin>)this;
        }
        /// <summary>
        /// 转化成list
        /// </summary>
        /// <returns>List</returns>
        public System.Collections.Generic.List<GWFAdmin> Tolist()
        {
            System.Collections.Generic.List<GWFAdmin> list = new System.Collections.Generic.List<GWFAdmin>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add((GWFAdmin)this[i]);
            }
            return list;
        }
        #endregion 为了适应自动翻译成java的需要,把实体转换成List.
    }

}
