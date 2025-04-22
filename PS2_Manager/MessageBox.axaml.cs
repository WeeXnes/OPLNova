using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace PS2_Manager;

public partial class MessageBox : Window
{
    public MessageBox(string messageContent)
    {
        InitializeComponent();
        ErrorDump.Content = "Exception: " + messageContent;
    }
    private void WindowDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}