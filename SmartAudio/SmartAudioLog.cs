namespace SmartAudio
{
    using System;
    using System.Windows;
    using System.Xml;

    public class SmartAudioLog
    {
        private XmlDocument _logFile = new XmlDocument();
        private XmlElement _rootNode;
        private static SmartAudioLog _SmartAudioLog = new SmartAudioLog();

        public SmartAudioLog()
        {
            this._rootNode = this._logFile.CreateElement("SmartAudioLog");
            this._logFile.AppendChild(this._rootNode);
        }

        private void _Log(string location, Severity severity, Exception e)
        {
            if (this.IsDemoMode)
            {
                string text1 = "Location{" + location + "},Severity{" + severity.ToString() + "Source{" + e.Source + "},Message{" + e.Message + "}";
            }
            else
            {
                XmlElement newChild = this._logFile.CreateElement("LogEntry");
                XmlAttribute node = this._logFile.CreateAttribute("Time");
                node.Value = DateTime.Now.ToString();
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Location");
                node.Value = location;
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Severity");
                node.Value = severity.ToString();
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Message");
                node.Value = e.Message;
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Source");
                node.Value = e.Source;
                newChild.Attributes.Append(node);
                newChild.InnerText = e.StackTrace;
                this._rootNode.AppendChild(newChild);
                SaveLog();
            }
        }

        private void _Log(string location, Severity severity, string formatSpec, params object[] args)
        {
            string str = string.Format(formatSpec, args);
            if (this.IsDemoMode)
            {
                string text1 = "Location{" + location + "},Severity{" + severity.ToString() + "User Arguments{" + str + "}";
            }
            else
            {
                XmlElement newChild = this._logFile.CreateElement("LogEntry");
                XmlAttribute node = this._logFile.CreateAttribute("Time");
                node.Value = DateTime.Now.ToString();
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Location");
                node.Value = location;
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Severity");
                node.Value = severity.ToString();
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Message");
                node.Value = formatSpec;
                newChild.Attributes.Append(node);
                node = this._logFile.CreateAttribute("Source");
                node.Value = "";
                newChild.Attributes.Append(node);
                this._logFile.AppendChild(newChild);
                SaveLog();
            }
        }

        public static void Log(string logString)
        {
            _SmartAudioLog._Log(logString, Severity.INFORMATION, new Exception(logString));
        }

        public static void Log(string logString, params object[] args)
        {
            string location = string.Format(logString, args);
            _SmartAudioLog._Log(location, Severity.INFORMATION, new Exception(logString));
        }

        public static void Log(string location, Severity severity, Exception e)
        {
            _SmartAudioLog._Log(location, severity, e);
        }

        public static void Log(string location, Severity severity, string formatSpec, params object[] args)
        {
            _SmartAudioLog._Log(location, severity, formatSpec, args);
        }

        private bool Open() => 
            true;

        private bool Save()
        {
            if (SmartAudioSettings.LogFilePath != null)
            {
                this._logFile.Save(SmartAudioSettings.LogFilePath);
            }
            return true;
        }

        private static void SaveLog()
        {
            _SmartAudioLog.Save();
        }

        public bool IsDemoMode
        {
            get
            {
                App current = Application.Current as App;
                return ((current != null) && current.IsDemoMode);
            }
        }
    }
}

