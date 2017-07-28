namespace SmartAudio
{
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    public class RearJacksPanel : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        internal TextBlock _lineinText;
        internal TextBlock _lineOutText;
        internal TextBlock _micText;

        public RearJacksPanel()
        {
            this.InitializeComponent();
            this.RefreshJacks();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/rearjackspanel.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void RefreshJacks()
        {
            this._lineinText.Text = Resources.SA_LineIn.Trim() + " (" + Resources.SA_Surround + ")";
            this._lineOutText.Text = Resources.SA_LineOut.Trim() + "/" + Resources.SA_CHeadPhones;
            this._micText.Text = Resources.SA_Microphone + " (" + Resources.SA_Center.Trim() + "/" + Resources.SA_Subwoofer.Trim() + ")";
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._lineinText = (TextBlock) target;
                    return;

                case 2:
                    this._lineOutText = (TextBlock) target;
                    return;

                case 3:
                    this._micText = (TextBlock) target;
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

