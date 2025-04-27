using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.VisualBasic.CompilerServices;
using Nocksoft.IO.ConfigFiles;

namespace PS2_Manager.Core;

public class Game
{
    public string Name { get; set; }
    public string GameID { get; set; }
    public string GamePath { get; set; }
    public Bitmap? ArtworkFront { get; set; }
    public Bitmap? ArtworkBack { get; set; }
    public Bitmap? ArtworkDVD { get; set; }
    public Config? Config { get; set; }
    public DiscIcon Icon { get; set; }
    public EventHandler? InstallationFinished { get; set; }
    public UpdateVar<double> InstallProgress { get; private set; }

    public Game(string isoPath, bool installed = false)
    {
        this.GamePath = isoPath;
        this.GameID = ISO.GetSerial(isoPath);
        this.Icon = new DiscIcon(DiscType.DVD);
        this.Config = new Config(this.GetConfigPath());
        if (!installed)
        {
            this.Name = this.GetGameTitle();
            this.ArtworkFront = this.DownloadCover(Artwork.Type.Front);
            this.ArtworkBack = this.DownloadCover(Artwork.Type.Back);
            this.ArtworkDVD = this.DownloadCover(Artwork.Type.Disc);
        }
        else
        {
            this.Name = ParseFormattedFilename(Path.GetFileName(isoPath));
            this.ArtworkFront = this.LoadCover(Artwork.Type.Front);
            this.ArtworkBack = this.LoadCover(Artwork.Type.Back);
            this.ArtworkDVD = this.LoadCover(Artwork.Type.Disc);
        }
    }



    public void ChangeName(string newName)
    {
        Console.Info("Changing name of " + this.Name + " to " + newName);
        try
        {
            this.Name = newName;
            string targetDirectory = settings.library_path.GetValue<string>();
            string newFileName = $"{this.GameID}.{this.Name}.iso";
            string destPath = Path.Combine(Path.Combine(targetDirectory, "DVD"), newFileName);
            File.Move(this.GamePath, destPath);
            Console.Success(this.Name + " renamed successfully");
        }
        catch (Exception e)
        {
            Console.Error(e.Message);
        }
    }

    public string GetGameTitle()
    {
        string url = $"https://weexnes.dev:7722/search/{this.GameID}";

        try
        {
            using HttpClient client = new();
            string json = client.GetStringAsync(url).GetAwaiter().GetResult();
            Console.WriteLine(json);

            GameInfoApi? game = JsonSerializer.Deserialize<GameInfoApi>(json);
            string title = game?.title ?? "Title not found";

            if (title.Length > 32)
            {
                title = title.Substring(0, 32);
            }

            return title;
        }
        catch (Exception ex)
        {
            Console.Error($"{ex.Message}");
            return "";
        }
    }

    

    private string ParseFormattedFilename(string filename)
    {
        string[] parts = filename.Split('.');
        string parsedName = parts[^2];
        Console.Info(parsedName + " parsed from " + filename);
        return parsedName;
    }

    public async void Install()
    {
        this.InstallProgress = new UpdateVar<double>();
        await this.CopyIsoWithProgressAsync();
    }

    private Bitmap DownloadCover(Artwork.Type type)
    {
        Console.Info("Downloading " + type + " Artwork for " + this.GameID);
        Bitmap cover = null;
        string url = "";
        switch (type)
        {
            case Artwork.Type.Front:
                url = $"https://weexnes.dev:7722/art/{this.GameID}/{this.GameID}_COV.png";
                break;
            case Artwork.Type.Back:
                url = $"https://weexnes.dev:7722/art/{this.GameID}/{this.GameID}_COV2.png";
                break;
            case Artwork.Type.Disc:
                url = $"https://weexnes.dev:7722/art/{this.GameID}/{this.GameID}_ICO.png";
                break;
        }

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
            Console.Error($"Could not load cover for {this.GameID}: {ex.Message}");
        }
        
        Console.Success(type + " Artwork for " + this.GameID + " downloaded successfully");

        return cover;
    }

    public void Uninstall()
    {
        File.Delete(this.GamePath);
    }
    
    public async Task CopyIsoWithProgressAsync()
    {
        Console.Info("Copying ISO file for " + this + "...");
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
        Console.Success("ISO File copied successfully for " + this);
        
        this.SaveCover(Artwork.Type.Front);
        this.SaveCover(Artwork.Type.Back);
        this.SaveCover(Artwork.Type.Disc);
        this.InstallationFinished?.Invoke(this, EventArgs.Empty);
        MainWindow.RefreshGamesListTrigger?.Invoke(this, EventArgs.Empty);
    }





    public Bitmap? LoadCover(Artwork.Type artworkType)
    {
        Console.Info("Loading " + artworkType + " Artwork for " + this.GameID);
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
        Console.Success("Saved " + artworkType + " Artwork for " + this.GameID);
    }

    public override string ToString()
    {
        return this.Name + " -> " + this.GameID;
    }


    private string GetConfigPath()
    {
        return Path.Combine(Path.Combine(settings.library_path.GetValue<string>(), "CFG"), this.GameID + ".cfg");
    }
    private string GetArtworkPath(Artwork.Type artworkType)
    {
        string targetDirectory = settings.library_path.GetValue<string>();
        string modifier = "";
        switch (artworkType)
        {
            case Artwork.Type.Front:
                modifier = "_COV.png";
                break;
            case Artwork.Type.Back:
                modifier = "_COV2.png";
                break;
            case Artwork.Type.Disc:
                modifier = "_ICO.png";
                break;
        }
        return Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + modifier);
    }
}




