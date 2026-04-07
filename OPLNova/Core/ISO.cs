using System;
using System.IO;
using System.Runtime.InteropServices;
using DiscUtils.Iso9660;

public static class ISO
{
    public static string GetSerial(string isoPath)
    {
        using (FileStream isoStream = File.OpenRead(isoPath))
        {
            CDReader cd = new CDReader(isoStream, true);

            if (cd.FileExists("SYSTEM.CNF"))
            {
                using (Stream fileStream = cd.OpenFile("SYSTEM.CNF", FileMode.Open))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line.StartsWith("BOOT2") || line.StartsWith("BOOT"))
                        {
                            int pathStart = line.IndexOf(@"\");
                            int pathEnd = line.IndexOf(";");

                            if (pathStart != -1 && pathEnd != -1 && pathEnd > pathStart)
                            {
                                string bootPath = line.Substring(pathStart + 1, pathEnd - pathStart - 1);
                                return bootPath;
                            }
                        }
                    }
                }
            }
        }

        return null;
    }


}