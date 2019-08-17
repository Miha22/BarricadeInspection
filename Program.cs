using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace practise
{
    class Example
    {
        public Example(int mes)
        {
            num = mes;
        }

        int num;
    }
    class Program
    {
        static string path = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Player\Clothing.dat";
        static string path2 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Clothing.dat";
        static string path3 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Player\Inventory.dat";
        static string path5 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Players\76561198112559333_0\Washington\Player\Player.dat";
        static string path4 = $@"E:\Program Files (x86)\steam\steamapps\common\Unturned - Copy\Servers\test\Level\Washington\Barricades.dat";
        static void Main(string[] args)
        {
            //Object[] objs = new object[3] { 1, 2, 3 };
            //object[] newobj = objs;
            //objs = new object[1] { 333 };
            //foreach (var item in newobj)
            //{
            //    Console.WriteLine(item);
            //}
            for (byte i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
            for (byte i = 0; i < 10; ++i)
            {
                Console.WriteLine(i);
            }
            //Process[] processlist = Process.GetProcesses();

            //foreach (Process theprocess in processlist)
            //{
            //    Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
            //}
            //string buttonName = "text3";
            //Regex regex1 = new System.Text.RegularExpressions.Regex(@"text[0-9]", RegexOptions.Compiled);
            //Regex regex2 = new System.Text.RegularExpressions.Regex(@"text[0-9]{2}$", RegexOptions.Compiled);
            //Regex[] regices = new Regex[2];
            //regices[0] = regex1;
            //regices[1] = regex2;

            //for (byte i = 0; i < regices.Length; i++)
            //{
            //    if(regices[i].IsMatch(buttonName))
            //        Console.WriteLine($"regex{i+1} match!");
            //    else
            //        Console.WriteLine($"regex{i+1} no match");
            //}
            //BarricadeManager.load(path4);
            //Example e = new Example(1);
            //string path = $@"E:\Program Files (x86)\steam\steamapps\workshop\content\304930\1805478406\Effect+.unity3d";
            //Bundle ab = new Bundle(path, false, false);
            //Console.WriteLine();
            //SleekItem sleekItem = new SleekItem(new ItemJar(new Item()))
            //Console.WriteLine($"ab null?: {ab == null}");
            //Asset asset = Assets.find(EAssetType.ITEM, 15);

            //Block block;// = Functions.ReadBlock(path, 0);
            //using (FileStream fileStream = new FileStream(path4, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    byte[] buffer = new byte[fileStream.Length];
            //    if (fileStream.Read(buffer, 0, buffer.Length) != buffer.Length)
            //    {
            //        System.Console.WriteLine("Failed to read the correct file size.");
            //        block = new Block(0, null);
            //    }
            //    block = new Block(0, buffer);
            //    fileStream.Close();
            //    //fileStream.Dispose();
            //}
            //if(block != null)
            //{
            //    for (int i = 0; i < block.block.Length; i++)
            //    {
            //        //Console.WriteLine($"read byte: {block.readByte()}");
            //        foreach (var item in block.readByteArray())
            //        {
            //            Console.WriteLine($"read byte: {item}");
            //        }
            //    }
            //}
            //Block block2;// = Functions.ReadBlock(path, 0);
            //using (FileStream fileStream = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    byte[] buffer = new byte[fileStream.Length];
            //    if (fileStream.Read(buffer, 0, buffer.Length) != buffer.Length)
            //    {
            //        System.Console.WriteLine("Failed to read the correct file size.");
            //        block2 = new Block(0, (byte[])null);
            //    }
            //    block2 = new Block(0, buffer);
            //    fileStream.Close();
            //    fileStream.Dispose();
            //}
            //byte pages = block.readByte();
            //Console.WriteLine($"Pages: {pages}");
            //for (int i = 0; i < pages+2; i++)
            //{
            //    //Console.WriteLine($"{block.readByte()} : {block2.readByte()}");
            //    Console.WriteLine();
            //    Console.WriteLine($"Width: {block.readByte()}");
            //    Console.WriteLine($"Hegiht: {block.readByte()}");
            //    byte items = block.readByte();

            //    for (int j = 0; j < items; j++)
            //    {
            //        Console.WriteLine("-----------------------------");
            //        Console.WriteLine($"item #{j}");
            //        Console.WriteLine($"x: {block.readByte()}");
            //        Console.WriteLine($"y: {block.readByte()}");
            //        Console.WriteLine($"rot: {block.readByte()}");
            //        Console.WriteLine($"id: {block.readUInt16()}");
            //        Console.WriteLine($"amount: {block.readByte()}");
            //        Console.WriteLine($"quality: {block.readByte()}");
            //        foreach (var state in block.readByteArray())
            //        {
            //            Console.WriteLine($"state {state}");
            //        }
            //        Console.WriteLine("-----------------------------");
            //    }
            //    Console.WriteLine();
            //}

            //return buffer;
            //Console.WriteLine(block.block.Length);
        }
    }
    class Functions
    {

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
