using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// Helper class used to attach config data specific to each playlist item.
    /// </summary>
    public static class AdScheduler
    {
        /// <summary>
        /// Identifies the Advertisements attached property.
        /// </summary>
        public static DependencyProperty AdvertisementsProperty { get { return advertisementsProperty; } }
        static readonly DependencyProperty advertisementsProperty = DependencyProperty.RegisterAttached("Advertisements", typeof(IList<IAdvertisement>), typeof(AdScheduler), null);

        /// <summary>
        /// Sets the Advertisements attached property value.
        /// </summary>
        /// <param name="obj">An instance of the MediaPlayer or PlaylistItem.</param>
        /// <param name="propertyValue">A value containing the Advertisements to apply to the plugin.</param>
        public static void SetAdvertisements(DependencyObject obj, IList<IAdvertisement> propertyValue)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            obj.SetValue(AdvertisementsProperty, propertyValue);
        }

        /// <summary>
        /// Gets the Advertisements attached property value.
        /// </summary>
        /// <param name="obj">An instance of the MediaPlayer or PlaylistItem.</param>
        /// <returns>A value containing the Advertisements to apply to the plugin.</returns>
        public static IList<IAdvertisement> GetAdvertisements(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            return obj.GetValue(AdvertisementsProperty) as IList<IAdvertisement>;
        }

    }
}
