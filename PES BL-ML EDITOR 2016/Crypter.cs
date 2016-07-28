using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace PES_BL_ML_EDITOR_2016
{
    class Crypter
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void decrypt_ex(string pathIn, string pathOut);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void encrypt_ex(string pathIn, string pathOut);
        private static string res = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Temp";
        [DllImport("kernel32.dll")]
        public static extern UInt64 LoadLibrary(string dllToLoad);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(UInt64 hModule, string procedureName);
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(UInt64 hModule);



        public static List<byte[]> decrypt(string s)
        {
            List<byte[]> res = new List<byte[]>();

            //if (Directory.Exists(Crypter.res + "\\data\\")) Directory.Delete(Crypter.res + "\\data\\");
            //if (Directory.Exists(Crypter.res + "\\data2\\")) Directory.Delete(Crypter.res + "\\data2\\");

            Directory.CreateDirectory(Crypter.res + "\\data\\");
            Directory.CreateDirectory(Crypter.res + "\\data2\\");

            UInt64 lib = Crypter.LoadLibrary(@"pes16decrypter.dll");
            IntPtr proc = Crypter.GetProcAddress(lib, "decrypt_ex");

            

            Marshal.GetDelegateForFunctionPointer(proc, typeof(Crypter.decrypt_ex));

            Crypter.decrypt_ex arg_45_0 = (Crypter.decrypt_ex)Marshal.GetDelegateForFunctionPointer(proc, typeof(Crypter.decrypt_ex));
            string text = Crypter.res + "\\data\\" + Path.GetFileNameWithoutExtension(s);
            
            arg_45_0(s, text);
            res.Add(File.ReadAllBytes(text + "\\data.dat"));
            res.Add(File.ReadAllBytes(text + "\\description.dat"));
            res.Add(File.ReadAllBytes(text + "\\encryptHeader.dat"));
            res.Add(File.ReadAllBytes(text + "\\header.dat"));
            res.Add(File.ReadAllBytes(text + "\\logo.png"));
            res.Add(File.ReadAllBytes(text + "\\version.txt"));
            return res;
        }
        public static void encrypt(string s,List<byte[]>  bt)
        {
            //Directory.CreateDirectory(Crypter.res + "\\data\\");
            //Directory.CreateDirectory(Crypter.res + "\\data2\\");
            Crypter.encrypt_ex arg_191_0 = (Crypter.encrypt_ex)Marshal.GetDelegateForFunctionPointer(Crypter.GetProcAddress(Crypter.LoadLibrary("pes16decrypter.dll"), "encrypt_ex"), typeof(Crypter.encrypt_ex));
            string text = Crypter.res + "\\data2";
            File.WriteAllBytes(text + "\\description.dat", bt[1]);
            File.WriteAllBytes(text + "\\header.dat", bt[3]);
            File.WriteAllBytes(text + "\\encryptHeader.dat", bt[2]);
            File.WriteAllBytes(text + "\\version.txt", bt[5]);
            File.WriteAllBytes(text + "\\logo.png", bt[4]);
            File.WriteAllBytes(text + "\\data.dat", bt[0]);
            arg_191_0(text, s);
        }


        
    }
}
