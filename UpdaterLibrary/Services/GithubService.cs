#nullable enable
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
    public class GithubService : IGithubService
    {
        private readonly IOctokitGithubClientFactory _octokitGithubClientFactory;
        private readonly IOptions<AppSettings> _appSettings;

        public GithubService(IOctokitGithubClientFactory octokitGithubClientFactory, IOptions<AppSettings> appSettings)
        {;
            _octokitGithubClientFactory = octokitGithubClientFactory;
            _appSettings = appSettings;
        }

        public async Task<Repository?> GetRepositoryAsync()
        {
            if (string.IsNullOrWhiteSpace(_appSettings.Value.RepositoryName) ||
                string.IsNullOrWhiteSpace(_appSettings.Value.Owner))
            {
                throw new EmptySettingsException();
            }

            var githubClient = _octokitGithubClientFactory.CreateGitHubClient();
            var repository = await githubClient.Repository.Get(_appSettings.Value.Owner,
                _appSettings.Value.RepositoryName);

            return repository;
        }

        public async Task<IEnumerable<GitHubCommit>?> GetAllCommitsAsync()
        {
            if (string.IsNullOrWhiteSpace(_appSettings.Value.RepositoryName) ||
                string.IsNullOrWhiteSpace(_appSettings.Value.Owner))
            {
                throw new EmptySettingsException();
            }

            var githubClient = _octokitGithubClientFactory.CreateGitHubClient();

            var repository = await GetRepositoryAsync();

            if (repository == null) return null;
            var commits = await githubClient.Repository.Commit.GetAll(repository.Id);

            return commits.ToList();
        }
    }
}
