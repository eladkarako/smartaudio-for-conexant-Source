namespace SmartAudio
{
    using System;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;

    public class StatusWindow : Window
    {
        private Timer _timer;

        public StatusWindow(int newTimeToWait)
        {
            this._timer = new Timer((double) newTimeToWait);
            base.Loaded += new RoutedEventHandler(this.StatusWindow_Loaded);
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
            base.Height = 100.0;
            base.Width = 300.0;
            base.AllowsTransparency = true;
            base.WindowStyle = WindowStyle.None;
            Border border = new Border {
                BorderBrush = Brushes.Black,
                CornerRadius = new CornerRadius(15.0),
                BorderThickness = new Thickness(3.0),
                Background = Brushes.Black
            };
            base.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            base.Background = Brushes.Transparent;
            TextBlock block = new TextBlock {
                Text = ". . . . . . . ",
                Foreground = Brushes.White
            };
            border.Child = block;
            base.Content = border;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnTimer(this.OnTimer));
        }

        private void OnTimer()
        {
            base.Close();
            this._timer.Enabled = false;
        }

        private void StatusWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._timer.Enabled = true;
        }
    }
}

