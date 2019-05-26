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
using SD220_Deliverable_1_DGrouette.Models.Helpers;

namespace SD220_Deliverable_1_DGrouette.Models.Filters
{


    internal class _HouseholdUserAuthorizationAttribute : ActionFilterAttribute // Make sure not using MVC namespace for actionFilters, atleast in Web APIS
    {
        private ApplicationUserManager UserManager => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();


        public Type IdType { get; set; }

        public _HouseholdUserAuthorizationAttribute(Type idType = null)
        {
            IdType = idType;
        }

        // Check if the user is the owner of the Household or the creator of the object
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var userId = actionContext.RequestContext.Principal.Identity.GetUserId();
            var id = actionContext.RequestContext.RouteData.Values["id"].ToString();

            if (!int.TryParse(id, out int objectId))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            ITypeFilterHelper domainClass = (ITypeFilterHelper)Activator.CreateInstance(IdType);

            if (!domainClass.Execute(objectId, userId))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}