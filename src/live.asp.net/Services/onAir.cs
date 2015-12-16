// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public interface IOnAir
    {
        OnAirQuestions AddQuestion(OnAirQuestions onAirQuestions);
        Collection<OnAirQuestions> GetQuestions();
        OnAirChatUsers AddUser(OnAirChatUsers user);
        OnAirChatUsers GetUser(string connectionId);
        void DelUser(string connectionId);
        bool UserExists(string connectionId);
        Collection<OnAirChatUsers> GetUsers();
        void Reset();
    }

    public class OnAir : IOnAir
    {
        private int _questionIndex;
        private Collection<OnAirQuestions> _questionLogs;
        private Collection<OnAirChatUsers> _users;

        public OnAir()
        {
            _questionLogs = new Collection<OnAirQuestions>();
            _users = new Collection<OnAirChatUsers>();
        }

        public void Reset()
        {
            _questionLogs = new Collection<OnAirQuestions>();
            _users = new Collection<OnAirChatUsers>();
            _questionIndex = 0;
        }


        public OnAirChatUsers AddUser(OnAirChatUsers user)
        {
            _users.Add(user);
            return user;
        }

        public OnAirChatUsers GetUser(string connectionId)
        {
            return _users?.FirstOrDefault(u => connectionId != null && connectionId == u.ConnectionId);
        }

        public void DelUser(string connectionId)
        {
            var user = _users.FirstOrDefault(u => connectionId != null && connectionId == u.ConnectionId);
            _users.Remove(user);
        }

        public bool UserExists(string connectionId)
        {
            if (_users != null)
                return _users.Any(user => user.ConnectionId == connectionId);
            return false;
        }

        public Collection<OnAirChatUsers> GetUsers()
        {
            return _users;
        }

        public Collection<OnAirQuestions> GetQuestions()
        {
            return _questionLogs;
        }

        public OnAirQuestions AddQuestion(OnAirQuestions onAirQuestion)
        {
            if (onAirQuestion.Question == null || onAirQuestion.UserName == null)
                return null;
            onAirQuestion.Id = _questionIndex;

            _questionLogs.Add(onAirQuestion);
            _questionIndex++;

            return onAirQuestion;
        }
    }
}