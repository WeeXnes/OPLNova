using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class GameInfo : UserControl
{
    public Game game { get; set; }
    public GameInfo(Game _game)
    {
        InitializeComponent();
        this.game = _game;

    }




    
}