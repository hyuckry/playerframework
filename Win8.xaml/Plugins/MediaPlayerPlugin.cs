using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Microsoft.PlayerFramework
{
    public static class MediaPlayerPlugin
    {
        static readonly DependencyProperty isEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(MediaPlayerPlugin), new PropertyMetadata(true));
        /// <summary>
        /// Identifies the IsEnabled attached property.
        /// </summary>
        public static DependencyProperty IsEnabledProperty
        {
            get
            {
                return isEnabledProperty;
            }
        }

        static void OnIsEnabledChanged(DependencyObject obj, bool oldValue, bool newValue)
        {
            var plugin = obj as IPlugin;
            if (plugin != null)
            {
                if (newValue)
                {
                    plugin.Unload();
                }
                else
                {
                    plugin.Load();
                }
            }
        }

        /// <summary>
        /// Sets the IsEnabled attached property value.
        /// </summary>
        /// <param name="obj">An instance of the MediaPlayer or PlaylistItem.</param>
        /// <param name="propertyValue">A value indicating if the plugin should be enabled.</param>
        public static void SetIsEnabled(DependencyObject obj, bool propertyValue)
        {
            obj.SetValue(IsEnabledProperty, propertyValue);
        }

        /// <summary>
        /// Gets the IsEnabled attached property value.
        /// </summary>
        /// <param name="obj">An instance of the MediaPlayer or PlaylistItem.</param>
        /// <returns>A value indicating if the plugin should be enabled.</returns>
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
    }
}
