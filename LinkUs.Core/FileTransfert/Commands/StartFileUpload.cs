﻿namespace LinkUs.Core.FileTransfert.Commands
{
    public class StartFileUpload
    {
        public string DestinationFilePath { get; set; }
        public long Length { get; set; }
    }
}
