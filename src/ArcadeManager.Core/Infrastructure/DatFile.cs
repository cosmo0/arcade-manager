using System.Xml.Linq;
using ArcadeManager.Models;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// DAT file processing
/// </summary>
public class DatFile(IFileSystem fs) : IDatFile
{
    /// <summary>
    /// Gets a roms list from a DAT file
    /// </summary>
    /// <param name="datFilePath">The DAT file path</param>
    /// <returns>The roms list</returns>
    public async Task<GameRomList> GetRoms(string datFilePath)
    {
        var result = new GameRomList();

        await fs.ReadFileStream(datFilePath, async (datStream) => {
            // read DAT file and detect the tags names
            XDocument doc = await XDocument.LoadAsync(datStream, LoadOptions.None, new CancellationToken());

            // ensure we skip the header, if any (assumes there are at least 2 games in the DAT)
            string gameTag = doc.Root.Elements().Skip(1).First().Name.LocalName; 

            // for each game in the dat file
            foreach (var gameXml in doc.Root.Elements(gameTag)) {
                // parse game infos
                result.Add(GameRom.FromXml(gameXml));
            }
        });

        return result;
    }
}
