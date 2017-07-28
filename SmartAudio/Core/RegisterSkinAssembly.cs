namespace SmartAudio.Core
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public class RegisterSkinAssembly : Attribute
    {
        private bool _isDefaultSkin;
        private string _skinName;
        private SmartAudio.Core.SkinPriority _skinPriority;
        private Guid _uniqueID;

        public RegisterSkinAssembly(string skinName)
        {
            this._skinName = skinName;
            this._isDefaultSkin = false;
            this._skinPriority = SmartAudio.Core.SkinPriority.NormalPriority;
        }

        public RegisterSkinAssembly(string skinName, bool isDefaultSkin, SmartAudio.Core.SkinPriority priority, string guid)
        {
            this._skinName = skinName;
            this._isDefaultSkin = isDefaultSkin;
            this._skinPriority = priority;
            this._uniqueID = new Guid(guid);
        }

        public bool IsDefaultSkin =>
            this._isDefaultSkin;

        public string SkinName =>
            this._skinName;

        public SmartAudio.Core.SkinPriority SkinPriority =>
            this._skinPriority;

        public Guid UniqueID =>
            this._uniqueID;
    }
}

