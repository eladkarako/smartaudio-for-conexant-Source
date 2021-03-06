﻿namespace SmartAudio
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class EndPointToolTipConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double num = (double) value;
            return num.ToString("0.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            null;
    }
}

