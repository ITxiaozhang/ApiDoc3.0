using ApiDoc.ApiDocAttribute;
using ApiDoc.ApiDocEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiDoc
{
    #region 字段属性说明的对象
    public class FiledsData
    {
        public FiledsData() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="filedType">字段类型string,int,bool</param>
        /// <param name="isWrite">是否必填 1:必填   非0:选填</param>
        /// <param name="remark">备注/说明</param>
        public FiledsData(string name, string filedType = "", int isWrite = 0, string remark = "", string value = null)
        {
            this._Name = name;
            this._Remark = remark;
            this._IsWrite = isWrite;
            this._FiledType = filedType;
            this._Value = value;
        }
        private string _Name = "";
        private string _FiledType = "";
        private int _IsWrite = 0;
        private string _Remark = "";
        private string _Value = "";
        private int _ZIndex = 0;

        [Remark("名称", "张三", FiledStatus.必填)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        [Remark("类型", "1", FiledStatus.必填)]
        public string FiledType
        {
            get { return _FiledType; }
            set { _FiledType = value; }
        }
        /// <summary>
        /// 1:必填   0:选填
        /// </summary>
        [Remark("是否必填", "1", FiledStatus.必填)]
        public int IsWrite
        {
            get { return _IsWrite; }
            set { _IsWrite = value; }
        }
        [Remark("备注")]
        public string Remark
        {
            get { return _Remark; }
            set { _Remark = value; }
        }
        [Remark("字段默认值")]
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
        [Remark("当前层级")]
        public int ZIndex
        {
            get { return _ZIndex; }
            set { _ZIndex = value; }
        }
    }
    #endregion
}