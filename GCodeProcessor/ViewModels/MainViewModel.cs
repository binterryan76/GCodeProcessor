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
            // TODO: add MessageBox.Show("Please select a file first.");
            return;
        }

        if(!File.Exists(FilePath))
        {
            // TODO: add MessageBox.Show;
            return;
        }

        string[] lines;

        try
        {
            lines = File.ReadAllLines(FilePath);
        }
        catch
        {
            // TODO: add MessageBox.Show($"Cannot open and read {textBoxFile.Text}");
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
}
