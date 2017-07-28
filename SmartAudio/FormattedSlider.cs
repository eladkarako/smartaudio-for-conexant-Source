namespace SmartAudio
{
    using System;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class FormattedSlider : Slider
    {
        private ToolTip _autoToolTip;
        private string _autoToolTipFormat;

        private void FormatAutoToolTipContent()
        {
            if (!string.IsNullOrEmpty(this.AutoToolTipFormat))
            {
                this.AutoToolTip.Content = string.Format(this.AutoToolTipFormat, this.AutoToolTip.Content);
            }
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            this.FormatAutoToolTipContent();
        }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            this.FormatAutoToolTipContent();
        }

        private ToolTip AutoToolTip
        {
            get
            {
                if (this._autoToolTip == null)
                {
                    FieldInfo field = typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
                    this._autoToolTip = field.GetValue(this) as ToolTip;
                }
                return this._autoToolTip;
            }
        }

        public string AutoToolTipFormat
        {
            get => 
                this._autoToolTipFormat;
            set
            {
                this._autoToolTipFormat = value;
            }
        }
    }
}

