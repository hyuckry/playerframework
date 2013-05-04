#if SILVERLIGHT
using System.Windows;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents methods that will handle various routed events that track property value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property value where changes in value are reported.</typeparam>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void InteractiveViewModelChangedEventHandler(object sender, InteractiveViewModelChangedEventArgs e);

    /// <summary>
    /// Provides data about a change in value to a dependency property as reported by particular routed events, including hte previous and current value of the property that changed.
    /// </summary>
    /// <typeparam name="T">The type of the dependency property that has changed.</typeparam>
    public sealed class InteractiveViewModelChangedEventArgs 
    {
        /// <summary>
        /// Creates a new instance of InteractiveViewModelChangedEventArgs.
        /// </summary>
        /// <param name="oldValue">the previous value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        internal InteractiveViewModelChangedEventArgs(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public IInteractiveViewModel NewValue { get; internal set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public IInteractiveViewModel OldValue { get; internal set; }
    }
}
