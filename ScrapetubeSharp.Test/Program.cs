﻿using System;

namespace ScrapetubeSharp.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            YouTube youTube = new YouTube();

            var videos = youTube.Get_Channel("UCCezIgC97PvUuR4_gbFUs5g", null);
            foreach (var item in videos)
            {
                Console.WriteLine(item["videoId"].ToString());
            }

            //var videos = youTube.Get_Playlist("PL-osiE80TeTt2d9bfVyTiXJA-UTHn6WwU");
            //foreach (var item in videos)
            //{
            //    Console.WriteLine(item["videoId"].ToString());
            //}

            //var videos = youTube.Get_Search("C#");
            //foreach (var item in videos)
            //{
            //    Console.WriteLine(item["videoId"].ToString());
            //}
            Console.ReadLine();
        }
    }
}
