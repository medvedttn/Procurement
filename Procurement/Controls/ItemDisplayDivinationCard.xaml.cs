using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using POEApi.Model;
using Procurement.ViewModel;
using POEApi.Infrastructure;
using Procurement.Utility;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace Procurement.Controls
{
    public partial class ItemDisplayDivinationCard : UserControl
    {
        private static List<Popup> annoyed = new List<Popup>();
        private Image itemImage=new Image();

        public ItemDisplayDivinationCard()
        {
            InitializeComponent();
            MainGridDivinationCard.DataContext = this;

            this.Loaded += new RoutedEventHandler(ItemDisplay_Loaded);
        }


        void ItemDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            DivinationCardItemHoverViewModel dcvm = this.DataContext as DivinationCardItemHoverViewModel;
            
            itemImage=dcvm.getImageDivinationCard(dcvm.Item);
            imgArtFilename.Source = itemImage.Source;
            txtFlavText.Text = dcvm.FlavText;
            txtStackSize.Text = dcvm.Properties[0].Values[0].Item1.ToString();
            txtTypeLine.Text = dcvm.TypeLine;
            txtItemName.Text = dcvm.getCardName(dcvm.Item);

            if (dcvm.ExplicitMods.Count > 0)
            {
                if (dcvm.ExplicitMods[0].Contains("magicitem")) txtItemName.Foreground = (SolidColorBrush)((new BrushConverter()).ConvertFromString("#FFB4B4FF"));
                if (dcvm.ExplicitMods[0].Contains("normalitem")) txtItemName.Foreground = Brushes.White;
                //TODO: update Item.Rarity according to these tags?
            }

            this.Height = 409;
            this.Width = 268;
            this.Loaded -= new RoutedEventHandler(ItemDisplay_Loaded);
        }
    }
}
