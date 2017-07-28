namespace SmartAudio
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class BoostToolTipConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double num = (double) value;
            return (num.ToString("0.") + " dB");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            null;
    }
}

