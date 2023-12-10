using System;
using System.Collections.Generic;
using System.Data;
using BP.DA;
using BP.Web;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BP.Sys;
using BP.Difference;
using Microsoft.Extensions.Caching.Memory;

namespace BP.Cloud.HttpHandler
{
    /// <summary>
    /// 页面功能实体
    /// </summary>
    public class App_Org : BP.WF.HttpHandler.DirectoryPageBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App_Org()
        {
        }
        public static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        
        public string Template_Save()
        {
            var files = HttpContextHelper.RequestFiles();
            string ext = ".xls";
            string fileName = System.IO.Path.GetFileName(files[0].FileName);
            if (fileName.Contains(".xlsx"))
                ext = ".xlsx";

            //设置文件名
            string fileNewName = DateTime.Now.ToString("yyyyMMddHHmmssff") + ext;

            //文件存放路径
            string filePath = SystemConfig.PathOfTemp + "\\" + fileNewName;
            HttpContextHelper.UploadFile(files[0], filePath);

            //string filePath = @"D:\ccflow组织结构批量导入模板.xls";

            #region 获得数据源.
            var sheetNameList = BP.DA.DBLoad.GenerTableNames(filePath).ToList();
            if (sheetNameList.Count < 3 || sheetNameList.Contains("部门$") == false || sheetNameList.Contains("角色$") == false || sheetNameList.Contains("人员$") == false)
                throw new Exception("excel不符合要求");

            //获得部门数据.
            DataTable dtDept = BP.DA.DBLoad.ReadExcelFileToDataTable(filePath, sheetNameList.IndexOf("部门$"));
            for (int i = 0; i < dtDept.Columns.Count; i++)
            {
                string name = dtDept.Columns[i].ColumnName;
                name = name.Replace(" ", "");
                name = name.Replace("*", "");
                dtDept.Columns[i].ColumnName = name;
            }

            //获得角色数据.
            DataTable dtStation = BP.DA.DBLoad.ReadExcelFileToDataTable(filePath, sheetNameList.IndexOf("角色$"));
            for (int i = 0; i < dtStation.Columns.Count; i++)
            {
                string name = dtStation.Columns[i].ColumnName;
                name = name.Replace(" ", "");
                name = name.Replace("*", "");
                dtStation.Columns[i].ColumnName = name;
            }

            //获得人员数据.
            DataTable dtEmp = BP.DA.DBLoad.ReadExcelFileToDataTable(filePath, sheetNameList.IndexOf("人员$"));
            for (int i = 0; i < dtEmp.Columns.Count; i++)
            {
                string name = dtEmp.Columns[i].ColumnName;
                name = name.Replace(" ", "");
                name = name.Replace("*", "");
                dtEmp.Columns[i].ColumnName = name;
            }

            #endregion 获得数据源.

            #region 检查是否有根目录为 0 的数据?
            //检查数据的完整性.
            //1.检查是否有根目录为0的数据?
            var num = 0;
            bool isHave = false;
            foreach (DataRow dr in dtDept.Rows)
            {
                string str1 = dr[0] as string;
                if (DataType.IsNullOrEmpty(str1) == true)
                    continue;

                num++;
                string str = dr[1] as string;
                if (str == null || str.Equals(DBNull.Value))
                    return "err@导入出现数据错误:" + str1 + "的.上级部门名称-不能用空行的数据， 第[" + num + "]行数据.";

                if (str.Equals("0") == true || str.Equals("root") == true)
                {
                    isHave = true;
                    break;
                }
            }
            if (isHave == false)
                return "err@导入数据没有找到部门根目录节点.";
            #endregion 检查是否有根目录为0的数据

            #region 检查部门名称是否重复?
            string deptStrs = "";
            foreach (DataRow dr in dtDept.Rows)
            {
                string deptName = dr[0] as string;
                if (DataType.IsNullOrEmpty(deptName) == true)
                    continue;

                if (deptStrs.Contains("," + deptName + ",") == true)
                    return "err@部门名称:" + deptName + "重复.";

                //加起来..
                deptStrs += "," + deptName + ",";
            }
            #endregion 检查部门名称是否重复?

            #region 检查人员帐号是否重复?
            string emps = "";
            foreach (DataRow dr in dtEmp.Rows)
            {
                string empNo = dr[0] as string;
                if (DataType.IsNullOrEmpty(empNo) == true)
                    continue;

                if (emps.Contains("," + empNo + ",") == true)
                    return "err@人员帐号:" + empNo + "重复.";

                //加起来..
                emps += "," + empNo + ",";
            }
            #endregion 检查人员帐号是否重复?

            #region 检查角色名称是否重复?
            string staStrs = "";
            foreach (DataRow dr in dtStation.Rows)
            {
                string staName = dr[0] as string;
                if (DataType.IsNullOrEmpty(staName) == true)
                    continue;

                if (staStrs.Contains("," + staName + ",") == true)
                    return "err@角色名称:" + staName + "重复.";

                //加起来..
                staStrs += "," + staName + ",";
            }
            #endregion 检查角色名称是否重复?

            #region 检查人员的部门名称是否存在于部门数据里?
            int idx = 0;
            foreach (DataRow dr in dtEmp.Rows)
            {
                string emp = dr[0] as string;
                if (DataType.IsNullOrEmpty(emp) == true)
                    continue;

                idx++;
                //去的部门编号.
                string strs = dr["部门名称"] as string;
                if (DataType.IsNullOrEmpty(strs) == true)
                    return "err@第[" + idx + "]行,人员[" + emp + "]部门不能为空:" + strs + ".";

                string[] mystrs = strs.Split(',');
                foreach (string str in mystrs)
                {
                    if (DataType.IsNullOrEmpty(str) == true)
                        continue;

                    if (str.Equals("0") || str.Equals("root") == true)
                        continue;

                    //先看看数据是否有?
                    Dept dept = new Dept();
                    if (dept.Retrieve("Name", str, "OrgNo", WebUser.OrgNo) == 1)
                        continue;

                    //从xls里面判断.
                    isHave = false;
                    foreach (DataRow drDept in dtDept.Rows)
                    {
                        if (str.Equals(drDept[0].ToString()) == true)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave == false)
                        return "err@第[" + idx + "]行,人员[" + emp + "]部门名[" + str + "]，不存在模版里。";
                }
            }
            #endregion 检查人员的部门名称是否存在于部门数据里

            #region 检查人员的角色名称是否存在于角色数据里?
            idx = 0;
            foreach (DataRow dr in dtEmp.Rows)
            {
                string emp = dr[0] as string;
                if (DataType.IsNullOrEmpty(emp) == true)
                    continue;

                idx++;

                //角色名称..
                string strs = dr["角色名称"] as string;
                if (DataType.IsNullOrEmpty(strs) == true)
                    continue;

                // return "err@第[" + idx + "]行,人员[" + emp + "]角色名称不能为空:" + strs + ".";

                //判断角色.
                string[] mystrs = strs.Split(',');
                foreach (string str in mystrs)
                {
                    if (DataType.IsNullOrEmpty(str) == true)
                        continue;

                    //先看看数据是否有?
                    Station stationEn = new Station();
                    if (stationEn.Retrieve("Name", str, "OrgNo", WebUser.OrgNo) == 1)
                        continue;

                    //从 xls 判断.
                    isHave = false;
                    foreach (DataRow drSta in dtStation.Rows)
                    {
                        if (str.Equals(drSta[0].ToString()) == true)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave == false)
                        return "err@第[" + idx + "]行,人员[" + emp + "]角色名称[" + str + "]，不存在模版里。";
                }
            }
            #endregion 检查人员的部门名称是否存在于部门数据里

            #region 检查部门负责人是否存在于人员列表里?
            string empStrs = ",";
            foreach (DataRow item in dtEmp.Rows)
            {
                empStrs += item[0].ToString() + ",";
            }
            idx = 0;
            foreach (DataRow dr in dtDept.Rows)
            {
                string empNo = dr[2] as string;
                if (DataType.IsNullOrEmpty(empNo) == true)
                    continue;

                idx++;
                if (empStrs.Contains("," + empNo + ",") == false)
                    return "err@部门负责人[" + empNo + "]不存在与人员表里，第[" + idx + "]行.";
            }
            #endregion 检查部门负责人是否存在于人员列表里

            #region 检查直属领导帐号是否存在于人员列表里?
            idx = 0;
            foreach (DataRow dr in dtEmp.Rows)
            {
                string empNo = dr[6] as string;
                if (DataType.IsNullOrEmpty(empNo) == true)
                    continue;

                idx++;
                if (empStrs.Contains("," + empNo + ",") == false)
                    return "err@部门负责人[" + empNo + "]不存在与人员表里，第[" + idx + "]行.";
            }
            #endregion 检查部门负责人是否存在于人员列表里

            #region 插入数据到 Port_StationType. 
            idx = -1;
            foreach (DataRow dr in dtStation.Rows)
            {
                idx++;
                string str = dr[1] as string;

                //判断是否是空.
                if (DataType.IsNullOrEmpty(str) == true)
                    continue;

                if (str.Equals("角色类型") == true)
                    continue;

                str = str.Trim();

                //看看数据库是否存在.
                StationType st = new StationType();
                if (st.IsExit("Name", str, "OrgNo", WebUser.OrgNo) == false)
                {
                    st.Name = str;
                    st.OrgNo = BP.Web.WebUser.OrgNo;
                    st.No = DBAccess.GenerGUID();
                    st.Insert();
                }
            }
            #endregion 插入数据到 Port_StationType. 

            #region 插入数据到 Port_Station. 
            idx = -1;
            foreach (DataRow dr in dtStation.Rows)
            {
                idx++;
                string str = dr[0] as string;

                //判断是否是空.
                if (DataType.IsNullOrEmpty(str) == true)
                    continue;

                if (str.Equals("角色名称") == true)
                    continue;


                //获得类型的外键的编号.
                string stationTypeName = dr[1].ToString().Trim();
                StationType st = new StationType();
                if (st.Retrieve("Name", stationTypeName, "OrgNo", WebUser.OrgNo) == 0)
                    return "err@系统出现错误,没有找到角色类型[" + stationTypeName + "]的数据.";

                //看看数据库是否存在.
                Station sta = new Station();
                sta.Name = str;
                sta.Idx = idx;

                //不存在就插入.
                if (sta.IsExit("Name", str, "OrgNo", WebUser.OrgNo) == false)
                {
                    sta.OrgNo = BP.Web.WebUser.OrgNo;
                    sta.FK_StationType = st.No;
                    sta.No = DBAccess.GenerGUID();
                    sta.Insert();
                }
                else
                {
                    //存在就更新.
                    sta.FK_StationType = st.No;
                    sta.Update();
                }
            }
            #endregion 插入数据到 Port_Station. 

            #region 插入数据到 Port_Dept.
            idx = -1;
            foreach (DataRow dr in dtDept.Rows)
            {
                //获得部门名称.
                string deptName = dr[0] as string;
                if (deptName.Equals("部门名称") == true)
                    continue;

                string parentDeptName = dr[1] as string;
                string leader = dr[2] as string;

                //说明是根目录.
                if (parentDeptName.Equals("0") == true || parentDeptName.Equals("root") == true)
                {
                    Dept root = new Dept();
                    root.No = BP.Web.WebUser.OrgNo;
                    if (root.RetrieveFromDBSources() == 0)
                        return "err@没有找到根目录节点，请联系管理员。";

                    root.Name = deptName;
                    root.Update();
                    continue;
                }


                //先求出来父节点.
                Dept parentDept = new Dept();
                int i = parentDept.Retrieve("Name", parentDeptName, "OrgNo", BP.Web.WebUser.OrgNo);
                if (i == 0)
                    return "err@没有找到当前部门[" + deptName + "]的上一级部门[" + parentDeptName + "]";

                Dept myDept = new Dept();

                //如果数据库存在.
                i = parentDept.Retrieve("Name", deptName, "OrgNo", BP.Web.WebUser.OrgNo);
                if (i >= 1)
                    continue;

                //插入部门.
                myDept.Name = deptName;
                myDept.OrgNo = BP.Web.WebUser.OrgNo;
                myDept.No = DBAccess.GenerGUID();
                myDept.ParentNo = parentDept.No;
                myDept.Leader = leader; //领导.
                myDept.Idx = idx;
                myDept.Insert();
            }
            #endregion 插入数据到 Port_Dept.

            #region 插入到 Port_Emp.
            idx = 0;
            foreach (DataRow dr in dtEmp.Rows)
            {
                string empNo = dr["人员帐号"].ToString();
                string empName = dr["人员姓名"].ToString();
                string deptNames = dr["部门名称"].ToString();
                string deptPaths = dr["部门路径"].ToString();

                string stationNames = dr["角色名称"].ToString();
                string tel = dr["电话"].ToString();
                string email = dr["邮箱"].ToString();
                string leader = dr["直属领导"].ToString(); //部门领导.

                Emp emp = new Emp();
                int i = emp.Retrieve("UserID", empNo, "OrgNo", BP.Web.WebUser.OrgNo);
                if (i >= 1)
                {
                    emp.Tel = tel;
                    emp.Name = empName;
                    emp.Email = email;
                    emp.Leader = leader;
                    emp.Update();
                    continue;
                }

                //找到人员的部门.
                string[] myDeptStrs = deptNames.Split(',');
                Dept dept = new Dept();
                foreach (string deptName in myDeptStrs)
                {
                    if (DataType.IsNullOrEmpty(deptName) == true)
                        continue;

                    i = dept.Retrieve("Name", deptName, "OrgNo", WebUser.OrgNo);
                    if (i <= 0)
                        return "err@部门名称不存在." + deptName;

                    DeptEmp de = new DeptEmp();
                    de.FK_Dept = dept.No;
                    de.FK_Emp = empNo;
                    de.OrgNo = WebUser.OrgNo;
                    de.setMyPK(de.FK_Dept + "_" + de.FK_Emp);
                    de.Delete();
                    de.Insert();
                }

                //插入角色.
                string[] staNames = stationNames.Split(',');
                Station sta = new Station();
                foreach (var staName in staNames)
                {
                    if (DataType.IsNullOrEmpty(staName) == true)
                        continue;

                    i = sta.Retrieve("Name", staName, "OrgNo", WebUser.OrgNo);
                    if (i == 0)
                        return "err@角色名称不存在." + staName;

                    DeptEmpStation des = new DeptEmpStation();
                    des.FK_Dept = dept.No;
                    des.FK_Emp = empNo;
                    des.FK_Station = sta.No;
                    des.OrgNo = WebUser.OrgNo;
                    des.setMyPK(des.FK_Dept + "_" + des.FK_Emp + "_" + des.FK_Station);
                    des.Delete();
                    des.Insert();
                }

                //插入到数据库.
                emp.No = BP.Web.WebUser.OrgNo + "_" + empNo;
                emp.UserID = empNo;
                emp.Name = empName;
                emp.FK_Dept = dept.No;
                emp.OrgNo = WebUser.OrgNo;
                emp.Tel = tel;
                emp.Email = email;
                emp.Leader = leader;
                emp.Idx = idx;

                emp.Insert();
            }
            #endregion 插入到 Port_Emp.


            //删除临时文件
            System.IO.File.Delete(filePath);

            return "执行完成.";
        }


        //httppost请求
        BP.WF.HttpWebResponseUtility httpWebResponseUtility = new BP.WF.HttpWebResponseUtility();
        /// <summary>
        /// 微信初始化数据.
        /// </summary>
        /// <returns></returns>
        public string OrganizationWX_Init()
        {
            string urlx = this.GetRequestVal("url");
            /*return WeixinUtil.getWxConfig(request, url, jsapi);*/
            BP.Cloud.Org org = new Org(WebUser.OrgNo);

            #region 获取应用的jsapi_ticket
            //如果jsapi_ticket接近失效，要重新获取，更新.
            String jsapi = "";
            if (_cache.Get("jsapi_expires_in") != null
                && DateTime.Compare(Convert.ToDateTime(DateTime.Now),
                Convert.ToDateTime(_cache.Get("jsapi_expires_in"))) > 0)

                //如果失效了，就直接更新一下.
                jsapi = _cache.Get("jsapi").ToString();
            else
                jsapi = getjssdk(org);

            #endregion

            #region JS-SDK使用权限签名算法 参与签名的参数有四个: noncestr（随机字符串）, jsapi_ticket(如何获取参考“获取企业jsapi_ticket”以及“获取应用的jsapi_ticket接口”), timestamp（时间戳）, url（当前网页的URL， 不包含#及其后面部分）

            String requestUrl = urlx;

            String timestamp = DateTime.Now.ToString("yyyyMMDDHHddss"); // 必填，生成签名的时间戳

            String nonceStr = BP.DA.DBAccess.GenerGUID();  // 必填，生成签名的随机串

            // 注意这里参数名必须全部小写，且必须有序
            String sign = "jsapi_ticket=" + jsapi + "&noncestr=" + nonceStr + "&timestamp=" + timestamp + "&url=" + requestUrl;

            //sha1签名
            String signature = GetHash(sign);
            string corpId = org.CorpID;
            #endregion

            Hashtable ht = new Hashtable();
            ht.Add("corpId", BP.Cloud.WeXinAPI.Glo.CorpID);
            ht.Add("appId", corpId);
            ht.Add("timestamp", timestamp);
            ht.Add("nonceStr", nonceStr);
            ht.Add("signature", signature);
            ht.Add("agentid", org.AgentId);
            return BP.Tools.Json.ToJson(ht);

        }
        /// <summary>
        /// 获取应用的jsapi_ticket
        /// </summary>
        /// <returns></returns>
        public String getjssdk(BP.Cloud.Org org)
        {
            //如果AccessToken接近失效，要重新获取，更新
            if (DataType.IsNullOrEmpty(org.AccessTokenExpiresIn) == false
                && DateTime.Compare(Convert.ToDateTime(DateTime.Now),
                Convert.ToDateTime(org.AccessTokenExpiresIn)) > 0)
            {
                //如果失效了，就直接更新一下.
                org.AccessToken = getAccessToken(org);//获取企业凭证,更新失效时间
            }

            string url = "https://qyapi.weixin.qq.com/cgi-bin/ticket/get?access_token=" + org.AccessToken + "&type=agent_config";
            string res = httpWebResponseUtility.HttpResponseGet(url);
            Dictionary<string, object> dd = res.Trim(new char[] { '{', '}' }).Split(',').ToDictionary(s => s.Split(':')[0].Trim('"'), s => (object)s.Split(':')[1].Trim('"'));
            string ticket = (string)dd["ticket"];//生成签名所需的jsapi_ticket，最长为512字节
            string expires_in = (string)dd["expires_in"];//凭证的有效时间（秒）
            if (string.IsNullOrEmpty(ticket) == true)
                return "";

            _cache.Set("jsapi", ticket);
            DateTime ss = DateTime.Now.AddSeconds(double.Parse(expires_in));

            _cache.Set("jsapi_expires_in", ss.ToString("yyyy-MM-dd HH:mm:ss"));
            return ticket;
        }
        /// <summary>
        /// 获取企业凭证 第三方服务商在取得企业的永久授权码后，通过此接口可以获取到企业的access_token。
        ///获取后可通过通讯录、应用、消息等企业接口来运营这些应用。
        /// </summary>
        /// <returns></returns>
        public string getAccessToken(BP.Cloud.Org org)
        {
            //获取第三方应用凭证
            string suitAccessToken = BP.Cloud.WeXinAPI.Glo.getSuitAccessToken();

            // string permanentCode = CreateOrg();//获取永久授权码
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("auth_corpid", org.CorpID);//授权方corpid
            parameters.Add("permanent_code", org.PermanentCode);//永久授权码，通过get_permanent_code获取
            string accessTokenUrl = "https://qyapi.weixin.qq.com/cgi-bin/service/get_corp_token?suite_access_token=" + suitAccessToken;

            string res = httpWebResponseUtility.HttpResponsePost_Json(accessTokenUrl, JsonConvert.SerializeObject(parameters));

            Dictionary<string, object> dd = res.Trim(new char[] { '{', '}' }).Split(',').ToDictionary(s => s.Split(':')[0].Trim('"'), s => (object)s.Split(':')[1].Trim('"'));
            string accessToken = (string)dd["access_token"];//授权方（企业）access_token,最长为512字节
            string expires_in = (string)dd["expires_in"];
            DateTime ss = DateTime.Now.AddSeconds(double.Parse(expires_in));
            //更新accessToken到org表中
            //BP.Cloud.Org org = new BP.Cloud.Org(corpid);

            org.AccessToken = accessToken;
            org.AccessTokenExpiresIn = ss.ToString("yyyy-MM-dd HH:mm:ss");

            org.Update();
            return accessToken;
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String GetHash(String input)
        {
            //建立SHA1对象
            SHA1 sha = new SHA1CryptoServiceProvider();
            //将mystr转换成byte[]
            UTF8Encoding enc = new UTF8Encoding();
            byte[] dataToHash = enc.GetBytes(input);
            //Hash运算
            byte[] dataHashed = sha.ComputeHash(dataToHash);

            //将运算结果转换成string
            string hash = BitConverter.ToString(dataHashed).Replace("-", "");

            return hash;
        }

        /// <summary>
        /// 主动发送应用消息：企业后台调用接口通过应用向指定成员发送单聊消息
        /// </summary>
        public void sendMessageQywx(string toUserIds, string sender, string title, string docs, string url)
        {
            //根据发送人ID取得组织信息
            BP.Cloud.Emp emp = new BP.Cloud.Emp();
            emp.No = sender;
            if (emp.RetrieveFromDBSources() == 0)
                return;
            string orgNo = emp.OrgNo;

            //根据orgNo取得AccessToken
            BP.Cloud.Org org = new BP.Cloud.Org();
            org.No = orgNo;
            if (emp.RetrieveFromDBSources() == 0)
                return;

            //如果AccessToken接近失效，要重新获取，更新
            string accessToken = "";
            if (DataType.IsNullOrEmpty(org.AccessTokenExpiresIn) == false
                && DateTime.Compare(Convert.ToDateTime(DateTime.Now),
                Convert.ToDateTime(org.AccessTokenExpiresIn)) > 0)
            {
                //如果失效了，就直接更新一下.
                accessToken = getAccessToken(org);//获取企业凭证,更新失效时间
            }
            else
            {
                accessToken = org.AccessToken;
            }

            //组织发送信息的参数
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("touser", toUserIds.Replace(",", "|"));//指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。特殊情况：指定为”@all”，则向该企业应用的全部成员发送
            parameters.Add("toparty", toUserIds.Replace(",", "|"));//指定接收消息的部门，部门ID列表，多个接收者用‘|’分隔，最多支持100个。 当touser为”@all”时忽略本参数
            parameters.Add("totag", toUserIds.Replace(",", "|"));//指定接收消息的标签，标签ID列表，多个接收者用‘|’分隔，最多支持100个。 当touser为”@all”时忽略本参数
            parameters.Add("msgtype", "text");//消息类型，此时固定为：text
            parameters.Add("msgtype", org.AgentId);//企业应用的id，整型。企业内部开发，可在应用的设置页面查看；第三方服务商，可通过接口 获取企业授权信息 获取该参数值
            parameters.Add("text", "{\"content\" : \"你有待办流程，请及时处理。\n<a href=\"http://work.weixin.qq.com\">XXXX</a>\"}");//消息内容，最长不超过2048个字节，超过将截断（支持id转译）
            parameters.Add("safe", "0");//表示是否是保密消息，0表示否，1表示是，默认0
            parameters.Add("enable_id_trans", "0");//表示是否开启id转译，0表示否，1表示是，默认0
            parameters.Add("enable_duplicate_check", "0");//表示是否开启重复消息检查，0表示否，1表示是，默认0
            parameters.Add("duplicate_check_interval", "1800");//表示是否重复消息检查的时间间隔，默认1800s，最大不超过4小时

            string sendUrl = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=" + accessToken;

            //获得返回的数据.
            string res = httpWebResponseUtility.HttpResponsePost_Json(sendUrl, JsonConvert.SerializeObject(parameters));

            //获取企业新信息，插入数据库
            //解析返回的json串
            Dictionary<string, object> dd = res.Trim(new char[] { '{', '}' }).Split(',').ToDictionary(s => s.Split(':')[0].Trim('"'), s => (object)s.Split(':')[1].Trim('"'));
            string errcode = (string)dd["errcode"];
            if (errcode.Equals("0"))
            {
                /*
                 * 如果部分接收人无权限或不存在，发送仍然执行，但会返回无效的部分（即invaliduser或invalidparty或invalidtag），
                 * 常见的原因是接收人不在应用的可见范围内。
                 * 如果全部接收人无权限或不存在，则本次调用返回失败，errcode为81013。
                 * 返回包中的userid，不区分大小写，统一转为小写
                 */
                string invaliduser = (string)dd["invaliduser"];//发送的接收人中无效的用户名
                string invalidparty = (string)dd["invalidparty"];//发送的接收人中无效的部门
                string invalidtag = (string)dd["invalidtag"];//发送的接收人中无效的标签
                return;
            }
            //如果全部接收人无权限或不存在，则本次调用返回失败，errcode为81013。
            if (errcode.Equals("81013"))
            {
                return;
            }
        }

        #region 组织结构维护.
        /// <summary>
        /// 创建人员
        /// </summary>
        /// <returns></returns>
        public string Organization_NewEmp()
        {
            BP.Cloud.Emp emp = new Emp();
            emp.No = BP.Web.WebUser.OrgNo + "_" + this.No;
            if (emp.RetrieveFromDBSources() == 1)
            {
                //插入数据.
                DeptEmp myde = new DeptEmp();
                myde.setMyPK(myde.FK_Dept + "_" + emp.UserID);
                int i = myde.RetrieveFromDBSources();
                if (i == 0)
                {
                    myde.FK_Dept = this.FK_Dept;
                    myde.FK_Emp = emp.UserID;
                    myde.OrgNo = emp.OrgNo;
                  //  myde.IsMainDept = false;
                    myde.Insert();
                    return "info@该人员的ID隶属于[" + emp.FK_DeptText + "],已经被关联到本部门作为兼职.";
                }
                else
                {
                    myde.FK_Dept = this.FK_Dept;
                    myde.FK_Emp = emp.UserID;
                    myde.OrgNo = emp.OrgNo;
                 //   myde.IsMainDept = false;
                    myde.Update();
                    return "info@该人员的ID隶属于[" + emp.FK_DeptText + "],已经被关联到本部门作为兼职.";
                }
            }

            emp.Name = "新同事";
            emp.FK_Dept = this.FK_Dept;
            emp.UserID = this.No; //设置userID.
            if (DataType.IsNumStr(this.No) == true)
                emp.Tel = this.No;
            emp.OrgNo = BP.Web.WebUser.OrgNo;
            emp.Insert();

            //插入数据,设置他的部门.
            DeptEmp de = new DeptEmp();
            de.FK_Dept = emp.FK_Dept;
            de.FK_Emp = emp.UserID;
            de.setMyPK(de.FK_Dept + "_" + de.FK_Emp);
            de.OrgNo = emp.OrgNo;
          //  de.IsMainDept = true;
            de.Save();

            return "创建成功.";
        }
        /// <summary>
        /// 初始化组织结构部门表维护.
        /// </summary>
        /// <returns></returns>
        public string Organization_Init()
        {
            BP.Cloud.Depts depts = new BP.Cloud.Depts();
            depts.Retrieve("OrgNo", WebUser.OrgNo);
            return depts.ToJson();
        }
        /// <summary>
        /// 获取该部门的所有人员
        /// </summary>
        /// <returns></returns>
        public string LoadDatagridDeptEmp_Init()
        {
            string deptNo = this.GetRequestVal("deptNo");
            if (string.IsNullOrEmpty(deptNo))
            {
                return "{ total: 0, rows: [] }";
            }
            string orderBy = this.GetRequestVal("orderBy");

            string searchText = this.GetRequestVal("searchText");
            if (!DataType.IsNullOrEmpty(searchText))
            {
                searchText.Trim();
            }
            string addQue = "";
            if (!string.IsNullOrEmpty(searchText))
            {
                addQue = "  AND (pe.No like '%" + searchText + "%' or pe.Name like '%" + searchText + "%') ";
            }

            string pageNumber = this.GetRequestVal("pageNumber");
            int iPageNumber = string.IsNullOrEmpty(pageNumber) ? 1 : Convert.ToInt32(pageNumber);
            //每页多少行
            string pageSize = this.GetRequestVal("pageSize");
            int iPageSize = string.IsNullOrEmpty(pageSize) ? 9999 : Convert.ToInt32(pageSize);

            string sql = "(select pe.*,pd.name FK_DutyText from port_emp pe left join port_duty pd on pd.no=pe.fk_duty where pe.no in (select fk_emp from Port_DeptEmp where fk_dept='" + deptNo + "') "
                + addQue + " ) dbSo ";

            return DBPaging(sql, iPageNumber, iPageSize, "No", orderBy);
        }
        /// <summary>
        /// 以下算法只包含 oracle mysql sqlserver 三种类型的数据库 qin
        /// </summary>
        /// <param name="dataSource">表名</param>
        /// <param name="pageNumber">当前页</param>
        /// <param name="pageSize">当前页数据条数</param>
        /// <param name="key">计算总行数需要</param>
        /// <param name="orderKey">排序字段</param>
        /// <returns></returns>
        public string DBPaging(string dataSource, int pageNumber, int pageSize, string key, string orderKey)
        {
            string sql = "";
            string orderByStr = "";

            if (!string.IsNullOrEmpty(orderKey))
                orderByStr = " ORDER BY " + orderKey;

            switch (DBAccess.AppCenterDBType)
            {
                case DBType.Oracle:
                case DBType.KingBaseR3:
                case DBType.KingBaseR6:
                    int beginIndex = (pageNumber - 1) * pageSize + 1;
                    int endIndex = pageNumber * pageSize;

                    sql = "SELECT * FROM ( SELECT A.*, ROWNUM RN " +
                        "FROM (SELECT * FROM  " + dataSource + orderByStr + ") A WHERE ROWNUM <= " + endIndex + " ) WHERE RN >=" + beginIndex;
                    break;
                case DBType.MSSQL:
                    sql = "SELECT TOP " + pageSize + " * FROM " + dataSource + " WHERE " + key + " NOT IN  ("
                    + "SELECT TOP (" + pageSize + "*(" + pageNumber + "-1)) " + key + " FROM " + dataSource + " )" + orderByStr;
                    break;
                case DBType.MySQL:
                    pageNumber -= 1;
                    sql = "select * from  " + dataSource + orderByStr + " limit " + pageNumber + "," + pageSize;
                    break;
                default:
                    throw new Exception("暂不支持您的数据库类型.");
            }

            DataTable DTable = DBAccess.RunSQLReturnTable(sql);

            int totalCount = DBAccess.RunSQLReturnCOUNT("select " + key + " from " + dataSource);

            return DataTableConvertJson.DataTable2Json(DTable, totalCount);
        }
        #endregion

    }
}
