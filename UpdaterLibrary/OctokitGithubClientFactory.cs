using Microsoft.Extensions.Options;
using Octokit;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace UpdaterLibrary
{
    public class OctokitGithubClientFactory : IOctokitGithubClientFactory
    {
        public IOptions<AppSettings> AppSettings { get; set; }

        public OctokitGithubClientFactory(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings;
        }

        public GitHubClient? CreateGitHubClient()
        {
            if (string.IsNullOrEmpty(AppSettings.Value.ApiKey)) return null;

            var credentials = new Credentials(AppSettings.Value.ApiKey);

            // ProductHeaderValue is used to generate the User Agent string sent with each request
            var client = new GitHubClient(new ProductHeaderValue(AppSettings.Value.ProductHeader))
            {
                Credentials = credentials
            };

            return client;
        }
    }
}