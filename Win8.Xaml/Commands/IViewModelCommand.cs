using System;
using System.Windows.Input;

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a Command object that requires an instance of InteractiveViewModel to work.
    /// </summary>
    public interface IViewModelCommand : ICommand
    {
        /// <summary>
        /// Gets or sets the ViewModel associated with the command
        /// </summary>
        IInteractiveViewModel ViewModel { get; set; }
    }
}
