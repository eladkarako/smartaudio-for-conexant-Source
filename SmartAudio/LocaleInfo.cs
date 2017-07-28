namespace SmartAudio
{
    using System;

    public class LocaleInfo
    {
        private string _description;
        private string _helpFileName;
        private string _localeName;
        private string _mappedLocaleName;
        private string _translation;

        public LocaleInfo(string localeName, string mappedLocaleName, string description, string translation, string helpFileName)
        {
            this._localeName = localeName;
            this._description = description;
            this._mappedLocaleName = mappedLocaleName;
            this._translation = translation;
            this._helpFileName = helpFileName;
        }

        public string Description
        {
            get => 
                this._description;
            set
            {
                this._description = value;
            }
        }

        public string HelpFileName
        {
            get => 
                this._helpFileName;
            set
            {
                this._helpFileName = value;
            }
        }

        public string LocaleName
        {
            get => 
                this._localeName;
            set
            {
                this._localeName = value;
            }
        }

        public string MappedLocaleName
        {
            get => 
                this._mappedLocaleName;
            set
            {
                this._mappedLocaleName = value;
            }
        }

        public string Translation
        {
            get => 
                this._translation;
            set
            {
                this._translation = value;
            }
        }
    }
}

