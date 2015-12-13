// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public interface IOnAir
    {
        Collection<OnAirChat> GetChat();
        bool AddChat(OnAirChat onAirChat);
        bool AddQuestion(OnAirQuestions onAirQuestions);
        Collection<OnAirQuestions> GetQuestions();
        void Reset();
    }

    public class OnAir : IOnAir
    {
        private Collection<OnAirChat> _chatLogs;
        private Collection<OnAirQuestions> _questionLogs;
        private int _questionIndex;

        public void Reset()
        {
            _chatLogs = new Collection<OnAirChat>();
            _questionLogs = new Collection<OnAirQuestions>();
            _questionIndex = 0;
        }

        public Collection<OnAirChat> GetChat()
        {
            return _chatLogs;
        }

        public OnAir()
        {
            _chatLogs = new Collection<OnAirChat>();
            _questionLogs = new Collection<OnAirQuestions>();
        }

        public bool AddChat(OnAirChat onAirChat)
        {
            if (onAirChat.Message == null || onAirChat.UserName == null)
                return false;
            _chatLogs.Add(onAirChat);
            return true;
        }

        public Collection<OnAirQuestions> GetQuestions()
        {
            return _questionLogs;
        }

        public bool AddQuestion(OnAirQuestions onAirQuestion)
        {
            if (onAirQuestion.Question == null || onAirQuestion.UserName == null)
                return false;
            onAirQuestion.Id = _questionIndex;
            _questionLogs.Add(onAirQuestion);
            _questionIndex++;

            return true;
        }
    }
}
