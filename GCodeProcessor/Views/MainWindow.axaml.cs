using Avalonia.Controls;
using GCodeProcessor.ViewModels;
using static GCodeProcessor.ViewModels.MainViewModel;

namespace GCodeProcessor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetMessageBoxEventHandler();
    }

    private void SetMessageBoxEventHandler()
    {
        MainViewModel vm = new();
        vm.ShowMessageBoxRequested += ShowMessageBox;
        this.MainView.DataContext = vm;
    }

    /// <summary>
    /// Creates a modal dialog popup with the given message and title.
    /// </summary>
    /// <param name="message"></param>
    public void ShowMessageBox(object sender, MessageBoxEventArgs args)
    {
        var viewModel = new MessageBoxViewModel(args.Message);
        var view = new MessageBoxView { DataContext = viewModel };
        var dialog = new Window
        {
            MinHeight = 100,
            MinWidth = 250,
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Title = args.Title
        };

        dialog.ShowDialog(this); // Show as a modal dialog
    }
}
