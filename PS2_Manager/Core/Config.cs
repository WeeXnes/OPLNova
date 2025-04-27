using System.Collections.Generic;
using System.IO;

namespace PS2_Manager.Core;


public class Config
{
    private string path { get; set; }
    public string MemoryCard1 { get; set; }
    public string MemoryCard2 { get; set; }
    public bool Mode1 { get; set; }
    public bool Mode2 { get; set; }
    public bool Mode3 { get; set; }
    public bool Mode4 { get; set; }
    public bool Mode5 { get; set; }
    public bool Mode6 { get; set; }

    public Config(string path)
    {
        this.path = path;
        this.ParseConfig(path);
    }

    public override string ToString()
    {
        return $"VMCs: {MemoryCard1} - {MemoryCard2}\n" +
               $"Mode 1: {Mode1}\n" +
               $"Mode 2: {Mode2}\n" +
               $"Mode 3: {Mode3}\n" +
               $"Mode 4: {Mode4}\n" +
               $"Mode 5: {Mode5}\n" +
               $"Mode 6: {Mode6}\n";
    }

    private void FromCompatibility(int value)
    {
        Mode1 = (value & (1 << 0)) != 0;
        Mode2 = (value & (1 << 1)) != 0;
        Mode3 = (value & (1 << 2)) != 0;
        Mode4 = (value & (1 << 3)) != 0;
        Mode5 = (value & (1 << 4)) != 0;
        Mode6 = (value & (1 << 5)) != 0;
    }

    private int ToCompatibility()
    {
        int value = 0;
        if (Mode1) value |= (1 << 0);
        if (Mode2) value |= (1 << 1);
        if (Mode3) value |= (1 << 2);
        if (Mode4) value |= (1 << 3);
        if (Mode5) value |= (1 << 4);
        if (Mode6) value |= (1 << 5);
        return value;
    }
    public void SaveConfig()
    {
        var lines = new List<string>();

        if (!string.IsNullOrEmpty(MemoryCard1))
            lines.Add($"$VMC_0={MemoryCard1}");

        if (!string.IsNullOrEmpty(MemoryCard2))
            lines.Add($"$VMC_1={MemoryCard2}");

        lines.Add($"$Compatibility={ToCompatibility()}");

        lines.Add($"$ConfigSource=1");

        File.WriteAllLines(path, lines);
    }



    private void ParseConfig(string path)
    {
        if (!File.Exists(path))
            return;

        var lines = File.ReadAllLines(path);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            switch (key)
            {
                case "$Compatibility":
                    if (int.TryParse(value, out int compat))
                        this.FromCompatibility(compat);
                    break;

                case "$VMC_0":
                    this.MemoryCard1 = value;
                    break;

                case "$VMC_1":
                    this.MemoryCard2 = value;
                    break;

                case "$ConfigSource":
                    // Optional: You can store/use this if needed
                    break;
            }
        }
    }
}