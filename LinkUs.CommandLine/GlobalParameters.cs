using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using IniParser;
using IniParser.Model;

namespace LinkUs.CommandLine
{
    public class GlobalParameters
    {
        private const string FILE_NAME = "lkus.config";
        private readonly string _filePath = GetUserConfigFilePath();

        private readonly FileIniDataParser _fileParser = new FileIniDataParser();
        private readonly IDictionary<string, IDictionary<string, Expression<Func<object>>>> _fileStructure;

        public string ServerHost { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 9000;

        // ----- Constructors
        public GlobalParameters()
        {
            _fileStructure = new Dictionary<string, IDictionary<string, Expression<Func<object>>>> {
                ["server"] = new Dictionary<string, Expression<Func<object>>> {
                    ["host"] = () => ServerHost,
                    ["port"] = () => ServerPort
                }
            };
        }

        // ----- Public methods
        public void Load()
        {
            if (!File.Exists(_filePath) == false) {
                var data = _fileParser.ReadFile(_filePath);
                ServerHost = data["server"]["host"];
                ServerPort = int.Parse(data["server"]["port"]);
            }
        }
        public void Save()
        {
            IniData data;
            if (File.Exists(_filePath) == false) {
                data = new IniData();
            }
            else {
                data = _fileParser.ReadFile(_filePath, Encoding.UTF8);
            }
            foreach (var section in _fileStructure.Keys) {
                foreach (var values in _fileStructure[section]) {
                    var getValue = values.Value.Compile();
                    data[section][values.Key] = getValue()?.ToString();
                }
            }
            _fileParser.WriteFile(_filePath, data, Encoding.UTF8);
        }

        private static string GetUserConfigFilePath()
        {
            // for .net < 4 : System.Environment.GetEnvironmentVariable("USERPROFILE");
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), FILE_NAME);
        }
        public void Edit()
        {
            var process = System.Diagnostics.Process.Start(_filePath);
            process?.WaitForExit();
        }
    }
}