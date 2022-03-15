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

        /// <summary>
        /// Retrieves the version of currently installed client
        /// from appsettings.json file.
        /// If version is not present in the appsettings.json
        /// file, then by default first commit hash is used.
        /// </summary>
        /// <returns>A string value representing the hash of current client version.</returns>
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

        /// <summary>
        /// Identifies if currently installed client is up to date.
        /// </summary>
        /// <returns>A boolean value indicating if client is up to date.</returns>
        public bool IsClientUpToDate()
        {
            var lastCommit = _commitService.GetLastCommitAsync()
                .GetAwaiter()
                .GetResult();

            return lastCommit.Sha == GetCurrentClientVersion();
        }

        /// <summary>
        /// Tries to update the current version of client stored inside appsettings.json file.
        /// </summary>
        /// <param name="commitSha">Hash that will be saved in appsettings.json file.</param>
        /// <returns>A boolean value indicating if version has been saved successfully.</returns>
        public bool UpdateClientVersion(string commitSha)
        {
            // TODO: check if commitSha is a valid hash
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
