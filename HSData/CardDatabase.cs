using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace HDT_CardPackOpeningCounter.HSData
{
    public static class CardDatabase
    {

        private static CardData[] data;

        public static void loadHSData()
        {
            CardDatabase.data = CardDatabase.getHSData("enUS");
            String output = "";
            foreach(CardData d in data)
            {
                output += "ID: " + d.dbfId + " - Rarity: " + d.rarity + "\n";
            }
            String filename = @"E:\Spiele\Blizzard\Hearthstone\CardDBList.txt";
            File.WriteAllText(filename, output);
        }

        public static RARITY getRarity(int cardId)
        {
            String rarity = "";
            foreach (CardData d in data)
            {
                if (d.dbfId == cardId)
                {
                    rarity = d.rarity;
                }
            }
            switch (rarity)
            {
                case "FREE":
                    return RARITY.FREE;
                case "COMMON":
                    return RARITY.COMMON;
                case "RARE":
                    return RARITY.RARE;
                case "EPIC":
                    return RARITY.EPIC;
                case "LEGENDARY":
                    return RARITY.LEGENDARY;
                default:
                    return RARITY.FREE;
            }
        }

        private static CardData[] getHSData(String languageCode)
        {
            //Copyright by HearthSim - All rights of the files used down below belong to them.
            Uri uri = new Uri("https://api.hearthstonejson.com/v1/latest/" + languageCode + "/cards.collectible.json");
            String jsonDocument = GetWebPage(uri);
            String filename = @"E:\Spiele\Blizzard\Hearthstone\CardDB.json";
            File.WriteAllText(filename, jsonDocument);
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 4000000;
            return serializer.Deserialize<CardData[]>(jsonDocument);

        }

        private static string GetWebPage(Uri uri)
        {
            if ((uri == null))
            {
                throw new ArgumentNullException("uri");
            }

            using (var request = new WebClient())
            {
                //Download the data
                var requestData = request.DownloadData(uri);

                //Return the data by encoding it back to text!
                return Encoding.ASCII.GetString(requestData);
            }
        }

        public class CardData
        {
            public String artist { get; set; }
            String cardClass { get; set; }
            Boolean collectible { get; set; }
            int cost { get; set; }
            public int dbfId { get; set; }
            String flavor { get; set; }
            String id { get; set; }
            String[] mechanics { get; set; }
            String name { get; set; }
            public String rarity { get; set; }
            String[] referencedTags { get; set; }
            String set { get; set; }
            String spellSchool { get; set; }
            String text { get; set; }
            String type { get; set; }
        }

        public enum RARITY
        {
            FREE,
            COMMON,
            RARE,
            EPIC,
            LEGENDARY
        }
    }
}
