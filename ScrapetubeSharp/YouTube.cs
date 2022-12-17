using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace ScrapetubeSharp
{
   public class YouTube
    {
        HttpClient httpClient;

        public List<JObject> Get_Channel(
                    string channel_id,
                    string channel_url,
                    int limit = 0,
                    int sleep = 1,
                    string sort_by = "newest",
                    string content_type = "videos")
        {
            var sort_by_map = new Dictionary<string, string> {
                {
                    "newest",
                    "dd"},
                {
                    "oldest",
                    "da"},
                {
                    "popular",
                      "p"}
            };

            var results_type_map = new Dictionary<string, string> { {
                    "videos", "videoRenderer"
                 }, {
                    "streams", "videoRenderer"
                 },{
                    "shorts", "reelItemRenderer"
                 }
                };

            if (string.IsNullOrEmpty(channel_url))
            {
                channel_url = string.Format("https://www.youtube.com/channel/{0}", channel_id);
            }

            var url = string.Format("{0}/{1}?view=0&sort={2}&flow=grid", channel_url, content_type, sort_by_map[sort_by]);
            string api_endpoint = "https://www.youtube.com/youtubei/v1/browse";
            var videos = Get_Videos(url, api_endpoint, results_type_map[content_type], limit, sleep);
            return videos;
        }

        public List<JObject> Get_Playlist(
                   string playlist_id,
                   int limit = 0,
                   int sleep = 1)
        {
            var url = string.Format("https://www.youtube.com/playlist?list={0}", playlist_id);
            string api_endpoint = "https://www.youtube.com/youtubei/v1/browse";
            var videos = Get_Videos(url, api_endpoint, "playlistVideoRenderer", limit, sleep);
            return videos;
        }

        public List<JObject> Get_Search(
                        string query,
                        int limit = 0,
                        int sleep = 1,
                        string sort_by = "relevance",
                        string results_type = "video")
        {

            var sort_by_map = new Dictionary<string, string> {
                {
                    "relevance",
                    "A"
                },
                {
                    "upload_date",
                    "I"},
                {
                    "view_count",
                    "M"},
                {
                    "rating",
                    "E"}};
            var results_type_map = new Dictionary<string, List<string>> {
                {
                    "video",
                    new List<string> {
                        "B",
                        "videoRenderer"
                    }},
                {
                    "channel",
                    new List<string> {
                        "C",
                        "channelRenderer"
                    }},
                {
                    "playlist",
                    new List<string> {
                        "D",
                        "playlistRenderer"
                    }},
                {
                    "movie",
                    new List<string> {
                        "E",
                        "videoRenderer"
                    }}};
            var param_string = string.Format("CA{0}SAhA{1}", sort_by_map[sort_by], results_type_map[results_type][0]);
            var url = string.Format("https://www.youtube.com/results?search_query={0}&sp={1}", query, param_string);
            var api_endpoint = "https://www.youtube.com/youtubei/v1/search";
            var videos = Get_Videos(url, api_endpoint, results_type_map[results_type][1], limit, sleep);
            return videos;
            //var damm = videos;
            //foreach (var video in videos)
            //{
            //   var vv = video["title"]["runs"][0]["text"].ToString();
            //}
        }

        private List<JObject> Get_Videos(
              string url,
              string api_endpoint,
              string selector,
              int limit,
              int sleep)
        {
            Dictionary<object, object> next_data = null;
            JObject data = new JObject();
            JToken client = null;
            var is_first = true;
            var quit = false;
            var count = 0;
            string api_key = "";
            List<JObject> objs = new List<JObject>();
            while (true)
            {
                if (is_first)
                {
                    httpClient = new HttpClient();

                    var req_hesder = httpClient.DefaultRequestHeaders;
                    //   req_hesder.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    req_hesder.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.101 Safari/537.36");
                    req_hesder.Add("sec-ch-ua-platform", "Windows");

                    var html = get_initial_data(httpClient, url);
                    api_key = get_json_from_html(html, "innertubeApiKey", 3);
                    var hhht = get_json_from_html(html, "INNERTUBE_CONTEXT", 2, "\"}},") + "\"}}";
                    var cc = (JObject)JsonConvert.DeserializeObject(hhht);
                    client = cc["client"];
                    req_hesder.Add("X-YouTube-Client-Name", "1");
                    req_hesder.Add("X-YouTube-Client-Version", cc["client"]["clientVersion"].ToString());
                    data = (JObject)JsonConvert.DeserializeObject(get_json_from_html(html, "var ytInitialData = ", 0, "};") + "}");
                    var test = get_json_from_html(html, "var ytInitialData = ", 0, "};") + "}";
                    next_data = get_next_data(data);
                    is_first = false;
                }
                else
                {
                    data = get_ajax_data(httpClient, api_endpoint, api_key, next_data, client);
                    next_data = get_next_data(data);
                }

                foreach (var result in get_videos_items(data, selector))
                {
                    try
                    {
                        count += 1;
                        if (count == limit)
                        {
                            quit = true;
                            break;
                        }
                        objs.Add(result);
                    }
                    catch (Exception ex)
                    {
                        quit = true;
                        break;
                    }
                }
                if (next_data == null || quit)
                {
                    break;
                }
                Thread.Sleep(1000 * sleep);
            }
            return objs;
            // session.close();
        }

        private string get_initial_data(HttpClient session, string url)
        {
            //  session..set("CONSENT", "YES+cb", domain: ".youtube.com");
            var response = session.GetAsync(url).Result;
            var html = response.Content.ReadAsStringAsync().Result;
            return html;
        }

        private string get_json_from_html(string html, string key, int num_chars = 2, string stop = "\"")
        {
            int pos_begin = html.IndexOf(key);
            pos_begin = pos_begin + key.Length + num_chars;
            var pos_end = html.IndexOf(stop, pos_begin);
            return html.Substring(pos_begin, pos_end - pos_begin);
        }

        private JObject get_ajax_data(
                HttpClient session,
                string api_endpoint,
                string api_key,
                 Dictionary<object, object> next_data,
                object client)
        {
            var data = new Dictionary<object, object> {
                {
                    "context", new Dictionary<object, object> {
                        {
                            "clickTracking",
                            next_data["click_params"]},{
                            "client",
                            client}
                    }},
                      {"continuation",next_data["token"]
                }
            };

            data.Add("key", api_key);
            var scontent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = session.PostAsync(api_endpoint, scontent).Result;
            string dt = response.Content.ReadAsStringAsync().Result;
            var cc = (JObject)JsonConvert.DeserializeObject(dt);
            return cc;
        }

        private Dictionary<object, object> get_next_data(JObject data)
        {
            var raw_next_data = search_dict(data, "continuationEndpoint");
            if (raw_next_data == null)
            {
                return null;
            }
            if (raw_next_data.Count == 0)
            {
                return null;
            }
            var next_data = new Dictionary<object, object> {
                {
                    "token" , raw_next_data[0]["continuationCommand"]["token"]},
                {
                    "click_params",
                    new Dictionary<object, object> {
                        { "clickTrackingParams" , raw_next_data[0]["clickTrackingParams"]}}}};
            return next_data;
        }

        private List<JObject> search_dict(JObject partial, string search_key)
        {
            var stack = new Stack<object>();
            stack.Push(partial);
            List<JObject> jo = new List<JObject>();
            while (stack.Count > 0)
            {
                var current_item = stack.Pop();
                var tkon = JToken.FromObject(current_item);
                if (tkon.Type == JTokenType.Object)
                {
                    var dic = (JObject)current_item;
                    var mm = dic.ToObject<Dictionary<string, object>>();
                    foreach (var _tup_1 in mm)
                    {
                        var key = _tup_1.Key;
                        var value = _tup_1.Value;
                        if (key == search_key)
                        {
                            //var jbo = (JObject)value;
                            jo.Add((JObject)value);
                        }
                        else
                        {
                            stack.Push(value);
                        }
                    }
                }
                else if (tkon.Type == JTokenType.Array)
                {
                    foreach (var value in (JArray)current_item)
                    {
                        stack.Push(value);
                    }
                }
            }
            return jo;
        }


        private List<JObject> get_videos_items(JObject data, string selector)
        {
            return search_dict(data, selector);
        }

    }
}
