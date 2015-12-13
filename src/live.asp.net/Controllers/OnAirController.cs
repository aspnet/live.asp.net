using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace live.asp.net.Controllers
{
 
    [Route("/OnAir")]
    public class OnAirController : Controller
    {
        private IOnAir _onAir;

        public OnAirController(IOnAir onAir)
        {
            _onAir = onAir;
        }

        [HttpGet]
        [Route("/onAir/getdata/{lastDateString?}")]
        public JsonResult GetData(string lastDateString)
        {
            DateTime lastDate = new DateTime();
            bool gotLastDate = true;
            try
            {
                lastDate = DateTime.ParseExact(lastDateString, "yyyyMMddHHmmssff", CultureInfo.InvariantCulture);

            }
            catch (FormatException)
            {
                gotLastDate = false;
            }
            ChatAndQuestions chatAndQuestions = new ChatAndQuestions();
            chatAndQuestions.chats = new List<OnAirChat>();
            chatAndQuestions.questions = new List<OnAirQuestions>();
            if (gotLastDate)
            {
                foreach (OnAirChat chat in _onAir.GetChat())
                {
                    int result = 0;
                    result = DateTime.Compare(chat.TimeStamp, lastDate);
                    if (result > 0)
                    {
                        chatAndQuestions.chats.Add(chat);
                    }
                }
                foreach (OnAirQuestions question in _onAir.GetQuestions())
                {
                    int result = 0;
                    result = DateTime.Compare(question.TimeStamp, lastDate);
                    if (result > 0)
                    {
                        if (User.Identity.IsAuthenticated)
                            if (question.Answering)
                                question.AdminMenu = "<a href=\"/onAir/Answered/" + question.Id +
                                                     "\" class=\"PlusOne\">Answered</a>";
                            else
                                question.AdminMenu = "<a href=\"/onAir/Answer/" + question.Id +
                                                     "\" class=\"PlusOne\">Answer</a>";
                        else
                        {
                            question.AdminMenu = "";
                        }
                        chatAndQuestions.questions.Add(question);
                    }
                }
            }


            chatAndQuestions.LastTime = DateTime.Now.ToString("yyyyMMddHHmmssff");
            return Json(chatAndQuestions);
        }
        
        [HttpPost]
        [Route("/onAir/chat")]
        public string Chat(OnAirChat model)
        {
            model.TimeStamp = DateTime.Now;
            model.Message = WebUtility.HtmlEncode(model.Message);
            model.UserName = WebUtility.HtmlEncode(model.UserName);
            if (User.Identity.IsAuthenticated)
            {
                model.UserName = $"<b>{User.Identity.Name}</b>";
                model.Message = $"<b>{model.Message}</b>";
            }
                foreach (Match match in Regex.Matches(model.Message, "url:(.*?) "))
            {
                string url = match.Value.Replace("url:", "").Replace(" ", "");
                model.Message = model.Message.Replace(match.Value, $"<a href=\"{url}\">{url}</a> ");
            }
            
            if (model.UserName != null && model.Message != null)
            {
                _onAir.AddChat(model);
            }
            return "";
        }

        [HttpPost]
        [Route("/onAir/Join")]
        public IActionResult Join(string userName)
        {
            userName = WebUtility.HtmlEncode(userName);
            Response.Cookies.Append("UserName", userName);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [Route("/onAir/AskQuestion")]
        public string AskQuestion(OnAirQuestions model)
        {
            model.TimeStamp = DateTime.Now;
            model.UserName = WebUtility.HtmlEncode(model.UserName);
            model.Vote = 0;
            if (model.Question != null && model.UserName != null)
            {
                _onAir.AddQuestion(model);
            }
            return "";
        }

        [HttpGet]
        [Route("/onAir/PlusOne/{id?}")]
        public void PlusOne(int id)
        {
            foreach (OnAirQuestions question in _onAir.GetQuestions())
            {
                if (question.Id == id)
                {
                    question.Vote++;
                    question.TimeStamp = DateTime.Now;
                }
            }
        }

        [HttpGet]
        [Authorize]
        [Route("/onAir/Answer/{id?}")]
        public void Answer(int id)
        {
            foreach (OnAirQuestions question in _onAir.GetQuestions())
            {
                if (question.Id == id)
                {
                    question.Answering = true;
                    question.Vote = int.MaxValue;
                    question.TimeStamp = DateTime.Now;
                }
            }
        }

        [HttpGet]
        [Authorize]
        [Route("/onAir/Answered/{id?}")]
        public void Answered(int id)
        {
            foreach (OnAirQuestions question in _onAir.GetQuestions())
            {
                if (question.Id == id)
                {
                    question.Answering = false;
                    question.Answered = true;
                    question.Vote = int.MinValue;
                    question.TimeStamp = DateTime.Now;
                }
            }
        }
    }
}
