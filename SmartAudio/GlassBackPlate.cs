namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    public class GlassBackPlate : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        internal Grid _grid;
        internal Border border;
        internal Border shine;

        public GlassBackPlate()
        {
            this.InitializeComponent();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/glassbackplate.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void ShowGrid(int numRows, int numCols, double height, double width)
        {
            double num = height / ((double) numRows);
            double num2 = width / ((double) numCols);
            this._grid.ShowGridLines = true;
            for (int i = 0; i <= numRows; i++)
            {
                RowDefinition definition = new RowDefinition {
                    Height = new GridLength(num, GridUnitType.Star)
                };
                this._grid.RowDefinitions.Add(definition);
            }
            for (int j = 0; j <= numCols; j++)
            {
                ColumnDefinition definition2 = new ColumnDefinition {
                    Width = new GridLength(num2, GridUnitType.Star)
                };
                this._grid.ColumnDefinitions.Add(definition2);
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._grid = (Grid) target;
                    return;

                case 2:
                    this.border = (Border) target;
                    return;

                case 3:
                    this.shine = (Border) target;
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

