using System;

namespace LinkUs.Client
{
    public interface IFileService
    {
        bool Exists(string filePath);
        string GetRandomFileName(string extension);
        void Copy(string source, string target);
        Version GetAssemblyVersion(string fileName);
    }
}