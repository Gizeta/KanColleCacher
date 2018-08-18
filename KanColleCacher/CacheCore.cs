using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Debug = System.Diagnostics.Debug;


namespace d_f_32.KanColleCacher
{

    enum filetype
    {
        not_file,
        unknown_file,

		game_entry,		//kcs2\js\
		
		resources,		//kcs2\resources\
						
		collection,		//kcs2\resources\ship\
                        //kcs2\resources\slot\
                        //kcs2\resources\useitem\
                        //kcs2\resources\furniture\

        sound,			//kcs2\sound

		world_name,		//kcs2\resources\world
		title_call,		//kcs2\sound\titlecall
                        //kcs2\resources\voice\
    }


	class CacheCore
	{
		#region 初始化与析构
		Settings set;
		string myCacheFolder;

		public CacheCore()
		{
			set = Settings.Current;
			//VersionChecker.Load();
			myCacheFolder = set.CacheFolder;
		}

		
		#endregion

		/// <summary>
		/// 对于一个新的客户端请求，根据url，决定下一步要对请求怎样处理
		/// </summary>
		/// <param name="url">请求的url</param>
		/// <param name="result">本地文件地址 or 记录的修改日期</param>
		/// <returns>下一步我们该做什么？忽略请求；返回缓存文件；验证缓存文件</returns>
		public Direction GotNewRequest(string url, out string result)
		{
			result = "";
			string filepath = "";

			Uri uri;
			try { uri = new Uri(url); }
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
				return Direction.Discharge_Response;
				//url无效，忽略请求（不进行任何操作）
			}

			if (!uri.IsFilePath())
			{
				return Direction.Discharge_Response;
				//url非文件，忽略请求
			}

			//识别文件类型
			filetype type = _RecognizeFileType(uri);
			if (type == filetype.unknown_file ||
				type == filetype.not_file)
			{
				return Direction.Discharge_Response;
				//无效的文件，忽略请求
			}

			//检查Title Call与World Name的特殊地址
			if (set.HackTitleEnabled)
			{
				if (type == filetype.title_call)
				{
					filepath = uri.AbsolutePath.Replace('/', '\\');
					filepath = filepath.Remove(filepath.LastIndexOf('\\')) + ".mp3";
					filepath = myCacheFolder + filepath;
					result = filepath;

					if (File.Exists(filepath))
						return Direction.Return_LocalFile;
				}
				else if (type == filetype.world_name)
				{
					filepath = myCacheFolder + @"\kcs2\resources\world.png";
					result = filepath;

					if (File.Exists(filepath))
						return Direction.Return_LocalFile;
				}
			}

			//检查一般文件地址
			if ((type == filetype.resources && set.CacheResourceFiles > 0) ||
				(type == filetype.game_entry && set.CacheEntryFiles > 0) ||
				(type == filetype.collection && set.CacheCollectionFiles > 0) ||
				(type == filetype.sound && set.CacheSoundFiles > 0) ||
				((type == filetype.title_call ||
				  type == filetype.world_name) && set.CacheResourceFiles > 0))
			{
				filepath = myCacheFolder + uri.AbsolutePath.Replace('/', '\\');

				//检查Hack文件地址
				if (set.HackEnabled)
				{
					var fnext = uri.Segments.Last().Split('.');
					string hfilepath = filepath.Replace(uri.Segments.Last(), fnext[0] + ".hack." + fnext.Last());

					if (File.Exists(hfilepath))
					{
						result = hfilepath;
						return Direction.Return_LocalFile;
						//存在hack文件，则返回本地文件
					}

				}

				//检查缓存文件
				if (File.Exists(filepath))
				{
					//存在本地缓存文件 -> 检查文件的最后修改时间
					//（验证所有文件 或 只验证非资源文件）
					if (set.CheckFiles > 1 || (set.CheckFiles > 0 && type != filetype.resources))
					{
						//只有png/mp3/json文件需要验证时间
						if (filepath.EndsWith(".png") || filepath.EndsWith(".mp3") || filepath.EndsWith(".json"))
						{
							//文件存在且需要验证时间
							//-> 请求服务器验证修改时间（记录读取和保存的位置）
							result = filepath;
							_RecordTask(url, filepath);
							return Direction.Verify_LocalFile;
						}

						////检查文件时间
						//int i = VersionChecker.GetFileLastTime(uri, out result);

						//if (i == 1)
						//{
						//	//存在这个文件的修改时间记录
						//	//-> 请求服务器验证修改时间（记录读取和保存的位置）
						//	_RecordTask(url, filepath);
						//	return Direction.Verify_LocalFile;
						//}
						//else if (i == 0)
						//{
						//	//没有关于这个文件最后修改时间的记录
						//	//-> 当做这个文件不存在
						//	//-> 下载文件（记录保存地址）
						//	_RecordTask(url, filepath);
						//	return Direction.Discharge_Response;
						//}
						//else
						//{
						//	//文件类型不需要验证时间（只有swf验证）
						//}
					}
					//文件不需验证
					//->返回本地缓存文件
					result = filepath;
					return Direction.Return_LocalFile;

				}
				else
				{
					//缓存文件不存在
					//-> 下载文件 （记录保存地址）
					_RecordTask(url, filepath);
					return Direction.Discharge_Response;
				}
			}

			//文件类型对应的缓存设置没有开启
			//-> 当做文件不存在
			return Direction.Discharge_Response;
		}

		filetype _RecognizeFileType(Uri uri)
		{
			if (!uri.IsFilePath())
				return filetype.not_file;

			var seg = uri.Segments;

			if (seg[1] != "kcs2/")
			{
				return filetype.not_file;
			}
			else
			{

				if (seg[2] == "resources/")
				{
					if (seg[3] == "world/")
					{
                        return filetype.world_name;
					}
					else if (seg[3] == "voice/")
					{
                        return filetype.title_call;
					}
                    else if (seg[3] == "ship/" || seg[3] == "slot/" || seg[3] == "useitem/" || seg[3] == "furniture/")
                    {
                        return filetype.collection;
                    }
					return filetype.resources;
				}
				else if (seg[2] == "sound/")
				{
					if (seg[3] == "titlecall/")
					{
						return filetype.title_call;
					}

					return filetype.sound;
				}
				else if (seg[2] == "js/")
				{
					return filetype.game_entry;
				}
				
				//Debug.WriteLine("CACHR> _RecogniseFileType检查到无法识别的文件");
				//Debug.WriteLine("		"+uri.AbsolutePath);
				return filetype.unknown_file;
			}

		}

		public void RecordNewModifiedTime(string url, string time)
		{
			Uri uri;
			try { uri = new Uri(url); }
			catch { return; }

			//VersionChecker.Add(uri, time);
		}

		public bool AllowedToSave(filetype type)
		{
			return (type == filetype.resources && set.CacheResourceFiles > 1) ||
					(type == filetype.game_entry && set.CacheEntryFiles > 1) ||
					(type == filetype.collection && set.CacheCollectionFiles > 1) ||
					(type == filetype.sound && set.CacheSoundFiles > 1) ||
					(type == filetype.title_call || 
					type == filetype.world_name) && set.CacheResourceFiles > 1;
		}
		
		void _RecordTask(string url, string filepath)
		{
			TaskRecord.Add(url, filepath);
		}
	}
}