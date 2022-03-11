using Microsoft.Extensions.Options;
using Octokit;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Interfaces
{
    public interface IOctokitGithubClientFactory
    {
        public GitHubClient CreateGitHubClient();

        public IOptions<AppSettings> AppSettings { get; set; }
    }
}
