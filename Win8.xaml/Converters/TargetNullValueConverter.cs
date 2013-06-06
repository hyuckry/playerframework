using System;
using System.Globalization;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Returns the parameter instead of the value if the value is null. Offers a good substitution for the TargetNullValue param on bindings in Silverlight or WPF
    /// </summary>
    public sealed class TargetNullValueConverter : IValueConverter
    {
        /// <inheritdoc /> 
#if SILVERLIGHT
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            return value ?? parameter;
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            throw new NotImplementedException();
        }
    }
}
