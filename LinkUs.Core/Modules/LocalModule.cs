using System;
using System.Collections.Generic;
using LinkUs.Core.PingLib;

namespace LinkUs.Core.Modules
{
    public class LocalModule : IModule
    {
        public IEnumerable<Type> AvailableHandlers
        {
            get { yield return typeof(PingHandler); }
        }
    }
}