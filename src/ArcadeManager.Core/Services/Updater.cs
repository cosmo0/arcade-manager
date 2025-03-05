using ArcadeManager.Core;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Github;
using ArcadeManager.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Services;

/// <summary>
/// Application update
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Updater"/> class.
/// </remarks>
/// <param name="downloaderService">The downloader service.</param>
/// <param name="fs">The file system service</param>
/// <param name="environment">The environment data accessor</param>
public class Updater(IDownloader downloaderService, IFileSystem fs, IEnvironment environment) : IUpdater {

    /// <summary>
    /// Checks for app updates.
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <returns>The new release details, if any</returns>
    public async Task<GithubRelease> CheckUpdate(string currentVersion) {
        try {
            var releases = Serializer.Deserialize<IEnumerable<GithubRelease>>(await downloaderService.DownloadApiUrl("cosmo0/arcade-manager", "releases"));
            if (releases != null && releases.Any()) {
                // get latest version
                var release = releases.First(r => !r.Draft && !r.Prerelease);

                // version is ignored
                if (release.Draft || environment.SettingsIgnoredVersionHas(release.TagName)) {
                    return null;
                }

                var version = release.TagName.Replace("v", "");

                // make release readable
                release.PublishedAtLocal = release.PublishedAt.ToShortDateString();
                release.Body = Markdig.Markdown.ToHtml(release.Body);
                foreach (var a in release.Assets) {
                    a.HumanSize = fs.HumanSize(a.Size);
                }

                if (new Version(version) > new Version(currentVersion)) {
                    return release;
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"An error has occurred during update check: {ex.Message}");
        }

        return null;
    }
}