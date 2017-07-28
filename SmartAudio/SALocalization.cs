namespace SmartAudio
{
    using SmartAudio.Properties;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;

    public class SALocalization
    {
        private static string _localeDLLName = "SmartAudio.resources.dll";
        private static Dictionary<string, LocaleInfo> _localeInfo = new Dictionary<string, LocaleInfo>();

        static SALocalization()
        {
            AddLocale("ar-AE", "ar-AE", "Arabic-United Arab Emirates", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-BH", "ar-AE", "Arabic -Bahrain", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-DZ", "ar-AE", "Arabic-Algeria", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-EG", "ar-AE", "Arabic-Egypt", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-IQ", "ar-AE", "Arabic-Iraq", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-JO", "ar-AE", "Arabic-Jordan", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-KW", "ar-AE", "Arabic-Kuwait", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-LB", "ar-AE", "Arabic-Lebanon", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-LY", "ar-AE", "Arabic-Libya", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-MA", "ar-AE", "Arabic-Morocco", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-OM", "ar-AE", "Arabic-Oman", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-QA", "ar-AE", "Arabic-Quatar", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-SA", "ar-AE", "Arabic-Saudi Arabia", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-SY", "ar-AE", "Arabic-Syria", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-TN", "ar-AE", "Arabic-Tunisia", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("ar-YE", "ar-AE", "Arabic-Yemen", Resources.SA_ARABIC, "ar-AE.chm");
            AddLocale("zh-Hans", "zh-Hans", "Chinese-Simplified", Resources.SA_CHINESEPRC, "zh-Hans.chm");
            AddLocale("zh-CN", "zh-Hans", "Chinese-Simplified-P.R.C.", Resources.SA_CHINESEPRC, "zh-Hans.chm");
            AddLocale("zh-SG", "zh-Hans", "Chinese-Simplified-Singapore]", Resources.SA_CHINESEPRC, "zh-Hans.chm");
            AddLocale("zh-Hant", "zh-Hant", "Chinese Traditional", Resources.SA_CHINESE, "zh-Hant.chm");
            AddLocale("zh-HK", "zh-Hant", "Chinese-Traditional-Hong Kong", Resources.SA_CHINESE, "zh-Hant.chm");
            AddLocale("zh-TW", "zh-Hant", "Chinese-Traditional-Taiwan", Resources.SA_CHINESE, "zh-Hant.chm");
            AddLocale("zh-MO", "zh-Hant", "Chinese-Traditional-Macao", Resources.SA_CHINESE, "zh-Hant.chm");
            AddLocale("hr-HR", "hr-HR", "Croatian", Resources.SA_Croatian, "hr-HR.chm");
            AddLocale("cs-CZ", "cs-CZ", "Czech", Resources.SA_CZECH, "cs-CZ.chm");
            AddLocale("da-DK", "da-DK", "Danish", Resources.SA_DANISH, "da-DK.chm");
            AddLocale("nl-NL", "nl-NL", "Dutch-Netherlands", Resources.SA_DUTCH, "nl-NL.chm");
            AddLocale("nl-BE", "nl-NL", "Dutch-Belgium", Resources.SA_DUTCH, "nl-NL.chm");
            AddLocale("en-US", "en-US", "English-United States", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-AU", "en-US", "English-Australia", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-BZ", "en-US", "English-Belize", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-CA", "en-US", "English-Canada", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-CB", "en-US", "English-Caribbean", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-IE", "en-US", "English-Ireland", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-IN", "en-US", "English-India", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-JM", "en-US", "English-Jamaica", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-MY", "en-US", "English-Malaysia", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-NZ", "en-US", "English-New Zealand", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-PH", "en-US", "English-Phillippines", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-SG", "en-US", "English-Singapore", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-ZA", "en-US", "English-South Africa", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-TT", "en-US", "English-Trinidad", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-GB", "en-US", "English-Great Britain", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("en-ZW", "en-US", "English-Zimbabwe", Resources.SA_ENGLISH, "en-US.chm");
            AddLocale("fi-FI", "fi-FI", "Finnish", Resources.SA_FINNISH, "fi-FI.chm");
            AddLocale("fr-FR", "fr-FR", "French-France", Resources.SA_FRENCH, "fr-FR.chm");
            AddLocale("fr-CA", "fr-FR", "French-Canada", Resources.SA_FRENCH, "fr-FR.chm");
            AddLocale("fr-BE", "fr-FR", "French-Belgium", Resources.SA_FRENCH, "fr-FR.chm");
            AddLocale("fr-LU", "fr-FR", "French-Luxembourg", Resources.SA_FRENCH, "fr-FR.chm");
            AddLocale("fr-MC", "fr-FR", "French-Monaco", Resources.SA_FRENCH, "fr-FR.chm");
            AddLocale("fr-CH", "fr-FR", "French-Switzerland", Resources.SA_FRENCH, "fr-FR.chm");
            AddLocale("de-DE", "de-DE", "German-Germany", Resources.SA_GERMAN, "de-DE.chm");
            AddLocale("de-AT", "de-DE", "German-Austria", Resources.SA_GERMAN, "de-DE.chm");
            AddLocale("de-LI", "de-DE", "German-Liechtenstein", Resources.SA_GERMAN, "de-DE.chm");
            AddLocale("de-LU", "de-DE", "German-Luxembourg", Resources.SA_GERMAN, "de-DE.chm");
            AddLocale("de-CH", "de-DE", "German-Switzerland", Resources.SA_GERMAN, "de-DE.chm");
            AddLocale("el-GR", "el-GR", "Greek", Resources.SA_GREEK, "el-GR.chm");
            AddLocale("he-IL", "he-IL", "Hebrew", Resources.SA_HEBREW, "he-IL.chm");
            AddLocale("hu-HU", "hu-HU", "Hungarian", Resources.SA_HUNGARIAN, "hu-HU.chm");
            AddLocale("it-IT", "it-IT", "Italian-Italy", Resources.SA_ITALIAN, "it-IT.chm");
            AddLocale("it-CH", "it-IT", "Italian-Switzerland", Resources.SA_ITALIAN, "it-IT.chm");
            AddLocale("ja-JP", "ja-JP", "Japanese", Resources.SA_JAPANESE, "ja-JP.chm");
            AddLocale("ko-KR", "ko-KR", "Korean", Resources.SA_KOREAN, "ko-KR.chm");
            AddLocale("nb-NO", "nb-NO", "Norwegian (Bokmal)", Resources.SA_NORWEGIAN, "nb-NO.chm");
            AddLocale("pl-PL", "pl-PL", "Polish", Resources.SA_POLISH, "pl-PL.chm");
            AddLocale("pt-BR", "pt-BR", "Portuguese (Brazilian)", Resources.SA_PORTUGUESE, "pt-BR.chm");
            AddLocale("pt-PT", "pt-PT", "Portuguese (European)", Resources.SA_PORTUGUESEEUR, "pt-PT.chm");
            AddLocale("ro-RO", "ro-RO", "Romanian-Romania", Resources.SA_Romanian, "ro-RO.chm");
            AddLocale("ro-MO", "ro-RO", "Romanian-Moldova", Resources.SA_Romanian, "ro-RO.chm");
            AddLocale("ru-RU", "ru-RU", "Russian-Russia", Resources.SA_RUSSIAN, "ru-RU.chm");
            AddLocale("ru-MO", "ru-RU", "Russian-Moldova", Resources.SA_RUSSIAN, "ru-RU.chm");
            AddLocale("sk-SK", "sk-SK", "Slovak", Resources.SA_Slovak, "sk-SK.chm");
            AddLocale("sl-SI", "sl-SI", "Slovenian", Resources.SA_Slovenian, "sl-SI.chm");
            AddLocale("es-ES", "es-ES", "Spanish-Castilian", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-AR", "es-ES", "Spanish-Argentina", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-BO", "es-ES", "Spanish-Bolivia", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-CL", "es-ES", "Spanish-Chile", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-CO", "es-ES", "Spanish-Colombia", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-CR", "es-ES", "Spanish-Costa Rica", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-DO", "es-ES", "Spanish-Dominican Republic", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-EC", "es-ES", "Spanish-Ecuador", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-GT", "es-ES", "Spanish-Guatemala", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-HN", "es-ES", "Spanish-Honduras", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-MX", "es-ES", "Spanish-Mexico", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-NI", "es-ES", "Spanish-Nicaragua", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-PA", "es-ES", "Spanish-Panama", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-PE", "es-ES", "Spanish-Peru", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-PR", "es-ES", "Spanish-Puerto Rico", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-PY", "es-ES", "Spanish-Paraguay", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-SV", "es-ES", "Spanish-El Salvador", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-UY", "es-ES", "Spanish-Uruguay", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-US", "es-ES", "Spanish-United States", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("es-VE", "es-ES", "Spanish-Venezuela", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("eu-ES", "es-ES", "Basque", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("ca-ES", "es-ES", "Catalan", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("gl-ES", "es-ES", "Galacian", Resources.SA_SPANISH, "es-ES.chm");
            AddLocale("sv-FI", "sv-SE", "Swedish-Finland", Resources.SA_SWEDISH, "sv-SE.chm");
            AddLocale("sv-SE", "sv-SE", "Swedish-Sweden", Resources.SA_SWEDISH, "sv-SE.chm");
            AddLocale("th-TH", "th-TH", "Thai", Resources.SA_THAI, "th-TH.chm");
            AddLocale("tr-TR", "tr-TR", "Turkish", Resources.SA_TURKISH, "tr-TR.chm");
        }

        private static void AddLocale(string localeName, string mappedLocaleName, string language, string translation, string helpFileName)
        {
            LocaleInfo info = new LocaleInfo(localeName, mappedLocaleName, language, translation, helpFileName);
            try
            {
                _localeInfo.Add(localeName.ToUpper(), info);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Failed in AddLocale.", Severity.WARNING, exception);
            }
        }

        public static string GetMappedName(string nameIn)
        {
            string mappedLocaleName = "en-US";
            try
            {
                mappedLocaleName = LocalizationTable[nameIn.ToUpper()].MappedLocaleName;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GetMappedName : Locale " + nameIn.ToUpper() + " not found in LocalizationTable", Severity.FATALERROR, exception);
            }
            return mappedLocaleName;
        }

        public static bool IsValidLocaleFolder(string nameIn)
        {
            bool flag = false;
            string mappedName = GetMappedName(nameIn);
            try
            {
                DirectoryInfo info = new DirectoryInfo(mappedName);
                if (info != null)
                {
                    FileInfo[] files = info.GetFiles(_localeDLLName);
                    if (((files != null) && (files.GetLength(0) > 0)) && (files[0].Name.ToUpper().CompareTo(_localeDLLName.ToUpper()) == 0))
                    {
                        flag = true;
                    }
                    else
                    {
                        SmartAudioLog.Log(mappedName + " is not a valid locale folder.", new object[] { Severity.WARNING });
                    }
                }
                return flag;
            }
            catch
            {
                SmartAudioLog.Log("IsValidLocaleFolder: exception thrown for language folder name " + mappedName + " ", new object[] { Severity.WARNING });
                return flag;
            }
        }

        public static bool LocalInfoExits(string name)
        {
            FileInfo info = new FileInfo(Application.Current.GetType().Assembly.Location);
            DirectoryInfo info2 = new DirectoryInfo(info.DirectoryName);
            if (info2 == null)
            {
                return false;
            }
            DirectoryInfo[] directories = info2.GetDirectories(name);
            return ((directories.GetLength(0) > 0) && IsValidLocaleFolder(directories[0].FullName));
        }

        public static Dictionary<string, LocaleInfo> LocalizationTable =>
            _localeInfo;
    }
}

