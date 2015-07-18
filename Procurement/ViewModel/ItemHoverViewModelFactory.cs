using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POEApi.Model;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Procurement.ViewModel
{
    class ItemHoverViewModelFactory
    {
        internal static ItemHoverViewModel Create(Item item)
        {
            
            Gear gear = item as Gear;
            Nullable<Rarity> r = null;

            if (gear != null)
                r = gear.Rarity;

            Map map = item as Map;
            if (map != null)
                r = map.Rarity;
            
            if (gear!=null && gear.GearType == GearType.DivinationCard) 
                return new DivinationCardItemHoverViewModel(item);

            if (r != null)
            {
                switch (r)
                {
                    case Rarity.Unique:
                        return new UniqueGearItemHoverViewModel(item);
                    case Rarity.Rare:
                        return new RareGearItemHoverViewModel(item); 
                    case Rarity.Magic:
                        return new MagicGearItemHoverViewModel(item);
                    case Rarity.Normal:
                        return new NormalGearItemHoverViewModel(item); 
                }
            }

            if (item is Gem)
                return new GemItemHoverViewModel(item);

            if (item is Currency)
                return new CurrencyItemHoverViewModel(item);

            return new ItemHoverViewModel(item);
        }
    }

    public class UniqueGearItemHoverViewModel : ItemHoverViewModel
    {
        public UniqueGearItemHoverViewModel(Item item)
            : base(item)
        { }
    }

    public class RareGearItemHoverViewModel : ItemHoverViewModel
    {
        public RareGearItemHoverViewModel(Item item)
            : base(item)
        { }
    }

    public class MagicGearItemHoverViewModel : ItemHoverViewModel
    {
        public MagicGearItemHoverViewModel(Item item)
            : base(item)
        { }
    }

    public class NormalGearItemHoverViewModel : ItemHoverViewModel
    {
        public NormalGearItemHoverViewModel(Item item)
            : base(item)
        { }
    }

    public class GemItemHoverViewModel : ItemHoverViewModel
    {
        public GemItemHoverViewModel(Item item)
            : base(item)
        { }
    }

    public class CurrencyItemHoverViewModel : ItemHoverViewModel
    {
        public CurrencyItemHoverViewModel(Item item)
            : base(item)
        { }
    }

    public class DivinationCardItemHoverViewModel : ItemHoverViewModel
    {
        internal string FlavText;
        const string DivinationCardURLFormat = "https://{0}/image/{1}.png";

        public DivinationCardItemHoverViewModel(Item item)
            : base(item)
        {
            Gear item_gear= item as Gear;

            if (item_gear != null)
            {
                string flav_text = "";
                foreach (string text_line in item_gear.FlavourText)
                {
                    flav_text += text_line;
                }
                this.FlavText = flav_text; ;
            }
            else
            {
                this.FlavText = "";
            }
        }

        public Image getImageDivinationCard(Item item)
        {
            string ArtFilenameHost = (new Uri(item.IconURL)).Host;
            string ItemArtFilenameURL = string.Format(DivinationCardURLFormat, ArtFilenameHost, item.ArtFilename);
            
            Image img = new Image();
            img.Source = ApplicationState.BitmapCache[ItemArtFilenameURL];
            img.Stretch = Stretch.None;

            CreateItemPopup(img, Item);

            return img;
        }

        public string getCardName(Item item)
        {
            string res = "";
            if (item.Explicitmods.Count > 0)
            {
                int name_start = 0, name_end = 0;
                name_start = item.Explicitmods[0].IndexOf("{")+1;
                name_end = item.Explicitmods[0].IndexOf("}");

                if (name_start > 0 && name_end > 0)
                {
                    res = item.Explicitmods[0].Substring(name_start, name_end - name_start);
                }
                else
                {
                    return "";
                }
            }

            return res;
        }
    }
}
