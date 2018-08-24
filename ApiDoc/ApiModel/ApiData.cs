using ApiDoc.ApiDocAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiDoc.ApiModel
{
    #region 返回特性配置读取结果对象
    public class ApiData
    {
        /// <summary>
        /// 传入参数说明集合
        /// </summary>
        public List<FiledsData> ParamDataList { get; set; }
        /// <summary>
        /// 返回参数说明集合
        /// </summary>
        public List<FiledsData> BackDataList { get; set; }
        /// <summary>
        /// 返回参数说明集合
        /// </summary>
        public string BackDataJson { get; set; }
        /// <summary>
        /// 方法的访问地址
        /// </summary>
        public string ApiUrl { get; set; }
        /// <summary>
        /// 方法显示名称
        /// </summary>
        public string Name { get; set; }
        ///// <summary>
        ///// 是否有前缀
        ///// </summary>
        //public bool IsPre { get; set; }
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string CName { get; set; }
        /// <summary>
        /// 方法名称
        /// </summary>
        public string FuncName { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string Methods { get; set; }
        /// <summary>
        /// 接口详细说明
        /// </summary>
        public string Describtion { get; set; }
        /// <summary>
        /// 返回内容的详细说明--使用注解中内容
        /// </summary>
        public string BackDescribtion { get; set; }
        /// <summary>
        /// 返回成功对象
        /// </summary>
        public Object BackSuccess { get; set; }
        /// <summary>
        /// 控制器信息
        /// </summary>
        public CApiControllerAttribute CApiController { get; set; }

    }
    #endregion
}