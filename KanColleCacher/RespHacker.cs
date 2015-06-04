using d_f_32.KanColleCacher;
using Fiddler;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;
using System;

namespace Gizeta.KanColleCacher
{
    public class RespHacker
    {
        private Settings set = Settings.Current;
        private int fcoin = 0; // 家具币

        public RespHacker()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;

            KanColleClient.Current.Proxy.api_get_member_basic.TryParse<kcsapi_basic>().Subscribe(x => fcoin = x.Data.api_fcoin);
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
        }
    }
}
