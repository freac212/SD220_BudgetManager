using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Web;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http;
using System.Net.Http;
using System.Net;

namespace SD220_Deliverable_1_DGrouette.Models.Filters
{
    internal class CreatorAuthorizationAttribute : ActionFilterAttribute // Make sure not using MVC namespace for actionFilters, atleast in Web APIS
    {
        private ApplicationUserManager UserManager => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();


        // Check if the user is the owner of the Household.
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var userId = actionContext.RequestContext.Principal.Identity.GetUserId(); // Better because not bringing entire user
            var id = actionContext.RequestContext.RouteData.Values["id"].ToString();

            if (!int.TryParse(id, out int resultUserId))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            var householdExists = DbContext.Households.Any(p => p.Id == resultUserId && p.CreatorId == userId);

            if (!householdExists)
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            // Check if creator of household obeject is the user trying to access this object.
        }
    }
}