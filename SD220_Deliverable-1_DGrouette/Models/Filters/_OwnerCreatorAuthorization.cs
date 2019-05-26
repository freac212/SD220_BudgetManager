using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SD220_Deliverable_1_DGrouette.Models.Filters
{
    internal class _OwnerCreatorAuthorization : ActionFilterAttribute
    {
        private ApplicationUserManager UserManager => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        HouseholdHelpers.ListTypes ListTypes { get; set; }
        public _OwnerCreatorAuthorization(HouseholdHelpers.ListTypes listTypes)
        {
            ListTypes = listTypes;
        }

        // Check if the user is the owner of the Household or the creator of the object
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var userId = actionContext.RequestContext.Principal.Identity.GetUserId();
            var id = actionContext.RequestContext.RouteData.Values["id"].ToString();

            if (!int.TryParse(id, out int objectId))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            switch (ListTypes)
            {
                case HouseholdHelpers.ListTypes.HouseHolds:
                    // Is the creator of the household
                    var householdExists = DbContext.Households.Any(p => p.Id == objectId && p.CreatorId == userId);
                    if (!householdExists)
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    break;

                case HouseholdHelpers.ListTypes.Transactions:
                    // is either the creator of the transaction or is the owner of the household
                    var transactionsExists = DbContext.Transactions.Any(p => p.Id == objectId && p.CreatorId == userId || p.BankAccount.Household.CreatorId == userId);
                    if (!transactionsExists)
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    break;

                case HouseholdHelpers.ListTypes.BankAccounts:
                    // Is the creator of the household
                    var bankaccountsExists = DbContext.BankAccounts.Any(p => p.Id == objectId && p.Household.CreatorId == userId);
                    if (!bankaccountsExists)
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    break;

                case HouseholdHelpers.ListTypes.Categories:
                    // Is the creator of the household
                    var categoriesExists = DbContext.Categories.Any(p => p.Id == objectId && p.Household.CreatorId == userId);
                    if (!categoriesExists)
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    break;

                default:
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    break;

            }
        }
    }
}