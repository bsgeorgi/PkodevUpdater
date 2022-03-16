using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit;
using UpdaterLibrary.Exceptions;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Services
{
    public class CommitService : ICommitService
    {
        private readonly IOctokitGithubClientFactory _octokitGithubClientFactory;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IRepositoryService _repositoryService;

        public CommitService(IOctokitGithubClientFactory octokitGithubClientFactory,
            IOptions<AppSettings> appSettings,
            IRepositoryService repositoryService)
        {
            _octokitGithubClientFactory = octokitGithubClientFactory;
            _appSettings = appSettings;
            _repositoryService = repositoryService;
        }

        /// <summary>
        /// Retrieves all commits that belong to the current repository.
        /// </summary>
        /// <returns>IEnumerable of GitHubCommit objects.</returns>
        public async Task<IEnumerable<GitHubCommit>?> GetAllCommitsAsync()
        {
            if (string.IsNullOrWhiteSpace(_appSettings.Value.RepositoryName) ||
                string.IsNullOrWhiteSpace(_appSettings.Value.Owner))
            {
                throw new EmptySettingsException();
            }

            var gitHubClient = _octokitGithubClientFactory.CreateGitHubClient();

            var repository = await _repositoryService.GetRepositoryAsync().ConfigureAwait(false);
            if (repository == null)
            {
                throw new NullReferenceException("Repository cannot be null");
            }

            var commits = await gitHubClient.Repository.Commit.GetAll(repository.Id);
            
            return commits.ToList();
        }

        /// <summary>
        /// Retrieves info associated with the specified hash.
        /// </summary>
        /// <param name="commitSha">A hash of the commit.</param>
        /// <returns>A GitHubCommit object of the specified commit hash.</returns>
        public async Task<GitHubCommit> GetCommitInfoAsync(string commitSha)
        {
            if (string.IsNullOrEmpty(commitSha))
            {
                throw new ArgumentException("Parameter cannot be null", nameof(commitSha));
            }

            var gitHubClient = _octokitGithubClientFactory.CreateGitHubClient();

            var repository = await _repositoryService.GetRepositoryAsync().ConfigureAwait(false);
            if (repository == null)
            {
                throw new NullReferenceException("Repository cannot be null");
            }

            var commitDetails = await gitHubClient.Repository.Commit.Get(repository.Id, commitSha);

            return commitDetails;
        }

        /// <summary>
        /// Retrieves the latest commit from the repository.
        /// </summary>
        /// <returns>A GitHubCommit object.</returns>
        public async Task<GitHubCommit> GetLastCommitAsync()
        {
            var commits = await GetAllCommitsAsync()
                .ConfigureAwait(false);

            if (commits == null)
            {
                throw new NullReferenceException("Repository commits returned null value");
            }

            return commits.First();
        }

        /// <summary>
        /// Retrieves the first commit from the repository.
        /// </summary>
        /// <returns>A GitHubCommit object.</returns>
        public async Task<GitHubCommit> GetFirstCommitAsync()
        {
            var commits = await GetAllCommitsAsync()
                .ConfigureAwait(false);

            if (commits == null)
            {
                throw new NullReferenceException("Repository commits returned null value");
            }

            return commits.Reverse().First();
        }
    }
}
