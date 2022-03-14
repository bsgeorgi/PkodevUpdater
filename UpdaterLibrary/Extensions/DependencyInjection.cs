using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Services;

namespace UpdaterLibrary.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServiceCollection(this IServiceCollection services)
        {
            services.TryAddSingleton<IOctokitGithubClientFactory, OctokitGithubClientFactory>();
            services.TryAddSingleton<IRepositoryService, RepositoryService>();
            services.TryAddSingleton<ICommitService, CommitService>();
            services.TryAddSingleton<IVersionService, VersionService>();
            services.TryAddSingleton<IPatchService, PatchService>();

            return services;
        }
    }
}
