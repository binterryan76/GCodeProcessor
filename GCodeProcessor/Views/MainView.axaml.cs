using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GCodeProcessor.ViewModels;
using System.Collections.Generic;
using System.IO;

namespace GCodeProcessor.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void FileSelection(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
        {
            // TODO: add message box
            return;
        }

        // Start async operation to open the dialog.
        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count < 1)
            return;

        IStorageFile firstFile = files[0];
        TextBoxNCFile.Text = firstFile.Path.AbsolutePath;
    }
}
