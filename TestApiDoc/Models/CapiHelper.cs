using ApiDoc;
using ApiDoc.ApiDocAttribute;
using ApiDoc.ApiModel;
using System.Collections.Generic;
using System.Web.Http.Filters;

namespace TestApiDoc
{
    /// <summary>
    /// 使用示例
    /// </summary>
    public class CapiHelper : CApi
    {
        public CapiHelper()
            : base("ApiDoc_NameSpace", "ApiDoc_XmlPath", "ApiDoc_Url")
        {
        }

        public override string MakeSign(Dictionary<string, string> dic)
        {
            return "";
        }
        public override BaseResponse GetResponseData()
        {
            return new NewResponseData();
        }
    }

    /// <summary>
    /// 实现ApiConfigAttribute 可以将异常抛出到当前位置ApiException来处理
    /// </summary>
    public class MyApiAttribute : ApiConfigAttribute
    {
        public override HttpActionExecutedContext ApiException(HttpActionExecutedContext actionExecutedContext)
        {
            var list = new List<int>();
            var n = 10 * 10000;
            for (int i = 0; i < n; i++)
            {
                list.Add(i + 1);
            }
            var val = string.Join(",", list);
            return actionExecutedContext;
        }
    }

    public class NewResponseData : BaseResponse
    {
        [Remark("姓名11", "11")]
        public string Name1 { get; set; }
        [Remark("姓名22", "22")]
        public string Name2 { get; set; }
        [Remark("姓名33", "33")]
        public string Name3 { get; set; }
        [Remark("姓名44", "44")]
        public string Name4 { get; set; }

    }


    public class TestHelper
    {
        [Remark("*我是One|111")]
        public string one { get; set; }
        [Remark("我是Two|222")]
        public int two { get; set; }
        [Remark("*我是Three|333")]
        public bool three { get; set; }
    }
}