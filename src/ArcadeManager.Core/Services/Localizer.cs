using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ArcadeManager.Core.Services;

/// <summary>
/// Localization management
/// </summary>
public class Localizer : ILocalizer {
    private static readonly List<string> _locales = ["en", "fr", "sv"];

    private readonly Dictionary<string, Dictionary<string, string>> translations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Localizer"/> class.
    /// </summary>
    /// <param name="fs">The file system.</param>
    public Localizer(IFileSystem fs) {
        var translationsFolder = fs.GetDataPath("translations");

        foreach (var loc in _locales) {
            // reads the JSON file
            var translationFile = fs.PathJoin(translationsFolder, $"{loc}.txt");
            var words = new Dictionary<string, string>();

            var translationContent = fs.ReadAllLines(translationFile);
            foreach (var l in translationContent.Where(l => !string.IsNullOrWhiteSpace(l) && l.Contains('=', StringComparison.InvariantCultureIgnoreCase))) {
                var equal = l.IndexOf('='); // use IndexOf instead of Split because the translation text can also have "="
                var code = l[..equal].Trim().ToUpperInvariant();
                var text = l[(equal + 1)..].Trim();

                // removes Weblate quotes around translation
                if (text.StartsWith('"') && text.EndsWith('"')) {
                    text = text.Trim('"');
                }

                // removes Weblate weird quotes escaping (because I'm using Joomla format)
                text = text.Replace("\"_QQ_\"", "\"");

                words.Add(code, text);
            }

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
            return GetTranslationForLanguage(code, CurrentLocale());
        }
    }

    /// <summary>
    /// Ensures that the current locale has translations.
    /// </summary>
    public static void EnsureLocale() {
        var locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();

        // fallback on english
        if (!_locales.Contains(locale, StringComparer.InvariantCultureIgnoreCase)) {
            ChangeCulture(new CultureInfo("en"));
        }
    }

    /// <summary>
    /// Gets the list of supported cultures
    /// </summary>
    /// <returns>The list of supported cultures</returns>
    public static CultureInfo[] GetSupportedCultures() {
        return [.. _locales.Select(l => new CultureInfo(l))];
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

        ChangeCulture(culture);
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
        return CurrentLocale().Equals(locale, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Gets a string escaped for JS inclusion
    /// </summary>
    /// <param name="code">The translation code.</param>
    /// <returns>The JS-escaped translation</returns>
    public string Js(string code) {
        return this[code]?.Replace("'", "\\'");
    }

    private static void ChangeCulture(CultureInfo culture) {
        // this is stupid but it works and I've already spent too much time on that

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private string GetTranslationForLanguage(string code, string language) {
        try {
            code = code.ToUpperInvariant();

            // get translations dictionary of the language
            if (!translations.TryGetValue(language, out var t)) {
                return $"{code}_NO_LANGUAGE";
            }

            // fallback on english
            if (!t.TryGetValue(code, out var translation) && !language.Equals("en", StringComparison.InvariantCultureIgnoreCase)) {
                return GetTranslationForLanguage(code, "en");
            }

            if (!string.IsNullOrEmpty(translation)) {
                return translation;
            } else {
                return $"{code}_NO_TRANSLATION";
            }
        }
        catch (Exception ex) {
            return $"{code}_ERROR_{ex.Message}";
        }
    }
}