using System;
using System.IO;
using System.Linq;
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
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
        public string GetRandomFileName()
        {
            return Path.GetRandomFileName().Replace(".", "").Substring(0, 10) + ".exe";
        }
        public string GetFileNameCopiedFromExisting(string directoryPath)
        {
            try {
                var files = Directory.GetFiles(directoryPath);
                var random = new Random((int) DateTime.Now.Ticks);
                var index = random.Next(files.Length);

                for (var i = 0; i < 5; i++) {
                    var fileName = Path.GetFileName(files[index]);
                    var extension = Path.GetExtension(fileName);
                    if (string.IsNullOrEmpty(extension) == false) {
                        fileName = fileName.Replace(extension, string.Empty);
                    }
                    fileName += random.Next(10, 30) + ".exe";
                    if (files.All(x => Path.GetFileName(x) != fileName)) {
                        return fileName;
                    }
                }
                return null;
            }
            catch (Exception) {
                return null;
            }
        }
    }
}