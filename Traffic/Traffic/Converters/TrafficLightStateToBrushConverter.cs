using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Traffic.Models;

namespace Traffic.Converters
{
    public class TrafficLightStateToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TrafficLightState state)
            {
                return state == TrafficLightState.GreenForPedestrians
                    ? Brushes.LimeGreen
                    : Brushes.IndianRed;
            }

            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

