using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class ProcLauncher
{
    public static void CreateLinProcess(string lineagefolder, long ip, ushort port, string s3ep1, string logindll)
    {
        //"C:\\Lineage Tikal\\S3EP1.exe"
        //"2130706433 16600"
        //3325717308
        var binpath = System.IO.Path.Combine(lineagefolder, s3ep1);

        STARTUPINFO si = new STARTUPINFO();
        PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
        bool success = CreateProcess(binpath, string.Format("\"{0}\" {1} {2}", binpath, ip, port),
            IntPtr.Zero, IntPtr.Zero, false,
            ProcessCreationFlags.CREATE_SUSPENDED | ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE,
            IntPtr.Zero, null, ref si, out pi);

        var logindllpath = System.IO.Path.Combine(lineagefolder, logindll);
        DllInjector.GetInstance.bInject(pi.dwProcessId, logindllpath);
        System.Threading.Thread.Sleep(1000);
        //System.Windows.Forms.MessageBox.Show("Attach debugger");

        IntPtr t_handle = pi.hThread;
        ResumeThread(t_handle);
    }

    /* PINVOKE  */
    public struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        ZERO_FLAG = 0x00000000,
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_NO_WINDOW = 0x08000000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_SEPARATE_WOW_VDM = 0x00001000,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_SUSPENDED = 0x00000004,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        DEBUG_PROCESS = 0x00000001,
        DETACHED_PROCESS = 0x00000008,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        INHERIT_PARENT_AFFINITY = 0x00010000
    }

    [DllImport("kernel32.dll")]
    public static extern bool CreateProcess(string lpApplicationName,
           string lpCommandLine, IntPtr lpProcessAttributes,
           IntPtr lpThreadAttributes,
           bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
           IntPtr lpEnvironment, string lpCurrentDirectory,
           ref STARTUPINFO lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll")]
    public static extern uint ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    public static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out IntPtr lpNumberOfBytesWritten);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
    public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);
}