using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tohasoft.Utils
{
    public abstract class Logger
    {
        private readonly List<ILoggerEventSource> _sources = new List<ILoggerEventSource>();

        public virtual void AddSource(ILoggerEventSource source)
        {
            _sources.Add(source);
        }

        protected void AddSourceEventHander(EventHandler<ErrorMessageEventArgs> handler)
        {
            foreach (var source in _sources)
            {
                source.ErrorRaised += handler;
            }
        }
        protected void AddSourceEventHander(EventHandler<MessageEventArgs> handler)
        {
            foreach (var source in _sources)
            {
                source.MessageRaised += handler;
            }
        }
        protected void AddSourceEventHander(EventHandler<WarningMessageEventArgs> handler)
        {
            foreach (var source in _sources)
            {
                source.WarningRaised += handler;
            }
        }
    }
}
