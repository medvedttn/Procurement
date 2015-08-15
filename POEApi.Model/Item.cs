using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace POEApi.Model
{
    public enum ItemType : int
    {
        UnSet,
        Gear,
        Gem,
        Currency,
    }

    public enum Rarity : int
    {
        Normal,
        Magic,
        Rare,
        Unique
    }

    public abstract class Item : ICloneable
    {
        public bool Verified { get; private set; }
        public bool Identified { get; private set; }
        public int W { get; private set; }
        public int H { get; private set; }
        public string IconURL { get; private set; }
        public string League { get; private set; }
        public string Name { get; set; }
        public string TypeLine { get; set; }
        public string DescrText { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string InventoryId { get; set; }
        public string SecDescrText { get; private set; }
        public List<string> Explicitmods { get; set; }
        public ItemType ItemType { get; set; }
        public List<Property> Properties { get; set; }
        public bool IsQuality { get; private set; }
        public int Quality { get; private set; }
        public string UniqueIDHash { get; set; }
        public bool Corrupted { get; private set; }
        public List<string> Microtransactions { get; set; }

        public List<string> CraftedMods { get; set; }

        public int TradeX { get; set; }
        public int TradeY { get; set; }
        public string TradeInventoryId { get; set; }
        public string Character { get; set; }

        public bool IsSelectedManually { get; set; }
        public string ArtFilename { get; private set; }

        protected Item(JSONProxy.Item item)
        {
            this.Verified = item.Verified;
            this.Identified = item.Identified;
            this.W = item.W;
            this.H = item.H;
            this.IconURL = getIconUrl(item.Icon);
            this.League = item.League;
            this.Name = item.Name;
            this.TypeLine = item.TypeLine;
            this.DescrText = item.DescrText;
            this.X = item.X;
            this.Y = item.Y;
            this.InventoryId = item.InventoryId;
            this.SecDescrText = item.SecDescrText;
            this.Explicitmods = item.ExplicitMods;
            this.ItemType = Model.ItemType.UnSet;
            this.CraftedMods = item.CraftedMods;

            if (item.Properties != null)
            {
                this.Properties = item.Properties.Select(p => new Property(p)).ToList();
                
                {
                    if (this.Properties.Any(p => p.Name == POEApi.Model.ServerTypeRes.QualityText))
                    {
                        this.IsQuality = true;
                        this.Quality = ProxyMapper.GetQuality(item.Properties);
                    }
                }
            }

            this.Corrupted = item.Corrupted;
            this.Microtransactions = item.CosmeticMods == null ? new List<string>() : item.CosmeticMods;

            this.TradeX = this.X;
            this.TradeY = this.Y;
            this.TradeInventoryId = this.InventoryId;
            this.Character = string.Empty;

            this.IsSelectedManually = false;
            this.ArtFilename = item.ArtFilename;

            //TODO : get itemlvl from JSON (currently not returned by JSON)
        }

        private string getIconUrl(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                return url;

            return "http://webcdn.pathofexile.com" + url;
        }

        protected abstract string getConcreteHash();

        protected string getHash()
        {
            string str_hash_data = this.IconURL + this.League + this.Name + this.TypeLine + this.DescrText;
            if (this.Explicitmods != null) str_hash_data += string.Join(string.Empty, this.Explicitmods.ToArray());
            if (this.Properties != null) str_hash_data += string.Join(string.Empty, this.Properties.Select(p => string.Concat(p.DisplayMode, p.Name, string.Join(string.Empty, p.Values.Select(t => string.Concat(t.Item1, t.Item2)).ToArray()))).ToArray());
            str_hash_data += getConcreteHash();
            
            return getHashSHA1(str_hash_data);
        }

        protected string getHashSHA1(string hash_data)
        {
            string sha1_result = "";
            SHA1Managed sha1enc = new SHA1Managed();
            byte[] sha1=sha1enc.ComputeHash(Encoding.UTF8.GetBytes(hash_data));
            sha1_result = string.Join("", sha1.Select(b => b.ToString("X2")).ToArray());
            return sha1_result;
        }

        protected Rarity getRarity(JSONProxy.Item item)
        {
            if (item.frameType <= 3)
                return (Rarity)item.frameType;

            return Rarity.Normal;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
