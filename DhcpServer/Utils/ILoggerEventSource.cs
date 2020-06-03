using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tohasoft.Utils
{
   public  class WarningMessageEventArgs : MessageEventArgs
    {

    }

    public class ErrorMessageEventArgs : MessageEventArgs
    {

    }

    public class MessageEventArgs : EventArgs
    {
        public string Message;
    }


    public interface ILoggerEventSource
    {
        event EventHandler<WarningMessageEventArgs> WarningRaised;
        event EventHandler<ErrorMessageEventArgs> ErrorRaised;
        event EventHandler<MessageEventArgs> MessageRaised;
        
    }
}
