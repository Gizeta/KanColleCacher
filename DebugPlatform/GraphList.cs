using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Fiddler;
using System.IO;
using Debug = System.Diagnostics.Debug;
//using System.Runtime.Serialization.Json;
using System.Windows;


namespace d_f_32.KanColleCacher
{
	class GraphList
	{

		public static void DebugFunc()
		{
			GenerateList();
		}

		static List<ship_graph_item> graphList = new List<ship_graph_item>();

		/// <summary>
		/// 将解析完成的信息保存到本地
		/// </summary>
		static void PrintToFile()
		{
			//string filepath = Settings.Current.CacheFolder + "\\GraphList.txt";
			string filepath = @"E:\Game\KanColleViewer ver.3.3\MyCache\GraphList.txt";
			StringBuilder content = new StringBuilder();

			content.AppendFormat(
				"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
				"SortNo", "ShipId", "ShipName",
				"FileName", "FileVersion",
				"TypeName", "TypeId"
				);
			content.AppendFormat(
				"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
				"序号", "ID", "名称",
				"文件名", "文件版本", "文件序号",
				"类型", "类型序号", "类型ID"
				);
			try
			{
				graphList.Sort((x, y) =>
				{
					if (x.ship_sortno == y.ship_sortno)
					{
						if (x.ship_id == y.ship_id)
							return 0;

						return x.ship_id < y.ship_id ? -1 : 1;
					}

					return x.ship_sortno < y.ship_sortno ? -1 : 1;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Cachr>	GraphList.PrintToFile() 排序时发生异常（graphList.Sort）");
				Debug.WriteLine(ex);
			}


			graphList.ForEach(x =>
			{
				content.AppendFormat(
					"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
					x.ship_sortno, x.ship_id, x.ship_name,
					x.ship_filename, x.ship_version,
					x.ship_type_name, x.ship_type_id
				);
			});

			try
			{
				File.WriteAllText(filepath, content.ToString());
			}
			catch (Exception ex)
			{
				//Log.Exception(ex.Source, ex, "写入立绘列表文件时异常");
			}
		}

		/// <summary>
		/// 解析 api_start2 数据信息
		/// </summary>
		static void ParseSession(Session oSession)
		{
			SvData<kcsapi_start2> svd;
			if (!SvData.TryParse(oSession, out svd))
			{
				//Log.Warning("GraphList.ParseSession()", "TryParse失败，无效的Session对象！");
				return;
			}

			var mst_shipgraph = svd.Data.api_mst_shipgraph
									.ToDictionary(x => x.api_id);
			var mst_ship = svd.Data.api_mst_ship
									.ToDictionary(x => x.api_id);
			var mst_stype = svd.Data.api_mst_stype
									.ToDictionary(x => x.api_id);

			graphList.Clear();

			foreach (var _pair in mst_shipgraph)
			{
				var item = new ship_graph_item();
				var _loc1 = _pair.Value;

				item.ship_id = _loc1.api_id;
				item.ship_filename = _loc1.api_filename;
				item.ship_version = _loc1.api_version;
				item.ship_graph_sortno = _loc1.api_sortno;

				if (mst_ship.ContainsKey(item.ship_id))
				{
					var _loc2 = mst_ship[item.ship_id];

					item.ship_sortno = _loc2.api_sortno;
					item.ship_name = _loc2.api_name;
					item.ship_type_id = _loc2.api_stype;

					if (mst_stype.ContainsKey(item.ship_type_id))
					{
						var _loc3 = mst_stype[item.ship_type_id];
						item.ship_type_name = _loc3.api_name;
						item.ship_type_sortno = _loc3.api_sortno;
					}

					graphList.Add(item);
					mst_ship.Remove(item.ship_id);
				}
				else
				{
#if DEBUG
					Debug.WriteLine(@"CACHR> shipgraph->ship匹配失败
> {0} = {1} {2} {3}
", _loc1.ToString(), _loc1.api_id, _loc1.api_sortno, _loc1.api_filename);
#endif
				}
			}

#if DEBUG
			Debug.WriteLine("CACHR>	graphList = {0}, mst_shipgraph = {1}",
						graphList.Count.ToString(),
						mst_shipgraph.Count.ToString()
						);
#endif
		}

		/// <summary>
		/// 开始生成 GraphList.txt 文件
		/// </summary>
		static public void GenerateList()
		{
			//var path = Settings.Current.CacheFolder + "\\api_start2.session";
			var path = @"E:\Game\KanColleViewer ver.3.3\MyCache\api_start2.session";
			if (!File.Exists(path))
			{
				MessageBox.Show("无法生成舰娘列表，因为没有保存 api_start2 通信数据。", "提督很忙！缓存工具");
				return;
			}

			//kcsapi_start2 data;
			Session oSession;
			//try
			//{
				//data = (kcsapi_start2)ReadSessionData();
				oSession = ReadSessionData(path) as Session;
			//}
			//catch (Exception ex)
			//{
			//	MessageBox.Show("未能生成舰娘列表。读取本地保存的 api_start2 通信数据时发生异常。", "提督很忙！缓存工具");
			//	//Log.Exception(ex.Source, ex, "读取本地保存的 api_start2 通信数据时发生异常");
			//	throw ex;
			//}
			try
			{
				ParseSession(oSession);
			}
			catch (Exception ex)
			{
				MessageBox.Show("未能生成舰娘列表。解析 api_start2 数据时发生异常。", "提督很忙！缓存工具");
				//Log.Exception(ex.Source, ex, "解析 api_start2 数据时发生异常。");
				return;
			}
			try
			{
				PrintToFile();

				//string filepath = Settings.Current.CacheFolder + "\\GraphList.txt";
				string filepath = @"E:\Game\KanColleViewer ver.3.3\MyCache\GraphList.txt";
				var si = new System.Diagnostics.ProcessStartInfo()
				{
					FileName = filepath,
					UseShellExecute = true,
				};
				System.Diagnostics.Process.Start(si);
			}
			catch (Exception ex)
			{
				//Log.Exception(ex.Source, ex, "写入GraphList.txt时或启动进程时发生异常");
				return;
			}
		}

		/// <summary>
		/// 保存 api_start2 通信数据到本地
		/// </summary>
		static void SaveSessionData(Session session)
		{
			//var path = Settings.Current.CacheFolder + "\\api_start2.dat";

			session.SaveSession(Settings.Current.CacheFolder + "\\api_start2.session", false);

			//var data = session.GetRequestBodyAsString();
			//data = data.StartsWith("svdata=")
			//	? data.Substring(7) : data.Replace("svdata=", "");

			//Debug.WriteLineIf(data.Length < 100, data);

			//File.WriteAllText(path, data);
		}

		/// <summary>
		/// 从本地读取 api_start2 通信数据
		/// </summary>
		static object ReadSessionData(string path)
		{
			//var path = Settings.Current.CacheFolder + "\\api_start2.dat";
			//var bytes = Encoding.UTF8.GetBytes(File.ReadAllText(path));

			//var serializer = new DataContractJsonSerializer(typeof(svdata<kcsapi_start2>));
			//using (var stream = new MemoryStream(bytes))
			//{
			//	return serializer.ReadObject(stream) as svdata<kcsapi_start2>;
			//}

			Session session = new Session(new byte[] { 0 }, new byte[] { 0 });
			if (!session.LoadRequestBodyFromFile(path))
			{
				throw new ApplicationException("LoadRequestBodyFromFile Failed!! " + path);
			}
			if (!session.LoadResponseFromFile(path))
			{
				throw new ApplicationException("LoadResponseFromFile Failed!! " + path);
			}
			return session;
		}

		/// <summary>
		/// Fiddler规则（通信完成后
		/// </summary>
		static public void RulePrintGraphList(Session oSession)
		{
			if (oSession.PathAndQuery != "/kcsapi/api_start2")
				return;

			//Debug.WriteLine("【START2】" + oSession.PathAndQuery);

			SaveSessionData(oSession);

			//移除规则
			RemoveRule();
		}

		static public void AppendRule()
		{
			FiddlerApplication.AfterSessionComplete += RulePrintGraphList;
			//Debug.WriteLine("CACHR>	RulePrintGraphList Appended");
		}

		static public void RemoveRule()
		{
			FiddlerApplication.AfterSessionComplete -= RulePrintGraphList;
			//Debug.WriteLine("CACHR>	RulePrintGraphList Removed");
		}
	}


	class ship_graph_item
	{
		public int ship_id = 0;
		public int ship_sortno = 0;
		public string ship_name = "";

		public int ship_type_id = 0;
		public int ship_type_sortno = 0;
		public string ship_type_name = "";

		public int ship_graph_sortno = 0;
		public string ship_filename = "";
		public string ship_version = "";
	}
}
