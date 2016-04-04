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
using System.Runtime.Serialization.Json;
using System.Windows;


namespace d_f_32.KanColleCacher
{
	class GraphList
	{
		static List<ship_graph_item> graphList = new List<ship_graph_item>();
		static Session api_start2;

		/// <summary>
		/// 将解析完成的信息保存到本地
		/// </summary>
		static void PrintToFile(string filepath)
		{
			StringBuilder content = new StringBuilder();

			content.AppendFormat(
				"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35}\r\n",
				"SortNo", "ShipId", "ShipName",
				"FileName",
				"TypeName",  "TypeId",
                "boko_n_left", "boko_n_top",
                "boko_d_left", "boko_d_top",
                "map_n_left", "map_n_top",
                "map_d_left", "map_d_top",
                "battle_n_left", "battle_n_top",
                "battle_d_left", "battle_d_top",
                "ensyuf_n_left", "ensyuf_n_top",
                "ensyuf_d_left", "ensyuf_d_top",
                "ensyue_n_left", "ensyue_n_top",
                "kaizo_n_left", "kaizo_n_top",
                "kaizo_d_left", "kaizo_d_top",
                "kaisyu_n_left", "kaisyu_n_top",
                "kaisyu_d_left", "kaisyu_d_top",
                "weda_left", "weda_top",
                "wedb_left", "wedb_top"
				);
			content.AppendFormat(
				"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35}\r\n",
				"序号", "ID", "名称",
				"文件名",
				"类型",  "类型ID",
                "boko_n_left", "boko_n_top",
                "boko_d_left", "boko_d_top",
                "map_n_left", "map_n_top",
                "map_d_left", "map_d_top",
                "battle_n_left", "battle_n_top",
                "battle_d_left", "battle_d_top",
                "ensyuf_n_left", "ensyuf_n_top",
                "ensyuf_d_left", "ensyuf_d_top",
                "ensyue_n_left", "ensyue_n_top",
                "kaizo_n_left", "kaizo_n_top",
                "kaizo_d_left", "kaizo_d_top",
                "kaisyu_n_left", "kaisyu_n_top",
                "kaisyu_d_left", "kaisyu_d_top",
                "weda_left", "weda_top", 
                "wedb_left", "wedb_top"
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
						"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35}\r\n",
						x.ship_sortno, x.ship_id, x.ship_name,
						x.ship_filename,
						x.ship_type_name, x.ship_type_id,
                        x.ship_graph_boko_n_left, x.ship_graph_boko_n_top, 
                        x.ship_graph_boko_d_left, x.ship_graph_boko_d_top,
                        x.ship_graph_map_n_left, x.ship_graph_map_n_top,
                        x.ship_graph_map_d_left, x.ship_graph_map_d_top,
                        x.ship_graph_battle_n_left, x.ship_graph_battle_n_top,
                        x.ship_graph_battle_d_left, x.ship_graph_battle_d_top,
                        x.ship_graph_ensyuf_n_left, x.ship_graph_ensyuf_n_top,
                        x.ship_graph_ensyuf_d_left, x.ship_graph_ensyuf_d_top,
                        x.ship_graph_ensyue_n_left, x.ship_graph_ensyue_n_top,
                        x.ship_graph_kaizo_n_left, x.ship_graph_kaizo_n_top,
                        x.ship_graph_kaizo_d_left, x.ship_graph_kaizo_d_top,
                        x.ship_graph_kaisyu_n_left, x.ship_graph_kaisyu_n_top,
                        x.ship_graph_kaisyu_d_left, x.ship_graph_kaisyu_d_top,
                        x.ship_graph_weda_left, x.ship_graph_weda_top,
                        x.ship_graph_wedb_left, x.ship_graph_wedb_top
					);
				});

			try
			{
				File.WriteAllText(filepath, content.ToString());
			}
			catch (Exception ex)
			{
				Log.Exception(ex.Source, ex, "写入立绘列表文件时异常");
			}
		}

		/// <summary>
		/// 解析 api_start2 数据信息
		/// </summary>item.ship_graph_sortno
		static void ParseSession(Session oSession)
		{
			SvData<kcsapi_start2> svd;
			if (!SvData.TryParse(oSession, out svd))
			{
				Log.Warning("GraphList.ParseSession()", "TryParse失败，无效的Session对象！");
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
				item.ship_graph_sortno = _loc1.api_sortno;
                item.ship_graph_boko_n_left = _loc1.api_boko_n[0];
                item.ship_graph_boko_n_top = _loc1.api_boko_n[1];
                item.ship_graph_boko_d_left = _loc1.api_boko_d[0];
                item.ship_graph_boko_d_top = _loc1.api_boko_d[1];
                item.ship_graph_map_n_left = _loc1.api_map_n[0];
                item.ship_graph_map_n_top = _loc1.api_map_n[1];
                item.ship_graph_map_d_left = _loc1.api_map_d[0];
                item.ship_graph_map_d_top = _loc1.api_map_d[1];
                item.ship_graph_battle_n_left = _loc1.api_battle_n[0];
                item.ship_graph_battle_n_top = _loc1.api_battle_n[1];
                item.ship_graph_battle_d_left = _loc1.api_battle_d[0];
                item.ship_graph_battle_d_top = _loc1.api_battle_d[1];
                item.ship_graph_ensyuf_n_left = _loc1.api_ensyuf_n[0];
                item.ship_graph_ensyuf_n_top = _loc1.api_ensyuf_n[1];
                item.ship_graph_ensyuf_d_left = _loc1.api_ensyuf_d[0];
                item.ship_graph_ensyuf_d_top = _loc1.api_ensyuf_d[1];
                item.ship_graph_ensyue_n_left = _loc1.api_ensyue_n[0];
                item.ship_graph_ensyue_n_top = _loc1.api_ensyue_n[1];
                item.ship_graph_kaizo_n_left = _loc1.api_kaizo_n[0];
                item.ship_graph_kaizo_n_top = _loc1.api_kaizo_n[1];
                item.ship_graph_kaizo_d_left = _loc1.api_kaizo_d[0];
                item.ship_graph_kaizo_d_top = _loc1.api_kaizo_d[1];
                item.ship_graph_kaisyu_n_left = _loc1.api_kaisyu_n[0];
                item.ship_graph_kaisyu_n_top = _loc1.api_kaisyu_n[1];
                item.ship_graph_kaisyu_d_left = _loc1.api_kaisyu_d[0];
                item.ship_graph_kaisyu_d_top = _loc1.api_kaisyu_d[1];
                item.ship_graph_weda_left = _loc1.api_weda[0];
                item.ship_graph_weda_top = _loc1.api_weda[1];
                item.ship_graph_wedb_left = _loc1.api_wedb[0];
                item.ship_graph_wedb_top = _loc1.api_wedb[1];

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

			if (api_start2 == null)
			{
				MessageBox.Show("无法生成舰娘列表，因为没有保存 api_start2 通信数据。", "提督很忙！缓存工具");
				return;
			}

			try
			{
				ParseSession(api_start2);
			}
			catch (Exception ex)
			{
				MessageBox.Show("未能生成舰娘列表。解析 api_start2 数据时发生异常。", "提督很忙！缓存工具");
				Log.Exception(ex.Source, ex, "解析 api_start2 数据时发生异常。");
				return;
			}
			try
			{
				string filepath = Settings.Current.CacheFolder + "\\GraphList.txt";
				PrintToFile(filepath);
				var si = new System.Diagnostics.ProcessStartInfo()
				{
					FileName = filepath,
					UseShellExecute = true,
				};
				System.Diagnostics.Process.Start(si);
			}
			catch (Exception ex)
			{
				Log.Exception(ex.Source, ex, "写入GraphList.txt时或启动进程时发生异常");
				return;
			}
		}

		/// <summary>
		/// Fiddler规则（通信完成后
		/// </summary>
		static public void RulePrintGraphList(Session oSession)
		{
			if (oSession.PathAndQuery != "/kcsapi/api_start2")
				return;
			
			//Debug.WriteLine("【START2】" + oSession.PathAndQuery);
			
			api_start2 = oSession;
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
        public int ship_graph_boko_n_left = 0;
        public int ship_graph_boko_n_top = 0;
        public int ship_graph_boko_d_left = 0;
        public int ship_graph_boko_d_top = 0;
        public int ship_graph_map_n_left = 0;
        public int ship_graph_map_n_top = 0;
        public int ship_graph_map_d_left = 0;
        public int ship_graph_map_d_top = 0;
        public int ship_graph_battle_n_left = 0;
        public int ship_graph_battle_n_top = 0;
        public int ship_graph_battle_d_left = 0;
        public int ship_graph_battle_d_top = 0;
        public int ship_graph_ensyuf_n_left = 0;
        public int ship_graph_ensyuf_n_top = 0;
        public int ship_graph_ensyuf_d_left = 0;
        public int ship_graph_ensyuf_d_top = 0;
        public int ship_graph_ensyue_n_left = 0;
        public int ship_graph_ensyue_n_top = 0;
        public int ship_graph_kaizo_n_left = 0;
        public int ship_graph_kaizo_n_top = 0;
        public int ship_graph_kaizo_d_left = 0;
        public int ship_graph_kaizo_d_top = 0;
        public int ship_graph_kaisyu_n_left = 0;
        public int ship_graph_kaisyu_n_top = 0;
        public int ship_graph_kaisyu_d_left = 0;
        public int ship_graph_kaisyu_d_top = 0;
        public int ship_graph_weda_left = 0;
        public int ship_graph_weda_top = 0;
        public int ship_graph_wedb_left = 0;
        public int ship_graph_wedb_top = 0;
	}
}
