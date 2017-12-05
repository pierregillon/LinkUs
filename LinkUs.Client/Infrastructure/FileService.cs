using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LinkUs.Client.Infrastructure
{
    public class FileService : IFileService
    {
        public void Copy(string sourceFileName, string destinationFileName)
        {
            if (File.Exists(destinationFileName) == false) {
                File.Copy(sourceFileName, destinationFileName);
            }
        }
        public Version GetAssemblyVersion(string fileName)
        {
            return AssemblyName.GetAssemblyName(fileName).Version;
        }
        public IEnumerable<string> GetFiles(string folder, string extension)
        {
            return Directory.EnumerateFiles(folder, extension, SearchOption.AllDirectories);
        }
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
        public string GetRandomFileName(string extension)
        {
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            return fileName + extension;
        }
    }
}