using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.VisualBasic.CompilerServices;

namespace PS2_Manager.Core;

public class Game
{
    public string Name { get; set; }
    public string GameID { get; set; }
    public string GamePath { get; set; }
    public Bitmap? ArtworkFront { get; set; }
    public Bitmap? ArtworkBack { get; set; }
    public Bitmap? ArtworkDVD { get; set; }
    public EventHandler? InstallationFinished { get; set; }
    public UpdateVar<double> InstallProgress { get; private set; }

    public Game(string isoPath, bool installed = false)
    {
        this.GamePath = isoPath;
        this.GameID = ISO.GetSerial(isoPath);
        if (!installed)
        {
            this.ArtworkFront = this.GetCover();
        }
        else
        {
            this.Name = ParseFormattedFilename(Path.GetFileName(isoPath));
            this.ArtworkFront = this.LoadCover(Artwork.Type.Front);
            this.ArtworkBack = this.LoadCover(Artwork.Type.Back);
            this.ArtworkDVD = this.LoadCover(Artwork.Type.Disc);
        }
    }

    public void GetChecksum()
    {
        string filePath = this.GamePath;

        BackgroundWorker md5Worker = new BackgroundWorker();
        BackgroundWorker sha1Worker = new BackgroundWorker();
        
        md5Worker.DoWork += (s, e) =>
        {
            string md5 = Checksum.GetMD5Hash(filePath);
            Console.WriteLine("MD5: " + md5);
        };

        sha1Worker.DoWork += (s, e) =>
        {
            string sha1 = Checksum.GetSHA1Hash(filePath);
            Console.WriteLine("SHA1: " + sha1);
        };

        md5Worker.RunWorkerAsync();
        sha1Worker.RunWorkerAsync();

        Console.WriteLine("Hashing started...");
    }

    private string ParseFormattedFilename(string filename)
    {
        string[] parts = filename.Split('.');
        return parts[^2];
    }

    public async void Install()
    {
        this.InstallProgress = new UpdateVar<double>();
        await this.CopyIsoWithProgressAsync();
    }

    private Bitmap GetCover()
    {
        
        Bitmap cover = null;
        string url = $"https://github.com/xlenore/ps2-covers/blob/main/covers/default/{this.GameID.Replace(".", "").Replace("_", "-")}.jpg?raw=true";

        try
        {
            using HttpClient client = new();
            byte[] data = client.GetByteArrayAsync(url).GetAwaiter().GetResult();

            using MemoryStream stream = new(data);
            var bitmap = new Bitmap(stream);

            cover = bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Could not load cover for {this.GameID}: {ex.Message}");
        }

        return cover;
    }
    
    public async Task CopyIsoWithProgressAsync()
    {
        string targetDirectory = settings.library_path.GetValue<string>();
        Util.CheckDir(Path.Combine(targetDirectory, "DVD"));
        
        string newFileName = $"{this.GameID}.{this.Name}.iso";
        string destPath = Path.Combine(Path.Combine(targetDirectory, "DVD"), newFileName);

        const int bufferSize = 1024 * 1024;
        byte[] buffer = new byte[bufferSize];

        long totalBytes = new FileInfo(this.GamePath).Length;
        long bytesCopied = 0;

        using (FileStream source = new FileStream(this.GamePath, FileMode.Open, FileAccess.Read))
        using (FileStream destination = new FileStream(destPath, FileMode.Create, FileAccess.Write))
        {
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, bufferSize))) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
                bytesCopied += bytesRead;

                this.InstallProgress.Value = (double)bytesCopied / totalBytes * 100;
            }
        }
        
        //this.InstallCover();
        this.InstallationFinished?.Invoke(this, EventArgs.Empty);
        MainWindow.RefreshGamesListTrigger?.Invoke(this, EventArgs.Empty);

        Console.WriteLine($"Copied ISO to: {destPath}");
    }





    public Bitmap? LoadCover(Artwork.Type artworkType)
    {
        string targetDirectory = settings.library_path.GetValue<string>();
        Util.CheckDir(Path.Combine(targetDirectory, "ART"));
        try
        {
            switch (artworkType)
            {
                case Artwork.Type.Front:
                    return new Bitmap(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_COV.png"));
                    break;
                case Artwork.Type.Back:
                    return new Bitmap(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_COV2.png"));
                    break;
                case Artwork.Type.Disc:
                    return new Bitmap(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_ICO.png"));
                    break;
                default:
                    return null;
                    break;
            }
        }
        catch
        {
            return null;
        }
    }
    public void SaveCover(Artwork.Type artworkType)
    {
        string targetDirectory = settings.library_path.GetValue<string>();
        Util.CheckDir(Path.Combine(targetDirectory, "ART"));
        switch (artworkType)
        {
            case Artwork.Type.Front:
                this.ArtworkFront?.CreateScaledBitmap(
                    new PixelSize(353, 500),
                    BitmapInterpolationMode.HighQuality
                ).Save(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_COV.png"));
                break;
            case Artwork.Type.Back:
                this.ArtworkBack?.CreateScaledBitmap(
                    new PixelSize(353, 500),
                    BitmapInterpolationMode.HighQuality
                ).Save(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_COV2.png"));
                break;
            case Artwork.Type.Disc:
                this.ArtworkDVD?.CreateScaledBitmap(
                    new PixelSize(353, 353),
                    BitmapInterpolationMode.HighQuality
                ).Save(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_ICO.png"));
                break;
        }
    }
}


public static class Artwork
{
    public enum Type
    {
        Front,
        Back,
        Disc
    }

    public static Type NextType(Type _type)
    {
        return (Type)(((int)_type + 1) % Enum.GetValues(typeof(Type)).Length);
    }

    public static  Type PrevType(Type _type)
    {
        return (Type)(((int)_type - 1 + Enum.GetValues(typeof(Type)).Length) % Enum.GetValues(typeof(Type)).Length);
    }
}