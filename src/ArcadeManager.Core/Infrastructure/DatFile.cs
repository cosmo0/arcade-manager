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
    /// <param name="folder">The folder of the rom files</param>
    /// <returns>The roms list</returns>
    public async Task<GameRomList> GetRoms(string datFilePath, string folder)
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
                result.Add(GameRom.FromXml(gameXml, folder));
            }
        });

        // loop again on the files list to do a bios and clones matching
        foreach (var game in result) {
            // get parent
            if (!string.IsNullOrEmpty(game.ParentName)) {
                game.Parent = result.FirstOrDefault(g => g.Name == game.ParentName);

                // circular reference
                game.Parent?.Clones.Add(game);
                
                // get bios from parent
                if (!string.IsNullOrEmpty(game.Parent?.BiosName)) {
                    game.BiosName = game.Parent.BiosName;

                    if (game.Parent.Bios != null) {
                        game.Bios = game.Parent.Bios;
                    }
                }
            }

            // get bios
            if (game.Bios == null && !string.IsNullOrEmpty(game.BiosName)) {
                game.Bios = result.FirstOrDefault(g => g.Name == game.BiosName);
            }
        }

        return result;
    }
}
