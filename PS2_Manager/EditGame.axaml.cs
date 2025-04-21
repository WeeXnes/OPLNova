using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class EditGame : UserControl
{
    public Game game { get; set; }
    public EditGame(Game _game)
    {
        this.game = _game;
        InitializeComponent();
    }

    private void DisplayNameBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        this.game.Name = DisplayNameBox.Text;
        MainWindow.RefreshGamesListTrigger.Invoke(null, EventArgs.Empty);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.game.GetChecksum();
    }
}