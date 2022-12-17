# ScrapetubeSharp
scrape youtube without the official youtube api using c#

**Usage:**
Here's a few short code examples.
```
 YouTube youTube = new YouTube();
 List<Newtonsoft.Json.Linq.JObject> videos;
 
 videos = youTube.Get_Channel("UCCezIgC97PvUuR4_gbFUs5g", null);
 foreach (var item in videos)
 {
     Console.WriteLine(item["videoId"].ToString());
 }
 
videos = youTube.Get_Playlist("PL-osiE80TeTt2d9bfVyTiXJA-UTHn6WwU");
foreach (var item in videos)
{
    Console.WriteLine(item["videoId"].ToString());
}

videos = youTube.Get_Search("C#");
foreach (var item in videos)
{
    Console.WriteLine(item["videoId"].ToString());
}


```
