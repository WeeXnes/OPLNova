using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;

namespace OPLNova.Core;

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

    public static void CheckDir(string path)
    {
        //Console.Info("Checking directory: " + path);
        if (!Directory.Exists(path))
        {
            Console.Error("Directory " + path + " wasn't found, creating now.");
            Directory.CreateDirectory(path);
        }
        else
        {
            Console.Success("Directory was found: " + path);
        }
    }
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        // Format to 1 decimal place
        return string.Format("{0:0.0} {1}", len, sizes[order]);
    }
    
    public static bool IsBitmapEmpty(Bitmap? bitmap)
    {
        return bitmap == null;
    }
}


public static class Checksum
{
    public static string GetMD5Hash(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string GetSHA1Hash(string filePath)
    {
        using (var sha1 = SHA1.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    

}