using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Text;
using GCodeProcessor.Helpers;
using System.Linq;

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

        string fileExtension = FilePath.Split(".").Last();
        string newFilePath = FilePath.Replace($".{fileExtension}", $" Commented.{fileExtension}");

        File.WriteAllText(newFilePath, output.ToString());

        Process.Start("notepad.exe", newFilePath);
    }

}
