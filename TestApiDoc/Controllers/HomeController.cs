using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using ApiDoc.HelperUtil;

namespace TestApiDoc.Controllers
{
    public class HomeController : Controller
    {
        public static CapiHelper api = new CapiHelper();
        public HomeController()
        {
            api.IndexTitle = "测试的一个名字";
            //api.CApi_Create();
        }
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="c"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ActionResult Index2(int type = 0, string c = "", string f = "")
        {
            if (!string.IsNullOrEmpty(c))
                api.SetCreateData.Add(c, 1);
            if (!string.IsNullOrEmpty(f))
                api.SetCreateData.Add(f, 2);
            var list = api.CApi_Create(type);
            ViewBag.HtmlStr = "成功：" + list[0] + "<br>失败：" + list[1];
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramStr"></param>
        /// <param name="str"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        public JsonResult TestApiRequest(string paramStr, string apiUrl, int methods)
        {
            var msg = api.TestApiRequest(paramStr, apiUrl, methods);
            if (msg == "Error")
                return Json(new ResponseJson() { Status = 500, Message = "请求失败", Data = null }, JsonRequestBehavior.AllowGet);
            return Json(new ResponseJson() { Status = 200, Message = "ok", Data = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTimeTest(int time = 10, float minTime = 0)
        {
            return Json(new ResponseJson() { Status = 200, Message = "ok", Data = api.CApi_RequestTime(time, minTime) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TestHrmRequest(int num = 100)
        {
            var url = "http://localhost:15564/home/PersonnelData";
            //var url = "http://192.168.1.189:6312/home/PersonnelData";
            var resultList = new List<string>();
            var taskList = new List<Task>();
            for (int i = 1; i <= num; i++)
            {
                try
                {
                    var taskFac = new TaskFactory();
                    var task = taskFac.StartNew(() =>
                    {
                        using (var client = new HttpClient())
                        {
                            var msg = client.GetStringAsync(url).Result;
                        }
                        resultList.Add("第" + (resultList.Count + 1) + "次:ok");
                    });
                    taskList.Add(task);
                }
                catch (Exception ex)
                {
                    resultList.Add("第" + (resultList.Count + 1) + "次:" + ex.ToStr());
                }
            }
            Task.WaitAll(taskList.ToArray());
            ViewBag.Result = resultList;
            return View();
        }

        public ActionResult Versions()
        {
            return View();
        }

    }
}
