using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ApiDoc.ApiModel
{
    #region 配置-记录生成的文件的json
    /// <summary>
    /// CApiConfig 的摘要说明
    /// </summary>
    public class CApiConfig
    {
        public CApiConfig() { }
        public CApiConfig(string _CName, string _Name, string _Url, string _AName = "", bool _IsPre = true, string _CRemark = "", string _CIcon = "")
        {
            this.CName = _CName;
            this.AName = _AName;
            this.Name = _Name;
            this.Url = _Url;
            this.IsPre = _IsPre;
            this.CRemark = _CRemark;
            this.CIcon = _CIcon;
        }
        public static string fileUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\";
        public static string configUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\Config\\config.json";
        public static string templateUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\Config\\api-tmp.html";
        public static string templateUrl_index = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\Config\\index-tmp.html";
        public static string indexUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\index.html";
        public static List<CApiConfig> GetList()
        {
            var json = File.ReadAllText(configUrl);
            var sear = new JavaScriptSerializer();
            return sear.Deserialize<List<CApiConfig>>(json);
        }

        public static void SaveData(List<CApiConfig> list)
        {
            var sear = new JavaScriptSerializer();
            string json = sear.Serialize(list);
            json = json.Replace("[", "[\r\n	");
            json = json.Replace("},", "},\r\n	");
            json = json.Replace("}]", "}\r\n]");
            StreamWriter sw;
            if (!File.Exists(configUrl))
            {
                sw = File.CreateText(configUrl);
                sw.WriteLine(json.ToString());
                sw.Close();
            }
            else
                File.WriteAllText(configUrl, json);

        }
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string CName { get; set; }
        /// <summary>
        /// 方法名
        /// </summary>
        public string AName { get; set; }
        /// <summary>
        /// 方法显示名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件名  *.html
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 是否有前缀
        /// </summary>
        public bool IsPre { get; set; }
        /// <summary>
        /// 控制器说明
        /// </summary>
        public string CRemark { get; set; }
        /// <summary>
        /// 控制器图标-Icon
        /// </summary>
        private string _CIcon = "fa-home";
        public string CIcon
        {
            get { return _CIcon; }
            set { _CIcon = value; }
        }
    }
    #endregion
}