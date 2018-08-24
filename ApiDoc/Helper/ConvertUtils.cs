using ApiDoc.ApiDocAttribute;
using ApiDoc.ApiDocEnum;
using ApiDoc.ApiModel;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;

namespace ApiDoc.HelperUtil
{
    public static class ConvertUtils
    {
        #region 【2018-08-21】zc-Xml配置解析转成对象
        public static xmlDoc GetXmlConfig(string url)
        {
            var xmlText = File.ReadAllText(url);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlText);
            var _doc = xml.ChildNodes[1];       //doc节点
            var _members = _doc.ChildNodes[1];  //members节点

            var _xmlDoc = new xmlDoc();//实例一个Doc对象
            _xmlDoc.assembly = new xmlParamText();
            _xmlDoc.assembly.name = _doc.ChildNodes[0].ChildNodes[0].InnerText;//找到节点
            _xmlDoc.members = new xmlMember();
            var memberList = new List<xmlMethodJson>();

            //循环出子节点
            foreach (XmlNode member in _members.ChildNodes)
            {
                //类型为错误的节点时,
                if (member.NodeType == XmlNodeType.Comment)
                    continue;
                var _xmlMJ = new xmlMethodJson();
                _xmlMJ.name = member.Attributes["name"].InnerText;//方法名
                _xmlMJ.param = new List<xmlParamText>();
                foreach (XmlNode son in member.ChildNodes)
                {
                    //子节点-保存对应文本值
                    switch (son.Name)
                    {
                        case "param":
                            var sonName = son.Attributes["name"].InnerText;
                            var sonText = son.InnerText;
                            _xmlMJ.param.Add(new xmlParamText() { name = sonName, text = sonText });
                            break;
                        case "summary":
                            _xmlMJ.summary = son.InnerText;
                            break;
                        case "returns":
                            _xmlMJ.returns = son.InnerText;
                            break;
                    }
                }
                memberList.Add(_xmlMJ);
            }
            _xmlDoc.members.member = memberList;
            return _xmlDoc;
        }
        #endregion

        #region 【2018-08-21】zc-设置默认值（给对象）
        /// <summary>
        /// 将默认值赋值给对象（对象中存在对象，集合，将全部赋值默认）-然后序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public static Object SetDefaultValOnObject(object obj, MethodType mType, ref Tuple<int, List<FiledsData>> tuple)
        {
            tuple = new Tuple<int, List<FiledsData>>(1, new List<FiledsData>());
            var type = obj.GetType();
            var result = new Object();
            switch (mType)
            {
                //【值类型】值类型-string,int,double,decimal等
                case MethodType.Val:
                    result = obj.ToJson();
                    break;
                //【引用类型/集合】值类型的集合（string,int,double,decimal等）
                case MethodType.ValList:
                    var valList = new List<object>();
                    valList.Add(obj.ToString());
                    result = valList.ToJson();
                    break;
                //【引用类型】类/实体
                case MethodType.Class:
                    result = GetInstance(obj, type, mType, ref tuple, true);
                    break;
                //【引用类型】集合对象
                case MethodType.List:
                    result = GetInstance(obj, type, mType, ref tuple, true);
                    break;
            }
            //tuple = new Tuple<int, List<FiledsData>>(tuple.Item1, tuple.Item2.OrderBy(x => x.ZIndex).ToList());
            return result;
        }
        #region 实例对象的值
        /// <summary>
        /// 实例对象的值
        /// </summary>
        /// <param name="obj">初始传入的Obj对象</param>
        /// <param name="type">初始Obj对象的Type类型</param>
        /// <param name="mType">当前Obj配置的MethodType枚举类型</param>
        /// <param name="flag">【非必填】是否第一次（true:是  flase:否）</param>
        /// <param name="remark">【非必填】MethodType为值类型集合ValList时需要传入</param>
        /// <returns></returns>
        private static object GetInstance(Object obj, Type type, MethodType mType, ref Tuple<int, List<FiledsData>> tuple, bool flag = false, RemarkAttribute remark = null)
        {
            var zIndex = tuple.Item1;
            //值类型的枚举集合
            var valTypeList = new[] { MethodType.ValList, MethodType.Val };
            //值类型-不需要创建实例-直接赋值
            var info = !valTypeList.Contains(mType) ? Activator.CreateInstance(type) : remark.DefaultVal;
            //若flag=true。表示首次进入,初始对象obj等于当前首次实例对象
            if (flag)
                obj = info;
            //值类型-没有属性无需循环赋值
            if (!valTypeList.Contains(mType))
            {
                if (flag && remark != null)
                {
                    var attrInfo = remark;//获取特性
                    var filedsList = tuple.Item2 ?? new List<FiledsData>();
                    filedsList.Add(new FiledsData()
                    {
                        Remark = attrInfo.Remark,//说明
                        IsWrite = attrInfo.Remark.Replace(" ", "").IndexOf("*") == 0 ? (int)FiledStatus.必填 : (int)FiledStatus.非必填,//是否必填
                        Value = "",//默认值
                        ZIndex = zIndex - 1,//层级
                        Name = remark.FiledName,//字段名
                        FiledType = "--",//字段类型
                    });
                    tuple = new Tuple<int, List<FiledsData>>(tuple.Item1, filedsList);
                }
                //循环值类型对象的属性值
                foreach (var item in type.GetProperties())
                {
                    var attrInfo = (RemarkAttribute)item.GetCustomAttribute(typeof(RemarkAttribute));//获取特性
                    var val = SetFieldsValByType(obj, item, attrInfo, ref tuple);//获取属性的真实值
                    item.SetValue(info, val, null);//将值给当前实例对象info
                    var filedsList = tuple.Item2 ?? new List<FiledsData>();
                    if (!filedsList.Any(x => x.ZIndex == zIndex && x.Name == item.Name))
                    {
                        filedsList.Add(new FiledsData()
                        {
                            Remark = attrInfo.Remark,//说明
                            IsWrite = attrInfo.Remark.Replace(" ", "").IndexOf("*") == 0 ? (int)FiledStatus.必填 : (int)FiledStatus.非必填,//是否必填
                            Value = "",//默认值
                            ZIndex = zIndex,//层级
                            Name = item.Name,//字段名
                            FiledType = ConvertUtils.GetFieldsType(item.PropertyType),//字段类型
                        });
                    }
                    tuple = new Tuple<int, List<FiledsData>>(tuple.Item1, filedsList);
                }
            }
            //枚举类型为集合
            if (mType == MethodType.List || mType == MethodType.ValList)
            {
                //实例一个当前type类型的集合
                var generic = typeof(List<>).MakeGenericType(new Type[] { type });
                //创建集合实例并转为 集合类型
                var objList = Activator.CreateInstance(generic) as IList;
                objList.Add(info);//将设置在特性的值放入到集合
                return objList;
            }
            return info;
        }
        #endregion

        #region 获取对应字段类型值
        private static object SetFieldsValByType(Object obj, PropertyInfo proccessInfo, RemarkAttribute remark, ref Tuple<int, List<FiledsData>> tuple)
        {
            //当前属性的 Type类型
            var t = proccessInfo.PropertyType;
            if (t == typeof(string))
                return remark.DefaultVal.ToStr();
            else if (t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64))
                return remark.DefaultVal.ToInt(-1);
            else if (t == typeof(decimal))
                return remark.DefaultVal.ToDecimal();
            else if (t == typeof(Double) || t == typeof(double))
                return remark.DefaultVal.ToDouble();
            else if (t == typeof(float))
                return remark.DefaultVal.ToFloat();
            else if (t == typeof(bool) || t == typeof(Boolean))
                return remark.DefaultVal.ToBool();
            else if (t == typeof(DateTime))
                return remark.DefaultVal.ToDateTime();
            else if (!t.IsValueType)
            {
                remark.FiledName = proccessInfo.Name;
                var zIndex = tuple.Item1 + 1;
                //若类型不是值类型时-根据配置的类型枚举来实例子属性对象
                if (remark.MethodType == MethodType.Class)
                {
                    tuple = new Tuple<int, List<FiledsData>>(zIndex, tuple.Item2);
                    return GetInstance(obj, proccessInfo.PropertyType, remark.MethodType, ref tuple, true, remark);
                }
                else if (remark.MethodType == MethodType.List)
                {
                    tuple = new Tuple<int, List<FiledsData>>(zIndex, tuple.Item2);
                    //proccessInfo.PropertyType.GetGenericArguments()[0]获引用类型中包含的对象类型列表,[0]表示第一个类型,例：List<string>  获取到String 类型  List<Student>获取到Student
                    return GetInstance(obj, proccessInfo.PropertyType.GetGenericArguments()[0], remark.MethodType, ref tuple, true, remark);
                }
                else if (remark.MethodType == MethodType.ValList)
                {
                    tuple = new Tuple<int, List<FiledsData>>(zIndex, tuple.Item2);
                    //proccessInfo.PropertyType.GetGenericArguments()[0]获引用类型中包含的对象类型列表,[0]表示第一个类型,例：List<string>  获取到String 类型  List<Student>获取到Student
                    //值类型的列表-将特性Remark对象传入
                    return GetInstance(obj, proccessInfo.PropertyType.GetGenericArguments()[0], remark.MethodType, ref tuple, false, remark);
                }
                else
                {
                    //其他未配置类型-直接创建实例-返回结果例如：Dictionary<object,object>  等
                    return Activator.CreateInstance(proccessInfo.PropertyType);
                }
            }
            else
                return "";
        }
        #endregion
        #endregion

        #region 模拟Get请求方法
        /// <summary>
        /// Get获取请求结果
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string SendGet(this string url)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            string str = "";
            using (WebResponse wr = request.GetResponse())
            {
                response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                str = reader.ReadToEnd();
            }
            return str;
        }
        #endregion

        #region 模拟Post请求方法
        /// <summary>
        /// Post获取请求结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string SendPost(this string strURL, string param)
        {
            HttpWebRequest request = WebRequest.Create(strURL) as HttpWebRequest;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(param);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream()) { stream.Write(data, 0, data.Length); }
            var sm = (request.GetResponse() as HttpWebResponse).GetResponseStream();
            StreamReader sr = new StreamReader(sm, Encoding.UTF8);
            return sr.ReadToEnd();
        }
        #endregion

        #region 转成Url传参格式 a=1&b=3&c=3
        /// <summary>
        /// 转成Url传参格式 a=1&b=3&c=3
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string ToUrlParam(this Dictionary<string, string> dic)
        {
            var list = dic.Select(x => new { k = x.Key + "=" + x.Value }).Select(x => x.k).ToList();
            var str = string.Join("&", list);
            return str;
        }
        #endregion

        #region 转成Dictionary<string,string>原字符 a=1&b=3&c=3
        /// <summary>
        ///  转成Dictionary(string,string)字符 a=1&b=3&c=3
        /// </summary>
        /// <param name="paramStr"></param>
        /// <returns></returns>
        public static Dictionary<string, string> UrlParamToDic(string paramStr)
        {
            var dic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(paramStr))
                return dic;
            var paramArr = paramStr.Split('&');
            for (int i = 0; i < paramArr.Length; i++)
                dic.Add(paramArr[i].Split('=')[0], paramArr[i].Split('=')[1]);
            return dic;
        }
        #endregion

        #region 序列化对象为Json
        public static string ToJson(this Object obj)
        {
            var jserializer = new JavaScriptSerializer();
            var jsonStr = jserializer.Serialize(obj);
            return jsonStr;
        }
        #endregion

        #region 序列化对象为Json
        public static T JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
            //var jserializer = new JavaScriptSerializer();
            //return jserializer.Deserialize<T>(json);
        }
        #endregion


        #region 转为Bool类型
        public static bool ToBool(this object value, bool defaultValue = false)
        {
            bool rst;
            if (value == null) return defaultValue;
            if (bool.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion

        #region 转为时间日期格式
        public static DateTime ToDateTime(this object value)
        {
            DateTime rst;
            var defaultValue = DateTime.Now;
            if (value == null) return defaultValue;
            if (DateTime.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion

        #region 转浮点类格式
        public static decimal ToDecimal(this object value, decimal defaultValue = 0)
        {
            decimal rst;
            if (value == null) return defaultValue;
            if (decimal.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion

        #region Float格式
        public static float ToFloat(this object value, float defaultValue = 0f)
        {
            float rst;
            if (value == null) return defaultValue;
            if (float.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion

        #region Double类格式
        public static double ToDouble(this object value, double defaultValue = 0)
        {
            double rst;
            if (value == null) return defaultValue;
            if (double.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion


        #region 字符串MD5+Key 加密
        /// <summary>
        /// 字符串MD5+Key 加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToMD5(string data)
        {
            StringBuilder sb = new StringBuilder();
            MD5 md5 = new MD5CryptoServiceProvider();
            var Key = ""; //读取配置文件
            byte[] t = md5.ComputeHash(Encoding.UTF8.GetBytes("32333" + data + "z32!"));
            foreach (var t1 in t)
            {
                sb.Append(t1.ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }
        #endregion

        #region 获取配置的内容
        /// <summary>
        /// 获取配置中的内容-为空择返回""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetAppSettingsStr(this string str)
        {
            return ConfigurationManager.AppSettings[str].ToStr();
        }
        #endregion

        #region 数组拼接成字符串
        /// <summary>
        /// 数组拼接成字符串
        /// </summary>
        /// <param name="ienumerable"></param>
        /// <param name="splitStr">分割字符串:默认为逗号</param>
        /// <returns></returns>
        public static string ToJoinStr(this IEnumerable<object> ienumerable, string splitStr = ",")
        {
            return string.Join(splitStr, ienumerable);
        }
        #endregion

        #region 将后面字符串追加到当前字符后面
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="newStr"></param>
        /// <returns></returns>
        public static string ToAppend(this string str, string newStr)
        {
            var sb = new StringBuilder();
            sb.Append(str);
            sb.Append(newStr);
            return sb.ToStr();
        }
        #endregion


        public static string ToStr(this object value, string defaultValue = "")
        {
            if (value == null) return defaultValue;
            else
            {
                return value.ToString();
            }
        }
        public static int ToInt(this object value, int defaultValue = 0)
        {
            int rst;
            if (value == null) return defaultValue;
            if (int.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }

        #region 实体--有Null的string的成员变量改为""
        /// <summary>
        /// 实体--有Null的string的成员变量改为""
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T EntityNullToStr<T>(this T self)
        {
            try
            {
                foreach (PropertyInfo mItem in typeof(T).GetProperties())
                {
                    var mItemVal = mItem.GetValue(self, new object[] { });
                    if (mItem.PropertyType == typeof(string))
                    {
                        mItem.SetValue(self, mItemVal.ToStr(), null);
                    }
                }
            }
            catch (NullReferenceException NullEx)
            {
                throw NullEx;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            return self;
        }
        #endregion

        #region 日志记录
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="action">错误名称（文件名-不支持特殊符号）</param>
        /// <param name="strMessage">错误内容</param>
        public static void WriteTextLog(this string msg, string action)
        {
            string path = @"c:\Test_Log\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileFullPath = path + action + DateTime.Now.ToString("_yyyyMMddhhmmss") + ".txt";
            StringBuilder str = new StringBuilder();
            str.Append("Time:    " + DateTime.Now.ToString() + "\r\n");
            str.Append("Action:  " + action + "\r\n");
            str.Append("Message: " + msg + "\r\n");
            str.Append("-----------------------------------------------------------\r\n\r\n");
            StreamWriter sw;
            if (!File.Exists(fileFullPath))
            {
                sw = File.CreateText(fileFullPath);
            }
            else
            {
                sw = File.AppendText(fileFullPath);
            }
            sw.WriteLine(str.ToString());
            sw.Close();
        }
        #endregion


        #region 获取对应字段类型
        public static string GetFieldsType(Type t)
        {
            if (t == typeof(string))
                return MemberInfoName.String.ToString();
            else if (t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64))
                return MemberInfoName.Int.ToString();
            else if (t == typeof(decimal) || t == typeof(Double) || t == typeof(float) || t == typeof(double))
                return MemberInfoName.Float.ToString();
            else if (t == typeof(bool) || t == typeof(Boolean))
                return MemberInfoName.Bool.ToString();
            else if (t == typeof(DateTime))
                return MemberInfoName.DateTime.ToString();
            else if (t == typeof(object))
                return MemberInfoName.Object.ToString();
            else
                return "";
        }
        #endregion
    }
}