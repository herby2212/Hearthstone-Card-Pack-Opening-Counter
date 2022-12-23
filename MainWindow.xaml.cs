using HDT_CardPackOpeningCounter.HSData;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static HDT_CardPackOpeningCounter.HSData.CardDatabase;

namespace HDT_CardPackOpeningCounter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        protected static Overlay Overlay;

        protected static CardPackOpeningCounter cardPackOpeningCounter;

        public MainWindow(CardPackOpeningCounter instance, int cCommon, int cRare, int cEpic, int cLegendary,
            int cGoldenCommon, int cGoldenRare, int cGoldenEpic, int cGoldenLegendary)
        {
            InitializeComponent();
            cardPackOpeningCounter = instance;
            InitializeOverlay(cCommon, cRare, cEpic, cLegendary, cGoldenCommon, cGoldenRare, cGoldenEpic, cGoldenLegendary);
            refresh(cCommon, cRare, cEpic, cLegendary);
            refreshGolden(cGoldenCommon, cGoldenRare, cGoldenEpic, cGoldenLegendary);
        }

        private void InitializeOverlay(int cCommon, int cRare, int cEpic, int cLegendary,
            int cGoldenCommon, int cGoldenRare, int cGoldenEpic, int cGoldenLegendary)
        {
            if (Overlay == null)
            {
                Overlay = new Overlay(cCommon, cRare, cEpic, cLegendary, cGoldenCommon, cGoldenRare, cGoldenEpic, cGoldenLegendary);
            }
        }

        public void refresh(int cCommon, int cRare, int cEpic, int cLegendary)
        {
            this.updateLabel(this.lCountCommon, cCommon);
            this.updateLabel(this.lCountRare, cRare);
            this.updateLabel(this.lCountEpic, cEpic);
            this.updateLabel(this.lCountLegendary, cLegendary);
            if (Overlay != null && Overlay.IsVisible)
            {
                Overlay.refresh(cCommon, cRare, cEpic, cLegendary);
            }
        }

        public void refreshGolden(int cGoldenCommon, int cGoldenRare, int cGoldenEpic, int cGoldenLegendary)
        {
            this.updateLabel(this.lCountGoldenCommon, cGoldenCommon);
            this.updateLabel(this.lCountGoldenRare, cGoldenRare);
            this.updateLabel(this.lCountGoldenEpic, cGoldenEpic);
            this.updateLabel(this.lCountGoldenLegendary, cGoldenLegendary);
            if (Overlay != null && Overlay.IsVisible)
            {
                Overlay.refreshGolden(cGoldenCommon, cGoldenRare, cGoldenEpic, cGoldenLegendary);
            }
        }

        public void refreshDustValues(int craftingCost, int dustValue)
        {
            this.updateLabel(this.lCraftingCost, "Crafting Costs: " + craftingCost);
            this.updateLabel(this.lDustingValue, "Dusting Value: " + dustValue);
        }
        
        public void refreshPackValues(int totalCards, int totalPacks)
        {
            this.updateLabel(this.lTotalCards, "Total Cards: " + totalCards);
            this.updateLabel(this.lTotalPacks, "Total Packs: " + totalPacks);
        }

        private void updateLabel(Label label, string value)
        {
            label.Content = value;
        }
        private void updateLabel(Label label, int value)
        {
            label.Content = value;
        }
        private void cbOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Hidden;
        }

        private void cbOverlay_Checked(object sender, RoutedEventArgs e)
        {
            if(!Overlay.IsVisible)
            {
                Overlay.Show();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Overlay == null)
            {
                return;
            }
            Overlay.Close();
            Overlay = null;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to reset all card counters? \nThis process can not be undone!", "Warining!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(result == MessageBoxResult.Yes)
            {
                cardPackOpeningCounter.resetCount();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open Card Count File";
            dialog.Filter = "Card Count File | *.hcc";
            dialog.InitialDirectory = @"C:\";
            if (dialog.ShowDialog() == true)
            {
                string filename = dialog.FileName;
                cardPackOpeningCounter.loadFile(filename);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDia = new SaveFileDialog();
            saveFileDia.Filter = "Card Count File | *.hcc";
            saveFileDia.Title = "Save Card Count";
            saveFileDia.FileName = "HS Card Count - " + DateTime.Now.ToString("dd-MM-yyyy");
            List<string> lines = cardPackOpeningCounter.saveCounts();
            if (saveFileDia.ShowDialog() == true)
            {
                if (saveFileDia.FileName != "")
                {
                    File.WriteAllLines(saveFileDia.FileName, lines);
                    lines.Clear();
                }
            }
        }

        private void addCardButton_Click(object sender, RoutedEventArgs e)
        {
            object[] informations = getInformations(sender);
            cardPackOpeningCounter.addCardBasedOnRarity((RARITY)informations[0], (int)informations[1]);
        }

        private void removeCardButton_Click(object sender, RoutedEventArgs e)
        {
            object[] informations = getInformations(sender);
            cardPackOpeningCounter.removeCardBasedOnRarity((RARITY)informations[0], (int)informations[1]);
        }

        private object[] getInformations(object sender)
        {
            FrameworkElement button = (FrameworkElement)sender;
            FrameworkElement grid = (FrameworkElement)button.Parent;

            int premium = grid.Name.Contains("Golden") ? 1 : 0;
            RARITY rarity = RARITY.FREE;

            if (grid.Name.Contains("Common"))
            {
                rarity = RARITY.COMMON;
            }
            else if(grid.Name.Contains("Rare"))
            {
                rarity = RARITY.RARE;
            }
            else if(grid.Name.Contains("Epic"))
            {
                rarity = RARITY.EPIC;
            }
            else if(grid.Name.Contains("Legendary"))
            {
                rarity = RARITY.LEGENDARY;
            }

            return new object[] { rarity, premium };
        }
    }
}
