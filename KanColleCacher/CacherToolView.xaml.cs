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
    }
}
