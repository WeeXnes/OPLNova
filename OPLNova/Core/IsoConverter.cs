
using System;
using System.IO;

#nullable disable
namespace OPLNova.Core.IO.DiscImage.Type
{
    public class BinType : IDiscImage
    {
        private SectorStructure _sector;
        private string _imagePath;

        public string ImagePath => this._imagePath;

        public BinType(string imagePath)
        {
            this._imagePath = imagePath;
            this._sector = new SectorStructure();
        }

        public long FirstDumpSector() => 0;

        public SectorStructure GetSectorStucture()
        {
            this._sector = new SectorStructure();
            FileStream input = (FileStream) null;
            BinaryReader binaryReader = (BinaryReader) null;
            try
            {
                input = new FileStream(this._imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                binaryReader = new BinaryReader((Stream) input);
                byte[] numArray = new byte[16];
                binaryReader.Read(numArray, 0, numArray.Length);
                if (BytesHelper.CompareBytesArray(BytesHelper.CopyRange(numArray, 0, Iso9660Conv.SYNC_HEADER.Length), Iso9660Conv.SYNC_HEADER))
                {
                    switch (numArray[15])
                    {
                        case 1:
                            this._sector.sectorLength = 2352;
                            this._sector.userDataOffset = 16;
                            break;
                        case 2:
                            this._sector.sectorLength = 2352;
                            this._sector.userDataOffset = 24;
                            break;
                    }
                }
                else
                {
                    this._sector.sectorLength = 2336;
                    this._sector.userDataOffset = 8;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                binaryReader?.Close();
                if (input != null)
                {
                    input.Close();
                    input.Dispose();
                }
            }
            return this._sector;
        }
    }
}


namespace OPLNova.Core.IO.DiscImage
{
    public interface IDiscImage
    {
        string ImagePath { get; }

        long FirstDumpSector();

        SectorStructure GetSectorStucture();
    }
}


namespace OPLNova.Core.IO.DiscImage
{
    public struct SectorStructure(int length, int dataOffset)
    {
        public int sectorLength = length;
        public int userDataOffset = dataOffset;
    }
}



#nullable disable
namespace OPLNova.Core.IO.DiscImage
{
  public class Iso9660Conv
  {
    private const int ISO_SECTOR_SIZE = 2048;
    private const int BUFFER_LENGTH_FACTOR = 64;
    private const uint FAT32_DISK_LIMIT = 4294967295;
    private SectorStructure _sectorStruct;
    private long _startFirstSector;
    private IDiscImage _discImage;
    private FileStream _fsSrc = (FileStream) null;
    private FileStream _fsDst = (FileStream) null;
    private BinaryReader _brSrc = (BinaryReader) null;
    private BinaryWriter _wrDst = (BinaryWriter) null;
    internal static byte[] SYNC_HEADER = new byte[12]
    {
      (byte) 0,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      (byte) 0
    };

    public event EventHandler<Iso9660CreatorEventArgs> Progression;

    public event EventHandler Terminate;

    public IDiscImage DiscImage
    {
      get => this._discImage;
      set
      {
        this._discImage = value;
        this.SetDiscImage();
      }
    }

    public Iso9660Conv()
    {
    }

    public Iso9660Conv(IDiscImage converter)
    {
      this._discImage = converter;
      this.SetDiscImage();
    }

    public void Close()
    {
      if (this._brSrc != null)
        this._brSrc.Close();
      if (this._wrDst != null)
        this._wrDst.Close();
      if (this._fsSrc != null)
      {
        this._fsSrc.Close();
        this._fsSrc.Dispose();
      }
      if (this._fsDst == null)
        return;
      this._fsDst.Close();
      this._fsDst.Dispose();
    }

    public void Convert(IDiscImage discImage, string isoDestination)
    {
      this._discImage = discImage != null ? discImage : throw new ArgumentNullException(nameof (discImage), "The IDiscImage interface is null.");
      this.SetDiscImage();
      this.Convert(isoDestination);
    }

    public void Convert(string isoDestination)
    {
      if (this._discImage == null)
        throw new ArgumentNullException("The IDiscImage interface is null. You can specified an instance from DiscImage propertie.");
      if (isoDestination == string.Empty)
        throw new ArgumentNullException("output", "The output file path must not be null.");
      if (this._sectorStruct.sectorLength == 0)
        throw new IOException("The source image file format is unknown.");
      if (!File.Exists(this._discImage.ImagePath))
        throw new FileNotFoundException("The souce image file does not exists.");
      DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(isoDestination));
      long num1 = new FileInfo(this._discImage.ImagePath).Length / (long) this._sectorStruct.sectorLength * 2048L - this._startFirstSector;
      if (driveInfo.DriveFormat.Equals("FAT32") && num1 > (long) uint.MaxValue)
        throw new IOException("Not enough space on FAT32 destination drive.");
      if (driveInfo.TotalSize < num1)
        throw new IOException("Not enough space on the destination drive.");
      try
      {
        this._fsSrc = new FileStream(this._discImage.ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        this._brSrc = new BinaryReader((Stream) this._fsSrc);
        this._fsDst = new FileStream(isoDestination, FileMode.Create, FileAccess.Write, FileShare.None);
        this._wrDst = new BinaryWriter((Stream) this._fsDst);
        this._brSrc.BaseStream.Seek(this._startFirstSector, SeekOrigin.Begin);
        byte[] numArray = new byte[this._sectorStruct.sectorLength * 64];
        int num2;
        do
        {
          num2 = this._brSrc.Read(numArray, 0, numArray.Length);
          if (num2 == this._sectorStruct.sectorLength * 64)
            this._wrDst.Write(this.CleanUserDataBuffer(numArray, 131072U));
          else if (num2 > 0)
          {
            uint uint32 = System.Convert.ToUInt32(num2 / this._sectorStruct.sectorLength * 2048);
            this._wrDst.Write(this.CleanUserDataBuffer(numArray, uint32));
          }
          if (this.Progression != null)
            this.Progression((object) null, new Iso9660CreatorEventArgs(this._wrDst.BaseStream.Position, this._brSrc.BaseStream.Length));
        }
        while (num2 > 0 && this._wrDst.BaseStream.Position != num1);
        if (this.Terminate == null)
          return;
        this.Terminate((object) null, EventArgs.Empty);
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message, ex);
      }
      finally
      {
        this.Close();
      }
    }

    private byte[] CleanUserDataBuffer(byte[] source, uint dataSize)
    {
      byte[] dst = new byte[(int) dataSize];
      int num = 0;
      for (int index = 0; (long) index < (long) dataSize; index += 2048)
      {
        if ((long) dataSize - (long) index >= 2048L)
        {
          Buffer.BlockCopy((Array) source, num * this._sectorStruct.sectorLength + this._sectorStruct.userDataOffset, (Array) dst, num * 2048, 2048);
          ++num;
        }
        else
          Buffer.BlockCopy((Array) source, this._sectorStruct.sectorLength * num + this._sectorStruct.userDataOffset, (Array) dst, num * 2048, (int) ((long) dataSize - (long) index));
      }
      return dst;
    }

    private void SetDiscImage()
    {
      this._sectorStruct = this._discImage.GetSectorStucture();
      this._startFirstSector = this._discImage.FirstDumpSector();
    }
  }
}



#nullable disable
namespace OPLNova.Core
{
  internal static class BytesHelper
  {
    public static bool CompareBytesArray(byte[] a, byte[] b)
    {
      if (a == null || b == null || a.Length != b.Length)
        return false;
      for (int index = 0; index < a.Length; ++index)
      {
        if ((int) a[index] != (int) b[index])
          return false;
      }
      return true;
    }

    public static byte[] CopyRange(byte[] source, int srcOffset, int length)
    {
      byte[] dst = new byte[length];
      if (source.Length < length)
        return (byte[]) null;
      Buffer.BlockCopy((Array) source, srcOffset, (Array) dst, 0, dst.Length);
      return dst;
    }
  }
}




#nullable disable
namespace OPLNova.Core.IO.DiscImage
{
  public class Iso9660CreatorEventArgs : EventArgs
  {
    private long _bytesWritted;
    private long _discLength;

    public long DiscLength => this._discLength;

    public long BytesWritted => this._bytesWritted;

    public Iso9660CreatorEventArgs()
    {
    }

    public Iso9660CreatorEventArgs(long writted, long discLength)
    {
      this._bytesWritted = writted;
      this._discLength = discLength;
    }
  }
}


