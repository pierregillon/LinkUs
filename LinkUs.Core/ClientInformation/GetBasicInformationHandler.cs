using System;

namespace LinkUs.Core.ClientInformation
{
    public class GetBasicInformationHandler : IHandler<GetBasicInformation, ClientBasicInformation>
    {
        public ClientBasicInformation Handle(GetBasicInformation commandLine)
        {
            return new ClientBasicInformation {
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                OperatingSystem = Environment.OSVersion.Platform.ToString(),
                PublicIp = NetHelper.GetPublicIp()
            };
        }
    }
}