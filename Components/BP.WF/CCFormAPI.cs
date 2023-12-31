﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BP.WF;
using BP.DA;
using BP.Port;
using BP.Web;
using BP.WF.Template;
using BP.En;
using BP.Sys;
using BP.Difference;
using BP.WF.Template.Frm;
using System.Text.RegularExpressions;

namespace BP.WF
{
    /// <summary>
    /// 表单引擎api
    /// </summary>
    public class CCFormAPI : Dev2Interface
    {
        /// <summary>
        /// 生成报表
        /// </summary>
        /// <param name="templeteFilePath">模版路径</param>
        /// <param name="ds">数据源</param>
        /// <returns>生成单据的路径</returns>
        public static void Frm_GenerBill(string templeteFullFile, string saveToDir, string saveFileName,
            PrintFileType fileType, DataSet ds, string fk_mapData)
        {
            MapData md = new MapData(fk_mapData);
            GEEntity entity = md.GenerGEEntityByDataSet(ds);

            BP.Pub.RTFEngine rtf = new BP.Pub.RTFEngine();
            rtf.HisEns.Clear();
            rtf.EnsDataDtls.Clear();

            rtf.HisEns.AddEntity(entity);
            var dtls = entity.Dtls;

            foreach (var item in dtls)
                rtf.EnsDataDtls.Add(item);

            rtf.MakeDoc(templeteFullFile, saveToDir, saveFileName);
        }

        /// <summary>
        /// 仅获取表单数据
        /// </summary>
        /// <param name="enName"></param>
        /// <param name="pkval"></param>
        /// <param name="atParas"></param>
        /// <param name="specDtlFrmID"></param>
        /// <returns></returns>
        private static DataSet GenerDBForVSTOExcelFrmModelOfEntity(string enName, object pkval, string atParas, string specDtlFrmID = null)
        {
            DataSet myds = new DataSet();

            // 创建实体..
            #region 主表

            Entity en = BP.En.ClassFactory.GetEn(enName);
            en.PKVal = pkval;

            // if (DataType.IsNullOrEmpty(pkval)==false)
            en.Retrieve();


            //设置外部传入的默认值.
            if (BP.Difference.SystemConfig.IsBSsystem == true)
            {
                // 处理传递过来的参数。
                //2019-07-25 zyt改造
                foreach (string k in HttpContextHelper.RequestParamKeys)
                {
                    en.SetValByKey(k, HttpContextHelper.RequestParams(k));
                }
            }

            //主表数据放入集合.
            DataTable mainTable = en.ToDataTableField();
            mainTable.TableName = "MainTable";
            myds.Tables.Add(mainTable);

            #region 主表 Sys_MapData
            string sql = "SELECT * FROM Sys_MapData WHERE 1=2 ";
            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "Sys_MapData";

            Map map = en.EnMapInTime;
            DataRow dr = dt.NewRow();
            dr[MapDataAttr.No] = enName;
            dr[MapDataAttr.Name] = map.EnDesc;
            dr[MapDataAttr.PTable] = map.PhysicsTable;
            dt.Rows.Add(dr);
            myds.Tables.Add(dt);
            #endregion 主表 Sys_MapData

            #region 主表 Sys_MapAttr
            sql = "SELECT * FROM Sys_MapAttr WHERE 1=2 ";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "Sys_MapAttr";
            foreach (Attr attr in map.Attrs)
            {
                dr = dt.NewRow();
                dr[MapAttrAttr.MyPK] = enName + "_" + attr.Key;
                dr[MapAttrAttr.Name] = attr.Desc;

                dr[MapAttrAttr.MyDataType] = attr.MyDataType;   //数据类型.
                dr[MapAttrAttr.MinLen] = attr.MinLength;   //最小长度.
                dr[MapAttrAttr.MaxLen] = attr.MaxLength;   //最大长度.

                // 设置他的逻辑类型.
                dr[MapAttrAttr.LGType] = 0; //逻辑类型.
                switch (attr.MyFieldType)
                {
                    case FieldType.Enum:
                        dr[MapAttrAttr.LGType] = 1;
                        dr[MapAttrAttr.UIBindKey] = attr.UIBindKey;

                        //增加枚举字段.
                        if (myds.Tables.Contains(attr.UIBindKey) == false)
                        {
                            string mysql = "SELECT IntKey AS No, Lab as Name FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey='" + attr.UIBindKey + "' ORDER BY IntKey ";
                            DataTable dtEnum = DBAccess.RunSQLReturnTable(mysql);
                            dtEnum.TableName = attr.UIBindKey;
                            myds.Tables.Add(dtEnum);
                        }

                        break;
                    case FieldType.FK:
                        dr[MapAttrAttr.LGType] = 2;

                        Entities ens = attr.HisFKEns;
                        dr[MapAttrAttr.UIBindKey] = ens.ToString();

                        //把外键字段也增加进去.
                        if (myds.Tables.Contains(ens.ToString()) == false && attr.UIIsReadonly == false)
                        {
                            ens.RetrieveAll();
                            DataTable mydt = ens.ToDataTableDescField();
                            mydt.TableName = ens.ToString();
                            myds.Tables.Add(mydt);
                        }
                        break;
                    default:
                        break;
                }

                // 设置控件类型.
                dr[MapAttrAttr.UIContralType] = (int)attr.UIContralType;
                dt.Rows.Add(dr);
            }
            myds.Tables.Add(dt);
            #endregion 主表 Sys_MapAttr

            #region //主表 Sys_MapExt 扩展属性
            ////主表的配置信息.
            //sql = "SELECT * FROM Sys_MapExt WHERE 1=2";
            //dt = DBAccess.RunSQLReturnTable(sql);
            //dt.TableName = "Sys_MapExt";
            //myds.Tables.Add(dt);
            #endregion //主表 Sys_MapExt 扩展属性

            #endregion

            #region 从表
            foreach (EnDtl item in map.Dtls)
            {
                #region  把从表的数据放入集合.

                Entities dtls = item.Ens;

                QueryObject qo = qo = new QueryObject(dtls);

                if (dtls.ToString().Contains("CYSheBeiUse") == true)
                    qo.addOrderBy("RDT"); //按照日期进行排序，不用也可以.

                qo.AddWhere(item.RefKey, pkval);
                DataTable dtDtl = qo.DoQueryToTable();

                dtDtl.TableName = item.EnsName; //修改明细表的名称.
                myds.Tables.Add(dtDtl); //加入这个明细表.

                #endregion 把从表的数据放入.

                #region 从表 Sys_MapDtl (相当于mapdata)

                Entity dtl = dtls.GetNewEntity;
                map = dtl.EnMap;

                //明细表的 描述 . 
                sql = "SELECT * FROM Sys_MapDtl WHERE 1=2";
                dt = DBAccess.RunSQLReturnTable(sql);
                dt.TableName = "Sys_MapDtl_For_" + item.EnsName;

                dr = dt.NewRow();
                dr[MapDtlAttr.No] = item.EnsName;
                dr[MapDtlAttr.Name] = item.Desc;
                dr[MapDtlAttr.PTable] = dtl.EnMap.PhysicsTable;
                dt.Rows.Add(dr);
                myds.Tables.Add(dt);

                #endregion 从表 Sys_MapDtl (相当于mapdata)

                #region 明细表 Sys_MapAttr
                sql = "SELECT * FROM Sys_MapAttr WHERE 1=2";
                dt = DBAccess.RunSQLReturnTable(sql);
                dt.TableName = "Sys_MapAttr_For_" + item.EnsName;
                foreach (Attr attr in map.Attrs)
                {
                    dr = dt.NewRow();
                    dr[MapAttrAttr.MyPK] = enName + "_" + attr.Key;
                    dr[MapAttrAttr.Name] = attr.Desc;

                    dr[MapAttrAttr.MyDataType] = attr.MyDataType;   //数据类型.
                    dr[MapAttrAttr.MinLen] = attr.MinLength;   //最小长度.
                    dr[MapAttrAttr.MaxLen] = attr.MaxLength;   //最大长度.

                    // 设置他的逻辑类型.
                    dr[MapAttrAttr.LGType] = 0; //逻辑类型.
                    switch (attr.MyFieldType)
                    {
                        case FieldType.Enum:
                            dr[MapAttrAttr.LGType] = 1;
                            dr[MapAttrAttr.UIBindKey] = attr.UIBindKey;

                            //增加枚举字段.
                            if (myds.Tables.Contains(attr.UIBindKey) == false)
                            {
                                string mysql = "SELECT IntKey AS No, Lab as Name FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey='" + attr.UIBindKey + "' ORDER BY IntKey ";
                                DataTable dtEnum = DBAccess.RunSQLReturnTable(mysql);
                                dtEnum.TableName = attr.UIBindKey;
                                myds.Tables.Add(dtEnum);
                            }
                            break;
                        case FieldType.FK:
                            dr[MapAttrAttr.LGType] = 2;

                            Entities ens = attr.HisFKEns;
                            dr[MapAttrAttr.UIBindKey] = ens.ToString();

                            //把外键字段也增加进去.
                            if (myds.Tables.Contains(ens.ToString()) == false && attr.UIIsReadonly == false)
                            {
                                ens.RetrieveAll();
                                DataTable mydt = ens.ToDataTableDescField();
                                mydt.TableName = ens.ToString();
                                myds.Tables.Add(mydt);
                            }
                            break;
                        default:
                            break;
                    }

                    // 设置控件类型.
                    dr[MapAttrAttr.UIContralType] = (int)attr.UIContralType;
                    dt.Rows.Add(dr);
                }
                myds.Tables.Add(dt);
                #endregion 明细表 Sys_MapAttr

            }
            #endregion

            return myds;
        }
        /// <summary>
        /// 仅获取表单数据
        /// </summary>
        /// <param name="frmID">表单ID</param>
        /// <param name="pkval">主键</param>
        /// <param name="atParas">参数</param>
        /// <param name="specDtlFrmID">指定明细表的参数，如果为空就标识主表数据，否则就是从表数据.</param>
        /// <returns>数据</returns>
        public static DataSet GenerDBForVSTOExcelFrmModel(string frmID, object pkval, string atParas, string specDtlFrmID = null)
        {
            //如果是一个实体类.
            if (frmID.ToUpper().Contains("BP."))
            {
                // 执行map同步.
                Entities ens = BP.En.ClassFactory.GetEns(frmID + "s");
                Entity myen = ens.GetNewEntity;
                myen.DTSMapToSys_MapData();
                return GenerDBForVSTOExcelFrmModelOfEntity(frmID, pkval, atParas, specDtlFrmID = null);

                //上面这行代码的解释（2017-04-25）：
                //若不加上这行，代码执行到“ MapData md = new MapData(frmID); ”会报错：
                //@没有找到记录[表单注册表  Sys_MapData, [ 主键=No 值=BP.LI.BZQX ]记录不存在,请与管理员联系, 或者确认输入错误.@在Entity(BP.Sys.MapData)查询期间出现错误@   在 BP.En.Entity.Retrieve() 位置 D:\ccflow\Components\BP.En30\En\Entity.cs:行号 1051
                //即使加上：
                //frmID = frmID.Substring(0, frmID.Length - 1);
                //也会出现该问题
                //2017-04-25 15:26:34：new MapData(frmID)应传入“BZQX”，但考虑到 GenerDBForVSTOExcelFrmModelOfEntity()运行稳定，暂不采用『统一执行下方代码』的方案。
            }

            //数据容器,就是要返回的对象.
            DataSet myds = new DataSet();

            //映射实体.
            MapData md = new MapData(frmID);

            Map map = md.GenerHisMap();

            Entity en = null;
            if (map.Attrs.Contains("OID") == true)
            {
                //实体.
                GEEntity wk = new GEEntity(frmID);
                wk.OID = int.Parse(pkval.ToString());
                if (wk.RetrieveFromDBSources() == 0)
                    wk.Insert();

                ExecEvent.DoFrm(md,EventListFrm.FrmLoadBefore, wk, null);

                en = wk;
            }

            if (map.Attrs.Contains("MyPK") == true)
            {
                //实体.
                GEEntityMyPK wk = new GEEntityMyPK(frmID);
                wk.setMyPK(pkval.ToString());
                if (wk.RetrieveFromDBSources() == 0)
                    wk.Insert();
                ExecEvent.DoFrm(md,EventListFrm.FrmLoadBefore, wk, null);
                en = wk;
            }

            //加载事件.

            //把参数放入到 En 的 Row 里面。
            if (DataType.IsNullOrEmpty(atParas) == false)
            {
                AtPara ap = new AtPara(atParas);
                foreach (string key in ap.HisHT.Keys)
                {
                    switch(key)
                    {
                        case "FrmID":
                        case "FK_MapData":
                            continue;
                        default:
                            break;
                    }

                    if (en.Row.ContainsKey(key) == true) //有就该变.
                        en.Row[key] = ap.GetValStrByKey(key);
                    else
                        en.Row.Add(key, ap.GetValStrByKey(key)); //增加他.
                }
            }

            //属性.
            MapExt me = null;
            DataTable dtMapAttr = null;
            MapExts mes = null;

            #region 表单模版信息.（含主、从表的，以及从表的枚举/外键相关数据）.
            //增加表单字段描述.
            string sql = "SELECT * FROM Sys_MapData WHERE No='" + frmID + "' ";
            DataTable dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "Sys_MapData";
            myds.Tables.Add(dt);

            //增加表单字段描述.
            sql = "SELECT * FROM Sys_MapAttr WHERE FK_MapData='" + frmID + "' ";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "Sys_MapAttr";
            myds.Tables.Add(dt);

            //增加从表信息.
            sql = "SELECT * FROM Sys_MapDtl WHERE FK_MapData='" + frmID + "' ";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "Sys_MapDtl";
            myds.Tables.Add(dt);


            //主表的配置信息.
            sql = "SELECT * FROM Sys_MapExt WHERE FK_MapData='" + frmID + "'";
            dt = DBAccess.RunSQLReturnTable(sql);
            dt.TableName = "Sys_MapExt";
            myds.Tables.Add(dt);

            #region 加载 从表表单模版信息.（含 从表的枚举/外键相关数据）
            foreach (MapDtl item in md.MapDtls)
            {
                #region 返回指定的明细表的数据.
                if (DataType.IsNullOrEmpty(specDtlFrmID) == true)
                {
                }
                else
                {
                    if (item.No != specDtlFrmID)
                        continue;
                }
                #endregion 返回指定的明细表的数据.

                //明细表的主表描述
                sql = "SELECT * FROM Sys_MapDtl WHERE No='" + item.No + "'";
                dt = DBAccess.RunSQLReturnTable(sql);
                dt.TableName = "Sys_MapDtl_For_" + (string.IsNullOrWhiteSpace(item.Alias) ? item.No : item.Alias);
                myds.Tables.Add(dt);

                //明细表的表单描述
                sql = "SELECT * FROM Sys_MapAttr WHERE FK_MapData='" + item.No + "'";
                dtMapAttr = DBAccess.RunSQLReturnTable(sql);
                dtMapAttr.TableName = "Sys_MapAttr_For_" + (string.IsNullOrWhiteSpace(item.Alias) ? item.No : item.Alias);
                myds.Tables.Add(dtMapAttr);

                //明细表的配置信息.
                sql = "SELECT * FROM Sys_MapExt WHERE FK_MapData='" + item.No + "'";
                dt = DBAccess.RunSQLReturnTable(sql);
                dt.TableName = "Sys_MapExt_For_" + (string.IsNullOrWhiteSpace(item.Alias) ? item.No : item.Alias);
                myds.Tables.Add(dt);

                #region 从表的 外键表/枚举
                mes = new MapExts(item.No);
                foreach (DataRow dr in dtMapAttr.Rows)
                {
                    string lgType = dr["LGType"].ToString();
                    //不是枚举/外键字段
                    if (lgType.Equals("0"))
                        continue;

                    string uiBindKey = dr["UIBindKey"].ToString();
                    var mypk = dr["MyPK"].ToString();

                    #region 枚举字段
                    if (lgType.Equals("1"))
                    {
                        // 如果是枚举值, 判断是否存在.
                        if (myds.Tables.Contains(uiBindKey) == true)
                            continue;

                        string mysql = "SELECT IntKey AS No, Lab as Name FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey='" + uiBindKey + "' ORDER BY IntKey ";
                        DataTable dtEnum = DBAccess.RunSQLReturnTable(mysql);
                        dtEnum.TableName = uiBindKey;
                        myds.Tables.Add(dtEnum);
                        continue;
                    }
                    #endregion

                    string UIIsEnable = dr["UIIsEnable"].ToString();
                    if (UIIsEnable.Equals("0")) //字段未启用
                        continue;

                    #region 外键字段
                    // 检查是否有下拉框自动填充。
                    string keyOfEn = dr["KeyOfEn"].ToString();

                    #region 处理下拉框数据范围. for 小杨.
                    me = mes.GetEntityByKey(MapExtAttr.ExtType, MapExtXmlList.AutoFullDLL, MapExtAttr.AttrOfOper, keyOfEn) as MapExt;
                    if (me != null) //有范围限制时
                    {
                        string fullSQL = me.Doc.Clone() as string;
                        fullSQL = fullSQL.Replace("~", ",");
                        fullSQL = BP.WF.Glo.DealExp(fullSQL, en, null);

                        dt = DBAccess.RunSQLReturnTable(fullSQL);

                        dt.TableName = mypk;
                        myds.Tables.Add(dt);
                        continue;
                    }
                    #endregion 处理下拉框数据范围.
                    else //无范围限制时
                    {
                        // 判断是否存在.
                        if (myds.Tables.Contains(uiBindKey) == true)
                            continue;

                        myds.Tables.Add(BP.Pub.PubClass.GetDataTableByUIBineKey(uiBindKey));
                    }
                    #endregion 外键字段
                }
                #endregion 从表的 外键表/枚举

            }
            #endregion 加载 从表表单模版信息.（含 从表的枚举/外键相关数据）

            #endregion 表单模版信息.（含主、从表的，以及从表的枚举/外键相关数据）.

            #region 主表数据
            if (BP.Difference.SystemConfig.IsBSsystem == true)
            {
                // 处理传递过来的参数。
                foreach (string k in HttpContextHelper.RequestParamKeys)
                {
                    en.SetValByKey(k, HttpContextHelper.RequestParams(k));
                }
            }

            // 执行表单事件..
            string msg = ExecEvent.DoFrm(md,EventListFrm.FrmLoadBefore, en);
            if (DataType.IsNullOrEmpty(msg) == false)
                throw new Exception("err@错误:" + msg);

            //重设默认值.
            en.ResetDefaultVal();

            //执行装载填充.
            me = new MapExt();
            if (me.Retrieve(MapExtAttr.ExtType, MapExtXmlList.PageLoadFull, MapExtAttr.FK_MapData, frmID) == 1)
            {
                //执行通用的装载方法.
                MapAttrs attrs = new MapAttrs(frmID);
                MapDtls dtls = new MapDtls(frmID);
                en = BP.WF.Glo.DealPageLoadFull(en, me, attrs, dtls) as GEEntity;
            }

            //增加主表数据.
            DataTable mainTable = en.ToDataTableField(md.No);
            mainTable.TableName = "MainTable";
            myds.Tables.Add(mainTable);

            #endregion 主表数据

            #region  从表数据
            foreach (MapDtl dtl in md.MapDtls)
            {
                #region 返回指定的明细表的数据.
                if (DataType.IsNullOrEmpty(specDtlFrmID) == true)
                {
                }
                else
                {
                    if (dtl.No != specDtlFrmID)
                        continue;
                }
                #endregion 返回指定的明细表的数据.

                GEDtls dtls = new GEDtls(dtl.No);
                QueryObject qo = null;

                if (dtl.RefPK == "")
                {
                    try
                    {
                        qo = new QueryObject(dtls);
                        switch (dtl.DtlOpenType)
                        {
                            case DtlOpenType.ForEmp:  // 按人员来控制.
                                qo.AddWhere(GEDtlAttr.RefPK, pkval);
                                qo.addAnd();
                                qo.AddWhere(GEDtlAttr.Rec, WebUser.No);
                                break;
                            case DtlOpenType.ForWorkID: // 按工作ID来控制
                                qo.AddWhere(GEDtlAttr.RefPK, pkval);
                                break;
                            case DtlOpenType.ForPWorkID: // 按工作ID来控制
                                qo.AddWhere(GEDtlAttr.RefPK, DBAccess.RunSQLReturnValInt("SELECT PWorkID FROM WF_GenerWorkFlow WHERE WorkID="+ pkval) );
                                break;
                            case DtlOpenType.ForFID: // 按流程ID来控制.
                                qo.AddWhere(GEDtlAttr.FID, pkval);
                                break;
                        }
                    }
                    catch
                    {
                        dtls.GetNewEntity.CheckPhysicsTable();
                    }
                }
                else
                {
                    qo = new QueryObject(dtls);
                    qo.AddWhere(dtl.RefPK, pkval);
                }

                //条件过滤.
                if ( DataType.IsNullOrEmpty( dtl.FilterSQLExp)==false)
                {
                    string[] strs = dtl.FilterSQLExp.Split('=');
                    qo.addAnd();
                    qo.AddWhere(strs[0], strs[1]);
                }

                //排序.
                if (DataType.IsNullOrEmpty(dtl.OrderBySQLExp)==false)
                {
                    qo.addOrderBy(dtl.OrderBySQLExp);
                }


                //从表
                DataTable dtDtl = qo.DoQueryToTable();

                // 为明细表设置默认值.
                MapAttrs mattrs = new MapAttrs(dtl.No);
                foreach (MapAttr attr in mattrs)
                {
                    //处理它的默认值.
                    if (attr.DefValReal.Contains("@") == false)
                        continue;

                    foreach (DataRow dr in dtDtl.Rows)
                        dr[attr.KeyOfEn] = attr.DefVal;
                }

                dtDtl.TableName = string.IsNullOrWhiteSpace(dtl.Alias) ? dtl.No : dtl.Alias; //edited by liuxc,2017-10-10.如果有别名，则使用别名，没有则使用No
                myds.Tables.Add(dtDtl); //加入这个明细表, 如果没有数据，xml体现为空.
            }
            #endregion 从表数据

            #region 主表的 外键表/枚举
            dtMapAttr = myds.Tables["Sys_MapAttr"];
            mes = md.MapExts;
            foreach (DataRow dr in dtMapAttr.Rows)
            {
                string uiBindKey = dr["UIBindKey"] as string;
                if (DataType.IsNullOrEmpty(uiBindKey) == true)
                    continue;

                string myPK = dr["MyPK"].ToString();
                string lgType = dr["LGType"].ToString();

                if (lgType.Equals("1"))
                {
                    // 如果是枚举值, 判断是否存在., 
                    if (myds.Tables.Contains(uiBindKey) == true)
                        continue;

                    string mysql = "SELECT IntKey AS No, Lab as Name FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey='" + uiBindKey + "' ORDER BY IntKey ";
                    DataTable dtEnum = DBAccess.RunSQLReturnTable(mysql);
                    dtEnum.TableName = uiBindKey;
                    myds.Tables.Add(dtEnum);
                    continue;
                }

                if (lgType.Equals("2") == false)
                    continue;

                string UIIsEnable = dr["UIIsEnable"].ToString();
                if (UIIsEnable.Equals("0"))
                    continue;

                // 检查是否有下拉框自动填充。
                string keyOfEn = dr["KeyOfEn"].ToString();
                string fk_mapData = dr["FK_MapData"].ToString();

                #region 处理下拉框数据范围. for 小杨.
                me = mes.GetEntityByKey(MapExtAttr.ExtType, MapExtXmlList.AutoFullDLL, MapExtAttr.AttrOfOper, keyOfEn) as MapExt;
                if (me != null)
                {
                    string fullSQL = me.Doc.Clone() as string;
                    fullSQL = fullSQL.Replace("~", ",");
                    fullSQL = BP.WF.Glo.DealExp(fullSQL, en, null);
                    dt = DBAccess.RunSQLReturnTable(fullSQL);
                    dt.TableName = myPK; //可能存在隐患，如果多个字段，绑定同一个表，就存在这样的问题.
                    myds.Tables.Add(dt);
                    continue;
                }
                #endregion 处理下拉框数据范围.

                dt = BP.Pub.PubClass.GetDataTableByUIBineKey(uiBindKey);
                dt.TableName = uiBindKey;
                myds.Tables.Add(dt);
            }
            #endregion 主表的 外键表/枚举


            string name = "";
            foreach (DataTable item in myds.Tables)
            {
                name += item.TableName + ",";
            }
            //返回生成的dataset.
            return myds;
        }
        /// <summary>
        /// 获取从表数据，用于显示dtl.htm 
        /// </summary>
        /// <param name="frmID">表单ID</param>
        /// <param name="pkval">主键</param>
        /// <param name="atParas">参数</param>
        /// <param name="specDtlFrmID">指定明细表的参数，如果为空就标识主表数据，否则就是从表数据.</param>
        /// <returns>数据</returns>
        public static DataSet GenerDBForCCFormDtl(string frmID, MapDtl dtl, int pkval, string atParas,string dtlRefPKVal,Int64 fid)
        {
            //数据容器,就是要返回的对象.
            DataSet myds = new DataSet();

            //实体.
            GEEntity en = new GEEntity(frmID);
            en.OID = pkval;
            if (en.RetrieveFromDBSources() == 0)
                en.Insert();

            //把参数放入到 En 的 Row 里面。
            if (DataType.IsNullOrEmpty(atParas) == false)
            {
                AtPara ap = new AtPara(atParas);
                foreach (string key in ap.HisHT.Keys)
                {
                    try
                    {
                        if (en.Row.ContainsKey(key) == true) //有就该变.
                            en.Row[key] = ap.GetValStrByKey(key);
                        else
                            en.Row.Add(key, ap.GetValStrByKey(key)); //增加他.
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(key);
                    }
                }
            }
            if (BP.Difference.SystemConfig.IsBSsystem == true)
            {
                // 处理传递过来的参数。
                foreach (string k in HttpContextHelper.RequestParamKeys)
                {
                    en.SetValByKey(k, HttpContextHelper.RequestParams(k));
                }
            }


            #region 加载从表表单模版信息.

            DataTable Sys_MapDtl = dtl.ToDataTableField("Sys_MapDtl");
            myds.Tables.Add(Sys_MapDtl);

            //明细表的表单描述
            MapAttrs attrs = dtl.MapAttrs;
            DataTable Sys_MapAttr = attrs.ToDataTableField("Sys_MapAttr");
            myds.Tables.Add(Sys_MapAttr);

            //明细表的配置信息.
            MapExts mes = dtl.MapExts;
            DataTable Sys_MapExt = mes.ToDataTableField("Sys_MapExt");
            myds.Tables.Add(Sys_MapExt);

            //启用附件，增加附件信息
            DataTable Sys_FrmAttachment = dtl.FrmAttachments.ToDataTableField("Sys_FrmAttachment");
            myds.Tables.Add(Sys_FrmAttachment);
            #endregion 加载从表表单模版信息.

            #region 把从表的- 外键表/枚举 加入 DataSet.
            
            MapExt me = null;
            DataTable ddlTable = new DataTable();
            ddlTable.Columns.Add("No");
            foreach (MapAttr attr in attrs)
            {
               
                //没有绑定外键
                string uiBindKey = attr.UIBindKey;
                if (DataType.IsNullOrEmpty(uiBindKey) == true)
                    continue;

                #region 枚举字段
                if (attr.LGType == FieldTypeS.Enum)
                {
                    // 如果是枚举值, 判断是否存在.
                    if (myds.Tables.Contains(uiBindKey) == true)
                        continue;
                    string mysql = "SELECT IntKey AS No, Lab as Name FROM " + BP.Sys.Base.Glo.SysEnum() + " WHERE EnumKey='" + uiBindKey + "' ORDER BY IntKey ";
                    DataTable dtEnum = DBAccess.RunSQLReturnTable(mysql);
                    dtEnum.TableName = uiBindKey;

                    dtEnum.Columns[0].ColumnName = "No";
                    dtEnum.Columns[1].ColumnName = "Name";

                    myds.Tables.Add(dtEnum);
                    continue;
                }
                #endregion

                // 检查是否有下拉框自动填充。
                string keyOfEn = attr.KeyOfEn;

                #region 处理下拉框数据范围. for 小杨.
                me = mes.GetEntityByKey(MapExtAttr.ExtType, MapExtXmlList.AutoFullDLL,
                    MapExtAttr.AttrOfOper, keyOfEn) as MapExt;
                if (me != null && myds.Tables.Contains(uiBindKey) == false) //是否存在.
                {
                    string fullSQL = me.Doc.Clone() as string;
                    fullSQL = fullSQL.Replace("~", "'");
                    fullSQL = BP.WF.Glo.DealExp(fullSQL, en, null);

                    if (DataType.IsNullOrEmpty(fullSQL) == true)
                        throw new Exception("err@没有给AutoFullDLL配置SQL：MapExt：=" + me.MyPK+",原始的配置SQL为:"+me.Doc);

                    DataTable dt = DBAccess.RunSQLReturnTable(fullSQL);

                    dt.TableName = uiBindKey;

                    if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel == FieldCaseModel.UpperCase)
                    {
                        if (dt.Columns.Contains("NO") == true)
                            dt.Columns["NO"].ColumnName = "No";
                        if (dt.Columns.Contains("NAME") == true)
                            dt.Columns["NAME"].ColumnName = "Name";
                        if (dt.Columns.Contains("PARENTNO") == true)
                            dt.Columns["PARENTNO"].ColumnName = "ParentNo";
                    }

                    if (BP.Difference.SystemConfig.AppCenterDBFieldCaseModel == FieldCaseModel.Lowercase)
                    {
                        if (dt.Columns.Contains("no") == true)
                            dt.Columns["no"].ColumnName = "No";
                        if (dt.Columns.Contains("name") == true)
                            dt.Columns["name"].ColumnName = "Name";
                        if (dt.Columns.Contains("parentno") == true)
                            dt.Columns["parentno"].ColumnName = "ParentNo";
                    }

                    myds.Tables.Add(dt);
                    continue;
                }
                #endregion 处理下拉框数据范围.

                #region 外键字段

                // 判断是否存在.
                if (myds.Tables.Contains(uiBindKey) == true)
                    continue;

                // 获得数据.
                DataTable mydt = BP.Pub.PubClass.GetDataTableByUIBineKey(uiBindKey,en.Row);

                if (mydt == null)
                {
                    DataRow ddldr = ddlTable.NewRow();
                    ddldr["No"] = uiBindKey;
                    ddlTable.Rows.Add(ddldr);
                }
                else
                {
                    myds.Tables.Add(mydt);
                }
                #endregion 外键字段
            }
            ddlTable.TableName = "UIBindKey";
            myds.Tables.Add(ddlTable);
            #endregion 把从表的- 外键表/枚举 加入 DataSet.

            #region 把主表数据放入.
           
            //重设默认值.
            en.ResetDefaultVal();


            //增加主表数据.
            DataTable mainTable = en.ToDataTableField(frmID);
            mainTable.TableName = "MainTable";
            myds.Tables.Add(mainTable);
            #endregion 把主表数据放入.

            #region  把从表的数据放入.
            DataTable dtDtl = GetDtlInfo(dtl,en, dtlRefPKVal);
            //从表集合为空时填充从表的情况
            if (dtDtl.Rows.Count == 0)
            {
                GEDtl endtl = null;
                //1.行初始化字段，设置了改字段值时默认就添加枚举值集合的行数据，一般不再新增从表数据
                if (DataType.IsNullOrEmpty(dtl.InitDBAttrs) == false)
                {
                    string[] keys = dtl.InitDBAttrs.Split(',');
                   
                    MapAttr attr = null;
                    foreach (string keyOfEn in keys)
                    {
                        Entity ent = dtl.MapAttrs.GetEntityByKey(dtl.No + "_" + keyOfEn);
                        if (ent == null)
                            continue;
                        attr = ent as MapAttr;
                        if (DataType.IsNullOrEmpty(attr.UIBindKey) == true)
                            continue;
                        DataTable dt = null;
                        //枚举字段
                        if (attr.LGType == FieldTypeS.Enum && attr.MyDataType == DataType.AppInt)
                            dt = myds.Tables[attr.UIBindKey];
                        //外键、外部数据源
                        if ((attr.LGType == FieldTypeS.FK && attr.MyDataType == DataType.AppString)
                            || (attr.LGType == FieldTypeS.Normal && attr.MyDataType == DataType.AppString && attr.UIContralType == UIContralType.DDL))
                            dt = myds.Tables[attr.UIBindKey];
                        if (dt == null)
                            continue;
                        foreach (DataRow dr in dt.Rows)
                        {
                            endtl = new GEDtl(dtl.No);
                            endtl.ResetDefaultVal();
                            endtl.SetValByKey(keyOfEn, dr[0]);
                            endtl.RefPK = dtlRefPKVal;
                            endtl.FID = fid;
                            endtl.Insert();
                        }

                    }
                }
                //2.从表装载填充
                me = mes.GetEntityByKey("ExtModel", MapExtXmlList.PageLoadFullDtl) as MapExt;
                if (me != null && me.DoWay==1 && DataType.IsNullOrEmpty(me.Doc) ==false)
                {
                    string sql = Glo.DealSQLExp(me.Doc, en, null);
                    int num = Regex.Matches(sql.ToUpper(), "WHERE").Count;
                    if (num == 1)
                    {
                        string sqlext = sql.Substring(0, sql.ToUpper().IndexOf("WHERE"));
                        sqlext = sql.Substring(sqlext.Length + 1);
                        if (sqlext.Contains("@"))
                            throw new Exception("设置的sql有错误可能有没有替换的变量:" + sql);
                    }
                    if (num > 1 && sql.Contains("@"))
                        throw new Exception("设置的sql有错误可能有没有替换的变量:" + sql);
                    DataTable dt = DBAccess.RunSQLReturnTable(sql);
                    foreach (DataRow dr in dt.Rows)
                    {
                        endtl = new GEDtl(dtl.No);
                        foreach (DataColumn dc in dt.Columns)
                        {
                            endtl.SetValByKey(dc.ColumnName, dr[dc.ColumnName].ToString());
                        }
                        endtl.RefPK = dtlRefPKVal;
                        endtl.FID = fid;

                        endtl.RDT = DataType.CurrentDateTime;
                        endtl.Rec = WebUser.No;
                        endtl.Insert();
                    }
                }

               dtDtl = GetDtlInfo(dtl, en, dtlRefPKVal);
            }



            // 为明细表设置默认值.
            MapAttrs mattrs = new MapAttrs(dtl.No);
            foreach (MapAttr attr in mattrs)
            {
                if (attr.UIContralType == UIContralType.TB)
                    continue;

                //处理它的默认值.
                if (attr.DefValReal.Contains("@") == false)
                    continue;

                foreach (DataRow dr in dtDtl.Rows)
                {
                    if(dr[attr.KeyOfEn] == null || DataType.IsNullOrEmpty(dr[attr.KeyOfEn].ToString())==true)
                        dr[attr.KeyOfEn] = attr.DefVal;
                }
            }

            dtDtl.TableName = "DBDtl"; //修改明细表的名称.
            myds.Tables.Add(dtDtl); //加入这个明细表, 如果没有数据，xml体现为空.
            #endregion 把从表的数据放入.


            //放入一个空白的实体，用与获取默认值.
            GEDtl dtlBlank = new GEDtl(dtl.No);
            dtlBlank.ResetDefaultVal();

            myds.Tables.Add(dtlBlank.ToDataTableField("Blank"));

           //  myds.WriteXml("c:/xx.xml");

            return myds;
        }
        public static  DataTable GetDtlInfo(MapDtl dtl,GEEntity en,string dtlRefPKVal,bool isReload=false)
        {
            QueryObject qo = null;
            GEDtls dtls = new GEDtls(dtl.No);
            try
            {
                qo = new QueryObject(dtls);
                switch (dtl.DtlOpenType)
                {
                    case DtlOpenType.ForEmp:  // 按人员来控制.
                        qo.AddWhere(GEDtlAttr.RefPK, dtlRefPKVal);
                        qo.addAnd();
                        qo.AddWhere(GEDtlAttr.Rec, WebUser.No);
                        break;
                    case DtlOpenType.ForWorkID: // 按工作ID来控制

                        qo.AddWhere(GEDtlAttr.RefPK, dtlRefPKVal);

                        break;
                    case DtlOpenType.ForFID: // 按工作ID来控制
                        qo.AddWhere(GEDtlAttr.FID, dtlRefPKVal);
                        break;
                    default:
                        qo.AddWhere(GEDtlAttr.RefPK, dtlRefPKVal);
                        break;
                }

                //条件过滤.
                string exp = dtl.FilterSQLExp;
                if (DataType.IsNullOrEmpty(exp) == false)
                {
                    exp = Glo.DealExp(exp, en);
                    exp = exp.Replace("''", "'");

                    if (exp.Substring(0, 5).ToLower().Contains("and") == false)
                        exp = " AND " + exp;
                    qo.SQL = exp;
                }

                //排序.
                if (DataType.IsNullOrEmpty(dtl.OrderBySQLExp) == false)
                {
                    qo.addOrderBy(dtl.OrderBySQLExp);
                }
                else
                {
                    //增加排序.
                    qo.addOrderBy("Idx");
                }

                qo.DoQuery();
               
                return dtls.ToDataTableField();
            }
            catch (Exception ex)
            {
                dtl.IntMapAttrs();
                dtl.CheckPhysicsTable();
                CashFrmTemplate.Remove(dtl.No);
                Cash.SetMap(dtl.No, null);
                Cash.SQL_Cash.Remove(dtl.No);
                if (isReload == false)
                    return GetDtlInfo(dtl, en, dtlRefPKVal, true);
                else
                    throw new Exception("获取从表[" + dtl.Name + "]失败,错误:" + ex.Message);
            }
 
        }
    }

    
}
