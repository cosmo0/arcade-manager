using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ArcadeManager.Actions;
using ArcadeManager.Models;

namespace ArcadeManager.Services
{
    public class Downloader
    {
        private const string protocol = "https:";
        private const string api = "api.github.com";
        private const string raw = "raw.githubusercontent.com";

        /// <summary>
        /// Returns the list of available CSV files in the specified repository folder
        /// </summary>
        /// <param name="data">The download parameters</param>
        /// <returns>The list of files</returns>
        public static async Task<IEnumerable<CsvFile>> GetList(DownloadAction data)
        {
            // get the content of the JSON file that lists the possible CSV files and their descriptions
            var descriptor = await Downloader.DownloadFile<CsvFilesList>(data.repository, data.details);

            // list the actual files in the folder
            var files = await Downloader.ListFiles(data.repository, data.folder);

            // return the files that match in both lists
            return descriptor.files.Where(d => files.tree.Any((f) => f.path == d.filename));
        }

        /// <summary>
        /// Downloads a binary file
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="filePath">The file path</param>
        /// <param name="localPath">The local file path to save</param>
        /// <returns>The downloaded file data</returns>
        public static async Task DownloadFile(string repository, string filePath, string localPath)
        {
            var url = $"{protocol}//{raw}/{repository}/master/{filePath}";

            using (var wc = new ArcadeManagerWebClient())
            {
                await wc.DownloadFileTaskAsync(url, localPath);
            }
        }

        /// <summary>
        /// Downloads a JSON file and deserializes it to T
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="data">The download parameters</param>
        /// <returns>The downloaded file</returns>
        public static async Task<T> DownloadFile<T>(string repository, string filePath)
        {
            var url = $"{protocol}//{raw}/{repository}/master/{filePath}";

            using (var wc = new ArcadeManagerWebClient())
            {
                return Serializer.Deserialize<T>(await wc.DownloadStringTaskAsync(url));
            }
        }

		/// <summary>
        /// Lists the files in a Github folder
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="folder">The folder path</param>
        /// <returns>The list of files</returns>
        public static async Task<GithubTree> ListFiles(string repository, string folder)
        {
			// get level-up folder to get the SHA of the folder - easy to access but limited to 1000 files docs.github.com/en/rest/reference/repos#get-repository-content
			var urlUpFolders = $"{protocol}//{api}/repos/{repository}/contents/{folder.Substring(0, folder.LastIndexOf("/"))}";

			using (var wc = new ArcadeManagerWebClient())
            {
				var data = Serializer.Deserialize<IEnumerable<GithubContent>>(await wc.DownloadStringTaskAsync(urlUpFolders));

				// get SHA of the folder we're insterested in
				var sha = data.Where(d => d.path == folder).Select(d => d.sha).First();

				// get files list through git/tree, which has a much higher limit of items - docs.github.com/en/rest/reference/git#get-a-tree
				var filesListUrl = $"{protocol}//{api}/repos/{repository}/git/trees/{sha}";
				return Serializer.Deserialize<GithubTree>(await wc.DownloadStringTaskAsync(filesListUrl));
            }
        }

        /// <summary>
        /// Custom webclient to set the useragent at each request
        /// </summary>
        /// <remarks>
        /// Copied from stackoverflow.com/a/18516970/6776
        /// </remarks>
        private class ArcadeManagerWebClient : WebClient
        {
            private const string userAgent = "arcade-manager (cosmo0/arcade-manager)";

            public ArcadeManagerWebClient() { }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address) as HttpWebRequest;
                request.UserAgent = userAgent;

                return request;
            }
        }
    }
}
