﻿using System;
using System.Collections;
using BP.DA;
using BP.Difference;
using BP.Sys;
using BP.WF.Template;

namespace BP.WF.HttpHandler
{
    /// <summary>
    /// 初始化函数
    /// </summary>
    public class WF_Admin_CCBPMDesigner2018 : DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WF_Admin_CCBPMDesigner2018()
        {
        }

        #region 节点相关 Nodes
        /// <summary>
        /// 创建流程节点并返回编号
        /// </summary>
        /// <returns></returns>
        public string CreateNode()
        {
            try
            {
                string x = this.GetRequestVal("X");
                string y = this.GetRequestVal("Y");
                string icon = this.GetRequestVal("icon");
                int nodeModel = this.GetRequestValInt("NodeModel");

                int iX = 20;
                int iY = 20;

                if (DataType.IsNullOrEmpty(x) == false)
                    iX = (int)double.Parse(x);

                if (DataType.IsNullOrEmpty(y) == false)
                    iY = (int)double.Parse(y);

                Node node = BP.WF.Template.TemplateGlo.NewNode(this.FK_Flow, iX, iY, icon, nodeModel);


                Hashtable ht = new Hashtable();
                ht.Add("NodeID", node.NodeID);
                ht.Add("Name", node.Name);
                ht.Add("RunModel", (int)node.HisRunModel);

                return BP.Tools.Json.ToJsonEntityModel(ht);
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 根据节点编号删除流程节点
        /// </summary>
        /// <returns>执行结果</returns>
        public string DeleteNode()
        {
            try
            {
                BP.WF.Node node = new BP.WF.Node();
                node.NodeID = this.FK_Node;
                if (node.RetrieveFromDBSources() == 0)
                    return "err@删除失败,没有删除到数据，估计该节点已经别删除了.";

                if (node.IsStartNode == true)
                    return "err@开始节点不允许被删除。";

                node.Delete();
                return "删除成功.";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }

        public string EditNodePosition()
        {
            try
            {
                string FK_Node = this.GetValFromFrmByKey("NodeID");
                string x = this.GetValFromFrmByKey("X");
                string y = this.GetValFromFrmByKey("Y");
                BP.WF.Node node = new BP.WF.Node();
                node.NodeID = int.Parse(FK_Node);
                int left = DataType.IsNullOrEmpty(x) ? 20 : int.Parse(x);
                int top = DataType.IsNullOrEmpty(x) ? 20 : int.Parse(y);
                if (left <= 0) left = 20;
                if (top <= 0) top = 20;

                int iResult = node.RetrieveFromDBSources();
                if (iResult > 0)
                {
                    node.X = left;
                    node.Y = top;
                    node.Update();
                    return "修改成功.";
                }

                return "err@修改节点失败，请确认该节点是否存在？";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 修改节点名称
        /// </summary>
        /// <returns></returns>
        public string Node_EditNodeName()
        {
            string FK_Node = this.GetValFromFrmByKey("NodeID");
            string NodeName = HttpContextHelper.UrlDecode(this.GetValFromFrmByKey("NodeName"));

            BP.WF.Node node = new BP.WF.Node();
            node.NodeID = int.Parse(FK_Node);
            int iResult = node.RetrieveFromDBSources();
            if (iResult > 0)
            {
                node.Name = NodeName;
                node.Update();
                return "@修改成功.";
            }

            return "err@修改节点失败，请确认该节点是否存在？";
        }
        ///// <summary>
        ///// 修改节点运行模式
        ///// </summary>
        ///// <returns></returns>
        //public string Node_ChangeRunModel()
        //{
        //    string runModel = GetValFromFrmByKey("RunModel");
        //    BP.WF.Node node = new BP.WF.Node(this.FK_Node);
        //    //节点运行模式
        //    switch (runModel)
        //    {
        //        case "NodeOrdinary":
        //            node.HisRunModel = BP.WF.RunModel.Ordinary;
        //            break;
        //        case "NodeFL":
        //            node.HisRunModel = BP.WF.RunModel.FL;
        //            break;
        //        case "NodeHL":
        //            node.HisRunModel = BP.WF.RunModel.HL;
        //            break;
        //        case "NodeFHL":
        //            node.HisRunModel = BP.WF.RunModel.FHL;
        //            break;
        //        case "NodeSubThread":
        //            node.HisRunModel = BP.WF.RunModel.SubThread;
        //            break;
        //    }
        //    node.Update();

        //    return "设置成功.";
        //}
        #endregion end Node

        /// <summary>
        /// 删除连接线
        /// </summary>
        /// <returns></returns>
        public string Direction_Delete()
        {
            try
            {
                Directions di = new Directions();
                di.Retrieve(DirectionAttr.FK_Flow, this.FK_Flow, DirectionAttr.Node, this.FK_Node, DirectionAttr.ToNode, this.GetValFromFrmByKey("ToNode"));
                foreach (Direction direct in di)
                {
                    direct.Delete();
                }
                return "@删除成功！";
            }
            catch (Exception ex)
            {
                return "@err:" + ex.Message;
            }
        }
        public string Direction_Init()
        {
            try
            {
                string pk = this.FK_Flow + "_" + this.FK_Node + "_" + this.GetValFromFrmByKey("ToNode");

                Direction dir = new Direction();
                dir.setMyPK(pk);

                if (dir.RetrieveFromDBSources() > 0)
                {
                    return dir.Des;
                }

                return "";
            }
            catch (Exception ex)
            {
                return "@err:" + ex.Message;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Direction_Save()
        {
            try
            {
                string pk = this.FK_Flow + "_" + this.FK_Node + "_" + this.GetValFromFrmByKey("ToNode");

                Direction dir = new Direction();
                dir.setMyPK(pk);

                if (dir.RetrieveFromDBSources() > 0)
                {
                    dir.Des = this.GetValFromFrmByKey("Des");
                    dir.DirectUpdate();
                }

                return "@保存成功！";
            }
            catch (Exception ex)
            {
                return "@err:" + ex.Message;
            }
        }
        /// <summary>
        /// 添加标签
        /// </summary>
        /// <returns></returns>
        public string CreatLabNote()
        {
            try
            {
                LabNote lb = new LabNote();

                //获取当前流程已经存在的数量
                LabNotes labNotes = new LabNotes();
                int num = labNotes.Retrieve(LabNoteAttr.FK_Flow, this.FK_Flow);

                string Name = this.GetValFromFrmByKey("LabName");
                int x = int.Parse(this.GetValFromFrmByKey("X"));
                int y = int.Parse(this.GetValFromFrmByKey("Y"));

                lb.setMyPK(this.FK_Flow + "_" + x + "_" + y + "_" + (num + 1));
                lb.Name = Name;
                lb.FK_Flow = this.FK_Flow;
                lb.X = x;
                lb.Y = y;

                lb.DirectInsert();

                Hashtable ht = new Hashtable();
                ht.Add("MyPK", this.FK_Flow + "_" + x + "_" + y + "_" + (num + 1));
                ht.Add("FK_Flow", this.FK_Flow);

                return BP.Tools.Json.ToJsonEntityModel(ht);
            }
            catch (Exception ex)
            {
                return "@err:" + ex.Message;
            }
        }

        public void CheckBillFrm()
        {
            GEEntity en = new GEEntity(this.EnsName);
            en.CheckPhysicsTable();
        }
    }
}
