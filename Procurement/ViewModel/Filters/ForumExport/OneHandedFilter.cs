
namespace Procurement.ViewModel.Filters.ForumExport
{
    public class OneHandedFilter : XHandFilter
    {
        public OneHandedFilter()
            : base("One")
        { 
            if (Procurement.ViewModel.LoginWindowViewModel.ServerType == "Garena (RU)")
            {
                this.handed = "Одноручн";
            }
        }
    }
}
