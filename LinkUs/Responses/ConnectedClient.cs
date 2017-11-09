using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.Responses
{
    public class ConnectedClient
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public string OperatingSystem { get; set; }
        public string PublicIp { get; set; }
    }
}