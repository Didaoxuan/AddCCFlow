using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using BP.DA;
using BP.Sys;
using BP.Web;
using BP.Port;
using BP.En;
using BP.WF;
using BP.WF.Rpt;
using BP.WF.Template;
using BP.WF.Data;
using BP.Sys.XML;
using BP.Difference;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class App_OneFrm_FrmSearch : BP.WF.HttpHandler.DirectoryPageBase
    {
        #region 属性.
        /// <summary>
        /// 查询类型
        /// </summary>
        public string SearchType
        {
            get
            {
                string val = this.GetRequestVal("SearchType");

                if (val == null || val.Equals(""))
                    val = this.GetRequestVal("GroupType");
                return val;
            }


        }

        /// <summary>
        /// 分析类型
        /// </summary>
        public string GroupType
        {
            get
            {
                return this.GetRequestVal("GroupType");
            }
        }

        public bool IsContainsNDYF
        {
            get
            {
                return this.GetRequestValBoolen("IsContainsNDYF");
            }
        }

     
        #endregion 属性.

        /// <summary>
        /// 构造函数
        /// </summary>
        public App_OneFrm_FrmSearch()
        {
        }

        #region 执行父类的重写方法.
        /// <summary>
        /// 默认执行的方法
        /// </summary>
        /// <returns></returns>
        protected override string DoDefaultMethod()
        {
            switch (this.DoType)
            {
                case "DtlFieldUp": //字段上移
                    return "执行成功.";
                default:
                    break;
            }

            //找不不到标记就抛出异常.
            throw new Exception("@标记[" + this.DoType + "]，没有找到. @RowURL:" + HttpContextHelper.RequestRawUrl);
        }
        #endregion 执行父类的重写方法.

       
        /// <summary>
        /// 获取单流程查询条件
        /// </summary>
        /// <returns></returns>
        public string FrmSearch_InitToolBar()
        {
            if (string.IsNullOrWhiteSpace(this.FrmID))
                return "err@参数FrmID不能为空";

            #region 用户查询条件信息 不存在注册
            //默认显示的字段
            string selectFields = "," + GERptAttr.Title + "," + GERptAttr.BillNo + ",StarterName ," + GERptAttr.RDT + "," + GERptAttr.FK_Dept + ",BillState,";
            UserRegedit ur = new UserRegedit();
            string pk = WebUser.No + this.FrmID + "_SearchAttrs";
            ur.SetValByKey("MyPK", pk);
            if (ur.RetrieveFromDBSources() == 0)
            {
                ur.SetValByKey(UserRegeditAttr.FK_Emp, WebUser.No);
                ur.SetValByKey(UserRegeditAttr.CfgKey, this.FrmID + "_SearchAttrs");

                ur.SetPara("SelectFields", selectFields);
                ur.SetPara("IsSearchKeys", 1);
                ur.SetValByKey(UserRegeditAttr.OrderBy, "OID");
                ur.SetValByKey(UserRegeditAttr.OrderWay, " asc ");

                ur.Insert();
            }
            if (DataType.IsNullOrEmpty(ur.GetParaString("SelectFields")) == true)
            {
                if (DataType.IsNullOrEmpty(ur.GetParaString("SelectFields")) == true)
                    ur.SetPara("SelectFields", selectFields);
                ur.Update();
            }
            #endregion 用户查询条件



            DataSet ds = new DataSet();

            BP.Cloud.Sys.MapData md = new BP.Cloud.Sys.MapData(this.FrmID);
            #region  关键字 时间查询条件
            md.SetPara("DTSearchWay", ur.GetParaInt("DTSearchWay"));
            md.SetPara("DTSearchKey", ur.GetParaString("DTSearchKey"));
            md.SetPara("IsSearchKey", ur.GetParaString("IsSearchKeys"));
            md.SetPara("StringSearchKeys", ur.GetParaString("StringSearchKeys"));

            md.SetPara("SearchKey", ur.SearchKey);

            if (md.DTSearchWay != DTSearchWay.None)
            {
                MapAttr mapAttr = new MapAttr(this.FrmID, ur.GetParaString("DTSearchKey"));
               
                md.SetPara("DTSearchLabel", mapAttr.Name);

                if (md.DTSearchWay == DTSearchWay.ByDate)
                {
                    md.SetPara("DTFrom", ur.GetValStringByKey(UserRegeditAttr.DTFrom));
                    md.SetPara("DTTo", ur.GetValStringByKey(UserRegeditAttr.DTTo));
                }
                else
                {
                    md.SetPara("DTFrom", ur.GetValStringByKey(UserRegeditAttr.DTFrom));
                    md.SetPara("DTTo", ur.GetValStringByKey(UserRegeditAttr.DTTo));
                }
            }
            #endregion 关键字时间查询条件

            ds.Tables.Add(md.ToDataTableField("Sys_MapData"));

           

            #region 外键枚举的查询条件
            MapAttrs attrs = new MapAttrs(this.FrmID);

            DataTable dt = new DataTable();
            dt.Columns.Add("Field");
            dt.Columns.Add("Name");
            dt.Columns.Add("Width");
            dt.Columns.Add("UIContralType");
            dt.TableName = "Attrs";
            string[] ctrls = ur.GetParaString("RptDDLSearchKeys").Split('*');
            MapAttr attr;
            foreach (string ctrl in ctrls)
            {
                //增加判断，如果URL中有传参，则不进行此SearchAttr的过滤条件显示context.Request.QueryString[ctrl]
                if (string.IsNullOrWhiteSpace(ctrl) || !DataType.IsNullOrEmpty(HttpContextHelper.RequestParams(ctrl)))
                    continue;

                attr = attrs.GetEntityByKey(MapAttrAttr.KeyOfEn, ctrl) as MapAttr;
                if (attr == null)
                    continue;
                DataRow dr = dt.NewRow();
                dr["Field"] = attr.KeyOfEn;
                dr["Name"] = attr.HisAttr.Desc;
                dr["Width"] = attr.UIWidth; //下拉框显示的宽度.
                dr["UIContralType"] = attr.HisAttr.UIContralType;
                dt.Rows.Add(dr);

                Attr ar = attr.HisAttr;
                //判读该字段是否是枚举
                if (ar.IsEnum == true)
                {
                    SysEnums ses = new SysEnums();
                    ses.Retrieve(SysEnumAttr.EnumKey, attr.UIBindKey, SysEnumAttr.OrgNo, WebUser.OrgNo);
                    if(ses.Count == 0)
                    {
                        QueryObject qo = new QueryObject(ses);
                        qo.AddWhere(SysEnumAttr.EnumKey, attr.UIBindKey);
                        qo.addAnd();
                        qo.AddWhereIsNull(SysEnumAttr.OrgNo);
                        qo.DoQuery();
                    }
                    

                    DataTable dtEnum = ses.ToDataTableField();
                    dtEnum.TableName = attr.KeyOfEn;
                    ds.Tables.Add(dtEnum);
                    continue;
                }
                //判断是否是外键
                if (ar.IsFK == true)
                {
                    Entities ensFK = ar.HisFKEns;
                    ensFK.RetrieveAll();

                    DataTable dtEn = ensFK.ToDataTableField();
                    dtEn.TableName = ar.Key;
                    ds.Tables.Add(dtEn);
                    continue;
                }

                if (DataType.IsNullOrEmpty(ar.UIBindKey) == false
                    && ds.Tables.Contains(ar.Key) == false)
                {
                    //获取SQl
                    string sql = BP.WF.Glo.DealExp(attr.UIBindKey, null, null);
                    DataTable dtSQl = DBAccess.RunSQLReturnTable(sql);
                    foreach (DataColumn col in dtSQl.Columns)
                    {
                        string colName = col.ColumnName.ToLower();
                        switch (colName)
                        {
                            case "no":
                                col.ColumnName = "No";
                                break;
                            case "name":
                                col.ColumnName = "Name";
                                break;
                            case "parentno":
                                col.ColumnName = "ParentNo";
                                break;
                            default:
                                break;
                        }
                    }
                    dtSQl.TableName = ar.Key;
                    ds.Tables.Add(dtSQl);
                }


            }

            ds.Tables.Add(dt);

            #endregion 外键枚举的查询条件

            return BP.Tools.Json.ToJson(ds);

        }
        /// <summary>
        /// 单流程查询显示的列
        /// </summary>
        /// <returns></returns>
        public string FrmSearch_MapAttrs()
        {
            //查询出单流程的所有字段
            MapAttrs attrsOfAll = new MapAttrs();
            attrsOfAll.Retrieve(MapAttrAttr.FK_MapData, this.FK_MapData);

            MapAttrs mattrs = new MapAttrs();

            foreach (MapAttr mapAttr in attrsOfAll)
            {

                if (mapAttr.KeyOfEn.Equals(GERptAttr.AtPara) || mapAttr.KeyOfEn.Equals(GERptAttr.OID) || mapAttr.KeyOfEn.Equals("OrgNo"))
                    continue;
                //我创建的
                if (this.SearchType.Equals("My") == true 
                        && (mapAttr.KeyOfEn.Equals(GERptAttr.FK_Dept) || mapAttr.KeyOfEn.Equals("StarterName") || mapAttr.KeyOfEn.Equals("Starter")))
                    continue;

                //我部门创建的
                if (this.SearchType.Equals("MyDept") == true && (mapAttr.KeyOfEn.Equals(GERptAttr.FK_Dept) || mapAttr.KeyOfEn.Equals("Starter")))
                    continue;

                mattrs.AddEntity(mapAttr);
            }

            return BP.Tools.Json.ToJson(mattrs.ToDataTableField());
        }

        /// <summary>
        /// 单流程查询数据集合
        /// </summary>
        /// <returns></returns>
        public string FrmSearch_Data()
        {
         
            //当前用户查询信息表
            UserRegedit ur = new UserRegedit(WebUser.No, this.FrmID + "_SearchAttrs");
            //流程表单对应的流程数据
            GEEntitys ens = new GEEntitys(this.FrmID);
            QueryObject qo = FrmSearch_QuerySQL(ens, ur);
            ur.SetPara("Count", qo.GetCount());
            ur.Update();
            qo.DoQuery("OID", this.PageSize, this.PageIdx);
            return BP.Tools.Json.ToJson(ens.ToDataTableField("FlowSearch_Data"));

        }


        private QueryObject FrmSearch_QuerySQL(GEEntitys ens, UserRegedit ur)
        {
            //表单属性
            BP.Cloud.Sys.MapData mapData = new BP.Cloud.Sys.MapData(this.FrmID);
            MapAttrs attrs = new MapAttrs(this.FrmID);
            QueryObject qo = new QueryObject(ens);

            switch (this.SearchType)
            {
                case "My": //我发起的.
                    qo.AddWhere("Starter", WebUser.No);
                    break;
                case "MyDept": //我部门发起的.  
                    if (mapData.GetParaBoolen("IsSearchNextLeavel") == false)
                    {
                        //只查本部门及兼职部门
                        qo.AddWhereInSQL(GERptAttr.FK_Dept, "SELECT FK_Dept From Port_DeptEmp Where FK_Emp='" + WebUser.No + "'");
                    }
                    else
                    {
                        //查本部门及子级
                        string sql = "SELECT FK_Dept From Port_DeptEmp Where FK_Emp='" + WebUser.No + "'";
                        sql += " UNION ";
                        sql += "SELECT No AS FK_Dept From Port_Dept Where ParentNo IN(SELECT FK_Dept From Port_DeptEmp Where FK_Emp='" + WebUser.No + "')";
                        qo.AddWhereInSQL(GERptAttr.FK_Dept, sql);

                    }
                    break;

                default:
                    throw new Exception("err@" + this.SearchType + "标记错误.");
            }

            #region 关键字查询
            string searchKey = ""; //关键字查询
            if (ur.GetParaBoolen("IsSearchKeys"))
                searchKey = ur.SearchKey;

            if (ur.GetParaBoolen("IsSearchKeys") && DataType.IsNullOrEmpty(searchKey) == false && searchKey.Length >= 1)
            {
                int i = 0;
                Attr attr;
                foreach (MapAttr mapAttr in attrs)
                {
                    attr = mapAttr.HisAttr;
                    switch (attr.MyFieldType)
                    {
                        case FieldType.Enum:
                        case FieldType.FK:
                        case FieldType.PKFK:
                            continue;
                        default:
                            break;
                    }

                    if (attr.MyDataType != DataType.AppString)
                        continue;

                    if (attr.MyFieldType == FieldType.RefText)
                        continue;

                    if (attr.Key == "FK_Dept")
                        continue;

                    i++;

                    if (i == 1)
                    {
                        qo.addAnd();
                        qo.addLeftBracket();
                        if (BP.Difference.SystemConfig.AppCenterDBVarStr == "@" || BP.Difference.SystemConfig.AppCenterDBVarStr == "?")
                            qo.AddWhere(attr.Key, " LIKE ", BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL ? (" CONCAT('%'," + BP.Difference.SystemConfig.AppCenterDBVarStr + "SKey,'%')") : (" '%'+" + BP.Difference.SystemConfig.AppCenterDBVarStr + "SKey+'%'"));
                        else
                            qo.AddWhere(attr.Key, " LIKE ", " '%'||" + BP.Difference.SystemConfig.AppCenterDBVarStr + "SKey||'%'");
                        continue;
                    }

                    qo.addOr();

                    if (BP.Difference.SystemConfig.AppCenterDBVarStr == "@" || BP.Difference.SystemConfig.AppCenterDBVarStr == "?")
                        qo.AddWhere(attr.Key, " LIKE ", BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL ? ("CONCAT('%'," + BP.Difference.SystemConfig.AppCenterDBVarStr + "SKey,'%')") : ("'%'+" + BP.Difference.SystemConfig.AppCenterDBVarStr + "SKey+'%'"));
                    else
                        qo.AddWhere(attr.Key, " LIKE ", "'%'||" + BP.Difference.SystemConfig.AppCenterDBVarStr + "SKey||'%'");
                }



                qo.MyParas.Add("SKey", searchKey);
                qo.addRightBracket();
            }
            else if (DataType.IsNullOrEmpty(ur.GetParaString("StringSearchKeys")) == false)
            {
                string field = "";//字段名
                string fieldValue = "";//字段值
                int idx = 0;

                //获取查询的字段
                string[] searchFields = ur.GetParaString("StringSearchKeys").Split('*');
                foreach (String str in searchFields)
                {
                    if (DataType.IsNullOrEmpty(str) == true)
                        continue;

                    //字段名
                    string[] items = str.Split(',');
                    if (items.Length == 2 && DataType.IsNullOrEmpty(items[0]) == true)
                        continue;
                    field = items[0];
                    //字段名对应的字段值
                    fieldValue = ur.GetParaString(field);
                    if (DataType.IsNullOrEmpty(fieldValue) == true)
                        continue;
                    idx++;
                    if (idx == 1)
                    {
                        qo.addAnd();
                        /* 第一次进来。 */
                        qo.addLeftBracket();
                        if (BP.Difference.SystemConfig.AppCenterDBVarStr == "@" || BP.Difference.SystemConfig.AppCenterDBVarStr == "?")
                            qo.AddWhere(field, " LIKE ", BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL ? (" CONCAT('%'," + BP.Difference.SystemConfig.AppCenterDBVarStr + field + ",'%')") : (" '%'+" + BP.Difference.SystemConfig.AppCenterDBVarStr + field + "+'%'"));
                        else
                            qo.AddWhere(field, " LIKE ", " '%'||" + BP.Difference.SystemConfig.AppCenterDBVarStr + field + "||'%'");
                        qo.MyParas.Add(field, fieldValue);
                        continue;
                    }
                    qo.addAnd();

                    if (BP.Difference.SystemConfig.AppCenterDBVarStr == "@" || BP.Difference.SystemConfig.AppCenterDBVarStr == "?")
                        qo.AddWhere(field, " LIKE ", BP.Difference.SystemConfig.AppCenterDBType == DBType.MySQL ? ("CONCAT('%'," + BP.Difference.SystemConfig.AppCenterDBVarStr + field + ",'%')") : ("'%'+" + BP.Difference.SystemConfig.AppCenterDBVarStr + field + "+'%'"));
                    else
                        qo.AddWhere(field, " LIKE ", "'%'||" + BP.Difference.SystemConfig.AppCenterDBVarStr + field + "||'%'");
                    qo.MyParas.Add(field, fieldValue);


                }
                if (idx != 0)
                    qo.addRightBracket();
            }

            #endregion 关键字查询

            #region Url传参条件
            string val = "";
            List<string> keys = new List<string>();
            foreach (MapAttr attr in attrs)
            {
                if (DataType.IsNullOrEmpty(HttpContextHelper.RequestParams(attr.KeyOfEn)))
                    continue;
                qo.addAnd();
                qo.addLeftBracket();
                val = HttpContextHelper.RequestParams(attr.KeyOfEn);

                switch (attr.MyDataType)
                {
                    case DataType.AppBoolean:
                        qo.AddWhere(attr.KeyOfEn, Convert.ToBoolean(int.Parse(val)));
                        break;
                    case DataType.AppDate:
                    case DataType.AppDateTime:
                    case DataType.AppString:
                        qo.AddWhere(attr.KeyOfEn, val);
                        break;
                    case DataType.AppDouble:
                    case DataType.AppFloat:
                    case DataType.AppMoney:
                        qo.AddWhere(attr.KeyOfEn, double.Parse(val));
                        break;
                    case DataType.AppInt:
                        qo.AddWhere(attr.KeyOfEn, int.Parse(val));
                        break;
                    default:
                        break;
                }

                qo.addRightBracket();

                if (keys.Contains(attr.KeyOfEn) == false)
                    keys.Add(attr.KeyOfEn);
            }
            #endregion

            #region 过滤条件
            Dictionary<string, string> kvs = ur.GetVals();
            foreach (MapAttr attr1 in attrs)
            {
                Attr attr = attr1.HisAttr;
                //此处做判断，如果在URL中已经传了参数，则不算SearchAttrs中的设置
                if (keys.Contains(attr.Key))
                    continue;

                if (attr.MyFieldType == FieldType.RefText)
                    continue;

                string selectVal = string.Empty;
                string cid = string.Empty;

                switch (attr.UIContralType)
                {

                    case UIContralType.DDL:
                    case UIContralType.RadioBtn:
                        cid = attr.Key;

                        if (kvs.ContainsKey(cid) == false || string.IsNullOrWhiteSpace(kvs[cid]))
                            continue;

                        selectVal = kvs[cid];

                        if (selectVal == "all" || selectVal == "-1")
                            continue;

                        qo.addAnd();

                        qo.addLeftBracket();


                        string deptName = BP.Sys.Base.Glo.DealClassEntityName("BP.Port.Depts");

                        if (attr.UIBindKey.Equals(deptName) == true)  //判断特殊情况。
                            qo.AddWhere(attr.Key, " LIKE ", selectVal + "%");
                        else
                            qo.AddWhere(attr.Key, selectVal);

                        qo.addRightBracket();
                        break;

                    default:
                        break;
                }
            }
            #endregion

            #region 日期处理
            if ((DTSearchWay)ur.GetParaInt("DTSearchWay") != DTSearchWay.None)
            {
                string dtKey = ur.GetParaString("DTSearchKey");
                string dtFrom = ur.GetValStringByKey(UserRegeditAttr.DTFrom).Trim();
                string dtTo = ur.GetValStringByKey(UserRegeditAttr.DTTo).Trim();

                if (DataType.IsNullOrEmpty(dtFrom) == true)
                {
                    if (mapData.DTSearchWay == DTSearchWay.ByDate)
                        dtFrom = "1900-01-01";
                    else
                        dtFrom = "1900-01-01 00:00";
                }

                if (DataType.IsNullOrEmpty(dtTo) == true)
                {
                    if ((DTSearchWay)ur.GetParaInt("DTSearchWay") == DTSearchWay.ByDate)
                        dtTo = "2999-01-01";
                    else
                        dtTo = "2999-12-31 23:59";
                }

                if ((DTSearchWay)ur.GetParaInt("DTSearchWay") == DTSearchWay.ByDate)
                {

                    qo.addAnd();
                    qo.addLeftBracket();
                    qo.SQL = dtKey + " >= '" + dtFrom + "'";
                    qo.addAnd();
                    qo.SQL = dtKey + " <= '" + dtTo + "'";
                    qo.addRightBracket();
                }

                if ((DTSearchWay)ur.GetParaInt("DTSearchWay") == DTSearchWay.ByDateTime)
                {

                    qo.addAnd();
                    qo.addLeftBracket();
                    qo.SQL = dtKey + " >= '" + dtFrom + " 00:00'";
                    qo.addAnd();
                    qo.SQL = dtKey + " <= '" + dtTo + " 23:59'";
                    qo.addRightBracket();
                }
            }
            #endregion 日期处理
            qo.AddWhere(" AND  BillState!=0 ");
            if (DataType.IsNullOrEmpty(ur.OrderBy) == false)
                if (ur.OrderWay.ToUpper().Equals("DESC") == true)
                    qo.addOrderByDesc(ur.OrderBy);
                else
                    qo.addOrderBy(ur.OrderBy);
           
            return qo;
        }

        /// <summary>
        ///获取分组的外键、枚举
        /// </summary>
        /// <returns></returns>
        public string Group_ContentAttrs()
        {
            //获得
            Entities ens = ClassFactory.GetEns(this.FrmID);
            if (ens == null)
                return "err@参数FrnID不能为空";

            Entity en = ens.GetNewEntity;
            Map map = ens.GetNewEntity.EnMapInTime;
            Attrs attrs = map.Attrs;
            DataTable dt = new DataTable();
            dt.Columns.Add("Field");
            dt.Columns.Add("Name");
            dt.Columns.Add("Checked");
            dt.TableName = "Attrs";

            //获取注册信心表
            UserRegedit ur = new UserRegedit(WebUser.No, this.FrmID + "_Group");

            //判断是否已经选择分组
            bool contentFlag = false;
            foreach (Attr attr in attrs)
            {
                if (attr.UIContralType == UIContralType.DDL || attr.UIContralType == UIContralType.RadioBtn)
                {
                    DataRow dr = dt.NewRow();
                    dr["Field"] = attr.Key;
                    dr["Name"] = attr.Desc;

                    // 根据状态 设置信息.
                    if (ur.Vals.IndexOf(attr.Key) != -1)
                    {
                        dr["Checked"] = "true";
                        contentFlag = true;
                    }
                    dt.Rows.Add(dr);
                }

            }

            if (contentFlag == false && dt.Rows.Count != 0)
                dt.Rows[0]["Checked"] = "true";

            return BP.Tools.Json.ToJson(dt);
        }

        public string Group_Analysis()
        {
            //获得
            Entities ens = ClassFactory.GetEns(this.FrmID);
            if (ens == null)
                return "err@类名错误:" + this.FrmID;

            Entity en = ens.GetNewEntity;
            Map map = ens.GetNewEntity.EnMapInTime;
            DataSet ds = new DataSet();


            //获取注册信息表
            UserRegedit ur = new UserRegedit(WebUser.No, this.FrmID + "_Group");

            DataTable dt = new DataTable();
            dt.Columns.Add("Field");
            dt.Columns.Add("Name");
            dt.Columns.Add("Checked");

            dt.TableName = "Attrs";

            //默认手动添加一个求数量的分析项
            DataRow dtr = dt.NewRow();
            dtr["Field"] = "Group_Number";
            dtr["Name"] = "数量";
            dtr["Checked"] = "true";
            dt.Rows.Add(dtr);

            DataTable ddlDt = new DataTable();
            ddlDt.TableName = "Group_Number";
            ddlDt.Columns.Add("No");
            ddlDt.Columns.Add("Name");
            ddlDt.Columns.Add("Selected");
            DataRow ddlDr = ddlDt.NewRow();
            ddlDr["No"] = "SUM";
            ddlDr["Name"] = "求和";
            ddlDr["Selected"] = "true";
            ddlDt.Rows.Add(ddlDr);
            ds.Tables.Add(ddlDt);

            foreach (Attr attr in map.Attrs)
            {
                if (attr.IsPK || attr.IsNum == false)
                    continue;
                if (attr.UIContralType != UIContralType.TB)
                    continue;
                if (attr.UIVisible == false)
                    continue;
                if (attr.MyFieldType == FieldType.FK)
                    continue;
                if (attr.MyFieldType == FieldType.Enum)
                    continue;
                if (attr.Key == "OID" || attr.Key == "WorkID" || attr.Key == "MID")
                    continue;



                dtr = dt.NewRow();
                dtr["Field"] = attr.Key;
                dtr["Name"] = attr.Desc;


                // 根据状态 设置信息.
                if (ur.Vals.IndexOf(attr.Key) != -1)
                    dtr["Checked"] = "true";

                dt.Rows.Add(dtr);

                ddlDt = new DataTable();
                ddlDt.Columns.Add("No");
                ddlDt.Columns.Add("Name");
                ddlDt.Columns.Add("Selected");
                ddlDt.TableName = attr.Key;

                ddlDr = ddlDt.NewRow();
                ddlDr["No"] = "SUM";
                ddlDr["Name"] = "求和";
                if (ur.Vals.IndexOf("@" + attr.Key + "=SUM") != -1)
                    ddlDr["Selected"] = "true";
                ddlDt.Rows.Add(ddlDr);

                ddlDr = ddlDt.NewRow();
                ddlDr["No"] = "AVG";
                ddlDr["Name"] = "求平均";
                if (ur.Vals.IndexOf("@" + attr.Key + "=AVG") != -1)
                    ddlDr["Selected"] = "true";
                ddlDt.Rows.Add(ddlDr);

                if (this.IsContainsNDYF)
                {
                    ddlDr = ddlDt.NewRow();
                    ddlDr["No"] = "AMOUNT";
                    ddlDr["Name"] = "求累计";
                    if (ur.Vals.IndexOf("@" + attr.Key + "=AMOUNT") != -1)
                        ddlDr["Selected"] = "true";
                    ddlDt.Rows.Add(ddlDr);
                }

                ds.Tables.Add(ddlDt);


            }

            ds.Tables.Add(dt);
            return BP.Tools.Json.ToJson(ds);
        }

        public string Group_Search()
        {
            //获得
            Entities ens = ClassFactory.GetEns(this.FrmID);
            if (ens == null)
                return "err@类名错误:" + this.FrmID;

            Entity en = ens.GetNewEntity;
            Map map = ens.GetNewEntity.EnMapInTime;
            DataSet ds = new DataSet();

            //获取注册信息表
            UserRegedit ur = new UserRegedit(WebUser.No, this.FrmID + "_Group");

            // 查询出来关于它的活动列配置.
            ActiveAttrs aas = new ActiveAttrs();
            aas.RetrieveBy(ActiveAttrAttr.For, this.FrmID);

            ds = GroupSearchSet(ens, en, map, ur, ds, aas);
            if (ds == null)
                return "info@<img src='../Img/Warning.gif' /><b><font color=red> 您没有选择显示内容/分析项目</font></b>";

            return BP.Tools.Json.ToJson(ds);
        }

        private DataSet GroupSearchSet(Entities ens, Entity en, Map map, UserRegedit ur, DataSet ds, ActiveAttrs aas)
        {

            //查询条件
            //分组
            string Condition = ""; //处理特殊字段的条件问题。

            AtPara atPara = new AtPara(ur.Vals);
            //获取分组的条件
            string groupKey = atPara.GetValStrByKey("SelectedGroupKey");
            //分析项
            string analyKey = atPara.GetValStrByKey("StateNumKey");

            //设置显示的列
            Attrs mapAttrOfShows = new Attrs();

            //查询语句定义
            string sql = "";
            string selectSQL = "SELECT ";  //select部分的组合
            string groupBySQL = " GROUP BY "; //分组的组合

            #region SelectSQL语句的组合

            #region 分组条件的整合
            if (DataType.IsNullOrEmpty(groupKey) == false)
            {
                bool isSelected = false;
                string[] SelectedGroupKeys = groupKey.Split(',');
                foreach (string key in SelectedGroupKeys)
                {
                    if (DataType.IsNullOrEmpty(key) == true)
                        continue;
                    Attr attr = map.GetAttrByKey(key);
                    // 加入组里面。
                    mapAttrOfShows.Add(map.GetAttrByKey(key));

                    selectSQL += key + " \"" + key + "\",";

                    groupBySQL += key + ",";

                    if (attr.MyFieldType == FieldType.FK)
                    {
                        Map fkMap = attr.HisFKEn.EnMap;
                        string refText = fkMap.PhysicsTable + "_" + attr.Key + "." + fkMap.GetFieldByKey(attr.UIRefKeyText);
                        selectSQL += refText + "  AS " + key + "Text" + ",";
                        groupBySQL += refText + ",";
                        continue;
                    }

                    if (attr.MyFieldType == FieldType.Enum || attr.MyFieldType == FieldType.PKEnum)
                    {
                        //增加枚举字段
                        if (DataType.IsNullOrEmpty(attr.UIBindKey))
                            throw new Exception("@" + en.ToString() + " key=" + attr.Key + " UITag=" + attr.UITag + "");

                        BP.Sys.SysEnums ses = new BP.Sys.SysEnums(attr.UIBindKey, attr.UITag);
                        selectSQL += ses.GenerCaseWhenForOracle(en.EnMap.PhysicsTable + ".", attr.Key, attr.Field, attr.UIBindKey, int.Parse(attr.DefaultVal.ToString())) + ",";
                        continue;
                    }

                    //不是外键、枚举，就是外部数据源
                    selectSQL += key + "T" + " \"" + key + "T\",";




                }
            }
            #endregion 分组条件的整合

            #region 分析项的整合
            Attrs AttrsOfNum = new Attrs();
            string[] analyKeys = analyKey.Split(',');
            foreach (string key in analyKeys)
            {
                if (DataType.IsNullOrEmpty(key) == true)
                    continue;
                string[] strs = key.Split('=');
                if (strs.Length != 2)
                    continue;

                //求数据的总和
                if (strs[0].Equals("Group_Number"))
                {
                    selectSQL += " count(*) \"" + strs[0] + "\",";
                    mapAttrOfShows.Add(new Attr("Group_Number", "Group_Number", 1, DataType.AppInt, false, "数量(合计)"));
                    AttrsOfNum.Add(new Attr("Group_Number", "Group_Number", 1, DataType.AppInt, false, "数量"));
                    continue;
                }

                //判断分析项的数据类型
                Attr attr = map.GetAttrByKey(strs[0]);
                AttrsOfNum.Add(attr);

                int dataType = attr.MyDataType;
                switch (strs[1])
                {
                    case "SUM":
                        if (dataType == 2)
                            selectSQL += " SUM(" + strs[0] + ") \"" + strs[0] + "\",";
                        else
                        {
                            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL)
                                selectSQL += " round ( cast (SUM(" + strs[0] + ") as  numeric), 4)  \"" + strs[0] + "\",";
                            else
                                selectSQL += " round ( SUM(" + strs[0] + "), 4) \"" + strs[0] + "\",";
                        }
                        attr.Desc = attr.Desc + "(合计)";

                        break;
                    case "AVG":
                        if (BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL)
                            selectSQL += " round ( cast (AVG(" + strs[0] + ") as  numeric), 4)  \"" + strs[0] + "\",";
                        else
                            selectSQL += " round (AVG(" + strs[0] + "), 4)  \"" + strs[0] + "\",";
                        attr.Desc = attr.Desc + "(平均)";
                        break;
                    case "AMOUNT":
                        if (dataType == 2)
                            selectSQL += " SUM(" + strs[0] + ") \"" + strs[0] + "\",";
                        else
                        {
                            if (BP.Difference.SystemConfig.AppCenterDBType == DBType.PostgreSQL)
                                selectSQL += " round ( cast (SUM(" + strs[0] + ") as  numeric), 4)  \"" + strs[0] + "\",";
                            else
                                selectSQL += " round ( SUM(" + strs[0] + "), 4) \"" + strs[0] + "\",";
                        }
                        attr.Desc = attr.Desc + "(累计)";
                        break;
                    default:
                        throw new Exception("没有判断的情况.");
                }
                mapAttrOfShows.Add(attr);

            }
            #endregion 分析项的整合

            selectSQL = selectSQL.Substring(0, selectSQL.Length - 1);

            #endregion SelectSQL语句的组合

            #region WhereSQL语句的组合

            //获取查询的注册表
            BP.Sys.UserRegedit searchUr = new UserRegedit();
            searchUr.setMyPK(WebUser.No + this.FrmID + "_SearchAttrs");
            searchUr.RetrieveFromDBSources();

           QueryObject qo = FrmSearch_QuerySQL(ens as GEEntitys, searchUr);

            string whereSQL = " " + qo.SQL.Substring(qo.SQL.IndexOf("FROM "));

            #endregion WhereSQL语句的组合

            #region OrderBy语句组合

            string orderbySQL = "";
            string orderByKey = this.GetRequestVal("OrderBy");
            if (DataType.IsNullOrEmpty(orderByKey) == false && selectSQL.Contains(orderByKey) == true)
            {
                orderbySQL = " ORDER BY" + orderByKey;
                string orderWay = this.GetRequestVal("OrderWay");
                if (DataType.IsNullOrEmpty(orderWay) == false && orderWay.Equals("Up") == false)
                    orderbySQL += " DESC ";

            }

            #endregion OrderBy语句组合
            sql = selectSQL + whereSQL + groupBySQL.Substring(0, groupBySQL.Length - 1) + orderbySQL;

            DataTable dt = DBAccess.RunSQLReturnTable(sql, qo.MyParas);
            dt.TableName = "MainData";

            ds.Tables.Add(dt);
            ds.Tables.Add(mapAttrOfShows.ToMapAttrs.ToDataTableField("Sys_MapAttr"));
            ds.Tables.Add(AttrsOfNum.ToMapAttrs.ToDataTableField("AttrsOfNum"));

            return ds;
        }

        public string FrmSearch_DeleteFrmByOID()
        {
            int workModel = GetRequestValInt("WorkModel");
            //删除实体
            if(workModel==2)
                return BP.CCBill.Dev2Interface.MyDict_Delete(this.FrmID, this.WorkID);
            //删除单据
            if (workModel == 3)
                return BP.CCBill.Dev2Interface.MyBill_Delete(this.FrmID, this.WorkID);
            return "err@参数WorkModel类型不正确";
        }
    }
}
