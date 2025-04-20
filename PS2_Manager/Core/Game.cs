using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace PS2_Manager.Core;

public class Game
{
    public string Name { get; set; }
    public string GameID { get; set; }
    public string GamePath { get; set; }
    public Bitmap Cover { get; set; }
    public EventHandler? InstallationFinished { get; set; }
    public UpdateVar<double> InstallProgress { get; private set; }

    public Game(string isoPath)
    {
        this.GamePath = isoPath;
        this.GameID = ISO.GetSerial(isoPath);
        this.Cover = this.GetCover();
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
        if(!Directory.Exists(Path.Combine(targetDirectory, "DVD")))
            Directory.CreateDirectory(Path.Combine(targetDirectory, "DVD"));
        
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
        
        this.InstallCover();
        this.InstallationFinished?.Invoke(this, EventArgs.Empty);

        Console.WriteLine($"Copied ISO to: {destPath}");
    }
    




    public void InstallCover()
    {
        string targetDirectory = settings.library_path.GetValue<string>();
        if(!Directory.Exists(Path.Combine(targetDirectory, "ART")))
            Directory.CreateDirectory(Path.Combine(targetDirectory, "ART"));
        //this.Cover.Save(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_COV.png"));
        this.Cover.CreateScaledBitmap(
            new PixelSize(353, 500),
            BitmapInterpolationMode.HighQuality
        ).Save(Path.Combine(Path.Combine(targetDirectory, "ART"), this.GameID + "_COV.png"));
    }
}