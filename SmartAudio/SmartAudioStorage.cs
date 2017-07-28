namespace SmartAudio
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot("SmartAudioStorage", Namespace="", IsNullable=false)]
    public class SmartAudioStorage
    {
        private static string _filePath;
        private bool _hpLimiterDefaultSetting = false;
        private int _numberOfSAIIActivations;

        private SmartAudioStorage()
        {
        }

        public static SmartAudioStorage CreateStorage()
        {
            SmartAudioStorage storage = Load();
            if (storage == null)
            {
                storage = new SmartAudioStorage();
                storage.Save();
            }
            return storage;
        }

        public static SmartAudioStorage Load()
        {
            try
            {
                TextReader reader = new StreamReader(StoragePath);
                string pXmlString = reader.ReadToEnd();
                reader.Close();
                XmlSerializer serializer = new XmlSerializer(typeof(SmartAudioStorage));
                MemoryStream w = new MemoryStream(StringToUTF8ByteArray(pXmlString));
                new XmlTextWriter(w, Encoding.UTF8);
                return (SmartAudioStorage) serializer.Deserialize(w);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SmartAudioStorage::Load() got Exception", Severity.WARNING, exception);
                return null;
            }
        }

        public bool Save()
        {
            bool flag = false;
            if (StoragePath != null)
            {
                try
                {
                    MemoryStream w = new MemoryStream();
                    XmlSerializer serializer = new XmlSerializer(typeof(SmartAudioStorage));
                    XmlTextWriter writer = new XmlTextWriter(w, Encoding.UTF8);
                    serializer.Serialize((XmlWriter) writer, this);
                    string str = this.UTF8ByteArrayToString(((MemoryStream) writer.BaseStream).ToArray());
                    TextWriter writer2 = new StreamWriter(StoragePath);
                    writer2.Write(str);
                    writer2.Close();
                    flag = true;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("SmartAudioStorage::Save() got Exception", Severity.WARNING, exception);
                    Console.WriteLine(exception);
                }
            }
            return flag;
        }

        private static byte[] StringToUTF8ByteArray(string pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(pXmlString);
        }

        private string UTF8ByteArrayToString(byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(characters);
        }

        public static string ApplicationStorageFolder
        {
            get
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (folderPath.Length == 0)
                {
                    return null;
                }
                folderPath = folderPath + @"\Conexant\SmartAudio";
                new FileInfo(folderPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                return folderPath;
            }
        }

        public bool HPLimiterDefaultSetting
        {
            get => 
                this._hpLimiterDefaultSetting;
            set
            {
                this._hpLimiterDefaultSetting = value;
            }
        }

        public int NumberOfSAIIActivations
        {
            get => 
                this._numberOfSAIIActivations;
            set
            {
                this._numberOfSAIIActivations = value;
            }
        }

        public static string StoragePath
        {
            get
            {
                if (ApplicationStorageFolder != null)
                {
                    return (ApplicationStorageFolder + @"\SAStorage.xml");
                }
                return null;
            }
        }
    }
}

