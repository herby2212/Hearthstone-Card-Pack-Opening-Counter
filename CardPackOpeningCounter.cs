using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.API;
using MahApps.Metro.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using System.Diagnostics;
using HDT_CardPackOpeningCounter.HSData;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using static HDT_CardPackOpeningCounter.HSData.CardDatabase;

namespace HDT_CardPackOpeningCounter
{
    public class CardPackOpeningCounter : IPlugin
    {
        public static int common = 0;
        public static int rare = 0;
        public static int epic = 0;
        public static int legendary = 0;
        public static int goldenCommon = 0;
        public static int goldenRare = 0;
        public static int goldenEpic = 0;
        public static int goldenLegendary = 0;

        private BackgroundWorker worker;
        private string[] previousLog = new string[] { };
        private static List<string> newCards = new List<string>();
        private static List<CardCollectionModification> ccmQueue = new List<CardCollectionModification>();

        private String hearthStoneDirectory;

        public string Name => "Card Pack Opener Counter";
        public string Description => "A HDT Plugin that assists you when your opening your glorious packs \nby counting the received cards and displaying the amount\nbased on rarity and quality (normal & golden).";
        public string ButtonText => "Open the Card Pack Opener Counter";
        public string Author => "herby2212";
        public Version Version => new Version(1, 2, 0);


        protected int i = 0;
        protected static MainWindow MainWindow;
        protected MenuItem MainMenuItem;
        public void OnButtonPress()
        {
            initializeMainWindow();
            MainWindow.Show();
        }

        public void OnLoad()
        {
            CardDatabase.loadHSData();
            worker = new BackgroundWorker();
            initializeMainWindow();
            initializeMainMenuItem();
        }

        private void initializeMainWindow()
        {
            if(MainWindow == null)
            {
                MainWindow = new MainWindow(this, common, rare, epic, legendary, goldenCommon, goldenRare, goldenEpic, goldenLegendary);
                MainWindow.Closed += mainWindowClosedEventHandler;
            }
        }

        private void initializeMainMenuItem()
        {
            if(MainMenuItem == null)
            {
                MainMenuItem = new MainMenuItem();
                MainMenuItem.Click += mainMenuItemClickEventHandler;
            }
        }

        private void mainMenuItemClickEventHandler(object sender, EventArgs e)
        {
            OnButtonPress();
        }

        private void mainWindowClosedEventHandler(object sender, EventArgs e)
        {
            MainWindow.Closed -= mainWindowClosedEventHandler;
            MainWindow = null;
        }

        public void OnUnload()
        {
            if(MainWindow != null)
            {
                MainWindow.Close();
            }
        }

        public void OnUpdate()
        {
            if(!checkIfHearthstoneIsRunning())
            {
                return;
            }
            if(hearthStoneDirectory == null)
            {
                initializeHearthStonePath();           
            }           
            if (checkfInPackOpening())
            {
                setupLogFileAnalyzer();
                updateCardCountWindows();
            }
        }

        private void setupLogFileAnalyzer()
        {
            String netLogFilePath = this.hearthStoneDirectory + @"\Logs\Net.log";
            this.analyzeLogFile(netLogFilePath);
        }

        private void analyzeLogFile(String netLogFilePath)
        {
            FileStream stream = File.Open(netLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream);

            string test = "";

            List<String> logList = new List<String>();
            List<string> logNewRead = new List<string>();
            while (!reader.EndOfStream)
            {
                string text = reader.ReadLine();
                logNewRead.Add(text);

            }
            string[] logNew = logNewRead.ToArray();
            
            if (logNew.Length == previousLog.Length)
            {
                return;
            }
            test += "longer then previous reading out data... (" + logNew.Length + " - " + previousLog.Length + ")\n";
            List<string> newDataRead = new List<string>();

            int i = 0;
            foreach (string s in logNew)
            {
                if (i + 1 >= previousLog.Length)
                {
                    newDataRead.Add(s);
                    test += s + "\n";
                }
                else
                {
                    if (!s.Equals(previousLog[i]))
                    {
                        newDataRead.Add(s);
                        test += s + "\n";
                    }
                }
                i++;
            }
            string[] newData = newDataRead.ToArray();
            test += "reading data completed. NewData size: " + newData.Length + ". Setting logNew to previousLog.";
            previousLog = logNew;

            newCards.Clear();
            bool _inOpening = false;
            string packOpeningCards = "";
            foreach (string s in newData)
            {
                if (s.Contains("Network.OpenBooster")) {
                    _inOpening = true;
                    packOpeningCards += s;
                } else if(s.Contains("NetCacheBoosters") && _inOpening)
                {
                    packOpeningCards += s;
                    CardPackOpeningCounter.newCards.Add(packOpeningCards);
                    packOpeningCards = "";
                    _inOpening = false;
                } else if (_inOpening)
                {
                    packOpeningCards += s;
                }
            }

            worker.DoWork -= new DoWorkEventHandler(CardAddHandler);
            worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(CardAddCompleted);

            worker.DoWork += new DoWorkEventHandler(CardAddHandler);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CardAddCompleted);

            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync();
        }

        private void CardAddCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Check for outstanding card additions
            if(ccmQueue.Count > 0)
            {
                foreach (CardCollectionModification ccm in ccmQueue)
                {
                    addCardBasedOnCardId(ccm.AssetCardId, ccm.Premium);
                }
                ccmQueue.Clear();
            }
        }

        private void updateCardCountWindows()
        {
            if (!MainWindow.IsVisible)
            {
                return;
            }

            MainWindow.refresh(common, rare, epic, legendary);
            MainWindow.refreshGolden(goldenCommon, goldenRare, goldenEpic, goldenLegendary);
            DustCalculation();
        }

        private void CardAddHandler(object sender, DoWorkEventArgs e)
        {
            String openings = "";
            foreach (string packOpening in newCards)
            {
                openings += "PACK START\n";
                MatchCollection match = Regex.Matches(packOpening, @"{ (.+?)}");
                foreach(Match m in match)
                {
                    
                    var asset = Regex.Matches(m.Value, @"\d+");
                    CardCollectionModification ccm = new CardCollectionModification(int.Parse(asset[0].Value), int.Parse(asset[1].Value), int.Parse(asset[2].Value), int.Parse(asset[3].Value));
                    ccmQueue.Add(ccm);
                    openings += m.Value + " - " + asset[0].Value + " - " + asset[1].Value + " - " + asset[2].Value + " - " + asset[3].Value + " - queueSize: " + ccmQueue.Count + "\n";
                }
                openings += "PACK END\n";
            }
        }

        private void changeCardBasedOnRarity(RARITY rarity, int premium, CardCountModificationMode cardCountModificationMode)
        {
            bool golden = Convert.ToBoolean(premium);
            if (rarity == RARITY.COMMON)
            {
                switch (cardCountModificationMode)
                {
                    case CardCountModificationMode.ADD:
                        if (golden)
                        {
                            goldenCommon += 1;
                        }
                        else
                        {
                            common += 1;
                        }
                        break;
                    case CardCountModificationMode.REMOVE:
                        if (golden)
                        {
                            goldenCommon = goldenCommon == 0 ? 0 : goldenCommon -= 1;
                        }
                        else
                        {
                            common = common == 0 ? 0 : common -= 1;
                        }
                        break;
                }
            }
            else if (rarity == RARITY.RARE)
            {
                switch (cardCountModificationMode)
                {
                    case CardCountModificationMode.ADD:
                        if (golden)
                        {
                            goldenRare += 1;
                        }
                        else
                        {
                            rare += 1;
                        }
                        break;
                    case CardCountModificationMode.REMOVE:
                        if (golden)
                        {
                            goldenRare = goldenRare == 0 ? 0 : goldenRare  -= 1;
                        }
                        else
                        {
                            rare = rare == 0 ? 0 : rare -= 1;
                        }
                        break;
                }
            }
            else if (rarity == RARITY.EPIC)
            {
                switch (cardCountModificationMode)
                {
                    case CardCountModificationMode.ADD:
                        if (golden)
                        {
                            goldenEpic += 1;
                        }
                        else
                        {
                            epic += 1;
                        }
                        break;
                    case CardCountModificationMode.REMOVE:
                        if (golden)
                        {
                            goldenEpic = goldenEpic == 0 ? 0 : goldenEpic -= 1;
                        }
                        else
                        {
                            epic = epic == 0 ? 0 : epic -= 1;
                        }
                        break;
                }
            }
            else if (rarity == RARITY.LEGENDARY)
            {
                switch (cardCountModificationMode)
                {
                    case CardCountModificationMode.ADD:
                        if (golden)
                        {
                            goldenLegendary += 1;
                        }
                        else
                        {
                            legendary += 1;
                        }
                        break;
                    case CardCountModificationMode.REMOVE:
                        if (golden)
                        {
                            goldenLegendary = goldenLegendary == 0 ? 0 : goldenLegendary -= 1;
                        }
                        else
                        {
                            legendary = legendary == 0 ? 0 : legendary -= 1;
                        }
                        break;
                }
            }
        }

        public void addCardBasedOnRarity(RARITY rarity, int premium)
        {
            changeCardBasedOnRarity(rarity, premium, CardCountModificationMode.ADD);
            updateCardCountWindows();
        }

        public void removeCardBasedOnRarity(RARITY rarity, int premium)
        {
            changeCardBasedOnRarity(rarity, premium, CardCountModificationMode.REMOVE);
            updateCardCountWindows();
        }

        private enum CardCountModificationMode
        {
            ADD,
            REMOVE
        }

        private void addCardBasedOnCardId(int cardId, int premium)
        {
            RARITY rarity = CardDatabase.getRarity(cardId);
            addCardBasedOnRarity(rarity, premium);
        }

        private void initializeHearthStonePath()
        {
            Process hearthstone = Process.GetProcessesByName("hearthstone")[0];
            String path = hearthstone.MainModule.FileName;
            String directory = path.Remove(path.LastIndexOf("\\"), path.Length - path.LastIndexOf("\\"));
            hearthStoneDirectory = directory;
            
        }

        internal List<string> saveCounts() {
            List<String> lines = new List<string>();
            for (int i = 0; i < common; i++)
            {
                lines.Add("Card:Common");
            }
            for (int i = 0; i < rare; i++)
            {
                lines.Add("Card:Rare");
            }
            for (int i = 0; i < epic; i++)
            {
                lines.Add("Card:Epic");
            }
            for (int i = 0; i < legendary; i++)
            {
                lines.Add("Card:Legendary");
            }
            for (int i = 0; i < goldenCommon; i++)
            {
                lines.Add("Card:Golden:Common");
            }
            for (int i = 0; i < goldenRare; i++)
            {
                lines.Add("Card:Golden:Rare");
            }
            for (int i = 0; i < goldenEpic; i++)
            {
                lines.Add("Card:Golden:Epic");
            }
            for (int i = 0; i < goldenLegendary; i++)
            {
                lines.Add("Card:Golden:Legendary");
            }
            return lines;
        }

        internal void loadFile(string fileName)
        {
            resetCount();
            String[] log = File.ReadAllLines(fileName);
            foreach (string line in log)
            {
                if (line.Contains("Card:Common"))
                {
                    common += 1;
                }
                else if (line.Contains("Card:Rare"))
                {
                    rare += 1;
                }
                else if (line.Contains("Card:Epic"))
                {
                    epic += 1;
                }
                else if (line.Contains("Card:Legendary"))
                {
                    legendary += 1;
                }
                else if (line.Contains("Card:Golden:Common"))
                {
                    goldenCommon += 1;
                }
                else if (line.Contains("Card:Golden:Rare"))
                {
                    goldenRare += 1;
                }
                else if (line.Contains("Card:Golden:Epic"))
                {
                    goldenEpic += 1;
                }
                else if (line.Contains("Card:Golden:Legendary"))
                {
                    goldenLegendary += 1;
                }
            }
            updateCardCountWindows();
        }

        public void resetCount()
        {
            common = 0;
            rare = 0;
            epic = 0;
            legendary = 0;
            goldenCommon = 0;
            goldenRare = 0;
            goldenEpic = 0;
            goldenLegendary = 0;

            updateCardCountWindows();
        }

        private void cardsTotal()
        {
            int cardsTotal = 0;
            int cardPacks = 0;

            cardsTotal = common + rare + epic + legendary
                + goldenCommon + goldenRare + goldenEpic + goldenLegendary;

            cardPacks = cardsTotal / 5;

            MainWindow.refreshPackValues(cardsTotal, cardPacks);
        }

        private void DustCalculation()
        {
            int craftingCost = 0;
            int dustingValue = 0;
            /*
             * 
             * Crafting Cost
             * 
             */

            craftingCost = (common * 40) + (rare * 100) + (epic * 400) + (legendary * 1600)
                + (goldenCommon * 400) + (goldenRare * 800) + (goldenEpic * 1600) + (goldenLegendary * 3200);


            /*
             * 
             * Dusting Value
             * 
             */

            dustingValue = (common * 5) + (rare * 20) + (epic * 100) + (legendary * 400)
                + (goldenCommon * 50) + (goldenRare * 100) + (goldenEpic * 400) + (goldenLegendary * 1600);

            MainWindow.refreshDustValues(craftingCost, dustingValue);

            cardsTotal();
        }

        private bool checkIfHearthstoneIsRunning()
        {
            return Core.Game.IsRunning;
        }

        private Boolean checkfInPackOpening()
        {
            return Core.Game.CurrentMode == Mode.PACKOPENING;
        }
        public MenuItem MenuItem => MainMenuItem;
        public static PluginSettings Settings { get; set; }
    }

    class CardCollectionModification
    {
        public CardCollectionModification(int amountSeen, int assetCardId, int premium, int quantity)
        {
            AmountSeen = amountSeen;
            AssetCardId = assetCardId;
            Premium = premium;
            Quantity = quantity;
        }
        int AmountSeen { get; set; }
        public int AssetCardId { get; set; }
        //0 = Normal; 1 = Golden; 3 = Signature
        public int Premium { get; set; }
        int Quantity { get; set; }
    }
}
