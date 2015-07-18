using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using POEApi.Model;
using Procurement.Controls;
using Procurement.Utility;
using System.Windows.Input;
using System.Globalization;

namespace Procurement.ViewModel
{
    public class LinkPath
    {
        public Image image;
        public int row { get; set; }
        public int col { get; set; }
    }


    public class ItemDisplayViewModel
    {
        public static ItemHover ItemHover = new ItemHover();
        public static ItemDisplayDivinationCard ItemHoverDivinationCard=new ItemDisplayDivinationCard();

        public struct wDPS
        {
            public string dps;
            public string p_dps;
            public string e_dps;
        }

        public Item Item { get; set; }

        public bool HasSocket
        {
            get
            {
                var gear = Item as Gear;

                return gear != null && gear.Sockets.Count > 0;
            }
        }
        
        public ItemDisplayViewModel(Item item)
        {
            this.Item = item;
        }

        public Image getImage()
        {
            var img = new Image
            {
                Source = ApplicationState.BitmapCache[Item.IconURL],
                Stretch = Stretch.None
            };
            
            CreateItemPopup(img, Item);

            return img;
        }

        public UIElement GetSocket()
        {
            var gear = (Gear) Item; 

            Grid masterpiece = new Grid();

            masterpiece.HorizontalAlignment = HorizontalAlignment.Center;
            masterpiece.VerticalAlignment = VerticalAlignment.Center;

            List<Tuple<int, int>> myLittleDesign = getSocketTree(gear.W, gear.H).ToList();
            int rows = myLittleDesign.Max(t => t.Item2) + 1;
            int columns = myLittleDesign.Max(t => t.Item1) + 1;

            for (int i = 0; i < rows; i++)
                masterpiece.RowDefinitions.Add(new RowDefinition() { Height = i % 2 == 0 ? GridLength.Auto : new GridLength(0) });

            for (int i = 0; i < columns; i++)
                masterpiece.ColumnDefinitions.Add(new ColumnDefinition() { Width = i % 2 == 0 ? GridLength.Auto : new GridLength(0)  });

            //masterpiece.ShowGridLines = true;


            IEnumerator<Tuple<int, int>> sockets = getEveryOther(myLittleDesign, 0).GetEnumerator();
            IEnumerator<Tuple<int, int>> links = getEveryOther(myLittleDesign, 1).GetEnumerator();
            int currentGroup = -1;
            for (int i = 0; i < gear.Sockets.Count; i++)
			{
                Socket socket = gear.Sockets[i];
			
                sockets.MoveNext();
                var currentSocketPosition = sockets.Current;

                if (i > 0)
                {
                    links.MoveNext();
                    var link = links.Current;

                    if (currentGroup == socket.Group)
                    {
                        Image img = getLink(currentSocketPosition, link);
                        img.SetValue(Grid.RowProperty, link.Item2);
                        img.SetValue(Grid.ColumnProperty, link.Item1);
                        img.IsHitTestVisible = false;
                        masterpiece.Children.Add(img);
                    }
                }

                if (!isSocketed(currentSocketPosition, socket, i, gear))
                {
                    Image img = GetSocket(socket, string.Empty);
                    img.SetValue(Grid.RowProperty, currentSocketPosition.Item2);
                    img.SetValue(Grid.ColumnProperty, currentSocketPosition.Item1);
                    img.IsHitTestVisible = false;
                    masterpiece.Children.Add(img);
                }
                else
                {
                    string suffix = "-socketed";
                    Gem g = gear.SocketedItems.Find(si => si.Socket == i && (socket.Attribute == si.Color || socket.Attribute == "G" || si.Color == "G"));
                    if (g.Color == "G")
                        suffix += "-white";
                    Image img = GetSocket(socket, suffix);
                    img.SetValue(Grid.RowProperty, currentSocketPosition.Item2);
                    img.SetValue(Grid.ColumnProperty, currentSocketPosition.Item1);
                    CreateItemPopup(img, getSocketItemAt(currentSocketPosition, socket, i, gear));
                    masterpiece.Children.Add(img);
                }

                currentGroup = socket.Group;
            }

            return masterpiece;
        }

        private List<Tuple<int, int>> getEveryOther(List<Tuple<int, int>> myLittleDesign, int start)
        {
            List<Tuple<int, int>> ret = new List<Tuple<int, int>>();
            for (int i = start; i < myLittleDesign.Count; i = i + 2)
                ret.Add(myLittleDesign[i]);

            return ret;
        }

        private Image getLink(Tuple<int, int> currentSocket, Tuple<int, int> currentLink)
        {
            var img = new Image();
            const string linkFormat = "pack://application:,,,/Images/Sockets/{0}.png";
            string link;
            if (currentSocket.Item1 != currentLink.Item1)
            {
                link = "link-horizontal";
                img.SetValue(Grid.ColumnSpanProperty, 3);
                img.Margin = new Thickness(-19, 0, 0, 0);
                img.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                link = "link-vertical";
                img.SetValue(Grid.RowSpanProperty, 3);
                img.Margin = new Thickness(0, -20, 0, 0);
                img.VerticalAlignment = VerticalAlignment.Top;
            }

            var url = string.Format(linkFormat, link);

            img.SetValue(Panel.ZIndexProperty, 1);
            img.Stretch = Stretch.None;
            img.Source = ApplicationState.BitmapCache.GetByLocalUrl(url);

            return img;
        }

        private bool isSocketed(Tuple<int, int> nextAvail, Socket socket, int socketIndex, Gear item)
        {
            if (item.SocketedItems == null || item.SocketedItems.Count == 0)
                return false;

            return item.SocketedItems.Exists(i => i.Socket == socketIndex && (socket.Attribute == i.Color || socket.Attribute == "G" || i.Color == "G"));
        }

        private Gem getSocketItemAt(Tuple<int, int> nextAvail, Socket socket, int socketIndex, Gear item)
        {
            return item.SocketedItems.First(i => i.Socket == socketIndex && (socket.Attribute == i.Color || socket.Attribute == "G" || i.Color == "G"));
        }

        private Image GetSocket(Socket socket, string suffix)
        {
            const string socketFormat = "pack://application:,,,/Images/Sockets/{0}.png";
            string color;
            switch (socket.Attribute)
            {
                case "D":
                    color = "green" + suffix;
                    break;
                case "I":
                    color = "blue" + suffix;
                    break;
                case "S":
                    color = "red" + suffix;
                    break;
                default:
                    color = "white" + suffix;
                    break;
            }

            var url = string.Format(socketFormat, color);

            var img = new AlphaHittestedImage
            {
                Stretch = Stretch.None,
                Source = ApplicationState.BitmapCache.GetByLocalUrl(url)
            };

            return img;
        }

        private void CreateItemPopup(UIElement target, Item item)
        {
            var popup = new Popup
            {
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade,
                StaysOpen = true,
                PlacementTarget = target
            };

            // Use a grid as child to be able to disconnect the hover control properly. 
            var grid = new Grid();
            popup.Child = grid;

            target.MouseEnter += (o, e) =>
            {
                //user press SHIFT while moving mouse
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    wDPS wdps = getWeaponDPS(item);
                    Item dps_item = (Item)item.Clone();
                    dps_item.Name = "DPS@20q : " + wdps.dps;
                    dps_item.TypeLine = "pDPS : "+ wdps.p_dps + "; eDPS : "+wdps.e_dps;
                    
                    ItemHover.DataContext = ItemHoverViewModelFactory.Create(dps_item);
                    grid.Children.Add(ItemHover);
                    popup.IsOpen = true;
                }
                else
                {
                    //TODO: more care check for DivinationCard type
                    if (item.ArtFilename != null)
                    {
                        ItemHoverDivinationCard = new ItemDisplayDivinationCard();
                        ItemHoverDivinationCard.DataContext = ItemHoverViewModelFactory.Create(item);
                        grid.Children.Add(ItemHoverDivinationCard);
                    }
                    else
                    {
                        ItemHover.DataContext = ItemHoverViewModelFactory.Create(item);
                        grid.Children.Add(ItemHover);
                    }

                    //allow keyboard input for selected item
                    target.Focusable = true;
                    target.Focus();

                    popup.IsOpen = true;
                }
            };

            target.MouseLeave += (o, e) =>
            {
                popup.IsOpen = false;
                grid.Children.Clear();
            };

            target.KeyDown += (o, e) =>
            {
                if (e.Key == System.Windows.Input.Key.C && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    //user press CTRL+C
                    string text_item = ItemToText(item);
                    if (text_item != "")
                    {
                        Clipboard.SetText(text_item, TextDataFormat.UnicodeText);
                    }
                }
            };
        }

        public string ItemToText(Item item)
        {
            string result_text = "";
            
            try
            {
                Gear item_gear = null;

                if (item.ItemType == ItemType.Gear)
                {
                    item_gear = item as Gear;
                    result_text += AddTextNewLine("Rarity: " + GetItemRarity(item));
                }

                if (item.Name.Length > 0) result_text += AddTextNewLine(item.Name);//name first line
                result_text += AddTextNewLine(item.TypeLine);//name second line

                //Property block
                if (item.Properties != null && item.Properties.Count > 0)
                {
                    result_text += AddTextNewLine("--------");
                    
                    foreach (Property curr_prop in item.Properties)
                    {
                        if (curr_prop.Values.Count > 0)
                        {
                            result_text += AddTextNewLine(curr_prop.Name + ": " + curr_prop.Values[0].Item1);
                        }
                        else
                        {
                            result_text += AddTextNewLine(curr_prop.Name);//item main type, e.g. Bow
                        }
                    }
                }

                //Requirements block
                if (item_gear != null)
                {
                    if (item_gear.Requirements != null && item_gear.Requirements.Count > 0)
                    {
                        result_text += AddTextNewLine("--------");
                        result_text += AddTextNewLine("Requirements:");

                        foreach (Requirement curr_req in item_gear.Requirements)
                        {
                            result_text += AddTextNewLine(curr_req.Name + ": " + curr_req.Value);
                        }
                    }
                }

                //Sockets block
                if (item_gear != null)
                {
                    if (item_gear.Sockets != null && item_gear.Sockets.Count > 0)
                    {
                        string socket_text = "";
                        for (int i=0;i<item_gear.Sockets.Count;i++)
                        {
                            Socket curr_socket = item_gear.Sockets[i];

                            if (i!=item_gear.Sockets.Count-1 && curr_socket.Group == item_gear.Sockets[i + 1].Group)
                            {
                                //current socket and next socket are linked
                                socket_text += curr_socket.Attribute + "-";
                            }
                            else
                            {
                                //not linked or last socket
                                socket_text += curr_socket.Attribute + " ";
                            }
                        }
                        
                        socket_text = socket_text.Trim();

                        //make replace for colors D(ex)->G(reen), Str->Red, Int->Blue
                        socket_text = socket_text.Replace("D", "G").Replace("S", "R").Replace("I", "B");

                        result_text += AddTextNewLine("--------");
                        result_text += AddTextNewLine("Sockets: " + socket_text);
                    }
                }

                //Itemlvl block

                //Implicit mods block
                if (item_gear != null)
                {
                    if (item_gear.Implicitmods != null && item_gear.Implicitmods.Count > 0)
                    {
                        result_text += AddTextNewLine("--------");

                        foreach (string curr_impl_mod in item_gear.Implicitmods)
                        {
                            result_text += AddTextNewLine(curr_impl_mod);
                        }
                    }
                }

                //Explicit mods block
                if (item.Explicitmods != null && item.Explicitmods.Count > 0)
                {
                    result_text += AddTextNewLine("--------");

                    foreach (string curr_expl_mod in item.Explicitmods)
                    {
                        result_text += AddTextNewLine(curr_expl_mod);
                    }
                }

                //Crafted mods at last of mods block
                if (item.CraftedMods != null && item.CraftedMods.Count > 0)
                {
                    foreach (string curr_crafted_mod in item.CraftedMods)
                    {
                        result_text += AddTextNewLine(curr_crafted_mod);
                    }
                }

                //Flav text
                if (item_gear != null)
                {
                    if (item_gear.FlavourText != null && item_gear.FlavourText.Count > 0)
                    {
                        result_text += AddTextNewLine("--------");

                        foreach (string curr_flav_text in item_gear.FlavourText)
                        {
                            result_text += curr_flav_text;
                        }
                    }
                }

                //Corrupted block
                if (item.Corrupted)
                {
                    result_text += AddTextNewLine("--------");
                    result_text += AddTextNewLine("Corrupted");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Procurement.MessagesRes.CanTConvertItemToTextCopyError + ex.Message, Procurement.MessagesRes.ItemToTextError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }

            return result_text;
        }

        public string AddTextNewLine(string text)
        {
            return text + Environment.NewLine;
        }

        public string GetItemRarity(Item item)
        {
            Gear gear = item as Gear;
            Nullable<Rarity> r = null;

            if (gear != null)
                r = gear.Rarity;

            Map map = item as Map;
            if (map != null)
                r = map.Rarity;

            if (r != null)
            {
                return r.ToString();
            }

            return "Normal";
        }


        public IEnumerable<Tuple<int, int>> getSocketTree(int W, int H)
        {
            int maxWidth = W;
            if (W > 1)
                maxWidth = 3;

            int maxHeight = H;
            if (H == 2)
                maxHeight = 3;

            if (H == 3)
                maxHeight = 5;

            if (H == 4)
                maxHeight = 6;

            List<Tuple<int, int>> possible = new List<Tuple<int, int>>();

            possible.Add(new Tuple<int, int>(0, 0)); //Top Left
            possible.Add(new Tuple<int, int>(1, 0)); //Link
            possible.Add(new Tuple<int, int>(2, 0)); //Top Right
            possible.Add(new Tuple<int, int>(2, 1)); //Link
            if (W == 1)
                possible.Add(new Tuple<int, int>(0, 1)); //Not valid for everything but single column rapiers / wands meh.
 
            possible.Add(new Tuple<int, int>(2, 2)); //Middle Right
            possible.Add(new Tuple<int, int>(1, 2)); //Link
            possible.Add(new Tuple<int, int>(0, 2)); //Middle Left
            possible.Add(new Tuple<int, int>(0, 3)); //Link
            possible.Add(new Tuple<int, int>(0, 4)); //Bottom Left
            possible.Add(new Tuple<int, int>(1, 4)); //Link
            possible.Add(new Tuple<int, int>(2, 4)); //Bottom Right

            foreach (var turple in possible)
                if (turple.Item1 < maxWidth && turple.Item2 < maxHeight)
                    yield return turple;
        }

        private wDPS getWeaponDPS(Item item)
        {
            wDPS str_dps = new wDPS();
            try
            {
                Gear gear = item as Gear;
                if (gear != null)
                {
                    //check that item is weapon GearType
                    if (gear.GearType == GearType.Axe || gear.GearType == GearType.Bow || gear.GearType == GearType.Claw ||
                        gear.GearType == GearType.Dagger || gear.GearType == GearType.Mace || gear.GearType == GearType.Sceptre ||
                        gear.GearType == GearType.Staff || gear.GearType == GearType.Sword || gear.GearType == GearType.Wand)
                    {
                        int dmin = 0, dmax = 0, amin = 0, amax = 0, fmin = 0, fmax = 0, cmin = 0, cmax = 0, lmin = 0, lmax = 0;
                        int percent_dmg = 0;
                        int elem_dmg = 0;
                        float atk_speed = 0, base_min = 0, base_max = 0, p_min = 0, p_max = 0,
                            dps = 0, p_dps = 0, e_dps = 0;

                        foreach (Property curr_prop in item.Properties)
                        {
                            if (curr_prop.Name == Procurement.MessagesRes.wDPSPhysDamageProperty)
                            {
                                string[] arr_phys_dps = curr_prop.Values[0].Item1.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                dmin = int.Parse(arr_phys_dps[0]);
                                dmax = int.Parse(arr_phys_dps[1]);
                            }
                            else if (curr_prop.Name.Contains(Procurement.MessagesRes.wDPSAttacksPerSecond))
                            {
                                atk_speed = float.Parse(curr_prop.Values[0].Item1, new CultureInfo("en-US"));
                            }
                        }

                        if (item.Explicitmods != null)
                        {
                            foreach (string curr_mod in item.Explicitmods)
                            {
                                if (curr_mod.Contains(Procurement.MessagesRes.wDPSIncreasedPhysicalDamage))
                                {
                                    int percent_end_pos = 0;
                                    percent_end_pos = curr_mod.IndexOf("%");
                                    percent_dmg = int.Parse(curr_mod.Substring(0, percent_end_pos));
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSPhysicalDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSPhysicalDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    amin = int.Parse(arr_mod[0]);
                                    amax = int.Parse(arr_mod[1]);
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSFireDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSFireDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    fmin = int.Parse(arr_mod[0]);
                                    fmax = int.Parse(arr_mod[1]);
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSColdDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSColdDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    cmin = int.Parse(arr_mod[0]);
                                    cmax = int.Parse(arr_mod[1]);
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSLightningDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSLightningDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    lmin = int.Parse(arr_mod[0]);
                                    lmax = int.Parse(arr_mod[1]);
                                }
                            }
                        }

                        if (item.CraftedMods != null)
                        {
                            foreach (string curr_mod in item.CraftedMods)
                            {
                                if (curr_mod.Contains(Procurement.MessagesRes.wDPSIncreasedPhysicalDamage))
                                {
                                    int percent_end_pos = 0;
                                    percent_end_pos = curr_mod.IndexOf("%");
                                    percent_dmg = int.Parse(curr_mod.Substring(0, percent_end_pos));
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSPhysicalDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1; ;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSPhysicalDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    amin = int.Parse(arr_mod[0]);
                                    amax = int.Parse(arr_mod[1]);
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSFireDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1; ;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSFireDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    fmin = int.Parse(arr_mod[0]);
                                    fmax = int.Parse(arr_mod[1]);
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSColdDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSColdDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    cmin = int.Parse(arr_mod[0]);
                                    cmax = int.Parse(arr_mod[1]);
                                }
                                else if (curr_mod.Contains(Procurement.MessagesRes.wDPSAdds) && curr_mod.Contains(Procurement.MessagesRes.wDPSLightningDamage))
                                {
                                    int numbers_start_pos = 0, numbers_end_pos = 0;
                                    numbers_start_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSAdds) + Procurement.MessagesRes.wDPSAdds.Length + 1;
                                    numbers_end_pos = curr_mod.IndexOf(Procurement.MessagesRes.wDPSLightningDamage);
                                    string[] arr_mod = curr_mod.Substring(numbers_start_pos, numbers_end_pos - numbers_start_pos).Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    lmin = int.Parse(arr_mod[0]);
                                    lmax = int.Parse(arr_mod[1]);
                                }
                            }
                        }

                        elem_dmg = fmin + fmax + cmin + cmax + lmin + lmax;
                        base_min = dmin / (1f + (percent_dmg + item.Quality) / 100f) - amin;
                        base_max = dmax / (1f + (percent_dmg + item.Quality) / 100f) - amax;

                        p_min = (base_min + amin) * (1f + percent_dmg / 100f + 0.2f);
                        p_max = (base_max + amax) * (1f + percent_dmg / 100f + 0.2f);

                        p_dps = (p_min + p_max) / 2 * atk_speed;
                        e_dps = elem_dmg / 2 * atk_speed;
                        dps = p_dps + e_dps;

                        str_dps.dps = dps.ToString("f");
                        str_dps.e_dps = e_dps.ToString("f");
                        str_dps.p_dps = p_dps.ToString("f");
                        return str_dps;
                    }
                    else
                    {
                        str_dps.dps = "Not a weapon!";
                        return str_dps;
                    }
                }
            }
            catch (Exception ex)
            {
                str_dps.dps = "DPS calculation error";
                str_dps.p_dps = ex.Message;
                return str_dps;
            }

            return str_dps;
        }
    }
}
