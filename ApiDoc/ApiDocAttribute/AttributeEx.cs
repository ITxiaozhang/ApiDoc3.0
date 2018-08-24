using System;
using System.Web.Http.Filters;
using ApiDoc.ApiDocEnum;
using System.Reflection;
using ApiDoc.HelperUtil;

namespace ApiDoc.ApiDocAttribute
{
    #region 控制器-特性
    /// <summary>
    /// 控制器-特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CApiControllerAttribute : Attribute
    {
        public CApiControllerAttribute(string name, string icon = null)
        {
            this.Name = name;
            this.Icon = icon ?? "";
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 图标样式
        /// </summary>
        public string Icon { get; set; }
    }
    #endregion

    #region 方法-特性属性(进行异常检测)
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiConfigAttribute : ExceptionFilterAttribute
    {
        public ApiConfigAttribute() { }
        public ApiConfigAttribute(Type backclass, MethodType mType = MethodType.Class)
            : this("", backclass, mType)
        {
        }
        public ApiConfigAttribute(string name)
            : this(name, null, MethodType.Val)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="backclass">返回内容</param>
        public ApiConfigAttribute(string name, object backclass, MethodType mType = MethodType.Val)
        {
            this.Name = name;
            this.BackClassType = mType;
            this.BackContent = backclass;
        }
        public ApiConfigAttribute(string name, Type backclass, MethodType mType = MethodType.Class)
        {
            this.Name = name;
            this.BackClass = backclass;
            this.BackContent = Activator.CreateInstance(backclass);
            this.BackClassType = mType;
        }

        private Type _BackClass = null;
        private object _BackContent;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 详细说明
        /// </summary>
        public string Describtion { get; set; }
        /// <summary>
        /// 返回类型的对象
        /// </summary>
        public Type BackClass
        {
            get { return _BackClass; }
            set { _BackClass = value; }
        }
        /// <summary>
        /// 返回类型的对象
        /// </summary>
        public Object BackContent
        {
            get { return _BackContent; }
            set { _BackContent = value; }
        }
        /// <summary>
        /// 返回类型
        /// </summary>
        public MethodType BackClassType { get; set; }


        /// <summary>
        /// 子类继承-可将异常类的处理返回至外层处理
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <returns></returns>
        public virtual HttpActionExecutedContext ApiException(HttpActionExecutedContext actionExecutedContext)
        {
            return actionExecutedContext;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var assembly = Assembly.Load("ApiDoc_NameSpace".GetAppSettingsStr());
            var apiConfig = assembly.CreateInstance("ApiDoc_ApiConfigAttribute".GetAppSettingsStr());
            var method = apiConfig.GetType().GetMethod("ApiException");
            var result = (HttpActionExecutedContext)method.Invoke(apiConfig, new object[] { actionExecutedContext });
            base.OnException(result);
        }
    }
    #endregion

    #region 字段--特性属性
    [AttributeUsage(AttributeTargets.All)]
    public class RemarkAttribute : Attribute
    {

        /// <summary>
        /// 设置字段的属性含义值 
        /// </summary>
        /// <param name="_remark">字段含义说明,文本若以 * 开头,则FiledStatus枚举设置无效   例： *姓名  =>判断为必填</param>
        /// <param name="methodType">当属性为【实体】或者【实体的集合】</param>
        public RemarkAttribute(string _remark, MethodType methodType = MethodType.Val)
            : this(_remark, null, FiledStatus.非必填, methodType)
        {
        }
        /// <summary>
        /// 设置字段的属性含义值
        /// </summary>
        /// <param name="_remark">字段含义说明,文本若以 * 开头,则FiledStatus枚举设置无效</param>
        /// <param name="filedStatus">必填/非必填</param>
        /// <param name="methodType">当属性为【实体】或者【实体的集合】</param>
        public RemarkAttribute(string _remark, FiledStatus filedStatus, MethodType methodType = MethodType.Val)
            : this(_remark, "", FiledStatus.非必填, methodType)
        {
        }
        /// <summary>
        /// 设置字段的属性含义值 
        /// </summary>
        /// <param name="_remark">字段含义说明,文本若以 * 开头,则FiledStatus枚举设置无效</param>
        /// <param name="defaultVal">文本默认值-用于首次加载展示</param>
        /// <param name="methodType">当属性为【实体】或者【实体的集合】</param>
        public RemarkAttribute(string _remark, string defaultVal, FiledStatus filedStatus)
            : this(_remark, "", filedStatus, MethodType.Val)
        {
        }
        /// <summary>
        /// 设置字段的属性含义值 
        /// </summary>
        /// <param name="_remark">字段含义说明,文本若以 * 开头,则FiledStatus枚举设置无效</param>
        /// <param name="defaultVal">文本默认值-用于首次加载展示</param>
        /// <param name="methodType">当属性为【实体】或者【实体的集合】</param>
        public RemarkAttribute(string _remark, string defaultVal, MethodType methodType = MethodType.Val)
            : this(_remark, defaultVal, FiledStatus.非必填, methodType)
        {
        }
        /// <summary>
        /// 设置字段的属性含义值
        /// </summary>
        /// <param name="_remark">字段含义说明,文本若以 * 开头,则FiledStatus枚举设置无效   例： *姓名  =>判断为必填</param>
        /// <param name="defaultVal">文本默认值-用于首次加载展示</param>
        /// <param name="filedStatus">必填/非必填</param>
        /// <param name="methodType">当属性为【实体】或者【实体的集合】</param>
        public RemarkAttribute(string _remark, string defaultVal, FiledStatus FiledStatus, MethodType methodType)
        {
            this.DefaultVal = defaultVal;
            this.MethodType = methodType;
            if (_remark.IndexOf("*") == 0)
            {
                this.Remark = _remark.Substring(1, _remark.Length - 1);
                this.FiledStatus = FiledStatus.必填;
            }
            else
            {
                this.Remark = _remark;
                this.FiledStatus = FiledStatus;
            }
        }

        /// <summary>
        /// 字段含义说明,文本若以 * 开头,则FiledStatus枚举设置无效
        /// 例： *姓名  =>判断为必填
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 默认值(用于接口提示)
        /// </summary>
        public string DefaultVal { get; set; }
        /// <summary>
        /// 必填/非必填
        /// </summary>
        public FiledStatus FiledStatus { get; set; }
        /// <summary>
        /// 字段名称（实际在实体中名称）
        /// </summary>
        public string FiledName { get; set; }
        /// <summary>
        /// 集合/实体--默认为MethodType.Val
        /// </summary>
        public MethodType MethodType { get; set; }
    }
    #endregion
}