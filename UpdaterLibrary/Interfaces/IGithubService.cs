using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace UpdaterLibrary.Interfaces
{
    public interface IGithubService
    {
        public Task<Repository?> GetRepositoryAsync();

        public Task<IEnumerable<GitHubCommit>?> GetAllCommitsAsync();
    }
}
