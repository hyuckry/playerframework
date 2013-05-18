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
    public sealed class BufferingView : Control
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

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(BufferingView), new PropertyMetadata(null, (s, d) => ((BufferingView)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
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
