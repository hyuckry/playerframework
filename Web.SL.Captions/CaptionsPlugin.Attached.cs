using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.PlayerFramework.Captions
{
    public partial class CaptionsPlugin
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(CaptionsPlugin), new PropertyMetadata(true));

        public static void SetIsEnabled(DependencyObject obj, bool propertyValue)
        {
            obj.SetValue(IsEnabledProperty, propertyValue);
        }
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static readonly DependencyProperty AutoLoadMarkersProperty = DependencyProperty.RegisterAttached("AutoLoadMarkers", typeof(bool), typeof(CaptionsPlugin), new PropertyMetadata(false));

        public static void SetAutoLoadMarkers(DependencyObject obj, bool propertyValue)
        {
            obj.SetValue(AutoLoadMarkersProperty, propertyValue);
        }
        public static bool GetAutoLoadMarkers(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoLoadMarkersProperty);
        }

        public static readonly DependencyProperty CaptionsProperty = DependencyProperty.RegisterAttached("Captions", typeof(ObservableCollection<CaptionMarker>), typeof(CaptionsPlugin), new PropertyMetadata(new ObservableCollection<CaptionMarker>()));

        public static void SetCaptions(DependencyObject obj, ObservableCollection<CaptionMarker> propertyValue)
        {
            obj.SetValue(CaptionsProperty, propertyValue);
        }
        public static ObservableCollection<CaptionMarker> GetCaptions(DependencyObject obj)
        {
            return obj.GetValue(CaptionsProperty) as ObservableCollection<CaptionMarker>;
        }
    }
}
