using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public WebClient WebClient;

        public PatchService(IOptions<AppSettings> appSettings, ICommitService commitService)
        {
            _appSettings = appSettings;
            _commitService = commitService;
            WebClient = new WebClient();
        }

        /// <summary>
        /// Retrieves the version of currently installed client
        /// from appsettings.json file.
        /// If version is not present in appsettings.json file
        /// then by default the first commit hash is used.
        /// </summary>
        /// <returns>A string value representing the hash of current client version.</returns>
        public async Task<string> GetCurrentClientVersionAsync()
        {
            string clientVersion;

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

        /// <summary>
        /// Retrieves the most recent hash of client repository.
        /// </summary>
        /// <returns>A string value representing the hash of the last commit in the repository.</returns>
        public async Task<string> GetActualClientVersionAsync()
        {
            var lastCommit = await _commitService.GetLastCommitAsync()
                .ConfigureAwait(false);

            if (lastCommit == null)
            {
                throw new NullReferenceException("GitHub commit is null.");
            }

            return lastCommit.Sha ?? string.Empty;
        }

        /// <summary>
        /// Identifies if currently installed client is up to date.
        /// </summary>
        /// <returns>A boolean value indicating if client is up to date.</returns>
        public async Task<bool> IsClientUpToDateAsync()
        {
            try
            {
                var actualClientVersion = await GetActualClientVersionAsync();
                var currentClientVersion = await GetCurrentClientVersionAsync();

                return actualClientVersion == currentClientVersion;
            }
            catch
            {
                return false;
            }
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
                if (currentDirectory == null)
                {
                    throw new NullReferenceException("Current directory path is null");
                }

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

        /// <summary>
        /// Retrieves a list of commits that are missing in the current client version.
        /// </summary>
        /// <returns>A list of commits.</returns>
        private async Task<IEnumerable<GitHubCommit>> GetMissingCommitsAsync()
        {
            var missingCommits = new List<GitHubCommit>();

            try
            {
                var currentClientVersion = await GetCurrentClientVersionAsync();
                var actualClientVersion = await GetActualClientVersionAsync();

                if (currentClientVersion == actualClientVersion)
                {
                    return missingCommits;
                }

                var allCommits = await _commitService.GetAllCommitsAsync();

                if (allCommits != null)
                {
                    var gitHubCommits = allCommits as GitHubCommit[] ?? allCommits.ToArray();
                    if (!gitHubCommits.Any()) return missingCommits;

                    missingCommits.AddRange(gitHubCommits.TakeWhile(commit => commit.Sha != currentClientVersion));
                }

                // Reverse items in missingCommits list
                // Since we want to update the client chronologically
                if (missingCommits.Any() && missingCommits.Count > 1)
                {
                    missingCommits.Reverse();
                }
            }
            catch
            {
                // ignored
            }

            return missingCommits;
        }

        /// <summary>
        /// Retrieves a queue of updates required to bring the current
        /// client up to date.
        /// </summary>
        /// <returns>A queue of ComitFile objects.</returns>
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

        /// <summary>
        /// Downloads a file to current directory.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        public void DownloadFile(string url, string filePath)
        {
            try
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (currentDirectory == null) return;

                var path = Path.Combine(currentDirectory, filePath);

                // Create a folder if it does not exist for some reason
                var directoryInfo = new FileInfo(path).Directory;
                if (directoryInfo != null)
                {
                    var directory = directoryInfo.FullName;
                    _ = Directory.CreateDirectory(directory);
                }

                WebClient client = new WebClient();
                client.DownloadFileAsync(new Uri(url), path);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Tries to delete the specified file.
        /// </summary>
        /// <param name="file"></param>
        public void TryDeleteFile(string file)
        {
            // TODO: delete dir if it was the last file
            // Delete parent dir too if its child dir is empty
            // For instance:
            // If there is only 1 file in texture/character called: 1.bmp
            // texture/character/1.bmp
            // Then invoking this function should also delete both texture and character folders

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentDirectory == null) return;

            var filePath = Path.Combine(currentDirectory, file);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
