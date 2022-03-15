using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Services
{
    public class VersionService : IVersionService
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ICommitService _commitService;

        public VersionService(IOptions<AppSettings> appSettings, ICommitService commitService)
        {
            _appSettings = appSettings;
            _commitService = commitService;
        }

        public string GetCurrentClientVersion()
        {
            var clientVersion = string.Empty;

            var clientCommitAt = _appSettings.Value.ClientCommitAt;
            if (string.IsNullOrEmpty(clientCommitAt))
            {
                var firstCommit = _commitService.GetFirstCommitAsync()
                    .GetAwaiter()
                    .GetResult();
                clientVersion = firstCommit.Sha;
            }
            else
            {
                clientVersion = clientCommitAt;
            }

            return clientVersion;
        }

        public bool IsClientUpToDate()
        {
            var lastCommit = _commitService.GetLastCommitAsync()
                .GetAwaiter()
                .GetResult();

            return lastCommit.Sha == GetCurrentClientVersion();
        }

        public bool UpdateClientVersion(string commitSha)
        {
            try
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var appSettingsFile = Path.Combine(currentDirectory, "appsettings.json");

                if (!File.Exists(appSettingsFile))
                {
                    return false;
                }

                var appSettingsString = File.ReadAllText(appSettingsFile, Encoding.UTF8);
                var jsonObject = JsonConvert.DeserializeObject<AppSettings>(appSettingsString);

                if (jsonObject == null) return false;

                jsonObject.ClientCommitAt = commitSha;

                var json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                File.WriteAllText(appSettingsFile, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
