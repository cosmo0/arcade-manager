using System;
namespace ArcadeManager.Models
{
    /// <summary>
    /// A game entry in a CSV file
    /// </summary>
    public struct GameEntry
    {
        /// <summary>
        /// Gets or sets the game's name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the parent rom, if any
        /// </summary>
        public string cloneof { get; set; }
    }
}
