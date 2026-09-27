using ArcadeManager.Core.Actions.Overlays;
using ArcadeManager.Core.Exceptions;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Bezels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ArcadeManager.Core.Domain;

public class MameOverlaysProcessor(IFileSystem fs)
{
    /// <summary>
    /// Factory for the MAME processor
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="lay">The LAY file</param>
    /// <param name="cfg">The CFG file</param>
    /// <returns>The processor</returns>
    public static MameBezel MameGetBezel(MameToRaAction options, MameLayFile lay, MameCfgFile cfg)
    {
        // extract source data
        var view = GetView(lay, options.UseFirstView);
        var sourceRes = GetSourceResolution(view);
        var bezelFile = MameGetBezelFile(lay, view);
        var sourceScreenCoordinates = GetSourceScreenCoordinates(view);
        var off = GetOffset(cfg);

        return new MameBezel(off, sourceRes, sourceScreenCoordinates, bezelFile);
    }

    /// <summary>
    /// Extracts and deserializes the LAY and CFG files
    /// </summary>
    /// <param name="game">The game name.</param>
    /// <param name="zipFile">The zip file path.</param>
    /// <param name="cfgFile">The CFG file path.</param>
    /// <param name="tmpFolder">The temporary folder path.</param>
    /// <returns>The deserialized LAY and CFG files</returns>
    /// <exception cref="LayFileException">
    /// Unable to find a view in the LAY file
    /// </exception>
    public async Task<(MameLayFile lay, MameCfgFile cfg, byte[] bezel)> ExtractFiles(string game, string zipFile, string cfgFile, MameToRaAction options)
    {
        MameLayFile lay = null;
        MameCfgFile cfg = null;
        byte[] bezel = null;

        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Ignore
        };

        // extract files
        using (var archive = fs.OpenZipRead(zipFile))
        {
            // get layout file
            var layEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith("default.lay", StringComparison.InvariantCultureIgnoreCase))
                ?? throw new Exceptions.LayFileException($"Unable to find default.lay file in {zipFile}");

            using (var layStream = await layEntry.OpenAsync())
            {
                using var reader = XmlReader.Create(layStream, settings);
                lay = Serializer.DeserializeXml<MameLayFile>(reader);
            }

            // check that LAY is useful
            if (lay.Views.Length == 0) { throw new Exceptions.LayFileException("Unable to find a view in the LAY file"); }

            // get associated bezel
            var view = GetView(lay, options.UseFirstView);
            var bezelFileNameInLay = MameGetBezelFile(lay, view);
            if (!string.IsNullOrEmpty(bezelFileNameInLay))
            {
                // sometimes the bezel file name in LAY doesn't have the same case as the actual file
                var bezelFileNameInZip = fs.FindFileInZip(archive, bezelFileNameInLay);
                var bezelEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(bezelFileNameInZip, StringComparison.InvariantCultureIgnoreCase))
                    ?? throw new Exceptions.BezelNotFoundException($"Unable to find bezel file {bezelFileNameInZip} in {zipFile}");

                bezel = await bezelEntry.GetContentAsync();
            }
        }

        cfg = await MameGetConfigFile(cfgFile, game);

        return (lay, cfg, bezel);
    }

    /// <summary>
    /// Reads the files in the specified folder
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="folder">The folder to read</param>
    /// <param name="cfgFile">The config file</param>
    /// <param name="options">The options</param>
    /// <returns>The parsed files</returns>
    public async Task<(MameLayFile lay, MameCfgFile cfg, byte[] bezel)> MameReadFiles(string game, string folder, string cfgFile, MameToRaAction options)
    {
        byte[] bezel = null;

        Log($"{game} Reading files from folder {folder}");

        // get layout and bezel
        var layFiles = fs.FilesGetList(folder, "default.lay");
        if (layFiles == null || !layFiles.Any()) { throw new PathNotFoundException($"Unable to find a default.lay file in {folder}"); }
        var firstFileContent = await fs.FileReadAsync(layFiles.First());
        MameLayFile lay = Serializer.Deserialize<MameLayFile>(firstFileContent);

        // check that LAY is useful
        if (lay.Views.Length == 0) { throw new LayFileException("Unable to find a view in the LAY file"); }
        var view = GetView(lay, options.UseFirstView);
        var bezelFileNameInLay = MameGetBezelFile(lay, view);
        if (!string.IsNullOrEmpty(bezelFileNameInLay))
        {
            var bezelFilePath = fs.FilesGetList(folder, bezelFileNameInLay);
            if (bezelFilePath == null || !bezelFilePath.Any()) { throw new PathNotFoundException($"Unable to find the bezel file {bezelFileNameInLay}"); }
            bezel = await fs.FileReadBinaryAsync(bezelFilePath.First());
        }

        // get config file
        MameCfgFile cfg = await MameGetConfigFile(cfgFile, game);

        return (lay, cfg, bezel);
    }

    protected static void Log(string v)
    {
        throw new NotImplementedException();
    }

    protected static int LogAsk(string v, List<string> views)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds bounds to get the largest possible box
    /// </summary>
    /// <param name="a">The first bound</param>
    /// <param name="b">The second bound</param>
    /// <returns>The largest possible bounds</returns>
    private static Bounds AddToBounds(Bounds a, Bounds b)
    {
        var result = a.Clone();

        // b is further to the left
        if (a.X > b.X)
        {
            var diff = a.X - b.X;
            result.X -= diff;
            result.Width += diff;
        }

        // b is wider
        if (a.Width < b.Width)
        {
            result.Width += b.Width - a.Width;
        }

        // b is further to the top
        if (a.Y > b.Y)
        {
            var diff = a.Y - b.Y;
            result.Y -= diff;
            result.Height += diff;
        }

        // b is taller
        if (a.Height < b.Height)
        {
            result.Height += b.Height - a.Height;
        }

        return result;
    }

    /// <summary>
    /// Gets the offset from the config file
    /// </summary>
    /// <param name="cfg">The CFG file</param>
    /// <returns>The offset</returns>
    private static Offset GetOffset(MameCfgFile cfg)
    {
        var screen = cfg?.SystemConfig?.VideoConfig?.VideoScreen;
        float epsilon = 0.00001f;

        // check that at least an offset has a value
        if (screen != null &&
                (Math.Abs(screen.HOffset - Offset.DEFAULT_OFFSET) < epsilon
                    || Math.Abs(screen.HStretch - Offset.DEFAULT_STRETCH) < epsilon
                    || Math.Abs(screen.VOffset - Offset.DEFAULT_OFFSET) < epsilon
                    || Math.Abs(screen.VStretch - Offset.DEFAULT_STRETCH) < epsilon))
        {
            return new Offset
            {
                HOffset = screen.HOffset,
                VOffset = screen.VOffset,
                HStretch = screen.HStretch == 0 ? Offset.DEFAULT_STRETCH : screen.HStretch,
                VStretch = screen.VStretch == 0 ? Offset.DEFAULT_STRETCH : screen.VStretch
            };
        }

        return null;
    }

    /// <summary>
    /// Gets the source resolution for the specified view
    /// </summary>
    /// <param name="view">The view to process</param>
    /// <returns>The source resolution</returns>
    private static Bounds GetSourceResolution(MameLayFile.View view)
    {
        if (view.Bezels.Length == 0) { throw new Exceptions.BezelNotFoundException($"Unable to find a <bezel> for the <view> {view.Name}"); }

        var bezelOfView = view.Bezels.FirstOrDefault(b => b.Bounds.X == 0 && b.Bounds.Y == 0)
            ?? throw new Exceptions.CoordinatesException($"No <bezel> inside <view> {view.Name} has coordinates starting at (0,0): I don't know how to convert");
        return bezelOfView.Bounds;
    }

    /// <summary>
    /// Gets the source screen coordinates
    /// </summary>
    /// <returns></returns>
    private static Bounds GetSourceScreenCoordinates(MameLayFile.View view)
    {
        if (view.Screens == null || view.Screens.Length == 0) { throw new Exceptions.LayFileException($"No screen found in view {view.Name}"); }
        if (view.Screens.Length > 1) { throw new Exceptions.LayFileException($"Unable to automatically process a multi-screen machine (RetroArch doesn't support it)"); }

        var screen = view.Screens[0].Bounds;

        // base bounds: screen bounds
        var bounds = screen.Clone();

        // add overlay and backdrop positions to find the largest possible box
        if (view.Overlays != null && view.Overlays.Length != 0)
        {
            foreach (var o in view.Overlays)
            {
                bounds = AddToBounds(bounds, o.Bounds);
            }
        }

        if (view.Backdrops != null && view.Backdrops.Length != 0)
        {
            foreach (var b in view.Backdrops)
            {
                bounds = AddToBounds(bounds, b.Bounds);
            }
        }

        return bounds;
    }

    /// <summary>
    /// Gets the processed view
    /// </summary>
    /// <param name="lay">The LAY file</param>
    /// <param name="useFirstView">Whether to automatically use the first found view</param>
    /// <returns>The processed view</returns>
    private static MameLayFile.View GetView(MameLayFile lay, bool useFirstView)
    {
        MameLayFile.View view;
        if (lay.Views.Length > 1 && !useFirstView)
        {
            var views = new List<string>();
            for (int i = 0; i < lay.Views.Length; i++)
            {
                views.Add($"{i}: {lay.Views[i].Name}");
            }

            int viewIndex = LogAsk("Please choose which bezel you want", [.. views]);
            view = lay.Views[viewIndex];
        }
        else
        {
            view = lay.Views[0];
        }

        return view;
    }

    /// <summary>
    /// Gets the bezel file name from the LAY file and the view
    /// </summary>
    /// <param name="lay">The LAY file</param>
    /// <param name="view">The processed view</param>
    /// <returns>The bezel file name</returns>
    private static string MameGetBezelFile(MameLayFile lay, MameLayFile.View view)
    {
        var bezelElementName = view.Bezels[0].ElementName;

        var element = lay.Elements.FirstOrDefault(e => e.Name == bezelElementName)
            ?? throw new Exceptions.LayFileException($"Unable to find an <element> with name {bezelElementName} in LAY file");

        if (element.Images == null || element.Images.Length == 0) { throw new Exceptions.LayFileException($"No images inside <element> {bezelElementName} in LAY file"); }

        return element.Images[0].File;
    }

    /// <summary>
    /// Parses the specified config file
    /// </summary>
    /// <param name="cfgFile">The config file</param>
    /// <param name="game">The game name</param>
    /// <returns>The parsed config file</returns>
    private async Task<MameCfgFile> MameGetConfigFile(string cfgFile, string game)
    {
        // parse the config file if it exists
        if (!string.IsNullOrEmpty(cfgFile) && fs.FileExists(cfgFile))
        {
            Log($"{game} MAME config file exists");

            var fileContent = await fs.FileReadAsync(cfgFile);
            return Serializer.Deserialize<MameCfgFile>(fileContent);
        }
        else
        {
            Log($"{game} doesn't have a MAME config file");
            return null;
        }
    }
}