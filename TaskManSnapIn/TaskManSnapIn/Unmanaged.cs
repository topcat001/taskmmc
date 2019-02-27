using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.ManagementConsole.TaskMan
{
    /// <summary>
    /// Member of <see cref="BrowseInfo"/>.
    /// </summary>
    internal delegate int BrowseCallBackProc(IntPtr hwnd, int msg, IntPtr lp, IntPtr wp);

    /// <summary>
    /// Used as argument to SHBrowseForFolder.
    /// C# version of the C BROWSEINFO structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BrowseInfo
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string displayName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string title;
        public int flags;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public BrowseCallBackProc callback;
        public IntPtr lparam;
    }

    /// <summary>
    /// Enumerates values for the csidl input to SHGetSpecialFolderLocation.
    /// See C:\Program Files (x86)\Windows Kits\10\Include\10.0.17763.0\um\ShlObj_core.h
    /// line 802 for definitions.
    /// </summary>
    internal enum FolderBrowserFolder
    {
        Desktop = 0x0,
        Favorites = 0x6,
        MyComputer = 0x11,
        MyDocuments = 0x5,
        MyPictures = 0x27,
        NetAndDialUpConnections = 0x31,
        NetworkNeighborhood = 0x12,
        Printers = 0x4,
        Recent = 0x8,
        SendTo = 0x9,
        StartMenu = 0xb,
        Templates = 0x15
    }

    /// <summary>
    /// Values for <see cref="BrowseInfo.flags"/>.
    /// See C:\Program Files (x86)\Windows Kits\10\Include\10.0.17763.0\um\ShlObj_core.h
    /// line 1168 for definitions.
    /// </summary>
    [Flags]
    internal enum BrowseInfoFlags
    {
        AllowUrls = 0x80,
        BrowseForComputer = 0x1000,
        BrowseForEverything = 0x4000,
        BrowseForPrinter = 0x2000,
        DontGoBelowDomain = 0x2,
        ShowTextBox = 0x10,
        NewDialogStyle = 0x40,
        RestrictToSubfolders = 0x8,
        RestrictToFilesystem = 0x1,
        ShowShares = 0x8000,
        StatusText = 0x4,
        UseNewUI = 0x50,
        Validate = 0x20
    }

    /// <summary>
    /// COM Interop for  IMalloc.
    /// See \Program Files (x86)\Windows Kits\10\Include\10.0.17763.0\um\ObjIdlbase.h
    /// for the C version of this interface and GUID.
    /// </summary>
    [ComImport]
    [Guid("00000002-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMalloc
    {
        [PreserveSig]
        IntPtr Alloc(IntPtr cb);

        [PreserveSig]
        IntPtr Realloc(IntPtr pv, IntPtr cb);

        [PreserveSig]
        void Free(IntPtr pv);

        [PreserveSig]
        IntPtr GetSize(IntPtr pv);

        [PreserveSig]
        int DidAlloc(IntPtr pv);

        [PreserveSig]
        void HeapMinimize();
    }

    /// <summary>
    /// Used as argument to CredUIPromptForWindowsCredentials.
    /// C# version of the C CREDUI_INFO structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CREDUI_INFO
    {
        public int cbSize;
        public IntPtr hwndParent;
        public string pszMessageText;
        public string pszCaptionText;
        public IntPtr hbmBanner;
    }

    /// <summary>
    /// Enumerates values for the dwFlags input to CredUIPromptForWindowsCredentials.
    /// </summary>
    [Flags]
    internal enum PromptForWindowsCredentialsFlags
    {
        /// <summary>
        /// The caller is requesting that the credential provider return the user name and password in plain text.
        /// This value cannot be combined with SECURE_PROMPT.
        /// </summary>
        CREDUIWIN_GENERIC = 0x1,
        /// <summary>
        /// The Save check box is displayed in the dialog box.
        /// </summary>
        CREDUIWIN_CHECKBOX = 0x2,
        /// <summary>
        /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
        /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
        /// </summary>
        CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
        /// <summary>
        /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
        /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
        /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
        /// </summary>
        CREDUIWIN_IN_CRED_ONLY = 0x20,
        /// <summary>
        /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
        /// </summary>
        CREDUIWIN_ENUMERATE_ADMINS = 0x100,
        /// <summary>
        /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
        /// </summary>
        CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
        /// <summary>
        /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
        /// Windows Vista: This value is not supported until Windows Vista with SP1.
        /// </summary>
        CREDUIWIN_SECURE_PROMPT = 0x1000,
        /// <summary>
        /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
        /// </summary>
        CREDUIWIN_PACK_32_WOW = 0x10000000,
    }

    /// <summary>
    /// A class that defines all the unmanaged methods used in the assembly.
    /// </summary>
    internal class UnmanagedMethods
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        internal extern static System.IntPtr SHBrowseForFolder(ref BrowseInfo bi);

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] System.Text.StringBuilder pszPath);

        [DllImport("User32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool SendMessage(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("Shell32.dll")]
        internal extern static int SHGetMalloc([MarshalAs(UnmanagedType.IUnknown)]out object shmalloc);

        [DllImport("user32.dll")]
        internal extern static IntPtr GetActiveWindow();

        [DllImport("shell32.dll")]
        internal static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);

        /// <summary>
        /// Helper routine to free memory allocated using shell's malloc object.
        /// </summary> 
        internal static void SHMemFree(IntPtr ptr)
        {
            object shmalloc = null;

            if (SHGetMalloc(out shmalloc) == 0)
            {
                var malloc = (IMalloc)shmalloc;
                malloc.Free(ptr);
            }
        }

        /// <summary>
        /// Reference: http://pinvoke.net/default.aspx/credui/CredUIPromptForWindowsCredentials.html
        /// </summary>
        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CredUIPromptForWindowsCredentials(
            ref CREDUI_INFO notUsedHere,
            int authError,
            ref uint authPackage,
            IntPtr InAuthBuffer,
            uint InAuthBufferSize,
            out IntPtr refOutAuthBuffer,
            out uint refOutAuthBufferSize,
            ref bool fSave,
            PromptForWindowsCredentialsFlags flags);

        /// <summary>
        /// Reference: http://www.pinvoke.net/default.aspx/credui.credunpackauthenticationbuffer
        /// </summary>
        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        internal static extern bool CredUnPackAuthenticationBuffer(
            int dwFlags,
            IntPtr pAuthBuffer,
            uint cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainame,
            StringBuilder pszPassword,
            ref int pcchMaxPassword);

        /// <summary>
        /// Reference: http://www.pinvoke.net/default.aspx/ole32/CoTaskMemFree.html
        /// </summary>
        [DllImport("ole32.dll")]
        internal static extern void CoTaskMemFree(IntPtr pv);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/shellapi/nf-shellapi-extracticonexa
        /// Note typically large = 32x32 small = 16x16.
        /// </summary>
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        /// <summary>
        /// Extracts icons from a resorce file (for example, a dll).
        /// Adapted from https://stackoverflow.com/questions/6872957/how-can-i-use-the-images-within-shell32-dll-in-my-c-sharp-project
        /// </summary>
        /// <param name="file"></param>
        /// <param name="number">This is the zero based index of the icon.</param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        internal static Icon ExtractIcon(string file, int number, bool largeIcon)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }
        }
    }
} // namespace