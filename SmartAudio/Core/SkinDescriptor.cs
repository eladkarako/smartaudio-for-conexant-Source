namespace SmartAudio.Core
{
    using System;

    public class SkinDescriptor
    {
        private bool _isDefault;
        private bool _selectAsDefault;
        private string _skinAssemblyName;
        private RegisterSkinAssembly _skinInfo;
        private string _skinName;
        private SmartAudio.Core.SkinPriority _skinPriority;
        private Guid _uniqueID;

        public SkinDescriptor(RegisterSkinAssembly registeredSkin, string assemblyName)
        {
            this._uniqueID = registeredSkin.UniqueID;
            this._skinName = registeredSkin.SkinName;
            this._isDefault = registeredSkin.IsDefaultSkin;
            this._skinPriority = registeredSkin.SkinPriority;
            this._skinAssemblyName = assemblyName;
            this._skinInfo = registeredSkin;
            this._selectAsDefault = false;
        }

        public string AssemblyName
        {
            get => 
                this._skinAssemblyName;
            set
            {
                this._skinAssemblyName = value;
            }
        }

        public bool IsDefault
        {
            get => 
                this._isDefault;
            set
            {
                this._isDefault = value;
            }
        }

        public RegisterSkinAssembly RegistrationInfo =>
            this._skinInfo;

        public bool SelectAsDefault
        {
            get => 
                this._selectAsDefault;
            set
            {
                this._selectAsDefault = value;
            }
        }

        public string SkinName
        {
            get => 
                this._skinName;
            set
            {
                this._skinName = value;
            }
        }

        public SmartAudio.Core.SkinPriority SkinPriority
        {
            get => 
                this._skinPriority;
            set
            {
                this._skinPriority = value;
            }
        }

        public Guid UniqueID =>
            this._uniqueID;
    }
}

