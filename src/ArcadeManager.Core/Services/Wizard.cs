using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Services.Interfaces;
using System.Collections.Generic;

namespace ArcadeManager.Core.Services;

/// <summary>
/// The wizard service
/// </summary>
/// <seealso cref="IWizard"/>
public class Wizard : IWizard {
    private readonly IFileSystem fs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Wizard"/> class.
    /// </summary>
    /// <param name="fs">The file system infrastructure.</param>
    public Wizard(IFileSystem fs) {
        this.fs = fs;
    }

    /// <summary>
    /// Counts the games in the lists.
    /// </summary>
    /// <param name="emulator">The emulator.</param>
    /// <returns>The games in each list</returns>
    public Dictionary<string, int> CountGamesInLists(string emulator) {
        var files = fs.FilesGetList(fs.GetDataPath("csv", emulator), "*.csv");
        var result = new Dictionary<string, int>();
        foreach (var f in files) {
            var lines = fs.ReadAllLines(f);
            var filename = fs.FileNameWithoutExtension(f);
            result.Add(filename, lines.Length - 1);
        }

        return result;
    }
}