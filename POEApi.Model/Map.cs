using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
namespace POEApi.Model
{
    public class Map : Item
    {
        public Rarity Rarity { get; private set; }
        public int MapLevel { get; private set; }
        public int MapQuantity { get; private set; }

        internal Map(JSONProxy.Item item) : base(item)
        {
            this.ItemType = Model.ItemType.Gear;
            this.Properties = ProxyMapper.GetProperties(item.Properties);
            this.Rarity = getRarity(item);
            //changed to Map Tier value
            this.MapLevel = int.Parse(Properties.Find(p => p.Name == POEApi.Model.ServerTypeRes.MapText).Values[0].Item1);

            this.UniqueIDHash = base.getHash();
        }

        protected override string getConcreteHash()
        {
            string str_hash_data = Rarity.ToString() + MapLevel.ToString() + MapQuantity.ToString();
            
            return getHashSHA1(str_hash_data);
        }
    }
}
