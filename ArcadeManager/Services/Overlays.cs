using System;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Services
{
    public class Overlays
    {
        /// <summary>
        /// Downloads an overlay pack
        /// </summary>
        /// <param name="data">The parameters</param>
        /// <param name="progressor">The progressor</param>
        public static async Task Download(Actions.OverlaysAction data, MessageHandler.Progressor progressor)
        {
            progressor.Init("Download overlay pack");

            try
            {
                int installed = 0;

                var repository = data.pack.Repository;
                var roms = data.pack.Roms;
                var overlays = data.pack.Overlays;
                var common = data.pack.Common;
                var packBase = data.pack.Base;
                var os = ArcadeManagerEnvironment.SettingsOs;

                // check if the destination of rom cfg is the rom folder
                var romCfgFolder = data.pack.Roms.Dest[os] == "roms"
                    ? null // save rom cfg directly into rom folder(s)
                    : Path.Join(data.configFolder, data.pack.Roms.Dest[os]); // save rom cfg in config folder

                // download common files

                // list the available rom configs

                // check that there is a matching game in any of the roms folders

                // download the rom config and extract the overlay file name

                // download the overlay file name and extract the image file name

                // download the image

                // resize the overlay coordinates if necessary

                progressor.Done($"Installed {installed} overlay packs", null);
            }
            catch (Exception ex)
            {
                progressor.Error(ex);
            }
        }
    }
}
