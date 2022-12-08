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
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class Overlay : MetroWindow
    {
        public Overlay(int cCommon, int cRare, int cEpic, int cLegendary,
            int cGoldenCommon, int cGoldenRare, int cGoldenEpic, int cGoldenLegendary)
        {
            InitializeComponent();
            refresh(cCommon, cRare, cEpic, cLegendary);
            refreshGolden(cGoldenCommon, cGoldenRare, cGoldenEpic, cGoldenLegendary);
        }

        public void refresh(int cCommon, int cRare, int cEpic, int cLegendary)
        {
            this.updateLabel(this.lCountCommon, cCommon);
            this.updateLabel(this.lCountRare, cRare);
            this.updateLabel(this.lCountEpic, cEpic);
            this.updateLabel(this.lCountLegendary, cLegendary);
        }

        public void refreshGolden(int cGoldenCommon, int cGoldenRare, int cGoldenEpic, int cGoldenLegendary)
        {
            this.updateLabel(this.lCountGoldenCommon, cGoldenCommon);
            this.updateLabel(this.lCountGoldenRare, cGoldenRare);
            this.updateLabel(this.lCountGoldenEpic, cGoldenEpic);
            this.updateLabel(this.lCountGoldenLegendary, cGoldenLegendary);
        }

        private void updateLabel(Label label, string value)
        {
            label.Content = value;
        }
        private void updateLabel(Label label, int value)
        {
            label.Content = value;
        }
    }
}
