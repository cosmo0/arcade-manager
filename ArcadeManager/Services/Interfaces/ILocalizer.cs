using System.Collections.Generic;
using System.Globalization;

namespace ArcadeManager.Services;

/// <summary>
/// Interface for localization
/// </summary>
public interface ILocalizer {

    /// <summary>
    /// Gets the supported locales.
    /// </summary>
    List<string> Locales { get; }

    /// <summary>
    /// Gets the translation with the specified code.
    /// </summary>
    /// <param name="code">The translation code.</param>
    /// <returns>The translated string</returns>
    string this[string code] { get; }

    /// <summary>
    /// Changes the current culture.
    /// </summary>
    /// <param name="locale">The locale (en, fr...).</param>
    /// <returns>The new culture</returns>
    CultureInfo ChangeCulture(string locale);

    /// <summary>
    /// Gets the culture name
    /// </summary>
    /// <param name="locale">The locale (en, fr...).</param>
    /// <returns>The culture name (English, Français...)</returns>
    string CultureName(string locale);

    /// <summary>
    /// Gets the current locale
    /// </summary>
    /// <returns>The current locale (en, fr...)</returns>
    string CurrentLocale();

    /// <summary>
    /// Determines whether the provided locale is the current one.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <returns><c>true</c> if it is the current locale; otherwise, <c>false</c>.</returns>
    bool IsCurrentLocale(string locale);
}