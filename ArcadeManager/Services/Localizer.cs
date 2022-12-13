using ArcadeManager.Infrastructure;
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
            // reads the JSON file
            var translationFile = Path.Combine(translationsFolder, $"{loc}.json");
            var translationContent = File.ReadAllText(translationFile);

            var words = Serializer.Deserialize<JsonElement>(translationContent);

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
                    return $"{code}_NOT_FOUND";
                }

                return result;
            }
            catch {
                return $"{code}_ERROR";
            }
        }
    }

    /// <summary>
    /// Changes the current culture.
    /// </summary>
    /// <param name="locale">The locale (en, fr...).</param>
    /// <returns>The new culture</returns>
    public void ChangeCulture(string locale) {
        if (string.IsNullOrEmpty(locale)) {
            return;
        }

        CultureInfo culture = new("en");
        if (_locales.Contains(locale)) {
            culture = new CultureInfo(locale);
        }

        // this is stupid but it works and I've already spent too much time on that

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
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
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether the provided locale is the current one.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <returns><c>true</c> if it is the current locale; otherwise, <c>false</c>.</returns>
    public bool IsCurrentLocale(string locale) {
        return CurrentLocale().Equals(locale, System.StringComparison.InvariantCultureIgnoreCase);
    }
}