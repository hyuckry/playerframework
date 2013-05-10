using System.Collections.Generic;
using System.Collections.ObjectModel;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework.CaptionMarkers
{
    /// <summary>
    /// Represents a panel control to host closed captions
    /// </summary>
    public sealed class CaptionsPanel : Control
    {
        /// <summary>
        /// Creates a new instances of CaptionsPanel
        /// </summary>
        public CaptionsPanel()
        {
            this.DefaultStyleKey = typeof(CaptionsPanel);
        }

        #region ActiveCaptions
        /// <summary>
        /// ActiveCaptions DependencyProperty definition.
        /// </summary>
        public static DependencyProperty ActiveCaptionsProperty { get { return activeCaptionsProperty; } }
        static readonly DependencyProperty activeCaptionsProperty = DependencyProperty.Register("ActiveCaptions", typeof(IList<ActiveCaption>), typeof(CaptionsPanel), null);

        /// <summary>
        /// Gets or sets the active captions to be displayed
        /// </summary>
        public IList<ActiveCaption> ActiveCaptions
        {
            get { return GetValue(ActiveCaptionsProperty) as IList<ActiveCaption>; }
            set { SetValue(ActiveCaptionsProperty, value); }
        }

        #endregion

    }
}
