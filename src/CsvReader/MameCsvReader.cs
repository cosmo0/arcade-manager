using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CsvReader
{
    public class MameCsvReader : IDisposable
    {
        private readonly StreamReader fileReader;
        private readonly CsvHelper.CsvReader csvReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MameCsvReader.MameCsvReader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        public MameCsvReader(string filePath)
        {
            this.fileReader = File.OpenText(filePath);
            this.csvReader = new CsvHelper.CsvReader(this.fileReader);
        }

        /// <summary>
        /// Gets the roms listed in the file
        /// </summary>
        /// <returns>The roms.</returns>
        public List<Rom> GetRoms()
        {
            return this.csvReader.GetRecords<Rom>().ToList();
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:MameCsvReader.Reader"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:MameCsvReader.Reader"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="T:MameCsvReader.Reader"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="T:MameCsvReader.Reader"/>
        /// so the garbage collector can reclaim the memory that the <see cref="T:MameCsvReader.Reader"/> was occupying.</remarks>
        public void Dispose()
        {
            fileReader?.Dispose();
            csvReader?.Dispose();
        }
    }
}
