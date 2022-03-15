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
            // TODO: overwrite ClientCommitAt value in appsettings.json
            // With commitSha value
            // Return true if successful
            return true;
        }
    }
}
