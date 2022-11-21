using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Services
{
    /// <summary>
    /// Roms management
    /// </summary>
    public class Roms : IRoms
    {
        private readonly List<string> bioslist = new();
        private readonly BiosMatchList biosmatch = new();
        private readonly ICsv csvService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Roms"/> class.
        /// </summary>
        /// <param name="csvService">The CSV service.</param>
        public Roms(ICsv csvService)
        {
            this.csvService = csvService;

            this.bioslist = File.ReadAllLines(Path.Join(ArcadeManagerEnvironment.BasePath, "Data", "bioslist.txt")).ToList();
            this.biosmatch.AddRange(File.ReadAllLines(Path.Join(ArcadeManagerEnvironment.BasePath, "Data", "biosmatch.csv"))
                .Select(l => l.Split(";".ToCharArray()))
                .Select(l => new BiosMatch(l[0], l[1])));
        }

        /// <summary>
        /// Copies roms
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
        /// <exception cref="DirectoryNotFoundException">Unable to find romset folder {args.romset}</exception>
        public async Task Add(Actions.RomsAction args, IMessageHandler messageHandler)
        {
            messageHandler.Init("Copying roms");

            try
            {
                // check files and folders
                if (!File.Exists(args.main)) { throw new FileNotFoundException("Unable to find main CSV file", args.main); }
                if (!Directory.Exists(args.romset)) { throw new DirectoryNotFoundException($"Unable to find romset folder {args.romset}"); }
                if (!Directory.Exists(args.selection)) { Directory.CreateDirectory(args.selection); }

                // read CSV file
                var content = await csvService.ReadFile(args.main, false);

                var total = content.Games.Count;
                var i = 0;
                var copied = 0;

                // copy each file found in CSV
                foreach (var game in content.Games.Select(g => g.Name))
                {
                    if (messageHandler.MustCancel) { break; }

                    i++;

                    // build vars
                    var zip = $"{game}.zip";
                    var sourceRom = Path.Join(args.romset, zip);
                    var ext = "zip";

                    // always display progress
                    messageHandler.Progress(game, total, i);

                    // check that source rom exists
                    if (!File.Exists(sourceRom))
                    {
                        zip = $"{game}.7z";
                        sourceRom = Path.Join(args.romset, zip);
                        ext = "7z";
                    }

                    // still not found: next
                    if (!File.Exists(sourceRom))
                    {
                        continue;
                    }

                    var destRom = Path.Join(args.selection, zip);
                    var fi = new FileInfo(sourceRom);

                    // replace progress with file size (so the user knows when a file is large)
                    messageHandler.Progress($"{game} ({FileSystem.HumanSize(fi.Length)})", total, i);

                    // copy rom
                    if (!File.Exists(destRom) || args.overwrite)
                    {
                        File.Copy(sourceRom, destRom, true);
                        copied++;
                    }

                    // try to copy bios if it's used and can be found
                    var biosesforgame = this.biosmatch.GetBiosesForGame(game);
                    foreach (var bios in biosesforgame)
                    {
                        var sourcebios = Path.Join(args.romset, $"{bios}.{ext}");
                        var destbios = Path.Join(args.selection, $"{bios}.{ext}");
                        if (File.Exists(sourcebios) && !File.Exists(destbios))
                        {
                            File.Copy(sourcebios, destbios);
                            copied++;
                        }
                    }

                    // try to copy chd if it can be found
                    var sourceChd = Path.Join(args.romset, game);
                    var targetChd = Path.Join(args.selection, game);
                    if (Directory.Exists(sourceChd))
                    {
                        if (messageHandler.MustCancel) { break; }

                        messageHandler.Progress($"Copying {game} CHD ({FileSystem.HumanSize(FileSystem.DirectorySize(sourceChd))})", total, i);

                        copied += FileSystem.DirectoryCopy(sourceChd, targetChd, args.overwrite, false);
                    }
                }

                messageHandler.Done($"Copied {copied} file(s)", args.selection);
            }
            catch (Exception ex)
            {
                messageHandler.Error(ex);
            }
        }

        /// <summary>
        /// Deletes roms from a folder
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
        /// <exception cref="DirectoryNotFoundException">Unable to find selection folder {args.selection}</exception>
        public async Task Delete(Actions.RomsAction args, IMessageHandler messageHandler)
        {
            messageHandler.Init("Deleting roms");

            try
            {
                // check files and folders
                if (!File.Exists(args.main)) { throw new FileNotFoundException("Unable to find main CSV file", args.main); }
                if (!Directory.Exists(args.selection)) { throw new DirectoryNotFoundException($"Unable to find selection folder {args.selection}"); }

                // read CSV file
                var content = await csvService.ReadFile(args.main, false);

                var total = content.Games.Count;
                var i = 0;
                var deleted = 0;

                foreach (var game in content.Games.Select(g => g.Name))
                {
                    if (messageHandler.MustCancel) { break; }
                    i++;

                    // build vars
                    var zip = $"{game}.zip";
                    var filePath = Path.Join(args.selection, zip);

                    messageHandler.Progress(game, total, i);

                    // check that source rom exists
                    if (!File.Exists(filePath))
                    {
                        zip = $"{game}.7z";
                        filePath = Path.Join(args.romset, zip);
                    }

                    // still not found: next
                    if (!File.Exists(filePath))
                    {
                        continue;
                    }

                    File.Delete(filePath);
                    deleted++;
                }

                messageHandler.Done($"Deleted {deleted} file(s)", args.selection);
            }
            catch (Exception ex)
            {
                messageHandler.Error(ex);
            }
        }

        /// <summary>
        /// Keeps only listed roms in a folder
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
        /// <exception cref="DirectoryNotFoundException">Unable to find selection folder {args.selection}</exception>
        public async Task Keep(Actions.RomsAction args, IMessageHandler messageHandler)
        {
            messageHandler.Init("Filtering roms");

            try
            {
                // check files and folders
                if (!File.Exists(args.main)) { throw new FileNotFoundException("Unable to find main CSV file", args.main); }
                if (!Directory.Exists(args.selection)) { throw new DirectoryNotFoundException($"Unable to find selection folder {args.selection}"); }

                // read CSV file
                var content = await csvService.ReadFile(args.main, false);

                // get list of files
                var di = new DirectoryInfo(args.selection);
                var files = di.GetFiles("*.zip").ToList();
                files.AddRange(di.GetFiles("*.7z"));

                var total = content.Games.Count;
                var i = 0;
                var deleted = 0;

                // check if files exist in games list
                foreach (var f in files)
                {
                    if (messageHandler.MustCancel) { break; }
                    i++;

                    messageHandler.Progress(f.Name, total, i);

                    var nameNoExt = f.Name.Contains('.') ? f.Name.Substring(0, f.Name.LastIndexOf(".")) : f.Name;

                    // don't auto-delete bios files
                    if (this.bioslist.Contains(nameNoExt, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    // delete if it's not found in the provided list
                    if (!content.Games.Any(c => c.Name.Equals(nameNoExt, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        File.Delete(f.FullName);
                        deleted++;
                    }
                }

                messageHandler.Done($"Deleted {deleted} files", args.selection);
            }
            catch (Exception ex)
            {
                messageHandler.Error(ex);
            }
        }

        /// <summary>
        /// A game/bios match
        /// </summary>
        private sealed class BiosMatch
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BiosMatch"/> class.
            /// </summary>
            /// <param name="game">The game.</param>
            /// <param name="bios">The bios.</param>
            public BiosMatch(string game, string bios)
            {
                this.Game = game;
                this.Bios = bios;
            }

            /// <summary>
            /// Gets or sets the bios name.
            /// </summary>
            public string Bios { get; set; }

            /// <summary>
            /// Gets or sets the game name.
            /// </summary>
            public string Game { get; set; }
        }

        /// <summary>
        /// A game/bios matches list
        /// </summary>
        /// <seealso cref="System.Collections.Generic.List&lt;ArcadeManager.Services.Roms.BiosMatch&gt;"/>
        private sealed class BiosMatchList : List<BiosMatch>
        {
            /// <summary>
            /// Gets the bioses for a game (it might change between MAME/FBNeo versions).
            /// </summary>
            /// <param name="game">The game name.</param>
            /// <returns>The potential bioses names</returns>
            public IEnumerable<string> GetBiosesForGame(string game)
            {
                var result = new List<string>();
                foreach (var item in this.Where(bm => bm.Game.Equals(game, StringComparison.InvariantCultureIgnoreCase)))
                {
                    result.Add(item.Bios);
                }

                return result;
            }
        }
    }
}