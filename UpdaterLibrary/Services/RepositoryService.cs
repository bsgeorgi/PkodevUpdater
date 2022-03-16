using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit;
using UpdaterLibrary.Exceptions;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IOctokitGithubClientFactory _octokitGithubClientFactory;
        private readonly IOptions<AppSettings> _appSettings;

        public RepositoryService(IOctokitGithubClientFactory octokitGithubClientFactory, IOptions<AppSettings> appSettings)
        {
            _octokitGithubClientFactory = octokitGithubClientFactory;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Retrieves an instance of the current repository.
        /// </summary>
        /// <returns>A Repository instance.</returns>
        public async Task<Repository?> GetRepositoryAsync()
        {
            if (string.IsNullOrWhiteSpace(_appSettings.Value.RepositoryName) ||
                string.IsNullOrWhiteSpace(_appSettings.Value.Owner))
            {
                throw new EmptySettingsException();
            }

            var gitHubClient = _octokitGithubClientFactory.CreateGitHubClient();
            var repository = await gitHubClient.Repository.Get(_appSettings.Value.Owner,
                _appSettings.Value.RepositoryName);

            return repository;
        }
    }
}
