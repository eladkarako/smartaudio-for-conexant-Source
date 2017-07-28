namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Shapes;

    public class EnergyBarsUserControl : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        internal Rectangle rectangle1;
        internal Rectangle rectangle10;
        internal Rectangle rectangle11;
        internal Rectangle rectangle12;
        internal Rectangle rectangle13;
        internal Rectangle rectangle14;
        internal Rectangle rectangle2;
        internal Rectangle rectangle3;
        internal Rectangle rectangle4;
        internal Rectangle rectangle5;
        internal Rectangle rectangle6;
        internal Rectangle rectangle7;
        internal Rectangle rectangle8;
        internal Rectangle rectangle9;

        public EnergyBarsUserControl()
        {
            this.InitializeComponent();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/energybarsusercontrol.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.rectangle1 = (Rectangle) target;
                    return;

                case 2:
                    this.rectangle2 = (Rectangle) target;
                    return;

                case 3:
                    this.rectangle3 = (Rectangle) target;
                    return;

                case 4:
                    this.rectangle4 = (Rectangle) target;
                    return;

                case 5:
                    this.rectangle5 = (Rectangle) target;
                    return;

                case 6:
                    this.rectangle6 = (Rectangle) target;
                    return;

                case 7:
                    this.rectangle7 = (Rectangle) target;
                    return;

                case 8:
                    this.rectangle8 = (Rectangle) target;
                    return;

                case 9:
                    this.rectangle9 = (Rectangle) target;
                    return;

                case 10:
                    this.rectangle10 = (Rectangle) target;
                    return;

                case 11:
                    this.rectangle11 = (Rectangle) target;
                    return;

                case 12:
                    this.rectangle12 = (Rectangle) target;
                    return;

                case 13:
                    this.rectangle13 = (Rectangle) target;
                    return;

                case 14:
                    this.rectangle14 = (Rectangle) target;
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

