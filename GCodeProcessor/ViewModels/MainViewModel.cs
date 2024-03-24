using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Text;
using GCodeProcessor.Helpers;
using System.Linq;
using System.Runtime.CompilerServices;
using GCodeProcessor.Views;
using System;
using StringHelpers;

namespace GCodeProcessor.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string filePath = "";

    [RelayCommand]
    private void ProcessNCFile()
    {
        if (FilePath.IsNullOrWhitespace())
        {
            ShowMessageBox("Please select a file first.", "Select File");
            return;
        }

        if(!File.Exists(FilePath))
        {
            ShowMessageBox($"Could not find {FilePath}", "File Not Found");
            return;
        }

        string[] lines;

        try
        {
            lines = File.ReadAllLines(FilePath);
        }
        catch
        {
            ShowMessageBox($"Cannot open and read {FilePath}", "File Error");
            return;
        }

        StringBuilder output = GCodeHelpers.GetCommentedFile(lines);

        string newFilePath = GetNewFilePath(FilePath);

        File.WriteAllText(newFilePath, output.ToString());

        Process.Start("notepad.exe", newFilePath);
    }

    /// <summary>
    /// Appends " Commented" to the given file name.
    /// </summary>
    /// <param name="oldFilePath"></param>
    /// <returns></returns>
    private static string GetNewFilePath(string oldFilePath)
    {
        return oldFilePath.AppendToFileName(" Commented").MakeUnique();
    }

    #region "MessageBox"
    public event EventHandler<MessageBoxEventArgs> ShowMessageBoxRequested;

    public class MessageBoxEventArgs : EventArgs
    {
        public MessageBoxEventArgs(string message, string title)
        {
            Message= message;
            Title = title;
        }

        public string Message { get; set; }
        public string Title { get; set; }
    }

    private void ShowMessageBox(string message, string title)
    {
        MessageBoxEventArgs args = new(message, title);
        ShowMessageBoxRequested?.Invoke(this, args);
    }
    #endregion
}
