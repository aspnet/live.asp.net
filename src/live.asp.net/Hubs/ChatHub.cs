// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using Microsoft.AspNet.SignalR;

namespace live.asp.net.Hubs
{
    public class ChatHub : Hub
    {
        private const string ChatDiv = @"<div class=""panel panel-primary OnAir"">
            <div class=""panel-heading SmallPaddingNoMargin"">Chat</div>
            <div class=""panel-body SmallPaddingNoMargin"">
            <div class=""OnAirTextArea"" id=""chatWindow""></div>
            <input type=""text"" id=""new-message""/></div></div>";
        private const string QuestionDev = @"
            <div class=""panel panel-primary OnAir"">
            <div class=""panel-heading SmallPaddingNoMargin"">Ask a question</div>
            <div class=""panel-body SmallPaddingNoMargin"">
            <textarea id = ""question"" class=""OnAirTextArea""></textarea>
            <button class=""btn btn-primary"" id=""ask"">Ask!</button>
            </div></div>";
        private readonly IOnAir _onAir;

        public ChatHub(IOnAir onAir)
        {
            _onAir = onAir;
        }

        public void Send(string message)
        {
            foreach (Match match in Regex.Matches(message, "url:(.*?) "))
            {
                var url = match.Value.Replace("url:", "").Replace(" ", "");
                message = message.Replace(match.Value, $"<a href=\"{url}\">{url}</a> ");
            }
            if (Context.User.Identity.IsAuthenticated)
                Clients.All.UpdateMessages($"<b>{Context.User.Identity.Name}</b> {message}");
            else
            {
                Clients.All.UpdateMessages(_onAir.GetUser(Context.ConnectionId).Username + " - " + message);
            }
        }

        public void SetChatHandle(string chatHandle)
        {
            if (_onAir.UserExists(Context.ConnectionId))
                _onAir.DelUser(Context.ConnectionId);
            var user = new OnAirChatUsers {ConnectionId = Context.ConnectionId, IsAuth = false, Username = chatHandle};
            _onAir.AddUser(user);

            Clients.Caller.Join(
                $"<div class=\"row\"><div class=\"row\"><div class=\"col-md-9\">{ChatDiv}</div ><div class=\"col-md-3\">{QuestionDev}</div ></div ></div >");
        }

        public void AskQuestion(string message)
        {
            var question = new OnAirQuestions
            {
                UserName =
                    Context.User.Identity.IsAuthenticated
                        ? Context.User.Identity.Name
                        : _onAir.GetUser(Context.ConnectionId).Username,
                Question = message
            };

            question = _onAir.AddQuestion(question);

            var adminMenu =
                $"<a href=\"{question.Id}\" id=\"{question.Id}\" action=\"Answer\" class=\"adminMenu\">Answer</a>";
            //Working around "group issue with SignalR?"
            foreach (var user in _onAir.GetUsers())
            {
                Clients.Client(user.ConnectionId).NewQuestion(question, user.IsAuth ? adminMenu : "");
            }
        }

        public void UpdateQuestion(int id, string action)
        {
            var question = _onAir.GetQuestions().FirstOrDefault(q => q.Id == id);
            if (question == null) throw new ArgumentNullException(nameof(question));
            var adminMenu = "";
            switch (action)
            {
                case "PlusOne":
                    question.Vote++;
                    break;
                case "Answer":
                    if (Context.User.Identity.IsAuthenticated)
                    {
                        question.Vote = int.MaxValue;
                        adminMenu =
                            $"<a href=\"{question.Id}\" id=\"{question.Id}\" action=\"Answered\" class=\"adminMenu\">Answered</a>";
                        question.Answering = true;
                    }
                    break;
                case "Answered":
                    if (Context.User.Identity.IsAuthenticated)
                    {
                        question.Answering = false;
                        question.Vote = int.MinValue;
                        question.Answered = true;
                    }
                    break;
            }
            Clients.All.UpdateQuestion(question, adminMenu);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Clients.All.UpdateMessages(Clients.Caller.username + " - Goodbye");

            return base.OnDisconnected(stopCalled);
        }

        public override async Task OnConnected()
        {
            await base.OnConnected();
            if (Context.User.Identity.IsAuthenticated)
            {
                Clients.Caller.Join(
                    $"<div class=\"row\"><div class=\"row\"><div class=\"col-md-9\">{ChatDiv}</div ><div class=\"col-md-3\">{QuestionDev}</div ></div ></div >");
                _onAir.AddUser(new OnAirChatUsers
                {
                    Username = Context.User.Identity.Name,
                    ConnectionId = Context.ConnectionId,
                    IsAuth = true
                });
            }
            foreach (var question in _onAir.GetQuestions())
            {
                var adminMenu = "";
                if (Context.User.Identity.IsAuthenticated)
                {
                    if (!question.Answered && !question.Answering)
                        adminMenu =
                            $"<a href=\"{question.Id}\" id=\"{question.Id}\" action=\"Answer\" class=\"adminMenu\">Answer</a>";
                    else if (question.Answering)
                        adminMenu =
                            $"<a href=\"{question.Id}\" id=\"{question.Id}\" action=\"Answered\" class=\"adminMenu\">Answered</a>";
                }

                Clients.Caller.NewQuestion(question, adminMenu);
            }
        }
    }
}