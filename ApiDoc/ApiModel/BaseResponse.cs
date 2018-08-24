using ApiDoc.ApiDocAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiDoc.ApiModel
{
    public class BaseResponse
    {
        /// <summary>
        /// 存放数据的对象
        /// </summary>
        [Remark("存放数据的对象", "我是默认值")]
        public Object Data { get; set; }
    }
}