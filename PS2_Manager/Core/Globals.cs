using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using Microsoft.VisualBasic.CompilerServices;
using Path = System.IO.Path;

namespace PS2_Manager.Core;

public static class Globals
{
    public static string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ps2_manager", "settings.ini");
    public static string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ps2_manager", "output.log");
    public static string UserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ps2_manager");
    
    public static string ApiEndpointBase = "ps2api.weexnes.dev";
    
    public static List<Game> Games { get; set; }
    
    public static void LoadSettings()
    {
        Games = new List<Game>();
        
        ConfigUtils.InitConfig(SettingsPath);
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
    library_path,
    preferred_installation_method
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