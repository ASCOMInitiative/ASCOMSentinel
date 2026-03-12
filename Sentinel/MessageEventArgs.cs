using System;
namespace Sentinel
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; } = "";
    }
}