using Microsoft.Extensions.Options;
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
                var lastCommitHash = _commitService.GetLastCommitAsync()
                    .GetAwaiter()
                    .GetResult();
                clientVersion = lastCommitHash.Sha;
            }
            else
            {
                clientVersion = clientCommitAt;
            }

            return clientVersion;
        }

        public bool IsClientUpToDate()
        {
            // TODO: Check if client is up to date by comparing
            // ClientCommitAt value from appsettings.json against latest
            // Commit Sha from HEAD
            return false;
        }

        public bool UpdateClientVersion(string commitSha)
        {
            // TODO: overwrite ClientCommitAt value in appsettings.json
            // With commitSha value
            // Return true if successful
            return true;
        }
    }
}
