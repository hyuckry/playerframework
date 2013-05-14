#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A control that indicates buffering is occuring.
    /// </summary>
    public sealed class BufferingView : Control, IMediaPlayerControl
    {
        /// <summary>
        /// Creates a new instance of the control
        /// </summary>
        public BufferingView()
        {
            this.DefaultStyleKey = typeof(BufferingView);
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            UpdateVisualStates();
        }

        void IMediaPlayerControl.OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            if (oldValue != null)
            {
                oldValue.CurrentStateChanged -= ViewModel_CurrentStateChanged;
            }

            UpdateVisualStates();

            if (newValue != null)
            {
                newValue.CurrentStateChanged += ViewModel_CurrentStateChanged;
            }
        }

        IInteractiveViewModel ViewModel
        {
            get { return MediaPlayerControl.GetViewModel(this); }
        }
        
        void ViewModel_CurrentStateChanged(object sender, object e)
        {
            UpdateVisualStates();
        }

        private void UpdateVisualStates()
        {
            if (ViewModel == null)
            {
                VisualStateManager.GoToState(this, MediaElementState.Closed.ToString(), true);
            }
            else
            {
                VisualStateManager.GoToState(this, ViewModel.CurrentState.ToString(), true);
            }
        }
    }
}
