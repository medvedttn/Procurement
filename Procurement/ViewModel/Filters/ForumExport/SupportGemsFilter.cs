using POEApi.Model;
using System.Linq;

namespace Procurement.ViewModel.Filters.ForumExport
{
    internal class SupportGemsFilter : IFilter
    {
        public FilterGroup Group
        {
            get { return FilterGroup.Gems; }
        }

        public SupportGemsFilter()
        { }

        public bool CanFormCategory
        {
            get { return true; }
        }

        public string Keyword
        {
            get
            {
                if (Procurement.ViewModel.LoginWindowViewModel.ServerType == "Garena (RU)")
                {
                    return "Камни поддержки";
                }
                else
                {
                    return "Support Gems";
                }
            }
        }

        public string Help
        {
            get { return "Gems that modify skill gems they are linked to"; }
        }

        public bool Applicable(Item item)
        {
            Gear gear = item as Gear;
            if (gear != null && gear.SocketedItems.Any(x => Applicable(x)))
                return true; 

            Gem gem = item as Gem;
            if (gem == null)
                return false;

            if (Procurement.ViewModel.LoginWindowViewModel.ServerType == "Garena (RU)")
            {
                return item.Properties[0].Name.Contains("Поддержка");
            }
            else
            {
                return item.Properties[0].Name.Contains("Support");
            }
        }
    }
}
