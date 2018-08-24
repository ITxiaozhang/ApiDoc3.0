using System;
using System.Collections.Generic;
using System.Web.Http;
using ApiDoc.ApiDocAttribute;
using ApiDoc.ApiDocEnum;

namespace TestApiDoc.Controllers
{
    [CApiController("我是用户的接口")]
    public class UserController : ApiController
    {
        public UserController()
        {
        }

        /// <summary>
        /// 检测是否有HttpGet/HttpPost特性【所属命名空间：System.Web.Http（并非System.Web.Mvc）】
        /// 来自动识别Post或Get请求
        /// </summary>
        /// <returns>我是获取学生的返回值说明</returns>
        [HttpGet]
        [ApiConfig("获取学生信息", typeof(Student))]
        public ResponseJson GetStudent()
        {
            return new ResponseJson() { Status = 200, Message = "ok", Data = new Student() };
        }


        /// <summary>
        /// 获取到老师信息
        /// </summary>
        /// <returns>我是获取老师的返回值说明</returns>
        [HttpGet]
        [ApiConfig("获取老师信息", typeof(Teacher))]
        public ResponseJson GetTeacher()
        {
            return new ResponseJson() { Status = 200, Message = "ok", Data = new Teacher() };
        }

    }

    public class Student
    {
        [Remark("成绩列表", "80", MethodType.List)]
        public List<Score> Socre1 { get; set; }
        [Remark("学生姓名", "小明", MethodType.ValList)]
        public List<string> listName { get; set; }
        [Remark("学生年龄", "10")]
        public string Age { get; set; }

    }

    public class Score
    {
        [Remark("分数成绩", "80")]
        public int Num { get; set; }
        [Remark("老师个人信息", "张老师", MethodType.Class)]
        public Teacher Teacher1 { get; set; }
        [Remark("班级", "1101")]
        public string ClassName { get; set; }

    }

    public class Teacher
    {
        [Remark("老师姓名", "张老师")]
        public string Name { get; set; }
        [Remark("老师年龄", "20")]
        public int Age { get; set; }
        [Remark("老师班级编辑", "1001班级")]
        public string ClassNo { get; set; }
    }

    public class TestClass
    {
        [Remark("*我是a|aaa")]
        public string a { get; set; }
        [Remark("*我是b|bbb")]
        public int b { get; set; }
        [Remark("我是c|ccc")]
        public bool c { get; set; }
        [Remark("我是d|ddd")]
        public double d { get; set; }
    }

    /// <summary>
    /// 固定返回对象
    /// </summary>
    public class ResponseJson
    {
        /// <summary>
        /// 返回操作状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 返回消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回数据对象
        /// </summary>
        public object Data { get; set; }

    }
}