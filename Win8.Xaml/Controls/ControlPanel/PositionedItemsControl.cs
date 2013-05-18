using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// An ItemsControl used to position items in a linear path based on relative coordinates (defined by attached properties).
    /// </summary>
    public sealed class PositionedItemsControl : Panel
    {
        /// <summary>
        /// Raised when a new item is loaded/added
        /// </summary>
        public event EventHandler<FrameworkElementEventArgs> ItemLoaded;

        /// <summary>
        /// Raised when an item is unloaded/removed
        /// </summary>
        public event EventHandler<FrameworkElementEventArgs> ItemUnloaded;

        #region ItemsSource
        /// <summary>
        /// ItemsSource DependencyProperty definition.
        /// </summary>
#if SILVERLIGHT
        public static DependencyProperty ItemsSourceProperty { get { return itemsSourceProperty; } }
        static readonly DependencyProperty itemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PositionedItemsControl), new PropertyMetadata(null, (d, e) => ((PositionedItemsControl)d).OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable)));

#else
        // TODO: Bug in Win8 doesn't allow us to bind to IEnumerable. Remove when fixed.
        public static DependencyProperty ItemsSourceProperty { get { return itemsSourceProperty; } }
        static readonly DependencyProperty itemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(PositionedItemsControl), new PropertyMetadata(null, (d, e) => ((PositionedItemsControl)d).OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable)));

#endif
        void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue != null)
            {
                if (oldValue is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)oldValue).CollectionChanged -= CollectionChanged;
                }
            }
            Children.Clear();

            if (newValue != null)
            {
                if (newValue is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)newValue).CollectionChanged += CollectionChanged;
                }

                foreach (var item in newValue)
                {
                    LoadNewItem(item);
                }
            }
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    LoadNewItem(item);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    UnloadNewItem(item);
                }
            }
        }

        private void UnloadNewItem(object item)
        {
            var child = Children.OfType<FrameworkElement>().FirstOrDefault(c => c.DataContext == item);
            if (child != null)
            {
                child.DataContext = null;
                Children.Remove(child);
                if (ItemUnloaded != null) ItemLoaded(this, new FrameworkElementEventArgs(child));
            }
        }

        private void LoadNewItem(object item)
        {
            var child = ItemTemplate.LoadContent() as FrameworkElement;
            if (child != null)
            {
                child.DataContext = item;
                Children.Add(child);
                if (ItemLoaded != null) ItemLoaded(this, new FrameworkElementEventArgs(child));
            }
        }

        /// <summary>
        /// Gets or sets the actual value of the slider to be able to maintain the value of the slider while the user is scrubbing.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        #endregion

        #region ItemTemplate
        /// <summary>
        /// ItemTemplate DependencyProperty definition.
        /// </summary>
        public static DependencyProperty ItemTemplateProperty { get { return itemTemplateProperty; } }
        static readonly DependencyProperty itemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(PositionedItemsControl), new PropertyMetadata(null, (d, e) => ((PositionedItemsControl)d).OnItemTemplateChanged(e.NewValue as DataTemplate)));

        void OnItemTemplateChanged(DataTemplate newValue)
        {
        }

        /// <summary>
        /// Gets or sets the actual value of the slider to be able to maintain the value of the slider while the user is scrubbing.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        #endregion

        // dependency property notification
        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PositionedItemsControl)d).InvalidateArrange();
        }

        #region Minimum
        /// <summary>
        /// Minimum DependencyProperty definition.
        /// </summary>
        public static DependencyProperty MinimumProperty { get { return minimumProperty; } }
        static readonly DependencyProperty minimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(PositionedItemsControl), new PropertyMetadata(0.0, OnDependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the minimum position of the items.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        #endregion

        #region Maximum
        /// <summary>
        /// Maximum DependencyProperty definition.
        /// </summary>
        public static DependencyProperty MaximumProperty { get { return maximumProperty; } }
        static readonly DependencyProperty maximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(PositionedItemsControl), new PropertyMetadata(100.0, OnDependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the maximum position of the items.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        #endregion

        #region MaxPosition
        /// <summary>
        /// MaxPosition DependencyProperty definition.
        /// </summary>
        public static DependencyProperty MaxPositionProperty { get { return maxPositionProperty; } }
        static readonly DependencyProperty maxPositionProperty = DependencyProperty.Register("MaxPosition", typeof(double?), typeof(PositionedItemsControl), new PropertyMetadata(null, OnDependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the max position.
        /// </summary>
        public double? MaxPosition
        {
            get { return (double?)GetValue(MaxPositionProperty); }
            set { SetValue(MaxPositionProperty, value); }
        }
        #endregion

        #region MinPosition
        /// <summary>
        /// MinPosition DependencyProperty definition.
        /// </summary>
        public static DependencyProperty MinPositionProperty { get { return minPositionProperty; } }
        static readonly DependencyProperty minPositionProperty = DependencyProperty.Register("MinPosition", typeof(double?), typeof(PositionedItemsControl), new PropertyMetadata(null, OnDependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the min position.
        /// </summary>
        public double? MinPosition
        {
            get { return (double?)GetValue(MinPositionProperty); }
            set { SetValue(MinPositionProperty, value); }
        }
        #endregion

        #region DisplayAllItems
        /// <summary>
        /// DisplayAllItemsProperty DependencyProperty definition.
        /// </summary>
        public static DependencyProperty DisplayAllItemsProperty { get { return displayAllItemsProperty; } }
        static readonly DependencyProperty displayAllItemsProperty = DependencyProperty.Register("DisplayAllItems", typeof(bool), typeof(PositionedItemsControl), new PropertyMetadata(true, OnDependencyPropertyChanged));

        /// <summary>
        /// Gets or sets whether items outside the MinPosition and MaxPosition are displayed
        /// </summary>
        public bool DisplayAllItems
        {
            get { return (bool)GetValue(DisplayAllItemsProperty); }
            set { SetValue(DisplayAllItemsProperty, value); }
        }
        #endregion

        /// <inheritdoc /> 
        protected override Size MeasureOverride(Size availableSize)
        {
            // this is the first pass in the layout, each control updates
            // its DesiredSize property, which is used later in ArrangeOverride
            foreach (var child in Children)
            {
                child.Measure(availableSize);
            }
            return base.MeasureOverride(availableSize);
        }

        /// <inheritdoc /> 
        protected override Size ArrangeOverride(Size finalSize)
        {
            // this is the bounds where items are visisble
            double startPosition = MinPosition.HasValue && !DisplayAllItems ? MinPosition.Value : Minimum;
            double endPosition = MaxPosition.HasValue && !DisplayAllItems ? MaxPosition.Value : Maximum;

            // go through each marker control and layout on the timeline
            foreach (UIElement childControl in Children)
            {
                DependencyObject child;
                if (childControl is ContentPresenter)
                {
                    ContentPresenter presenter = childControl as ContentPresenter;
                    child = presenter.Content as DependencyObject;
                }
                else
                {
                    child = childControl;
                }

                double childPosition = child != null ? GetPosition(child) : (double)PositionProperty.GetMetadata(typeof(PositionedItemsControl)).DefaultValue;

                // make sure the child is within the range
                if (childPosition < startPosition || childPosition > endPosition)
                {
                    // don't display the marker
                    childControl.Arrange(new Rect(0, 0, 0, 0));
                }
                else
                {
                    double relativePosition = (childPosition - Minimum) / (Maximum - Minimum);

                    // calculate the top position, center the item vertically
                    double top = (finalSize.Height - childControl.DesiredSize.Height) / 2;

                    // calculate the left position, first get the pixel position
                    double left = relativePosition * finalSize.Width;

                    // next adjust the position so the center of the control
                    // note that the control can overhang the left or right side of the timeline
                    left -= (childControl.DesiredSize.Width / 2);

                    // display the marker
                    childControl.Arrange(new Rect(left, top, childControl.DesiredSize.Width, childControl.DesiredSize.Height));
                }
            }

            return base.ArrangeOverride(finalSize);
        }

        /// <summary>
        /// Position AttachedProperty definition.
        /// </summary>
        public static DependencyProperty PositionProperty { get { return positionProperty; } }
        static readonly DependencyProperty positionProperty = DependencyProperty.RegisterAttached("Position", typeof(double), typeof(PositionedItemsControl), new PropertyMetadata(0.0));

        /// <summary>
        /// Sets the position on an item.
        /// </summary>
        /// <param name="obj">The object to set the position on.</param>
        /// <param name="propertyValue">The position of the object.</param>
        public static void SetPosition(DependencyObject obj, double propertyValue)
        {
            obj.SetValue(PositionProperty, propertyValue);
        }

        /// <summary>
        /// Gets the position on an item.
        /// </summary>
        /// <param name="obj">The object to retrieve the position from.</param>
        /// <returns></returns>
        public static double GetPosition(DependencyObject obj)
        {
            return (double)obj.GetValue(PositionProperty);
        }
    }

    /// <summary>
    /// Represents event args that contain a FrameworkElement.
    /// </summary>
#if SILVERLIGHT
    public sealed class FrameworkElementEventArgs : EventArgs
#else
    public sealed class FrameworkElementEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of FrameworkElementEventArgs.
        /// </summary>
        /// <param name="element">The element associated with the event args.</param>
        public FrameworkElementEventArgs(FrameworkElement element)
        {
            Element = element;
        }

        /// <summary>
        /// Gets the element associated with the event args.
        /// </summary>
        public FrameworkElement Element { get; private set; }
    }
}
