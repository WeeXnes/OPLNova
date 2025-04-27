using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PS2_Manager.Core;


public class DiscIcon
{
    public bool ShowDVD { get; set; }
    public bool ShowCD { get; set; }
    public DiscType type { get; set; }

    public DiscIcon(DiscType _type)
    {
        this.type = _type;
        if (type == DiscType.CD)
        {
            this.ShowCD = true;
            this.ShowDVD = false;
        }else if (type == DiscType.DVD)
        {
            this.ShowCD = false;
            this.ShowDVD = true;
        }
    }
}


public enum DiscType
{
    DVD,
    CD
}