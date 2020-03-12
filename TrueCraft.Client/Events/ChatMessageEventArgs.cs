using System;

namespace TrueCraft.Client.Events
{
    public class ChatMessageEventArgs : EventArgs
    {
        public ChatMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}