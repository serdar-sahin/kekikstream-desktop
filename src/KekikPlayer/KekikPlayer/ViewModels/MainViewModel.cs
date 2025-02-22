using Avalonia.Threading;
using KekikPlayer.Core.ViewModel;
using System;

namespace KekikPlayer.ViewModels;

public partial class MainViewModel : KekikPlayerBaseViewModel
{
    public MainViewModel()
    {
        this.Volume = 50;
    }

    public override void InvokeInUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

}
