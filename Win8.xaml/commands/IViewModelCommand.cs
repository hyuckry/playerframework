using System;
using System.Windows.Input;

namespace Microsoft.PlayerFramework
{
    public interface IViewModelCommand : ICommand
    {
        IInteractiveViewModel ViewModel { get; set; }
    }
}
