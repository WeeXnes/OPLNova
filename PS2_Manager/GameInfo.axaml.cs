using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class GameInfo : UserControl
{
    public Game game { get; set; }
    public GameInfo(Game _game)
    {
        InitializeComponent();
        this.game = _game;
        CoverImage.Source = game.Cover;
        GameNameTextBlock.Text = game.Name;
        GameIdTextBlock.Text = game.GameID;
        FileInfo fileInfo = new FileInfo(game.GamePath);
        IsoSizeTextBlock.Text = Util.FormatFileSize(fileInfo.Length);
        DateTime lastModified = fileInfo.LastWriteTime;
        string formattedDate = lastModified.ToString("dd.MM.yyyy HH:mm");
        GameDateTextBlock.Text = formattedDate;
    }
}