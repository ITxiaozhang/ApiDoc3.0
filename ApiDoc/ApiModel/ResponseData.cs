using ApiDoc.ApiDocAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiDoc.ApiModel
{
    public class ResponseData : BaseResponse
    {
        /// <summary>
        /// 状态--
        /// </summary>
        [Remark("状态", "我是默认值")]
        public int Status { get; set; }
        /// <summary>
        /// 返回文本
        /// </summary>
        [Remark("文本说明", "我是默认值")]
        public string Message { get; set; }
    }
}