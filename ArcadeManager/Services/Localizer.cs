using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ArcadeManager.Services;

/// <summary>
/// Localization management
/// </summary>
public class Localizer : ILocalizer {
    private static readonly List<string> _locales = new() { "en", "fr" };

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
            return code;
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
}