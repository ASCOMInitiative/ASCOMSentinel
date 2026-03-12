using System;
namespace ObsMan
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; } = "";
    }
}