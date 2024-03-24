using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GCodeProcessor.ViewModels;

internal partial class MessageBoxViewModel : ViewModelBase
{
    [ObservableProperty]
    private string message = "";

    public MessageBoxViewModel(string message)
    {
        Message = message;
    }
}
