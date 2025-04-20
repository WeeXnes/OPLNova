using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace PS2_Manager.Core;

public static class Util
{
    public static string? OpenFileDialogSync(Window parent)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select PS2 ISO",
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "PS2 ISO", Extensions = { "iso", "bin" } },
                new FileDialogFilter { Name = "All Files", Extensions = { "*" } }
            }
        };

        var result = dialog.ShowAsync(parent).GetAwaiter().GetResult();
        return result?.FirstOrDefault();
    }
}