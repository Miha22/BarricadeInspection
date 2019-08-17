using SDG.Unturned;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace practise
{
    class Program
    {
        static string path = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Player\Clothing.dat";
        static string path2 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Clothing.dat";
        static string path3 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Player\Inventory.dat";
        static string pathW = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Level\Washington\Barricades.dat";
        //static string path4 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Level\Washington\Barricades.dat";
        static readonly string pathM = @"/Users/Test/Documents/Github/BarricadeManager/Barricades.dat";
        static void Main(string[] args)
        {
            //BarricadeManager.load(path4);
            River river = new River(pathW, true, false, true);
            byte version = river.readByte();
            uint serverSaveDate = river.readUInt32();
            for (byte index1 = 0; index1 < 64; ++index1)
            {
                for (byte index2 = 0; index2 < 64; ++index2)
                {
                    Functions.loadRegion(river);
                }
            }

        }
    }
    class Functions
    {
        public static byte version = BarricadeManager.SAVEDATA_VERSION;
        public static void loadRegion(River river)
        {
            ushort num1 = river.readUInt16();
            for (ushort index = 0; (int)index < (int)num1; ++index)
            {
                ushort num2 = river.readUInt16();
                Console.WriteLine();
                Console.WriteLine($"barricade ID: {num2}");
                ushort newHealth = river.readUInt16();
                byte[] numArray = river.readBytes();
                for (byte i = 0; i < numArray.Length; i++)
                {
                    Console.WriteLine($"{i}. state: {numArray[i]}");
                }
                Console.WriteLine();
                Vector3 vector3 = river.readSingleVector3();
                byte num3 = 0;
                if (version > (byte)2)
                    num3 = river.readByte();
                byte num4 = river.readByte();
                byte num5 = 0;
                if (version > (byte)3)
                    num5 = river.readByte();
                ulong num6 = 0;
                ulong num7 = 0;
                if (version > (byte)4)
                {
                    num6 = river.readUInt64();
                    num7 = river.readUInt64();
                }
                uint newObjActiveDate = river.readUInt32();
            }
        }
        public static Block ReadBlock(string path, byte prefix)
        {
            return readBlock(ServerSavedata.directory + "/" + Provider.serverID + path, false, prefix);
        }
        private static Block readBlock(string path, bool useCloud, byte prefix)
        {
            return readBlockRW(path, useCloud, true, prefix);
        }
        private static Block readBlockRW(string path, bool useCloud, bool usePath, byte prefix)
        {
            byte[] contents = readBytes(path, useCloud, usePath);
            if (contents == null)
                return (Block)null;
            return new Block((int)prefix, contents);
        }
        private static byte[] readBytes(string path, bool useCloud, bool usePath)
        {
            //if (useCloud)
            //    return ReadWrite.cloudFileRead(path);
            //if (usePath)
            //    path = ReadWrite.PATH + path;
            //if (!Directory.Exists(Path.GetDirectoryName(path)))
            //    Directory.CreateDirectory(Path.GetDirectoryName(path));
            //if (!File.Exists(path))
            //{
            //    System.Console.WriteLine((object)("Failed to find file at: " + path));
            //    return (byte[])null;
            //}
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = new byte[fileStream.Length];
            if (fileStream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                System.Console.WriteLine("Failed to read the correct file size.");
                return (byte[])null;
            }
            fileStream.Close();
            fileStream.Dispose();
            return buffer;
        }
    }
}
