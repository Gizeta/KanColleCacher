using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
//using System.Text.RegularExpressions;

using System.Xml;
using System.Xml.Linq;
using System.IO;

using d_f_32.KanColleCacher;
using d_f_32.KanColleCacher.Configuration;

namespace DebugPlatform
{
	class Program
	{

		static void Main(string[] args)
		{
			Console.WriteLine("-----------------------------------------");
			GraphList.DebugFunc();
			Console.ReadLine();
		}
		static void Main_old(string[] args)
		{
			Console.WriteLine("-----------------------------------------");
			Console.WriteLine("KanColleCacher_1.3_Updater	Copyright d.f.32 - 2015 \n");
			Console.WriteLine("本程序是KCV缓存插件 KanColleCacher 的更新补丁程序。");
			Console.WriteLine("插件从1.3更新到2.0时废弃了原有的设置文件与部分数据文件的储存方式，这将造成设置的丢失以及部分缓存文件需要重新下载。");
			Console.WriteLine("本程序则是用来解决这一问题的。");
			Console.WriteLine();
			Console.WriteLine(@"在程序运行前，请核实以下描述：
* 你在使用名为【舰队很忙！KanColleViewer】的游戏程序
* 你安装使用了插件 KanColleCacher 一段时间，插件为你缓存了大量的游戏文件
* 你需要将插件 KanColleCacher 从1.3更新到2.0，且不希望再次这些下载缓存文件
* 你知道本程序只是补丁，并不包含插件主体的dll文件，且也不会为你拷贝移动插件主体
* 你已经在运行前将本程序放置在了【舰队很忙！KanColleViewer】的程序目录下，或其目录的Plugins子目录下

若以上描述核实无误，请按回车键以开始执行补丁...");
			Console.ReadLine();

			List<string> setFiles = new List<string>();
			List<string> cacheFolders = new List<string>();
			string dir = Directory.GetCurrentDirectory();

			Console.WriteLine("-----------------------------------------");
			Console.WriteLine("检索设置文件...");

			var path = dir + @"\Plugins\KanColleCacher.xml";
			if (File.Exists(path)) setFiles.Add(path);

			path = dir + @"\KanColleCacher.xml";
			if (File.Exists(path)) setFiles.Add(path);

			path = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						"grabacr.net",
						"KanColleViewer",
						"KanColleCacher.xml"
						);
			if (File.Exists(path)) setFiles.Add(path);

			if (setFiles.Count == 0)
			{
				Console.WriteLine("没有找到任何设置文件！更新无法继续进行。");
				Console.WriteLine("请按下回车键以退出程序...");
				Console.ReadLine();
				return;
			}

			Console.WriteLine("找到设置文件共 {0} 个",setFiles.Count);

			foreach (var fpath in setFiles)
			{
				Console.WriteLine("处理设置文件...\n\t" + fpath);
				try
				{
					string cachefolder;
					ProcessSettingFile(fpath, out cachefolder);
					if (String.IsNullOrEmpty(cachefolder) || !Directory.Exists(cachefolder))
					{
						Console.WriteLine("无效的缓存文件夹！\n\t" + cachefolder);
					}
					cacheFolders.Add(cachefolder);
				}
				catch (Exception ex)
				{
					Console.WriteLine("处理设置文件时发生异常。可能是文件损坏。");
					Console.WriteLine(ex.Message);
				}
			}

			Console.WriteLine("设置文件处理结束。找到缓存文件夹 {0} 个", cacheFolders.Count);
			Console.WriteLine();
			Console.WriteLine("-----------------------------------------");

			if (cacheFolders.Count == 0)
			{
				Console.WriteLine("没有找到任何缓存文件夹的设置！更新无法继续进行。");
				Console.WriteLine("请手动输入缓存文件夹地址或按回车键结束程序：");
				string input;
				while (!String.IsNullOrWhiteSpace(input = Console.ReadLine()))
				{
					if (Directory.Exists(input))
					{
						cacheFolders.Add(input);
						break;
					}
					Console.WriteLine("无效地址");
				}
				if (cacheFolders.Count==0) return;
			}

			Console.WriteLine();

			foreach (var folder in cacheFolders)
			{
				Console.WriteLine("处理缓存文件夹...\n\t" + folder);
				ProcessCacheFolder(folder);
			}

			Console.WriteLine("缓存文件处理结束\n");
			Console.WriteLine("-----------------------------------------");
			Console.WriteLine("是否移除废弃的文件？		[Y] 删除 [N] 不删除");

			string ipt = Console.ReadLine().ToLower();
			while (!ipt.StartsWith("y") && !ipt.StartsWith("n"))
			{
				ipt = Console.ReadLine().ToLower();
			}

			if (ipt.StartsWith("y"))
			{
				foreach (var f in setFiles)
				{
					try { 
						File.Delete(f);
						Console.WriteLine("删除 " + f);
					}
					catch { }
				}
				foreach (var p in cacheFolders)
				{
					try {
						File.Delete(p + "\\Last-Modified.xml");
						Console.WriteLine("删除 " + p + "\\Last-Modified.xml");
					}
					catch { }
				}
			}

			Console.WriteLine("按任意键退出程序");
			Console.ReadKey();
		}

		static void ProcessSettingFile(string filepath, out string cachefolder)
		{
			var doc = XDocument.Load(filepath);
			var parser = new ConfigParser();
			var section = parser["Settings"] = new Section();
			parser.SerializeObject<Settings>(new Settings(), "Settings");

			foreach (var elm in doc.Root.Elements())
			{
				var name = elm.Name.ToString();
				var val = elm.Value;

				if (!String.IsNullOrEmpty(val))
				{
					section[name] = val;
				}
			}

			section["SaveApiStart2"] = section["PrintGraphList"];
			section["PrintGraphList"] = null;

			parser.SaveIniFile(filepath.Substring(0, filepath.Length - 3) + "ini");
			cachefolder = section["CacheFolder"];
		}

		static void ProcessCacheFolder(string cachefolder)
		{
			var xmlf = cachefolder + "//Last-Modified.xml";

			if (!File.Exists(xmlf))
			{
				Console.WriteLine("没有找到Last-Modified.xml。");
				return;
			}
			XDocument doc;
			try
			{
				doc = XDocument.Load(xmlf);
			}
			catch (Exception)
			{
				Console.WriteLine("Last-Modified.xml文件损坏。");
				return;
			} 

			int count = 0;
			foreach (var elm in doc.Root.Elements())
			{
				count++;
				try
				{
					var path = elm.Element("Path").Value;
					var time = elm.Element("Time").Value;

					path = (cachefolder + "\\" + path).Replace('/', '\\');
					if (!File.Exists(path))
					{
						Console.WriteLine("无效地址：" + path);
						continue;
					}

					var fi = new FileInfo(path);
					fi.LastWriteTime =GMTHelper.GMT2Local(time);
				}
				catch(Exception ex)
				{
					Console.WriteLine("第 {0} 个元素损坏。" + ex.Message, count);
				}
			}
			Console.WriteLine("Last-Modified.xml 处理结束，共处理 {0} 个文件", count);
		}
	}


	




	class ModifiedRecord
	{
		static string filepath;
		static XDocument fileXML;
		static IEnumerable<XElement> recordList;

		const string _RootName = "Last-Modified";
		const string _ItemElm = "Record";
		const string _ElmPath = "Path";
		const string _ElmTime = "Time";
		const string _ElmVersion = "Version";

		#region 加载与保存

		/// <summary>
		/// 从Last-Modified.xml加载记录
		/// </summary>
		static public void Load()
		{
			//filepath = Settings.Current.CacheFolder + "\\Last-Modified.xml";
			filepath = @"B:\GitHub\KanColleCacher\DebugPlatform\Last-Modified - 副本.xml";
			try
			{
				if (File.Exists(filepath))
					fileXML = XDocument.Load(filepath);
			}
			catch (Exception ex)
			{
				//Log.Exception(ex.InnerException, ex, "加载Last-Modified.xml时发生异常");
				throw ex;
			}

			if (fileXML == null)
			{
				fileXML = new XDocument();
				fileXML.Add(new XElement(_RootName));
			}

			recordList = fileXML.Descendants(_ItemElm);
			//recordList 和 fileXML是同步的
		}

		/// <summary>
		/// 保存到Last-Modified.xml
		/// </summary>
		static public void Save()
		{
			filepath = @"B:\GitHub\KanColleCacher\DebugPlatform\Last-Modified.xml";



			try
			{
				var elms = fileXML.Descendants(_ItemElm)
							.OrderBy(elm =>
								{ return elm.Element(_ElmPath).Value; }
							).ToArray();

				fileXML.Root.Elements().Remove();
				fileXML.Root.Add(elms);
			}
			catch
			{
				//Log.Exception(ex.InnerException, ex, "保存Last-Modified.xml时发生异常");
			}
			
			fileXML.Save(filepath);
		}

	}
		#endregion



}
