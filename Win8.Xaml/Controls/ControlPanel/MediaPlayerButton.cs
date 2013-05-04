using System;
using System.ComponentModel;
#if SILVERLIGHT
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Automation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Automation;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a special button that expects and manages a ViewModelCommand.
    /// </summary>
    public class MediaPlayerButton : Button
    {
        /// <summary>
        /// Creates a new instance of MediaPlayerButton
        /// </summary>
        internal MediaPlayerButton()
        {
            DefaultStyleKey = typeof(MediaPlayerButton);
        }

        /// <summary>
        /// Gets or sets the ViewModelCommand associated with the button.
        /// </summary>
        public IViewModelCommand ViewModelCommand
        {
            get { return Command as ViewModelCommand; }
            set
            {
                if (ViewModelCommand != null)
                {
                    ViewModelCommand.ViewModel = null;
                }
                Command = value;
                if (ViewModelCommand != null)
                {
                    ViewModelCommand.ViewModel = ViewModel;
                }
            }
        }

        /// <summary>
        /// Identifies the MediaPlayer dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(MediaPlayerButton), new PropertyMetadata(null, (d, e) => ((MediaPlayerButton)d).OnViewModelChanged(e.OldValue as IInteractiveViewModel, e.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Provides notification that the view model has changed.
        /// </summary>
        /// <param name="oldValue">The old view model. Note: this could be null.</param>
        /// <param name="newValue">The new view model. Note: this could be null.</param>
        protected virtual void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            if (ViewModelCommand != null)
            {
                ViewModelCommand.ViewModel = newValue;
            }
        }

        /// <summary>
        /// The InteractiveMediaPlayer object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        #region UI helper properties

        /// <summary>
        /// Identifies the Size dependency property.
        /// </summary>
        public static DependencyProperty SizeProperty { get { return sizeProperty; } }
        static readonly DependencyProperty sizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(MediaPlayerButton), new PropertyMetadata(26.0));

        /// <summary>
        /// Gets or sets the diameter of the button.
        /// </summary>
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static DependencyProperty StrokeThicknessProperty { get { return strokeThicknessProperty; } }
        static readonly DependencyProperty strokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MediaPlayerButton), new PropertyMetadata(2.0));

        /// <summary>
        /// Gets or sets the thickness of the button border.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentTransform dependency property.
        /// </summary>
        public static DependencyProperty ContentTransformProperty { get { return contentTransformProperty; } }
        static readonly DependencyProperty contentTransformProperty = DependencyProperty.Register("ContentTransform", typeof(Transform), typeof(MediaPlayerButton), null);

        /// <summary>
        /// Gets or sets the Transform to apply to the inner content of the button.
        /// </summary>
        public Transform ContentTransform
        {
            get { return GetValue(ContentTransformProperty) as Transform; }
            set { SetValue(ContentTransformProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentHover dependency property.
        /// </summary>
        public static DependencyProperty ContentHoverProperty { get { return contentHoverProperty; } }
        static readonly DependencyProperty contentHoverProperty = DependencyProperty.Register("ContentHover", typeof(object), typeof(MediaPlayerButton), new PropertyMetadata(null, OnContentHoverChanged));

        static void OnContentHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ButtonBase;
#if SILVERLIGHT
            if (control.IsMouseOver)
#else
            if (control.IsPointerOver)
#endif
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

#if SILVERLIGHT
        /// <inheritdoc />
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            Content = ContentHover ?? Content;
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Content = ContentUnhover ?? Content;
        }
#else
        /// <inheritdoc />
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            Content = ContentHover ?? Content;
        }

        /// <inheritdoc />
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            Content = ContentUnhover ?? Content;
        }
#endif
        /// <summary>
        /// Gets or sets the content that should appear when the user is hovering over the button
        /// </summary>
        public object ContentHover
        {
            get { return GetValue(ContentHoverProperty); }
            set { SetValue(ContentHoverProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentUnhover dependency property.
        /// </summary>
        public static DependencyProperty ContentUnhoverProperty { get { return contentUnhoverProperty; } }
        static readonly DependencyProperty contentUnhoverProperty = DependencyProperty.Register("ContentUnhover", typeof(object), typeof(MediaPlayerButton), new PropertyMetadata(null, OnContentUnhoverChanged));

        static void OnContentUnhoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ButtonBase;
#if SILVERLIGHT
            if (!control.IsMouseOver)
#else
            if (!control.IsPointerOver)
#endif
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content that should appear when the user is not hovering over the button
        /// </summary>
        public object ContentUnhover
        {
            get { return GetValue(ContentUnhoverProperty); }
            set { SetValue(ContentUnhoverProperty, value); }
        }


        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static DependencyProperty IsSelectedProperty { get { return isSelectedProperty; } }
        static readonly DependencyProperty isSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(MediaPlayerButton), new PropertyMetadata(false, OnIsSelectedChanged));

        static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayerButton;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                control.Content = control.SelectedContent ?? control.Content;
                AutomationProperties.SetName(control, control.SelectedName ?? AutomationProperties.GetName(control));
            }
            else
            {
                control.Content = control.UnselectedContent ?? control.Content;
                AutomationProperties.SetName(control, control.UnselectedName ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets whether the button is in a selected state. This can impact UI aspects of the control.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        #region Content
        /// <summary>
        /// Identifies the SelectedContent dependency property.
        /// </summary>
        public static DependencyProperty SelectedContentProperty { get { return selectedContentProperty; } }
        static readonly DependencyProperty selectedContentProperty = DependencyProperty.Register("SelectedContent", typeof(object), typeof(MediaPlayerButton), new PropertyMetadata(null, OnSelectedContentChanged));

        static void OnSelectedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayerButton;
            if (control.IsSelected)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in a selected state (IsSelected=true).
        /// </summary>
        public object SelectedContent
        {
            get { return GetValue(SelectedContentProperty); }
            set { SetValue(SelectedContentProperty, value); }
        }


        /// <summary>
        /// Identifies the UnselectedContent dependency property.
        /// </summary>
        public static DependencyProperty UnselectedContentProperty { get { return unselectedContentProperty; } }
        static readonly DependencyProperty unselectedContentProperty = DependencyProperty.Register("UnselectedContent", typeof(object), typeof(MediaPlayerButton), new PropertyMetadata(null, OnUnselectedContentChanged));

        static void OnUnselectedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayerButton;
            if (!control.IsSelected)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in an unselected state (IsSelected=false).
        /// </summary>
        public object UnselectedContent
        {
            get { return GetValue(UnselectedContentProperty); }
            set { SetValue(UnselectedContentProperty, value); }
        }

        #endregion

        #region Name
        /// <summary>
        /// Identifies the SelectedName dependency property.
        /// </summary>
        public static DependencyProperty SelectedNameProperty { get { return selectedNameProperty; } }
        static readonly DependencyProperty selectedNameProperty = DependencyProperty.Register("SelectedName", typeof(string), typeof(MediaPlayerButton), new PropertyMetadata(null, OnSelectedNameChanged));

        static void OnSelectedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayerButton;
            var newValue = e.NewValue as string;
            if (control.IsSelected)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an selected state (IsSelected=true).
        /// </summary>
        public string SelectedName
        {
            get { return GetValue(SelectedNameProperty) as string; }
            set { SetValue(SelectedNameProperty, value); }
        }

        /// <summary>
        /// Identifies the UnselectedName dependency property.
        /// </summary>
        public static DependencyProperty UnselectedNameProperty { get { return unselectedNameProperty; } }
        static readonly DependencyProperty unselectedNameProperty = DependencyProperty.Register("UnselectedName", typeof(string), typeof(MediaPlayerButton), new PropertyMetadata(null, OnUnselectedNameChanged));

        static void OnUnselectedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayerButton;
            var newValue = e.NewValue as string;
            if (!control.IsSelected)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an unselected state (IsSelected=false).
        /// </summary>
        public string UnselectedName
        {
            get { return GetValue(UnselectedNameProperty) as string; }
            set { SetValue(UnselectedNameProperty, value); }
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// Represents a special toggle button that expects and manages a ViewModelCommand.
    /// </summary>
    public class MediaPlayerToggleButton : ToggleButton
    {
        /// <summary>
        /// Creates a new instance of MediaPlayerToggleButton
        /// </summary>
        internal MediaPlayerToggleButton()
        {
            DefaultStyleKey = typeof(MediaPlayerToggleButton);
        }

        /// <summary>
        /// Gets or sets the ViewModelCommand associated with the button.
        /// </summary>
        public IViewModelCommand ViewModelCommand
        {
            get { return base.Command as ViewModelCommand; }
            set
            {
                if (ViewModelCommand != null)
                {
                    ViewModelCommand.ViewModel = null;
                }
                base.Command = value;
                if (ViewModelCommand != null)
                {
                    ViewModelCommand.ViewModel = ViewModel;
                }
            }
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(MediaPlayerToggleButton), new PropertyMetadata(null, (d, e) => ((MediaPlayerToggleButton)d).OnViewModelChanged(e.OldValue as IInteractiveViewModel, e.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Provides notification that the view model has changed.
        /// </summary>
        /// <param name="oldValue">The old view model. Note: this could be null.</param>
        /// <param name="newValue">The new view model. Note: this could be null.</param> 
        protected virtual void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            if (ViewModelCommand != null)
            {
                ViewModelCommand.ViewModel = newValue;
            }
        }

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        #region UI helper properties

        /// <summary>
        /// Identifies the Size dependency property.
        /// </summary>
        public static DependencyProperty SizeProperty { get { return sizeProperty; } }
        static readonly DependencyProperty sizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(MediaPlayerToggleButton), new PropertyMetadata(26.0));

        /// <summary>
        /// Gets or sets the diameter of the button.
        /// </summary>
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static DependencyProperty StrokeThicknessProperty { get { return strokeThicknessProperty; } }
        static readonly DependencyProperty strokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MediaPlayerToggleButton), new PropertyMetadata(2.0));

        /// <summary>
        /// Gets or sets the thickness of the button border.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentTransform dependency property.
        /// </summary>
        public static DependencyProperty ContentTransformProperty { get { return contentTransformProperty; } }
        static readonly DependencyProperty contentTransformProperty = DependencyProperty.Register("ContentTransform", typeof(Transform), typeof(MediaPlayerToggleButton), null);

        /// <summary>
        /// Gets or sets the Transform to apply to the inner content of the button.
        /// </summary>
        public Transform ContentTransform
        {
            get { return GetValue(ContentTransformProperty) as Transform; }
            set { SetValue(ContentTransformProperty, value); }
        }

        #endregion
    }

    /// <summary>
    /// Represents the base class for a control that needs to bind to the InteractiveViewModel
    /// </summary>
    public class MediaPlayerControl : ContentControl
    {
        /// <summary>
        /// Creates a new instance of MediaPlayerControl
        /// </summary>
        internal MediaPlayerControl()
        {
            DefaultStyleKey = typeof(MediaPlayerControl);
        }

        /// <summary>
        /// Identifies the MediaPlayer dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(MediaPlayerControl), new PropertyMetadata(null, (d, e) => ((MediaPlayerControl)d).OnViewModelChanged(e.OldValue as IInteractiveViewModel, e.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Provides notification that the view model has changed.
        /// </summary>
        /// <param name="oldValue">The old view model. Note: this could be null.</param>
        /// <param name="newValue">The new view model. Note: this could be null.</param>
        protected virtual void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
        }

        /// <summary>
        /// The InteractiveMediaPlayer object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
