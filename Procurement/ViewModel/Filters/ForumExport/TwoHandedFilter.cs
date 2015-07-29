using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Procurement.ViewModel.Filters.ForumExport
{
    public class TwoHandedFilter : XHandFilter
    {
        public TwoHandedFilter()
            : base("Two")
        {
            if (Procurement.ViewModel.LoginWindowViewModel.ServerType == "Garena (RU)")
            {
                this.handed = "Двуручн";
            }
        }
    }
}
