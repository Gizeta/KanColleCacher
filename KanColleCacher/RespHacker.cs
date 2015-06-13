using d_f_32.KanColleCacher;
using d_f_32.KanColleCacher.Properties;
using Fiddler;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Gizeta.KanColleCacher
{
    public class RespHacker
    {
        private Settings set = Settings.Current;
        private int fcoin = 0; // 家具币
        private kcsapi_start2 initData = null;

        public RespHacker()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;

            KanColleClient.Current.Proxy.api_start2.TryParse<kcsapi_start2>().Subscribe(x => initData = x.Data);
            KanColleClient.Current.Proxy.api_get_member_basic.TryParse<kcsapi_basic>().Subscribe(x => fcoin = x.Data.api_fcoin);

            if (!File.Exists(Directory.GetCurrentDirectory() + @"\Plugins\picture_book_ext.dat"))
            {
                var res = new byte[Resources.picture_book_ext.Length];
                Resources.picture_book_ext.CopyTo(res, 0);
                var fs = new FileStream(Directory.GetCurrentDirectory() + @"\Plugins\picture_book_ext.dat", FileMode.Create, FileAccess.Write);
                fs.Write(res, 0, res.Length);
                fs.Close();
            }
        }

        public void Dispose()
        {
            FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
        }

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            if (!set.CacheEnabled) return;

            if (oSession.PathAndQuery.StartsWith("/kcsapi/api_req_furniture/music_play") && set.HackMusicRequestEnabled)
            {
                oSession.utilCreateResponseAndBypassServer();
                oSession.oResponse.headers.Add("Content-Type", "text/plain");
                oSession.utilSetResponseBody(@"svdata={""api_result"":1,""api_result_msg"":""\u6210\u529f"",""api_data"":{""api_coin"":" + fcoin.ToString() + @"}}");
            }
            else if (oSession.PathAndQuery.StartsWith("/kcsapi/api_get_member/picture_book") && set.HackBookEnabled)
            {
                oSession.utilCreateResponseAndBypassServer();
                oSession.oResponse.headers.Add("Content-Type", "text/plain");

                int type = 1; // 1: 舰娘图鉴, 2: 装备图鉴
                int no = 1;   // 页数
                var param = oSession.GetRequestBodyAsString().Split('&');
                foreach (var p in param)
                {
                    var kv = p.Split('=');
                    if (kv[0] == "api%5Ftype")
                    {
                        type = int.Parse(kv[1]);
                    }
                    else if (kv[0] == "api%5Fno")
                    {
                        no = int.Parse(kv[1]);
                    }
                }

                if (type == 1)
                {
                    oSession.utilSetResponseBody("svdata=" + ShipBookData.Generate(initData, no * 70 - 69, no * 70).ToJsonString());
                }
                else
                {
                    oSession.utilSetResponseBody("svdata=" + EquipmentBookData.Generate(initData, no * 50 - 49, no * 50).ToJsonString());
                }
            }
        }
    }

    [DataContract]
    internal class ShipBookData
    {
        [DataMember]
        public int api_result;
        [DataMember]
        public string api_result_msg;
        [DataMember]
        public ShipItemCollection api_data;

        public static ShipBookData Generate(kcsapi_start2 initData, int beginNo, int endNo)
        {
            return new ShipBookData
            {
                api_result = 1,
                api_result_msg = @"\u6210\u529f",
                api_data = new ShipItemCollection(initData, beginNo, endNo)
            };
        }

        public string ToJsonString()
        {
            var serializer = new DataContractJsonSerializer(typeof(ShipBookData));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, this);

            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);

            return Encoding.UTF8.GetString(dataBytes);
        }

        [DataContract]
        internal class ShipItemCollection
        {
            [DataMember]
            public ShipItem[] api_list;

            public ShipItemCollection(kcsapi_start2 initData, int beginNo, int endNo)
            {
                var list = new List<ShipItem>();
                initData.api_mst_ship.Where(x => x.api_sortno >= beginNo && x.api_sortno <= endNo).ToList()
                    .ForEach(x =>
                    {
                        var ship = new ShipItem(x);
                        initData.api_mst_ship.Where(y => y.api_id >= 900 && y.api_yomi == x.api_yomi).ToList() // 查找节日立绘
                            .ForEach(y => ship.AddGraph(y.api_id));
                        list.Add(ship);
                    });

                this.api_list = list.Count > 0 ? list.ToArray() : null;
            }

            [DataContract]
            internal class ShipItem
            {
                [DataMember]
                public int api_index_no;
                [DataMember]
                public int[][] api_state;
                [DataMember]
                public int[] api_table_id;
                [DataMember]
                public string api_name;
                [DataMember]
                public string api_yomi;
                [DataMember]
                public int api_stype;
                [DataMember]
                public int api_ctype;
                [DataMember]
                public int api_cnum;
                [DataMember]
                public int api_taik;
                [DataMember]
                public int api_souk;
                [DataMember]
                public int api_kaih;
                [DataMember]
                public int api_houg;
                [DataMember]
                public int api_raig;
                [DataMember]
                public int api_tyku;
                [DataMember]
                public int api_tais;
                [DataMember]
                public int api_leng;
                [DataMember]
                public string api_sinfo;

                public ShipItem(kcsapi_mst_ship ship)
                {
                    this.api_index_no = ship.api_sortno;
                    this.api_state = new int[][] { new int[] { 1, 1, 1, 0, 0 } };
                    this.api_table_id = new int[] { ship.api_id };
                    this.api_name = ship.api_name;
                    this.api_yomi = ship.api_yomi;
                    this.api_stype = ship.api_stype;
                    this.api_ctype = ship.api_ctype;
                    this.api_cnum = ship.api_cnum;
                    this.api_taik = ship.api_taik[0];
                    this.api_souk = ship.api_souk[0];
                    this.api_houg = ship.api_houg[0];
                    this.api_raig = ship.api_raig[0];
                    this.api_tyku = ship.api_tyku[0];
                    this.api_leng = ship.api_leng;

                    var ext = PictureBookExt.GetItem(ship.api_sortno);
                    this.api_kaih = ext == null ? 0 : ext.api_kaih;
                    this.api_tais = ext == null ? 0 : ext.api_tais;
                    this.api_sinfo = ext == null ? "" : ext.api_sinfo;
                }

                public void AddGraph(int id)
                {
                    var list = new List<int>(this.api_table_id);
                    list.Add(id);
                    this.api_table_id = list.ToArray();
                    this.api_state = new int[list.Count][];
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.api_state[i] = new int[] { 1, 1, 1, 0, 0 };
                    }
                }
            }
        }
    }

    [DataContract]
    internal class EquipmentBookData
    {
        [DataMember]
        public int api_result;
        [DataMember]
        public string api_result_msg;
        [DataMember]
        public EquipmentItemCollection api_data;

        public static EquipmentBookData Generate(kcsapi_start2 initData, int beginNo, int endNo)
        {
            return new EquipmentBookData
            {
                api_result = 1,
                api_result_msg = @"\u6210\u529f",
                api_data = new EquipmentItemCollection(initData, beginNo, endNo)
            };
        }

        public string ToJsonString()
        {
            var serializer = new DataContractJsonSerializer(typeof(EquipmentBookData));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, this);

            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);

            return Encoding.UTF8.GetString(dataBytes);
        }

        [DataContract]
        internal class EquipmentItemCollection
        {
            [DataMember]
            public EquipmentItem[] api_list;

            public EquipmentItemCollection(kcsapi_start2 initData, int beginNo, int endNo)
            {
                var list = new List<EquipmentItem>();
                initData.api_mst_slotitem.Where(x => x.api_sortno >= beginNo && x.api_sortno <= endNo).ToList()
                    .ForEach(x => list.Add(new EquipmentItem(x)));

                this.api_list = list.Count > 0 ? list.ToArray() : null;
            }

            [DataContract]
            internal class EquipmentItem
            {
                [DataMember]
                public int api_index_no;
                [DataMember]
                public int[] api_state;
                [DataMember]
                public int[] api_table_id;
                [DataMember]
                public string api_name;
                [DataMember]
                public int[] api_type;
                [DataMember]
                public int api_souk;
                [DataMember]
                public int api_houg;
                [DataMember]
                public int api_raig;
                [DataMember]
                public int api_soku;
                [DataMember]
                public int api_baku;
                [DataMember]
                public int api_tyku;
                [DataMember]
                public int api_tais;
                [DataMember]
                public int api_houm;
                [DataMember]
                public int api_houk;
                [DataMember]
                public int api_saku;
                [DataMember]
                public int api_leng;
                [DataMember]
                public int[] api_flag;
                [DataMember]
                public string api_info;

                public EquipmentItem(kcsapi_mst_slotitem item)
                {
                    this.api_index_no = item.api_sortno;
                    this.api_state = new int[] { 1, 0, 0, 0, 0 };
                    this.api_table_id = new int[] { item.api_id };
                    this.api_name = item.api_name;
                    this.api_type = item.api_type;
                    this.api_souk = item.api_souk;
                    this.api_houg = item.api_houg;
                    this.api_raig = item.api_raig;
                    this.api_soku = item.api_soku;
                    this.api_baku = item.api_baku;
                    this.api_tyku = item.api_tyku;
                    this.api_tais = item.api_tais;
                    this.api_houm = item.api_houm;
                    this.api_houk = item.api_houk;
                    this.api_saku = item.api_saku;
                    this.api_leng = item.api_leng;
                    this.api_info = item.api_info;

                    this.api_flag = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    var type = item.api_type;
                    if (type[2] == 1 || type[2] == 2 || type[2] == 3 || type[2] == 4) // 小口径主炮 + 中口径主炮 + 大口径主炮 + 副炮
                    {
                        this.api_flag[0] = 1;
                    }
                    if (type[2] == 5 || type[2] == 22) // 鱼雷 + 甲标的
                    {
                        this.api_flag[1] = 1;
                    }
                    if (type[2] == 8) // 舰攻
                    {
                        this.api_flag[2] = 1;
                    }
                    if (type[2] == 7 || type[2] == 11 || type[2] == 25 || type[2] == 26) // 舰爆 + 水爆 + カ号 + 三式指挥联络机
                    {
                        this.api_flag[3] = 1;
                    }
                    if (type[2] == 21) // 对空机铳
                    {
                        this.api_flag[4] = 1;
                    }
                    if (type[2] == 6) // 舰战
                    {
                        this.api_flag[5] = 1;
                    }
                    if (type[2] == 9 || type[2] == 10 || type[2] == 26) // 舰侦 + 水侦 + 三式指挥联络机
                    {
                        this.api_flag[6] = 1;
                    }
                    if (type[2] == 6 || type[2] == 18 || type[2] == 21) // 舰战 + 三式弹 + 对空机铳
                    {
                        this.api_flag[7] = 1;
                    }
                }
            }
        }
    }

    internal class PictureBookExt
    {
        private static List<BookExtItem> list = new List<BookExtItem>();

        static PictureBookExt()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + @"\Plugins\picture_book_ext.dat"))
            {
                var fs = new FileStream(Directory.GetCurrentDirectory() + @"\Plugins\picture_book_ext.dat", FileMode.Open);
                var sr = new StreamReader(fs);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var matches = Regex.Match(line, @"api_index_no:(.+?),api_kaih:(.+?),api_tais:(.+?),api_sinfo:""(.+?)""");
                    list.Add(new BookExtItem
                    {
                        api_index_no = int.Parse(matches.Groups[1].Value),
                        api_kaih = int.Parse(matches.Groups[2].Value),
                        api_tais = int.Parse(matches.Groups[3].Value),
                        api_sinfo = matches.Groups[4].Value
                    });
                }
                sr.Close();
            }
        }

        public static BookExtItem GetItem(int id)
        {
            return list.FirstOrDefault(x => x.api_index_no == id);
        }

        internal class BookExtItem
        {
            public int api_index_no;
            public int api_kaih;
            public int api_tais;
            public string api_sinfo;
        }
    }
}
