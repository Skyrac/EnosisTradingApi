using System.Collections.Generic;

namespace API.Utility
{
    public static class Language
    {
        private const string INVALID_TRANSLATION = "translate_not_found";
        private static readonly Dictionary<SystemLanguage, Dictionary<string, string>> Languages = new Dictionary<SystemLanguage, Dictionary<string, string>>() {
        {
            SystemLanguage.German, new Dictionary<string, string>() {
                { "translate_not_found", "Übersetzung für den Wert {0} nicht gefunden!" },
                { "title_finish_registration", "Registrierung für Money Moon abschließen" },
                { "content_finish_registration", "Dein Code für das aktivieren deines Accounts: {0}" },
                { "title_account_updated", "Dein Money Moon Account wurde aktualisiert." },
                { "content_account_updated", "Wenn du diese Änderungen nicht autorisiert hast, wende dich bitte schnellstmöglich an den Support." },
                { "title_successfull_activation", "Registrierung erfolgreich abgeschlossen!" },
                { "content_successfull_activation", "Herzlich wilkommen in der Money Moon Rakete!\nBei Fragen scheue dich nicht mich persönlich anzuschreiben oder eines der Hilfevideo anzusehen.\n\n\nViel Spaß beim Geld verdienen :)" },
                { "title_forgot_password", "Money Moon - Passwort Vergessen" },
                { "content_forgot_password", "Servus {0},\nIch hoffe es geht dir gut?\n\nDein Code zum zurücksetzen deines Passworts lautet: {1}" }
            }
        },
        {
            SystemLanguage.English, new Dictionary<string, string>() {
                { "translate_not_found", "Translation for the key {0} not found!" },
                { "title_finish_registration", "Finish your registration for Money Moon" },
                { "content_finish_registration", "Your activation key for money moon: {0}" },
                { "title_account_updated", "Your Money Moon Account was updated." },
                { "content_account_updated", "If it wasn't you, pleace contact the support ASAP!" },
                { "title_successfull_activation", "Successfully activated your Money Moon Account!" },
                { "content_successfull_activation", "Welcome to the Money Moon Rocket!\n\n\n\nWe wish you a lot of fun making money :)" },
                { "title_forgot_password", "Money Moon - Password Service" },
                { "content_forgot_password", "Heya {0},\nI hope you are alright?\n\nYour code for resetting your password: {1}" }
            }
        }

    };
        public static Dictionary<string, string> SelectLanguage(SystemLanguage language)
        {
            if (Languages.ContainsKey(language))
            {
                return Languages[language];
            }

            return Languages[SystemLanguage.English];
        }

        public static string Translate(SystemLanguage language, string key)
        {
            var selectedLanguage = SelectLanguage(language);
            if (string.IsNullOrEmpty(key))
            {
                key = "unknown";
            }
            if (selectedLanguage.ContainsKey(key))
            {
                return selectedLanguage[key];
            }
            return key;
        }
    }

    public enum SystemLanguage
    {
        //
        // Zusammenfassung:
        //     Afrikaans.
        Afrikaans = 0,
        //
        // Zusammenfassung:
        //     Arabic.
        Arabic = 1,
        //
        // Zusammenfassung:
        //     Basque.
        Basque = 2,
        //
        // Zusammenfassung:
        //     Belarusian.
        Belarusian = 3,
        //
        // Zusammenfassung:
        //     Bulgarian.
        Bulgarian = 4,
        //
        // Zusammenfassung:
        //     Catalan.
        Catalan = 5,
        //
        // Zusammenfassung:
        //     Chinese.
        Chinese = 6,
        //
        // Zusammenfassung:
        //     Czech.
        Czech = 7,
        //
        // Zusammenfassung:
        //     Danish.
        Danish = 8,
        //
        // Zusammenfassung:
        //     Dutch.
        Dutch = 9,
        //
        // Zusammenfassung:
        //     English.
        English = 10,
        //
        // Zusammenfassung:
        //     Estonian.
        Estonian = 11,
        //
        // Zusammenfassung:
        //     Faroese.
        Faroese = 12,
        //
        // Zusammenfassung:
        //     Finnish.
        Finnish = 13,
        //
        // Zusammenfassung:
        //     French.
        French = 14,
        //
        // Zusammenfassung:
        //     German.
        German = 15,
        //
        // Zusammenfassung:
        //     Greek.
        Greek = 16,
        //
        // Zusammenfassung:
        //     Hebrew.
        Hebrew = 17,
        Hugarian = 18,
        //
        // Zusammenfassung:
        //     Hungarian.
        Hungarian = 18,
        //
        // Zusammenfassung:
        //     Icelandic.
        Icelandic = 19,
        //
        // Zusammenfassung:
        //     Indonesian.
        Indonesian = 20,
        //
        // Zusammenfassung:
        //     Italian.
        Italian = 21,
        //
        // Zusammenfassung:
        //     Japanese.
        Japanese = 22,
        //
        // Zusammenfassung:
        //     Korean.
        Korean = 23,
        //
        // Zusammenfassung:
        //     Latvian.
        Latvian = 24,
        //
        // Zusammenfassung:
        //     Lithuanian.
        Lithuanian = 25,
        //
        // Zusammenfassung:
        //     Norwegian.
        Norwegian = 26,
        //
        // Zusammenfassung:
        //     Polish.
        Polish = 27,
        //
        // Zusammenfassung:
        //     Portuguese.
        Portuguese = 28,
        //
        // Zusammenfassung:
        //     Romanian.
        Romanian = 29,
        //
        // Zusammenfassung:
        //     Russian.
        Russian = 30,
        //
        // Zusammenfassung:
        //     Serbo-Croatian.
        SerboCroatian = 31,
        //
        // Zusammenfassung:
        //     Slovak.
        Slovak = 32,
        //
        // Zusammenfassung:
        //     Slovenian.
        Slovenian = 33,
        //
        // Zusammenfassung:
        //     Spanish.
        Spanish = 34,
        //
        // Zusammenfassung:
        //     Swedish.
        Swedish = 35,
        //
        // Zusammenfassung:
        //     Thai.
        Thai = 36,
        //
        // Zusammenfassung:
        //     Turkish.
        Turkish = 37,
        //
        // Zusammenfassung:
        //     Ukrainian.
        Ukrainian = 38,
        //
        // Zusammenfassung:
        //     Vietnamese.
        Vietnamese = 39,
        //
        // Zusammenfassung:
        //     ChineseSimplified.
        ChineseSimplified = 40,
        //
        // Zusammenfassung:
        //     ChineseTraditional.
        ChineseTraditional = 41,
        //
        // Zusammenfassung:
        //     Unknown.
        Unknown = 42
    }
}
