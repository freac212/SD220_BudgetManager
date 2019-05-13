using System.Web;
using System.Web.Mvc;

namespace SD220_Deliverable_1_DGrouette
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
