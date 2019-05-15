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
    internal class CategoryAuthorizationAttribute : ActionFilterAttribute // Make sure not using MVC namespace for actionFilters, atleast in Web APIS
    {
        private ApplicationUserManager UserManager => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();


        // Check if the user is the owner of the Household.
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var userId = actionContext.RequestContext.Principal.Identity.GetUserId(); // Better because not bringing entire user
            var catId = actionContext.RequestContext.RouteData.Values["id"].ToString(); // Id is of the category

            if (!int.TryParse(catId, out int categoryId))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            // If the categories household's creator is the users id, continue
            var userIsCreator = DbContext.Categories.Any(p => p.Household.CreatorId == userId && p.Id == categoryId);

            if (!userIsCreator)
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            // Check if creator of household obeject is the user trying to access this object.
        }
    }
}