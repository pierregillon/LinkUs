using System;

namespace LinkUs.Client.Install
{
    public class HigherVersionAlreadyInstalled : Exception
    {
        public string FilePath { get; }

        public HigherVersionAlreadyInstalled(string filePath) : base("An application with a higher version is already installed.")
        {
            FilePath = filePath;
        }
    }
}