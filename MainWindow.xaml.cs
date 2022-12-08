using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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

namespace HDT_CardPackOpeningCounter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        protected static Overlay Overlay;

        public MainWindow(int cCommon, int cRare, int cEpic, int cLegendary,
            int cGoldenCommon, int cGoldenRare, int cGoldenEpic, int cGoldenLegendary)
        {
            InitializeComponent();
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

        public void setTest(String text)
        {
            this.lCommon.Content = text;
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
    }
}
