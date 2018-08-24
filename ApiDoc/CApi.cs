using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Reflection;
using ApiDoc.ApiDocAttribute;
using ApiDoc.ApiDocEnum;
using ApiDoc.HelperUtil;
using ApiDoc.ApiModel;
using System.Web.Http;

namespace ApiDoc
{
    /// <summary>
    /// zc【2017-10-16】
    /// </summary>
    public abstract class CApi
    {

        //#region 读取配置
        ///// <summary>
        ///// 自定义的参数描述配置-通过字段名获取字段信息
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //private FiledsData GetParamFiledByName(string name)
        //{
        //    return this.AddGetParam().Where(x => x.Name.ToUpper() == name.ToUpper()).FirstOrDefault();
        //}
        //#endregion

        #region 构造函数-及属性
        public CApi() { }
        /// <summary>
        /// 主要实现的方法
        /// 基础配置（自动读取配置文件中名称对应的值）
        /// 例如： key="ApiDoc_NameSpace" value="ApiDoc"        则填写：ApiDoc_NameSpace
        /// 例如： key="ApiDoc_XmlPath" value="E:\\ApiDoc.xml"  则填写：ApiDoc_XmlPath
        /// 例如： key="ApiDoc_Url" value="localhost:1111"      则填写：ApiDoc_Url
        /// </summary>
        /// <param name="NameSpace">命名空间在配置文件中的名称。</param>
        /// <param name="XmlPath">Xml文件在配置文件中的名称。</param>
        /// <param name="url">Url在配置文件中的名称。</param>
        public CApi(string NameSpace, string XmlPath, string Url)
        {
            this.NameSpace = NameSpace.GetAppSettingsStr();
            this.XmlPath = XmlPath.GetAppSettingsStr();
            this.ApiRequestUrl = Url.GetAppSettingsStr();
            this.XmlConfigData = GetAllMethodData();
        }

        private string _XmlPath = "";
        private string _NameSpace = "";
        private string _ApiKey = "";
        private string _IndexTitle = "CApi一键生成文档首页";
        private string _ApiRequestUrl = "";
        private string _PreName = "Api";
        private Dictionary<string, int> _SetCreateData = new Dictionary<string, int>();
        /// <summary>
        /// 注解xml解析后获取到的对象集
        /// </summary>
        private List<xmlMethodData> _XmlConfigData = new List<xmlMethodData>();

        /// <summary>
        /// Xml文件地址
        /// </summary>
        public string XmlPath
        {
            get { return _XmlPath; }
            set { _XmlPath = value; }
        }
        /// <summary>
        /// 当前项目命名
        /// </summary>
        public string NameSpace
        {
            get { return _NameSpace; }
            set { _NameSpace = value; }
        }
        /// <summary>
        /// 接口密钥【若自定义签名方法-可不填写此参数】
        /// </summary>
        public string ApiKey
        {
            get { return _ApiKey; }
            set { _ApiKey = value; }
        }
        /// <summary>
        /// 接口请求地址【模拟请求的Url】
        /// </summary>
        public string ApiRequestUrl
        {
            get { return _ApiRequestUrl; }
            set { _ApiRequestUrl = value; }
        }
        /// <summary>
        /// 文档首页标题:CApi一键生成文档首页
        /// </summary>
        public string IndexTitle
        {
            get { return _IndexTitle; }
            set { _IndexTitle = value; }
        }
        /// <summary>
        /// 接口方法公共前缀:默认值[Api]
        /// </summary>
        public string PreName
        {
            get { return _PreName; }
            set { _PreName = value; }
        }
        /// <summary>
        /// (配置中的控制器或者方法名的文件,用于生成文件)
        /// Dictionary(string,int)
        /// string:控制器/方法名
        /// int: 1=控制器  2：方法名
        /// </summary>
        public Dictionary<string, int> SetCreateData
        {
            get { return _SetCreateData; }
            set { _SetCreateData = value; }
        }
        /// <summary>
        /// 注解对应的数据集
        /// </summary>
        private List<xmlMethodData> XmlConfigData
        {
            get { return _XmlConfigData; }
            set { _XmlConfigData = value; }
        }
        #endregion

        #region 检验重要参数
        private string _ControllName = "";
        private void CheckParam(int type = 0)
        {

            if (string.IsNullOrEmpty(this.NameSpace))
                throw new Exception("CApi未设置参数：NameSpace【当前项目命名】");
            if (string.IsNullOrEmpty(this.ApiRequestUrl))
                throw new Exception("CApi未设置参数：ApiRequestUrl【接口请求地址(模拟请求的Url)】");
            if ((this.SetCreateData == null || this.SetCreateData.Count <= 0) && type == 1)
                throw new Exception("CApi未设置参数：SetCreateData【仅仅生成当前(配置中的控制器或者方法名的文件)】");
            if (string.IsNullOrEmpty(this.XmlPath))
                throw new Exception("CApi未设置参数：XmlPath【项目的xml注解文件地址】");


        }
        #endregion

        #region 启动并生成Api文档-全局配置过的自动生成
        /// <summary>
        /// 启动并生成Api文档-全局配置过的自动生成
        /// </summary>
        /// <param name="type">type: 1=仅使用配置中数据，2=仅排除配置中数据</param>
        /// <returns>list[0]:总条数  list[1]:失败条数</returns>
        public List<int> CApi_Create(int type = 0)
        {
            //验证默认配置参数
            CheckParam(type);
            XmlConfigData = GetAllMethodData();

            #region 读取-特殊(排除/选中)的(控制器/方法)
            var onlyCList = this.SetCreateData.Where(k => k.Value == 1).Select(k => k.Key.ToUpper()).ToList();
            var onlyFList = this.SetCreateData.Where(k => k.Value == 2).Select(k => k.Key.ToUpper()).ToList();
            #endregion
            var assembly = Assembly.Load(this.NameSpace);
            var types = assembly.GetTypes();

            #region 过滤Controller
            var controller_typeList = GetControllerType_List(types);
            if (type == 1 && onlyCList.Count > 0)
                controller_typeList = controller_typeList.Where(x => onlyCList.Contains(x.Name.Replace("Controller", "").ToUpper())).ToList();
            else if (type == 2 && onlyCList.Count > 0)
                controller_typeList = controller_typeList.Where(x => !onlyCList.Contains(x.Name.Replace("Controller", "").ToUpper())).ToList();
            #endregion


            #region 循环控制器-记录需要生成的接口方法
            var apiData_list = GetApiData_List(controller_typeList);
            if (type == 1 && onlyFList.Count > 0)
                apiData_list = apiData_list.Where(x => onlyFList.Contains(x.FuncName.ToUpper())).ToList();
            else if (type == 2 && onlyFList.Count > 0)
                apiData_list = apiData_list.Where(x => !onlyFList.Contains(x.FuncName.ToUpper())).ToList();
            #endregion

            #region 循环符合条件接口方法集合-创建Api的文档
            var list = CreateAllApiFile(apiData_list) ?? (new int[] { 0, 0 }).ToList();
            #endregion
            this.SetCreateData.Clear();//清空
            return list;
        }
        #region 过滤Controller
        /// <summary>
        /// 过滤Controller
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private List<Type> GetControllerType_List(Type[] types)
        {
            var controller_typeList = new List<Type>();
            foreach (var item in types)
            {
                var attrs_Controller = item.GetCustomAttributes().Where(x => x.GetType() == typeof(CApiControllerAttribute)).ToList();
                if (attrs_Controller.Count() > 0)
                    controller_typeList.Add(item);
            }
            return controller_typeList;
        }
        #endregion

        #region 循环控制器-记录需要生成的接口方法
        /// <summary>
        /// 循环控制器-记录需要生成的接口方法
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private List<ApiData> GetApiData_List(List<Type> types)
        {
            var apiData_list = new List<ApiData>();
            foreach (var item in types)
            {
                _ControllName = item.Name;
                var _controller = item.GetCustomAttributes().Where(x => x.GetType() == typeof(CApiControllerAttribute)).FirstOrDefault() as CApiControllerAttribute;
                var methods = item.GetMethods().Where(x => x.GetCustomAttributes().Where(k => k.GetType() == typeof(ApiConfigAttribute)).ToList().Count() > 0).ToList();
                foreach (var method in methods)
                {
                    var apiData = GetApiData(method, _ControllName, _controller);
                    if (apiData != null)
                        apiData_list.Add(apiData);
                }
            }
            return apiData_list;
        }
        #endregion
        #endregion


        #region 获取方法对象存储的接口配置数据
        /// <summary>
        /// 获取方法对象存储的接口配置数据
        /// </summary>
        /// <param name="method">方法对象</param>
        /// <param name="ControllName">控制器名</param>
        /// <returns></returns>
        private ApiData GetApiData(MethodInfo method, string ControllName, CApiControllerAttribute _controller)
        {
            var cName = ControllName.Replace("Controller", "");
            var funcName = method.Name;
            var paramArr = method.GetParameters();
            var apiData = new ApiData();

            #region 设置传入参数
            var paramFDList = new List<FiledsData>();
            var _MethodName = string.Format("{0}.{1}", method.DeclaringType.FullName, method.ToString().Split('(')[0].Split(' ')[1]);
            var paramConfigData = this.XmlConfigData.Where(x => x.name == _MethodName && x.ParamList.Count() == paramArr.Count()).FirstOrDefault();
            foreach (var param in paramArr)
            {
                if (param.ParameterType.IsValueType || param.ParameterType == typeof(string))
                {

                    var pFD = paramConfigData.ParamList.Where(x => x.Name == param.Name).FirstOrDefault() ?? new FiledsData(param.Name);
                    //pFD.FiledType = GetFieldsType(param.ParameterType);
                    paramFDList.Add(pFD);
                }
                else
                {
                    var getItem = param.ParameterType;
                    var FiledsDataList = GetFiledsDataList(getItem);
                    paramFDList.AddRange(FiledsDataList);
                }
            }
            apiData.ParamDataList = paramFDList;
            #endregion

            #region 设置其他项
            apiData.CApiController = _controller;
            apiData.CName = cName.ToStr();
            apiData.Methods = "Get/Post";
            apiData.FuncName = funcName.ToStr();
            var preStr = this.PreName;
            preStr = "/" + preStr.Replace(" ", "");

            var url = "/" + cName + "/" + funcName;
            apiData.ApiUrl = preStr.Length == 1 ? url : (preStr + url);
            #endregion
            var attrs = method.GetCustomAttributes().ToList();
            #region 通过特性查找各种属性值
            for (int i = 0; i < attrs.Count(); i++)
            {
                if (attrs[i].GetType() == typeof(ApiConfigAttribute))
                {
                    #region 设置其他项-描述
                    var t1 = (ApiConfigAttribute)attrs[i];
                    apiData.Describtion = t1.Describtion.ToStr();
                    apiData.Name = t1.Name.ToStr();
                    //apiData.IsPre = t1.IsPre;

                    if (paramConfigData != null)
                    {
                        apiData.BackDescribtion = paramConfigData.returns;
                        if (!string.IsNullOrEmpty(paramConfigData.summary))
                            apiData.Describtion = paramConfigData.summary;
                    }
                    #endregion

                    #region 添加-返回参数
                    //var bcName = t1.BackClass.ToStr();
                    //var bcDic = new Dictionary<string, object>();// this.AddBackClassDic();
                    //if (!string.IsNullOrEmpty(bcName) && bcDic != null)
                    //{
                    //    foreach (var item in bcDic)
                    //    {
                    //        if (bcName.Split('.')[0] == item.Value)
                    //        {
                    //            bcName = item.Key + "." + bcName.Split('.')[1];
                    //            break;
                    //        }
                    //    }
                    //    var cxjName = bcName.Split('.')[0];//读取数组第一个值(正常情况下为程序集名称，特殊程序集命名不支持(例如)：Pro.Web)
                    //    var bcType = Assembly.Load(cxjName).GetType(bcName);
                    //    var FiledsDataList = GetFiledsDataList(bcType);
                    //    apiData.BackDataList = FiledsDataList;
                    //}
                    //else
                    var tuple = new Tuple<int, List<FiledsData>>(0, null);
                    var backSuccess = ConvertUtils.SetDefaultValOnObject(t1.BackContent, t1.BackClassType, ref tuple);
                    apiData.BackDataList = tuple.Item2;
                    apiData.BackSuccess = backSuccess;
                    #endregion
                }
                else if (attrs[i].GetType() == typeof(HttpGetAttribute))
                    apiData.Methods = "Get";
                else if (attrs[i].GetType() == typeof(HttpPostAttribute))
                    apiData.Methods = "Post";
            }
            #endregion

            return apiData;
        }
        #endregion

        #region 批量生成-动态读取数据
        private List<int> CreateAllApiFile(List<ApiData> list)
        {
            var okApiDataList = new List<ApiData>();
            if (list == null || list.Count <= 0)
                return null;
            var i = 0;
            //生成单页
            foreach (var item in list)
            {
                if (CreateApiHtml(item).Count > 0)
                    i++;
                else
                    okApiDataList.Add(item);
            }
            int[] arr = { list.Count(), i };
            //生成首页
            CreateApiIndexHtml(okApiDataList);
            return arr.ToList();
        }
        #endregion

        #region 模版读取-将动态数据写入模版

        #region 创建-Api说明单页
        private List<string> CreateApiHtml(ApiData apiData)
        {
            var resultList = new List<string>();
            try
            {
                var dic = new Dictionary<string, object>() { };
                var backDataList = apiData.BackDataList ?? new List<FiledsData>();
                var jsonHelper = new JavaScriptSerializer();
                apiData.EntityNullToStr<ApiData>();//对象中的string类型空值转成""

                #region 读取配置的返回格式数据
                var data = this.GetFiledsDataList(this.GetResponseData().GetType());
                var i = 0;
                foreach (var item in data)
                {
                    if (item.Name == "Data" && !dic.Any(x => x.Key == "Data"))
                        dic.Add(item.Name, apiData.BackSuccess);
                    else
                        dic.Add(item.Name, item.Value);
                    item.ZIndex = 0;
                    backDataList.Insert(i, item);
                    i++;
                }
                #endregion
                var html = File.ReadAllText(CApiConfig.templateUrl);
                var paramHtml = GetFiledsHtml(apiData.ParamDataList, 1);
                var backparamHtml = GetFiledsHtml(backDataList, 0, true);
                var backSuccess = jsonHelper.Serialize(dic);
                #region 替换文本
                html = html.Replace("{{ParamDataList}}", paramHtml);
                html = html.Replace("{{BackDataList}}", backparamHtml);
                html = html.Replace("{{Name}}", apiData.Name);
                html = html.Replace("{{BackSuccess}}", backSuccess);
                html = html.Replace("{{Describtion}}", apiData.Describtion);
                html = html.Replace("{{BackDescribtion}}", apiData.BackDescribtion);
                html = html.Replace("{{ApiUrl}}", apiData.ApiUrl);
                html = html.Replace("{{Methods}}", apiData.Methods);
                html = html.Replace("{{FuncName}}", apiData.FuncName);
                #endregion

                #region 生成文件-html
                string path = CApiConfig.fileUrl;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string fileName = string.Format("{0}{1}.html", apiData.CName, "_" + apiData.FuncName);
                string fileFullPath = CApiConfig.fileUrl + fileName;
                StreamWriter sw;
                if (!File.Exists(fileFullPath))
                {
                    sw = File.CreateText(fileFullPath);
                    sw.WriteLine(html.ToString());
                    sw.Close();
                }
                else
                    File.WriteAllText(fileFullPath, html);
                #endregion
            }
            catch (Exception ex)
            {
                resultList.Add(ex.Message);
            }
            return resultList;
        }
        #endregion

        #region 创建-Api首页
        private List<string> CreateApiIndexHtml(List<ApiData> apiData_list)
        {
            var resultList = new List<string>();
            try
            {
                #region ApiData转换成CApiConfig
                var list = new List<CApiConfig>();
                foreach (var apiData in apiData_list)
                {
                    string fileName = string.Format("{0}{1}.html", apiData.CName, "_" + apiData.FuncName);
                    var capi = new CApiConfig();
                    capi.CName = apiData.CName;
                    capi.AName = apiData.FuncName;
                    capi.Name = apiData.Name;
                    capi.Url = fileName;
                    //capi.IsPre = apiData.IsPre;
                    capi.CRemark = apiData.CApiController.Name;
                    capi.CIcon = apiData.CApiController.Icon.ToStr();
                    list.Add(capi);
                }
                #endregion

                #region 生成文件-Index文件
                var IndexHtml = File.ReadAllText(CApiConfig.templateUrl_index);//读取模板文件
                var one_Url = list == null ? "" : list[0].Url;
                var one_Name = list == null ? "" : list[0].Name;
                var parentHtml = new StringBuilder();//父菜单
                var n = 0;
                foreach (var item in list.GroupBy(x => x.CName).Select(x => x.FirstOrDefault()).ToList())
                {
                    var sonHtml = new StringBuilder();//子菜单
                    var i = 0;
                    foreach (var a in list.Where(x => x.CName == item.CName))
                        sonHtml.AppendFormat(aHtml, a.Url, a.Name, i++);
                    var icon = string.IsNullOrEmpty(item.CIcon) ? "fa-home" : item.CIcon;
                    parentHtml.AppendFormat(Menu_html, n == 0 ? "active" : "", icon, item.CRemark, sonHtml.ToStr());
                    n++;
                }
                IndexHtml = IndexHtml.Replace("{{Menu}}", parentHtml.ToStr());

                #region 替换配置信息
                IndexHtml = IndexHtml.Replace("{{One_Url}}", one_Url);
                IndexHtml = IndexHtml.Replace("{{One_Name}}", one_Name);
                IndexHtml = IndexHtml.Replace("{{IndexTitle}}", this.IndexTitle);
                IndexHtml = IndexHtml.Replace("{{Api_Url}}", this.ApiRequestUrl);
                #endregion

                string IndexFileFullPath = CApiConfig.indexUrl;
                File.WriteAllText(IndexFileFullPath, IndexHtml);
                #endregion
            }
            catch (Exception ex)
            {
                resultList.Add(ex.Message);
            }
            return resultList;
        }
        #endregion

        #region 每个Html标签块
        private string trHtml = @"<tr class='dygsj{5}' data-zindex='{5}'>
                                    <td>{4}{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                  </tr>";
        private string aHtml = "<a class=\"J_menuItem\" href=\"{0}\"  data-index=\"{2}\">{1}</a>";
        private string Menu_html = @"<li class='{0}'>
                                        <a href='#'>
                                            <i class='fa {1}'></i>
                                            <span class='nav-label'>{2}</span>
                                            <span class='fa arrow'></span>
                                        </a>
                                        <ul class='nav nav-second-level'>
                                            <li>
                                                {3}
                                            </li>
                                        </ul>
                                    </li>";
        #endregion

        #region 多行行单元格Html
        /// <summary>
        /// 生成多行单元格的Html
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type">1：生成空的一行TR(内容为--)   0：不生成空TR</param>
        /// <param name="isBack">是否为返回参数</param>
        /// <returns></returns>
        private string GetFiledsHtml(List<FiledsData> list, int type = 0, bool isBack = false)
        {
            if (list == null || list.Count <= 0)
                return "";
            var sb = new StringBuilder();
            foreach (var item in list)
                sb.AppendFormat(trHtml, item.Name, item.FiledType, item.IsWrite == 1 ? "(必填)" : "--", item.Remark, (item.IsWrite == 1 && !isBack) ? "<font>*</font>" : "", item.ZIndex);
            if (string.IsNullOrEmpty(sb.ToStr()) && type == 1)
                sb.AppendFormat(trHtml, "--", "--", "--", "--", "", 0);
            return sb.ToString();
        }
        #endregion

        #endregion

        #region Api接口测试
        /// <summary>
        /// Api接口测试-返回结果
        /// </summary>
        /// <param name="paramStr"></param>
        /// <param name="apiUrl"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        public string TestApiRequest(string paramStr, string apiUrl, int methods)
        {
            var msg = "";
            try
            {
                #region 生成最终需要传递给接口的参数【方法中已生成Sign】
                var paramDic = ConvertUtils.UrlParamToDic(paramStr);//参数转Dictionary
                var sign = MakeSign(paramDic);
                //paramDic.Add("APIKEY", sign);
                #endregion
                if (methods == 0 || methods == 1)
                    msg = apiUrl.ToAppend("?" + paramDic.ToUrlParam()).SendGet();
                else
                    msg = apiUrl.SendPost(paramDic.ToUrlParam());
            }
            catch (Exception)
            {
                msg = "Error";
            }
            return msg;
        }
        #endregion

        #region 自动访问接口-测试速度
        /// <summary>
        /// 自动访问接口-测试速度
        /// </summary>
        /// <param name="num">访问测试的次数(默认最少5次)</param>
        /// <param name="minTime">低于多少秒不显示（默认：0不限制 ）</param>
        public List<string> CApi_RequestTime(int num = 10, float minTime = 0)
        {
            //验证默认配置参数
            CheckParam();

            var assembly = Assembly.Load(this.NameSpace);
            var types = assembly.GetTypes();

            #region 过滤Controller
            var controller_typeList = GetControllerType_List(types);
            #endregion

            #region 循环控制器-记录需要生成的接口方法
            var apiData_list = GetApiData_List(controller_typeList);
            #endregion

            #region 接口调用
            var result = new List<string>();
            var startTime = DateTime.Now;
            num = num < 5 ? 5 : num;
            for (int i = 1; i <= num; i++)
            {
                result.Add("\r\n------------------------------第" + i + "次请求------------------------------");
                List<Task> taskList = new List<Task>();
                foreach (var item in apiData_list)
                {
                    TaskFactory taskFac = new TaskFactory();
                    var task = taskFac.StartNew(() =>
                    {
                        var tNum = 0;
                        var status = "正常";
                        try
                        {
                            var paramStr = item.ParamDataList.Count <= 0 ? "" : item.ParamDataList.Select(x => new { str = x.Name + "=" + x.Value }).ToJoinStr("&");
                            var sTime = DateTime.Now;
                            TestApiRequest(paramStr, this.ApiRequestUrl + item.ApiUrl, item.Methods.ToInt());
                            var eTime = DateTime.Now;
                            var tSpan = eTime - sTime;
                            tNum = tSpan.Minutes * 60 * 1000 + tSpan.Seconds * 1000 + tSpan.Milliseconds;//毫秒
                        }
                        catch (Exception)
                        {
                            tNum = -1;
                            status = "异常";
                        }
                        if ((minTime == 0) || (minTime > 0 && tNum >= minTime * 1000))
                            result.Add(string.Format("{0}-{1}【状态：{2}】【用时：{3}毫秒】", item.CName, item.FuncName, status, tNum));
                    });
                    taskList.Add(task);
                }
                Task.WaitAll(taskList.ToArray());
            }
            var endTime = DateTime.Now;
            var timeSpan = endTime - startTime;
            var timeNum = timeSpan.Minutes * 60 * 1000 + timeSpan.Seconds * 1000 + timeSpan.Milliseconds;//毫秒
            result[0] = "\r\n总用时：" + timeNum + "毫秒\r\n" + result[0];
            result.ToJoinStr("\r\n").WriteTextLog("CApi_RequestTime");
            #endregion
            return result;
        }
        #endregion

        #region 签名方法-设置数据对象
        /// <summary>
        /// 生成签名-示例11
        /// </summary>
        /// <returns></returns>
        public abstract string MakeSign(Dictionary<string, string> dic);
        #endregion

        #region 设置传入的返回的ResponseData对象
        /// <summary>
        /// 设置传入的ResponseData对象
        /// </summary>
        public virtual BaseResponse GetResponseData()
        {
            return new ResponseData();
        }
        #endregion

        #region 设置ApiConfig继承的子类
        /// <summary>
        /// 设置传入的ResponseData对象
        /// </summary>
        public virtual ApiConfigAttribute GetApiConfigSon()
        {
            return new ApiConfigAttribute();
        }
        #endregion

        #region 传入类型-返回类型对应字段的详情说明对象
        public List<FiledsData> GetFiledsDataList(Type bcType)
        {
            var FiledsDataList = new List<FiledsData>();
            if (bcType == null)
                return FiledsDataList;
            foreach (var bcItem in bcType.GetProperties())
            {
                if (!bcItem.PropertyType.IsValueType && bcItem.PropertyType != typeof(string) && bcItem.PropertyType != typeof(object))
                    continue;
                var fd = new FiledsData();
                fd.Name = bcItem.Name;
                fd.FiledType = GetFieldsType(bcItem.PropertyType);
                var bcAttrs = bcItem.GetCustomAttributes().ToList();
                for (int k = 0; k < bcAttrs.Count(); k++)
                {
                    if (bcAttrs[k].GetType() == typeof(RemarkAttribute))
                    {
                        var info = ((RemarkAttribute)bcAttrs[k]);
                        fd.Remark = info.Remark;
                        fd.IsWrite = Convert.ToInt32(Enum.Parse(typeof(FiledStatus), info.FiledStatus.ToString()));
                        fd.Value = ((RemarkAttribute)bcAttrs[k]).DefaultVal.ToStr();
                    }
                }
                FiledsDataList.Add(fd);
            }
            return FiledsDataList;
        }
        #endregion

        #region 获取对应字段类型
        private string GetFieldsType(Type t)
        {
            return ConvertUtils.GetFieldsType(t);
        }
        #endregion

        #region 获取xml文档中的描述结果集
        private List<xmlMethodData> GetAllMethodData()
        {
            var xmlDoc = ConvertUtils.GetXmlConfig(this.XmlPath);
            return xmlMember.memberData(xmlDoc.members.member);
        }
        #endregion



    }
}
