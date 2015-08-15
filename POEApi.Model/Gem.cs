using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace POEApi.Model
{
    public class Gem : Item
    {
        public List<Requirement> Requirements { get; set; }
        public int Socket { get; set; }
        public string Color { get; set; }

        public int Level { get; set; }

        public Gem(JSONProxy.Item item) : base(item)
        {
            this.Properties = ProxyMapper.GetProperties(item.Properties);
            this.ItemType = Model.ItemType.Gem;

            this.Socket = item.Socket;
            this.Color = item.Color;
            this.Requirements = ProxyMapper.GetRequirements(item.Requirements);
            this.Level = getLevel();

            this.UniqueIDHash = base.getHash();
        }

        private int getLevel()
        {
            int level;
            var levelProperty = Properties.Find(p => p.Name == POEApi.Model.ServerTypeRes.LevelText).Values[0].Item1;
            levelProperty = levelProperty.Split(' ')[0]; //fixes "20 (MAX)"

            if (!int.TryParse(levelProperty, out level))
                return 1;
            
            return level;
        }

        protected override string getConcreteHash()
        {
            string str_hash_data = Quality.ToString() + Color + Socket.ToString();
            if (this.Requirements != null) str_hash_data += string.Join(string.Empty, this.Requirements.Select(r => string.Concat(r.Name, r.Value)).ToArray());
            
            return getHashSHA1(str_hash_data);
        }
    }
}
