using System;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Data;
using System.Globalization;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// IValueConverter used to help Xaml bind a boolean to a Visibility property.
    /// </summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// A flag indicating whether or not the value should be flipped so false == Visible. Default false (true = Visible).
        /// </summary>
        public bool Inverse { get; set; }

        /// <summary>
        /// Converts a boolean to a Visibility enum
        /// </summary>
        /// <param name="visibility">the boolean to convert</param>
        /// <param name="inverse">A flag indicating whether or not the value should be flipped so false == Visible.</param>
        /// <returns>Visibility enum</returns>
        public static Visibility ConvertValue(bool visibility, bool inverse)
        {
            if (inverse)
                return visibility ? Visibility.Collapsed : Visibility.Visible;
            else
                return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility enum to a boolean
        /// </summary>
        /// <param name="visibility">The Visibility enum to convert</param>
        /// <param name="inverse">A flag indicating whether or not the value should be flipped so false == Visible.</param>
        /// <returns>A boolean value</returns>
        public static bool ConvertBackValue(Visibility visibility, bool inverse)
        {
            if (inverse)
                return visibility == Visibility.Collapsed;
            else
                return visibility == Visibility.Visible;
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            bool bValue;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else
            {
                bValue = value != null;
            }
            return ConvertValue(bValue, Inverse);
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            return ConvertBackValue((Visibility)value, Inverse);
        }
    }
}