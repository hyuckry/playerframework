using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.PlayerFramework.Captions
{
    public class CaptionMarkerConverter : IValueConverter
    {
        public static ObservableCollection<CaptionMarker> Convert(TimelineMarkerCollection markers)
        {
            var result = new ObservableCollection<CaptionMarker>();
            if (markers != null)
            {
                foreach (var marker in markers)
                {
                    result.Add(marker.ToCaptionMarker());
                }
            }
            return result;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value as TimelineMarkerCollection);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
