using System;
using LinkUs.Core.Commands;

namespace LinkUs.Modules.Default.ClientInformation
{
    public class GetBasicInformationCommandHandler : ICommandHandler<GetBasicInformation, ClientBasicInformation>
    {
        public ClientBasicInformation Handle(GetBasicInformation command)
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