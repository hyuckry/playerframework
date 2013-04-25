using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.PlayerFramework.Captions
{
    public class CaptionsPanel : Control
    {
        public CaptionsPanel()
        {
            this.DefaultStyleKey = typeof(CaptionsPanel);
        }

        #region ActiveCaptions
        /// <summary>
        /// ActiveCaptions DependencyProperty definition.
        /// </summary>
#if SILVERLIGHT
        public static readonly DependencyProperty ActiveCaptionsProperty = DependencyProperty.Register("ActiveCaptions", typeof(IEnumerable<CaptionMarker>), typeof(CaptionsPanel), null);
#else
        public static readonly DependencyProperty ActiveCaptionsProperty = DependencyProperty.Register("ActiveCaptions", "Object", typeof(CaptionsPanel).FullName, null);
#endif

        /// <summary>
        /// Gets or sets the active captions to be displayed
        /// </summary>
        public IEnumerable<CaptionMarker> ActiveCaptions
        {
            get { return (IEnumerable<CaptionMarker>)GetValue(ActiveCaptionsProperty); }
            set { SetValue(ActiveCaptionsProperty, value); }
        }

        #endregion

    }
}
