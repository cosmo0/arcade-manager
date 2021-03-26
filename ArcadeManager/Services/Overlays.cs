using ArcadeManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	public class Overlays {

		/// <summary>
		/// Downloads an overlay pack
		/// </summary>
		/// <param name="data">The parameters</param>
		/// <param name="progressor">The progressor</param>
		public static async Task Download(Actions.OverlaysAction data, MessageHandler.Progressor progressor) {
			progressor.Init("Download overlay pack");

			try {
				var os = ArcadeManagerEnvironment.SettingsOs;
				var pack = ArcadeManagerEnvironment.AppData.Overlays.Where(o => o.Name == data.pack).First();

				// check if the destination of rom cfg is the rom folder
				var romCfgFolder = pack.Roms.Dest[os] == "roms"
					? null // save rom cfg directly into rom folder(s)
					: Path.Join(data.configFolder, pack.Roms.Dest[os]); // save rom cfg in config folder

				// list the available rom configs
				progressor.Progress("Listing files to download", 1, 100);
				var romConfigs = await Downloader.ListFiles(pack.Repository, pack.Roms.Src);

				int installed = 0;
				var total = romConfigs.tree.Count;

				progressor.Progress("Downloading common files", 1, 100);

				// download common files
				if (pack.Common != null && !string.IsNullOrWhiteSpace(pack.Common.Src)) {
					await DownloadCommon(pack, Path.Join(data.configFolder, pack.Common.Dest[os]), data.overwrite, data.ratio, progressor, total, 1);
				}

				// check that there is a matching game in any of the roms folders

				// download the rom config and extract the overlay file name

				// download the overlay file name and extract the image file name

				// download the image

				// resize the overlay coordinates if necessary

				progressor.Done($"Installed {installed} overlay packs", null);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Changes the resolution in a file content.
		/// </summary>
		/// <param name="content">The file content.</param>
		/// <param name="ratio">The ratio.</param>
		/// <returns>The modified content</returns>
		private static string ChangeResolution(string content, float ratio) {
			if (ratio == 1) {
				return content;
			}

			var parameters = new List<string> {
				"custom_viewport_width",
				"custom_viewport_height",
				"custom_viewport_x",
				"custom_viewport_y",
				"video_fullscreen_x",
				"video_fullscreen_y"
			};

			foreach (var p in parameters) {
				var regex = new Regex(p + "[\\s]*=[\\s]*\"?(\\d+)\"?", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				var foundValue = regex.Match(content).Groups[1].Value;
				if (double.TryParse(foundValue, out var value)) {
					value = Math.Round(value * ratio, 0);
					content = regex.Replace(content, $"{p} = {(int)value}");
				}
			}

			return content;
		}

		/// <summary>
		/// Downloads the common files.
		/// </summary>
		/// <param name="pack">The overlay pack to download.</param>
		/// <param name="destination">The destination path.</param>
		/// <param name="overwrite">if set to <c>true</c> overwrite existing files.</param>
		/// <param name="ratio">The ratio to change resolution.</param>
		/// <param name="progressor">The progressor.</param>
		/// <returns>
		/// How many files have been downloaded
		/// </returns>
		private static async Task DownloadCommon(OverlayBundle pack, string destination, bool overwrite, float ratio, MessageHandler.Progressor progressor, int total, int current) {
			if (MessageHandler.MustCancel) { return; }

			if (pack.Common != null && !string.IsNullOrEmpty(pack.Common.Src)) {
				IEnumerable<string> files = await Downloader.DownloadFolder(pack.Repository, pack.Common.Src, destination, overwrite, (entry) => {
					progressor.Progress($"downloading {entry.path}", total, current);
				});

				foreach (var f in files.Where(f => f.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))) {
					var fi = new FileInfo(f);
					progressor.Progress($"fixing {fi.Name}", total, current);

					if (overwrite || !fi.Exists) {
						var content = await File.ReadAllTextAsync(f);

						content = ChangeResolution(content, ratio);
						content = FixPaths(content, pack);

						await File.WriteAllTextAsync(f, content);
					}
				};
			}
		}

		/// <summary>
		/// Fixes the paths in a config.
		/// </summary>
		/// <param name="content">The config content.</param>
		/// <param name="pack">The pack to download.</param>
		/// <returns>
		/// The fixed content
		/// </returns>
		private static string FixPaths(string content, OverlayBundle pack) {
			if (ArcadeManagerEnvironment.SettingsOs != pack.BaseOs) {
				return content.Replace(pack.Base[pack.BaseOs], pack.Base[ArcadeManagerEnvironment.SettingsOs], StringComparison.InvariantCultureIgnoreCase);
			}

			return content;
		}
	}
}
