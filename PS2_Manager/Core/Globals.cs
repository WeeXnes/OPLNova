using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.VisualBasic.CompilerServices;

namespace PS2_Manager.Core;

public static class Globals
{
    
    public static List<Game> Games { get; set; }
    
    public static void LoadSettings()
    {
        Games = new List<Game>();
        ConfigUtils.InitConfig("settings.ini");
    }


    public static class FileDialogTypes
    {
        public static FilePickerFileType PS2Game { get; } = new("PS2 Game")
        {
            Patterns = new[] { "*.iso", "*.bin" }
        };
        public static FilePickerFileType PS2Cover { get; } = new("PS2 Cover")
        {
            Patterns = new[] { "*.png", "*.jpg" }
        };
    }
}

public enum settings
{
    library_path
}

public class UpdateVar<T>
{
    private T _value;

    public Action ValueChanged;

    public T Value
    {
        get => _value;

        set
        {
            _value = value;
            OnValueChanged();
        }
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke();
}

public class GameInfoApi
{
    public string id { get; set; }
    public string title { get; set; }
}