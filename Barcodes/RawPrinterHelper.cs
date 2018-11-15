using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PMDataPuller
{
    
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DOCINFOW
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern bool OpenPrinter(string src, ref IntPtr hPrinter, int pd);



        [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);



        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref RawPrinterHelper.DOCINFOW pDI);



        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);


        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);


        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);


        [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, ref int dwWritten);



        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            IntPtr hPrinter = System.IntPtr.Zero;
            Int32 dwError;
            DOCINFOW di = new DOCINFOW();
            Int32 dwWritten = 0;
            bool bSuccess;
            di.pDocName = "My Document";
            di.pDataType = "RAW";
            bSuccess = false;
            if (OpenPrinter(szPrinterName, ref hPrinter, (int)0))
            {
                if (StartDocPrinter(hPrinter, 1, ref di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, ref dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;

        }

        /*
        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            FileStream stream1 = new FileStream(szFileName, FileMode.Open);
            BinaryReader reader1 = new BinaryReader(stream1);
            byte[] buffer1 = new byte[((int)stream1.Length) + 1];
            buffer1 = reader1.ReadBytes((int)stream1.Length);
            IntPtr ptr1 = Marshal.AllocCoTaskMem((int)stream1.Length);
            Marshal.Copy(buffer1, 0, ptr1, (int)stream1.Length);
            bool flag1 = RawPrinterHelper.SendBytesToPrinter(szPrinterName, ptr1, (int)stream1.Length);
            Marshal.FreeCoTaskMem(ptr1);
            return flag1;

        }
        */

        public static void SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            dwCount = szString.Length;
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            //MessageBox.Show(pBytes.ToString());
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
        }
    }

}

