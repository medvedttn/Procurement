using System.Security.Cryptography;
using System.Text;
using System.Linq;
namespace POEApi.Model
{
    public class Currency : Item
    {
        public OrbType Type { get; private set; }
        public double ChaosValue { get; private set; }
        public StackInfo StackInfo { get; private set; }

        public Currency(JSONProxy.Item item) : base(item)
        {
            this.Type = ProxyMapper.GetOrbType(item);
            this.ChaosValue = CurrencyHandler.GetChaosValue(this.Type);
            this.StackInfo = ProxyMapper.GetStackInfo(item.Properties);

            this.UniqueIDHash = base.getHash();
        }

        protected override string getConcreteHash()
        {
            string str_hash_data = this.IconURL + this.Name + this.TypeLine + this.DescrText + this.X + this.Y + this.InventoryId;
            
            return getHashSHA1(str_hash_data);
        }
    }
}
