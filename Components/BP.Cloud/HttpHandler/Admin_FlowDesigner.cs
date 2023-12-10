using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using BP.DA;
using BP.Sys;
using BP.Web;
using BP.Port;
using BP.En;
using BP.WF;
using BP.WF.Template;
using BP.Cloud;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 流程设计器
    /// </summary>
    public class FlowDesigner : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FlowDesigner()
        {
        }

        public string Flow_Delete()
        {
            Flow flow = new Flow(this.No);
            return flow.DoDelete();
        }

        public string Do_Check()
        {

            BP.WF.HttpHandler.WF_Admin_CCBPMDesigner hand = new BP.WF.HttpHandler.WF_Admin_CCBPMDesigner();
            hand.DownFormTemplete();

            Nodes nds = new Nodes(this.FK_Flow);
            string msg = "";

            #region 如果是最后一个节点，就让其为开始节点的申请人发送消息 ..for 宜昌.
            foreach (Node nd in nds)
            {
                if (nd.HisNodePosType != NodePosType.End)
                    continue;
                if (nd.Name.Contains("反馈") || nd.Name.Contains("申请人"))
                    continue;

                BP.WF.Template.PushMsg pm = new PushMsg();
                int i = pm.Retrieve(PushMsgAttr.FK_Node, nd.NodeID, PushMsgAttr.SMSPushWay, 3);
                if (i == 1)
                    continue;
                pm.FK_Node = nd.NodeID;
                pm.FK_Flow = nd.FK_Flow;
                pm.FK_Event = "SendSuccess";
                pm.PushWay = 0;
                pm.SMSPushWay = 4;
                pm.SMSField = "Emps";
                pm.SMSDoc = "流程{{Title}}已经完成, 发送人:@WebUser.No, @WebUser.Name,打开{Url} .";
                pm.SMSNodes = int.Parse(this.No) + "01,";
                pm.SMSPushModel = "Email,CCMsg,SMS,DingDing,WeiXin,WS,";
                pm.MailPushWay = 0;
                pm.setMyPK(DBAccess.GenerGUID());
                pm.Insert();
                msg += "@信息:节点ID:" + nd.NodeID + " 名称:" + nd.Name + "是最后一个节点，增加了反馈个申请人信息.";
            }
            #endregion 如果是最后一个节点，就让其为开始节点的申请人发送消息.。

            return "执行成功.";
        }
    }
}
