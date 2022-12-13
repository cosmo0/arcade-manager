using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace ArcadeManager.Services;

/// <summary>
/// Localization management
/// </summary>
public class Localizer : ILocalizer {
    private readonly List<string> _locales = new() { "en", "fr" };

    private readonly Dictionary<string, JsonElement> translations = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Localizer"/> class.
    /// </summary>
    public Localizer() {
        var translationsFolder = Path.Combine(ArcadeManagerEnvironment.BasePath, "Data", "translations");

        foreach (var loc in _locales) {
            var words = ReadTranslationFile(translationsFolder, loc);

            translations.Add(loc, words);
        }
    }

    /// <summary>
    /// Gets the supported locales.
    /// </summary>
    public List<string> Locales {
        get {
            return _locales;
        }
    }

    /// <summary>
    /// Gets the translation with the specified code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The translated string</returns>
    public string this[string code] {
        get {
            try {
                if (!translations.ContainsKey(CurrentLocale())) {
                    return $"{code}_NO_LANGUAGE";
                }

                var t = translations[CurrentLocale()];
                var result = t.GetProperty(code).GetString();

                if (string.IsNullOrEmpty(result)) {
                    return $"{code}_NO_TRANSLATION";
                }

                return result;
            }
            catch {
                return $"{code}_ERROR";
            }
        }
    }

    /// <summary>
    /// Changes the locale.
    /// </summary>
    /// <param name="locale">The locale (en, fr...).</param>
    public void ChangeLocale(string locale) {
        CultureInfo culture = new("en");
        if (_locales.Contains(locale)) {
            culture = new CultureInfo(locale);
        }

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    /// <summary>
    /// Gets the culture name
    /// </summary>
    /// <param name="locale">The locale (en, fr...).</param>
    /// <returns>The culture name (English, Français...)</returns>
    public string CultureName(string locale) {
        var cult = new CultureInfo(locale);

        return cult.TextInfo.ToTitleCase(cult.NativeName);
    }

    /// <summary>
    /// Gets the current locale
    /// </summary>
    /// <returns>The current locale (en, fr...)</returns>
    public string CurrentLocale() {
        return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether the provided locale is the current one.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <returns><c>true</c> if it is the current locale; otherwise, <c>false</c>.</returns>
    public bool IsCurrentLocale(string locale) {
        return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals(locale, System.StringComparison.InvariantCultureIgnoreCase);
    }

    private static JsonElement ReadTranslationFile(string translationsFolder, string loc) {
        // reads the JSON file
        var translationFile = Path.Combine(translationsFolder, $"{loc}.json");
        var translationContent = File.ReadAllText(translationFile);

        return Serializer.Deserialize<JsonElement>(translationContent);
    }
}