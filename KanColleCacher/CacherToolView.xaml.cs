using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace d_f_32.KanColleCacher
{
    /// <summary>
    /// ModifierView.xaml 的交互逻辑
    /// </summary>
    public partial class CacherToolView : UserControl
    {
        public CacherToolView()
        {
            try 
            { 
				InitializeComponent(); 
			}
            catch (Exception  ex)
            {
				Log.Exception(ex.Source, ex, "ToolView初始化时发生异常");
            }
			
        }

		private void SelectCacheFolder_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog()
			{
				SelectedPath = Settings.Current.CacheFolder,
				ShowNewFolderButton = true,
				Description = "选择一个文件夹用于保存缓存文件。新的地址将在程序下次启动时生效。"
			};
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK 
				&& Directory.Exists(dlg.SelectedPath))
			{
				Settings.Current.CacheFolder = dlg.SelectedPath;
			}
		}

		private void GenerateFileList_Click(object sender, RoutedEventArgs e)
		{
			GraphList.GenerateList();
		}

        private void CleanModifiedCache_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<string>();

            var cacheFolder = new DirectoryInfo(Settings.Current.CacheFolder);
            if (cacheFolder.Exists)
            {
                Action<DirectoryInfo> checkDirectory = null;
                checkDirectory = (DirectoryInfo folder) =>
                {
                    foreach (var file in folder.GetFiles())
                    {
                        if (file.Name.Contains(".hack."))
                        {
                            list.Add(file.FullName.Replace(cacheFolder.FullName, "").Replace(".hack", "").Replace('\\', '/'));
                        }
                    }
                    foreach (var f in folder.GetDirectories())
                    {
                        checkDirectory(f);
                    }
                };

                checkDirectory(cacheFolder);
            }

            cleanIECache(list);
        }

        private void CleanCache_Click(object sender, RoutedEventArgs e)
        {
            cleanIECache(new List<string>{
                "/kcs/",
                "/gadget/js/",
                "/gadget/script/",
                "/kcscontents/"
            });
        }

        private void cleanIECache(List<string> filter)
        {
            int nNeeded = 0, nBufSize;
            IntPtr buf;
            INTERNET_CACHE_ENTRY_INFO CacheItem;
            IntPtr hEnum;
            bool r;

            FindFirstUrlCacheEntry(null, IntPtr.Zero, ref nNeeded);

            if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                return;

            nBufSize = nNeeded;
            buf = Marshal.AllocHGlobal(nBufSize);
            hEnum = FindFirstUrlCacheEntry(null, buf, ref nNeeded);
            while (true)
            {
                CacheItem = (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(buf, typeof(INTERNET_CACHE_ENTRY_INFO));
                string url = Marshal.PtrToStringAnsi(CacheItem.lpszSourceUrlName);
                if (filter.Any(x => url.Contains(x)))
                {
                    DeleteUrlCacheEntry(url);
                }

                nNeeded = nBufSize;
                r = FindNextUrlCacheEntry(hEnum, buf, ref nNeeded);

                if (!r && Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                    break;

                if (!r && nNeeded > nBufSize)
                {
                    nBufSize = nNeeded;
                    buf = Marshal.ReAllocHGlobal(buf, (IntPtr)nBufSize);
                    FindNextUrlCacheEntry(hEnum, buf, ref nNeeded);
                }
            }

            Marshal.FreeHGlobal(buf);
        }

        #region Win32

        public struct INTERNET_CACHE_ENTRY_INFO
        {
            public int dwStructSize;
            public IntPtr lpszSourceUrlName;
            public IntPtr lpszLocalFileName;
            public int CacheEntryType;
            public int dwUseCount;
            public int dwHitRate;
            public int dwSizeLow;
            public int dwSizeHigh;
            public FILETIME LastModifiedTime;
            public FILETIME ExpireTime;
            public FILETIME LastAccessTime;
            public FILETIME LastSyncTime;
            public IntPtr lpHeaderInfo;
            public int dwHeaderInfoSize;
            public IntPtr lpszFileExtension;
            public int dwExemptDelta;
        }

        const int ERROR_NO_MORE_ITEMS = 259;

        [DllImport(@"wininet.dll", SetLastError = true)]
        public static extern IntPtr FindFirstUrlCacheEntry(string lpszUrlSearchPattern, IntPtr lpFirstCacheEntryInfo, ref int lpdwFirstCacheEntryInfoBufferSize);
        [DllImport(@"wininet.dll", SetLastError = true)]
        public static extern bool FindNextUrlCacheEntry(IntPtr hEnumHandle, IntPtr lpNextCacheEntryInfo, ref int lpdwNextCacheEntryInfoBufferSize);
        [DllImport(@"wininet.dll", SetLastError = true)]
        public static extern bool FindCloseUrlCache(IntPtr hEnumHandle);
        [DllImport(@"wininet.dll", SetLastError = true)]
        public static extern long DeleteUrlCacheEntry(string lpszUrlName);

        #endregion

    }
}
