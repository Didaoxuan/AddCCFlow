using System;
using System.Data;
using BP.DA;
using BP.Sys;
namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 处理页面的业务逻辑
    /// </summary>
    public class Admin_CCFormDesigner_SFTable : BP.WF.HttpHandler.DirectoryPageBase
    { 

        #region SFList 外键表列表.
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public string SFList_Delete()
        {
            try
            {
                SFTable sf = new SFTable(this.FK_SFTable);
                sf.Delete();
                return "删除成功...";
            }
            catch (Exception ex)
            {
                return "err@" + ex.Message;
            }
        }
        /// <summary>
        /// 字典表列表.
        /// </summary>
        /// <returns></returns>
        public string SFList_Init()
        {
            DataSet ds = new DataSet();

            BP.Cloud.Sys.SFTables ens = new BP.Cloud.Sys.SFTables();
            ens.Retrieve("OrgNo", BP.Web.WebUser.OrgNo);

            DataTable dt = ens.ToDataTableField("SFTables");
            ds.Tables.Add(dt);

            int pTableModel = 0;
            if (this.GetRequestVal("PTableModel").Equals("2"))
                pTableModel = 2;

            //获得ptableModel.
            if (pTableModel == 0)
            {
                MapDtl dtl = new MapDtl();
                dtl.No = this.FK_MapData;
                if (dtl.RetrieveFromDBSources() == 1)
                {
                    pTableModel = dtl.PTableModel;
                }
                else
                {
                    MapData md = new MapData();
                    md.No = this.FK_MapData;
                    if (md.RetrieveFromDBSources() == 1)
                        pTableModel = md.PTableModel;
                }
            }

            if (pTableModel == 2)
            {
                DataTable mydt = MapData.GetFieldsOfPTableMode2(this.FK_MapData);
                mydt.TableName = "Fields";
                ds.Tables.Add(mydt);
            }

            return BP.Tools.Json.ToJson(ds);
        }
        public string SFList_SaveSFField()
        {
            MapAttr attr = new BP.Sys.MapAttr();
            attr.setMyPK(this.FK_MapData + "_" + this.KeyOfEn);
            if (attr.RetrieveFromDBSources() != 0)
                return "err@字段名[" + this.KeyOfEn + "]已经存在.";

            BP.Sys.CCFormAPI.SaveFieldSFTable(this.FK_MapData, this.KeyOfEn, null, this.GetRequestVal("SFTable"), 100, 100, 1);

            attr.Retrieve();
            Paras ps = new Paras();
            ps.SQL = "SELECT OID FROM Sys_GroupField A WHERE A.FrmID=" + BP.Difference.SystemConfig.AppCenterDBVarStr + "FrmID AND (CtrlType='' OR CtrlType IS NULL) ORDER BY OID DESC ";
            ps.Add("FrmID", this.FK_MapData);
            attr.GroupID = DBAccess.RunSQLReturnValInt(ps, 0);
            attr.Update();

            SFTable sf = new SFTable(attr.UIBindKey);

            if (sf.SrcType == SrcType.TableOrView || sf.SrcType == SrcType.BPClass || sf.SrcType == SrcType.CreateTable)
                return "../../Comm/En.htm?EnName=BP.Sys.FrmUI.MapAttrSFTable&PKVal=" + attr.MyPK;
            else
                return "../../Comm/En.htm?EnName=BP.Sys.FrmUI.MapAttrSFSQL&PKVal=" + attr.MyPK;
        }
        #endregion 外键表列表.

    }
}
