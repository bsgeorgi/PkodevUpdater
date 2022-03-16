using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Octokit;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Services
{
    public class PatchService : IPatchService
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ICommitService _commitService;

        public PatchService(IOptions<AppSettings> appSettings, ICommitService commitService)
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
        public async Task<string> GetCurrentClientVersionAsync()
        {
            var clientVersion = string.Empty;

            var clientCommitAt = _appSettings.Value.ClientCommitAt;
            if (string.IsNullOrEmpty(clientCommitAt))
            {
                var firstCommit = await _commitService.GetFirstCommitAsync()
                    .ConfigureAwait(false);
                clientVersion = firstCommit.Sha;
            }
            else
            {
                clientVersion = clientCommitAt;
            }

            return clientVersion;
        }

        public async Task<string> GetActualClientVersionAsync()
        {
            var lastCommit = await _commitService.GetLastCommitAsync()
                .ConfigureAwait(false);

            return lastCommit.Sha ?? string.Empty;
        }

        /// <summary>
        /// Identifies if currently installed client is up to date.
        /// </summary>
        /// <returns>A boolean value indicating if client is up to date.</returns>
        public async Task<bool> IsClientUpToDateAsync()
        {
            var actualClientVersion = await GetActualClientVersionAsync();
            var currentClientVersion = await GetCurrentClientVersionAsync();

            if (string.IsNullOrEmpty(actualClientVersion))
            {
                // TODO: throw exception rather than simply return false
                return false;
            }

            return actualClientVersion == currentClientVersion;
        }

        /// <summary>
        /// Tries to update the current version of client stored inside appsettings.json file.
        /// </summary>
        /// <param name="commitSha">Hash that will be saved in appsettings.json file.</param>
        /// <returns>A boolean value indicating if version has been saved successfully.</returns>
        public bool SetClientVersion(string commitSha)
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

        private async Task<IEnumerable<GitHubCommit>> GetMissingCommitsAsync()
        {
            var missingCommits = new List<GitHubCommit>();

            var currentClientVersion = await GetCurrentClientVersionAsync();
            var actualClientVersion = await GetActualClientVersionAsync();

            if (currentClientVersion == actualClientVersion)
            {
                return missingCommits;
            }

            var allCommits = await _commitService.GetAllCommitsAsync();

            var gitHubCommits = allCommits as GitHubCommit[] ?? allCommits.ToArray();
            if (!gitHubCommits.Any()) return missingCommits;

            missingCommits.AddRange(gitHubCommits.TakeWhile(commit => commit.Sha != currentClientVersion));

            // Reverse items in missingCommits list
            // Since we want to update the client chronologically
            missingCommits.Reverse();

            return missingCommits;
        }

        public async Task<Queue<CommitFile>> GetUpdateQueueAsync()
        {
            var updateQueue = new Queue<CommitFile>();
            var missingCommits = await GetMissingCommitsAsync();

            var gitHubCommits = missingCommits as GitHubCommit[] ?? missingCommits.ToArray();
            if (!gitHubCommits.Any()) return updateQueue;

            foreach (var commit in gitHubCommits)
            {
                // For some reason we can't directly access commit.Files
                // So we need to retrieve it from commit service
                var commitInfo = await _commitService.GetCommitInfoAsync(commit.Sha);

                // Skip this commit if there are no updates for some reason
                if (!commitInfo.Files.Any()) continue;

                foreach (var commitFile in commitInfo.Files)
                {
                    updateQueue.Enqueue(new CommitFile
                    {
                        Name = commitFile.Filename,
                        Status = commitFile.Status,
                        Url = commitFile.RawUrl
                    });
                }
            }

            return updateQueue;
        }
    }
}
