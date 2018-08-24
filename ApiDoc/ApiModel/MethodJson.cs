using ApiDoc.ApiDocEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiDoc.ApiModel
{
    public class xmlDoc
    {
        public xmlParamText assembly { get; set; }
        public xmlMember members { get; set; }
    }

    public class xmlMember
    {
        public List<xmlMethodJson> member { get; set; }

        public static List<xmlMethodData> memberData(List<xmlMethodJson> list)
        {
            return list.Select(x => new xmlMethodData()
            {
                name = GetName(x.name),
                aType = GetActionType(x.name),
                summary = x.summary,
                ParamList = GetParamList(x),
                returns = x.returns,
            }).ToList();
        }
        private static string GetName(string name)
        {
            name = name.Substring(2, name.Length - 2);
            var i = name.LastIndexOf('(');
            return i < 0 ? name : name.Substring(0, i);
        }
        private static ActionType GetActionType(string name)
        {
            return (ActionType)Enum.Parse(typeof(ActionType), name.Substring(0, 1));
        }
        private static List<FiledsData> GetParamList(xmlMethodJson json)
        {
            var result = new List<FiledsData>();
            //不是方法-无参数则返回Null
            if (GetActionType(json.name) != ActionType.M)
                return result;
            var name = json.name;
            //获取括号中的内容并分隔,号
            var paramArr = json.name.Substring(name.IndexOf('(') + 1, name.Length - name.IndexOf('(') - 2).Split(',').ToList();
            if (json.name.IndexOf("(") < 0)
                paramArr = new List<string>();
            for (int i = 0; i < paramArr.Count; i++)
            {
                var info = new FiledsData();
                var flag = json.param != null && json.param.Count - 1 >= i;
                info.FiledType = GetFieldsTypeByStr(paramArr[i]);

                info.Name = flag ? json.param[i].name : "";
                info.Remark = flag ? json.param[i].text : "";
                info.IsWrite = info.Remark.Replace(" ", "").IndexOf("*") == 0 ? (int)FiledStatus.必填 : (int)FiledStatus.非必填;
                info.Value = "";
                result.Add(info);
            }
            return result;
        }
        private static string GetFieldsTypeByStr(string t)
        {
            if (t == typeof(string).FullName || t == typeof(String).FullName)
                return MemberInfoName.String.ToString();
            else if (t == typeof(Int16).FullName || t == typeof(Int32).FullName || t == typeof(Int64).FullName)
                return MemberInfoName.Int.ToString();
            else if (t == typeof(decimal).FullName || t == typeof(Double).FullName || t == typeof(float).FullName || t == typeof(double).FullName)
                return MemberInfoName.Float.ToString();
            else if (t == typeof(bool).FullName || t == typeof(Boolean).FullName)
                return MemberInfoName.Bool.ToString();
            else if (t == typeof(DateTime).FullName)
                return MemberInfoName.DateTime.ToString();
            else if (t == typeof(List<>).FullName)
                return MemberInfoName.List.ToString();
            else if (t == typeof(object).FullName)
                return MemberInfoName.Object.ToString();
            else
                return t;
        }
    }
    public class xmlMethodJson
    {
        public string name { get; set; }
        public string summary { get; set; }
        public List<xmlParamText> param { get; set; }
        public string returns { get; set; }
    }
    public class xmlMethodData
    {
        public string name { get; set; }
        public ActionType aType { get; set; }
        public string summary { get; set; }
        public List<FiledsData> ParamList { get; set; }
        public string returns { get; set; }
    }

    public class xmlParamText
    {
        public string name { get; set; }
        public string text { get; set; }
    }
}