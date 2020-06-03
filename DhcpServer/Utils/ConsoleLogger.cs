using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tohasoft.Utils
{
    public class ConsoleLogger : Logger
    {
        public ConsoleLogger()
        {

        }

        public override void AddSource(ILoggerEventSource source)
        {
            base.AddSource(source);
            base.AddSourceEventHander(ErrorMessage_Raised);

        }

        private void ErrorMessage_Raised(object sender, ErrorMessageEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now:MM/dd HH: mm:ss}");
        }
    }
}
