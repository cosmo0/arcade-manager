using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcadeManager.Models;

/// <summary>
/// A CSV games list
/// </summary>
public class CsvGamesList {
    private readonly List<GameEntry> entries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvGamesList"/> class.
    /// </summary>
    public CsvGamesList() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvGamesList"/> class.
    /// </summary>
    /// <param name="entries">The entries.</param>
    public CsvGamesList(IEnumerable<GameEntry> entries) {
        this.entries.AddRange(entries.Select(e => new GameEntry(this, e)));
    }

    /// <summary>
    /// Gets the games list.
    /// </summary>
    public IReadOnlyCollection<GameEntry> Games => entries.AsReadOnly();

    /// <summary>
    /// Gets the headers list.
    /// </summary>
    public SortedSet<string> Headers { get; private set; } = new();

    /// <summary>
    /// Adds a new entry with the specified name and additional values.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="values">The additional values.</param>
    public void Add(string name, Dictionary<string, string> values) {
        this.Add(new GameEntry(this, name, values));
    }

    /// <summary>
    /// Adds the specified entry.
    /// </summary>
    /// <param name="entry">The entry.</param>
    public void Add(GameEntry entry) {
        // make sure all headers exist in the main list
        foreach (var h in entry.Values.Keys.Where(k => !string.IsNullOrEmpty(k))) {
            this.Headers.Add(h); // SortedList won't add a duplicate key
        }

        // add entry
        this.entries.Add(new GameEntry(this, entry));
    }

    /// <summary>
    /// Adds a list of name-only entries.
    /// </summary>
    /// <param name="names">The game names.</param>
    public void AddRange(IEnumerable<string> names) {
        this.AddRange(names.Select(n => new GameEntry(this, n, new Dictionary<string, string>())));
    }

    /// <summary>
    /// Adds a list of entries.
    /// </summary>
    /// <param name="entries">The game entries.</param>
    public void AddRange(IEnumerable<GameEntry> entries) {
        if (entries == null || !entries.Any()) { return; }

        // let's assume they all have the same headers for performances reasons
        foreach (var h in entries.First().Values.Keys.Where(k => !string.IsNullOrEmpty(k))) {
            this.Headers.Add(h); // SortedList won't add a duplicate key
        }

        // adds the items to the internal list
        foreach (var e in entries) {
            this.entries.Add(new GameEntry(this, e));
        }
    }

    /// <summary>
    /// Adds a list of entries.
    /// </summary>
    /// <param name="list">The list of entries.</param>
    public void AddRange(CsvGamesList list) {
        AddRange(list.entries.Select(e => e.Name));
    }

    /// <summary>
    /// Copies data from the specified entry.
    /// </summary>
    /// <param name="copyFrom">The entry to copy data from.</param>
    public void CopyEntry(GameEntry copyFrom) {
        var entry = entries.FirstOrDefault(e => e.Name.Equals(copyFrom.Name, StringComparison.InvariantCultureIgnoreCase));
        if (entry != null) {
            entry.CopyFrom(copyFrom);
        }
        else {
            this.Add(copyFrom);
        }
    }

    /// <summary>
    /// De-duplicates the list by name
    /// </summary>
    public void DeDuplicate() {
        // make sure to keep the ToList to copy the list now (otherwise during AddRange the source
        // will be empty when the expression is evaluated)
        var items = this.entries.DistinctBy(entry => entry.Name, StringComparer.InvariantCultureIgnoreCase).ToList();

        this.entries.Clear();
        this.entries.AddRange(items);
    }

    /// <summary>
    /// Gets the CSV header line.
    /// </summary>
    /// <param name="firstColumn">The first column.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>The CSV header line</returns>
    public string GetHeaderLine(string firstColumn, string delimiter) {
        return string.Concat(firstColumn, delimiter, string.Join(delimiter, this.Headers));
    }

    /// <summary>
    /// A CSV game entry
    /// </summary>
    public class GameEntry {
        private readonly string emptyValue = "-";
        private readonly CsvGamesList listRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameEntry"/> class.
        /// </summary>
        /// <param name="list">The reference list.</param>
        /// <param name="copyFrom">The entry to copy from.</param>
        public GameEntry(CsvGamesList list, GameEntry copyFrom) {
            this.listRef = list;
            this.Name = copyFrom.Name;
            this.Values = copyFrom.Values;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameEntry"/> class.
        /// </summary>
        /// <param name="list">The reference to the main games list.</param>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        public GameEntry(CsvGamesList list, string name, IDictionary<string, string> values) {
            this.listRef = list ?? throw new ArgumentNullException(nameof(list));
            this.Name = name;
            this.Values = new SortedDictionary<string, string>(values);
        }

        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the additional values.
        /// </summary>
        public SortedDictionary<string, string> Values { get; set; }

        /// <summary>
        /// Copies data from another entry.
        /// </summary>
        /// <param name="copyFrom">The entity to copy from.</param>
        public void CopyFrom(GameEntry copyFrom) {
            foreach (var v in copyFrom.Values) {
                if (!this.Values.ContainsKey(v.Key)) {
                    this.Values.Add(v.Key, v.Value);
                }
            }
        }

        /// <summary>
        /// Converts the entry to a CSV string.
        /// </summary>
        /// <param name="defaultDelimiter">The CSV default delimiter.</param>
        /// <returns>The string representation of the entry</returns>
        public string ToCSVString(string defaultDelimiter) {
            var line = new StringBuilder();

            // name
            line.Append(Name).Append(defaultDelimiter);

            // additional values
            foreach (var h in listRef.Headers) {
                line.Append(this.Values.ContainsKey(h) ? this.Values[h] : emptyValue).Append(defaultDelimiter);
            }

            return line.ToString();
        }
    }
}