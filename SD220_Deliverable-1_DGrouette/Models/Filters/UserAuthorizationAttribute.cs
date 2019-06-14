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
    interface ITypeFilterHelper
    {
        bool Execute(int id, string userId);
    }

    public class TransactionCreator : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            // Creator of transaction or is owner of household
            return DbContext.Transactions.Any(p => p.Id == id &&
            (p.CreatorId == userId || p.BankAccount.Household.CreatorId == userId));
        }
    }

    public class HouseholdCreator : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            return DbContext.Households.Any(p => p.Id == id && p.CreatorId == userId);
        }
    }

    public class CategoryCreator : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
           return DbContext.Categories.Any(p => p.Id == id && p.Household.CreatorId == userId);
        }
    }

    public class BankAccountCreator : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            return DbContext.BankAccounts.Any(p => p.Id == id && p.Household.CreatorId == userId);
        }
    }

    public class TransactionHouseMember : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            return DbContext.Transactions.Where(p => p.Id == id).Any(p => p.BankAccount.Household.Members.Any(i => i.Id == userId));
        }
    }

    public class HouseholdHouseMember : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            return DbContext.Households.Any(p => p.Id == id && p.Members.Any(user => user.Id == userId));
        }
    }

    public class CategoryHouseMember : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            return DbContext.Categories.Where(p => p.Id == id).Any(p => p.Household.Members.Any(i => i.Id == userId));
        }
    }

    public class BankAccountHouseMember : ITypeFilterHelper
    {
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public bool Execute(int id, string userId)
        {
            return DbContext.BankAccounts.Where(p => p.Id == id).Any(p => p.Household.Members.Any(i => i.Id == userId));
        }
    }


    internal class UserAuthorizationAttribute : ActionFilterAttribute
    {
        private ApplicationUserManager UserManager => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();

        public Type IdType { get; set; }

        public UserAuthorizationAttribute(Type idType = null)
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
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
    }
}
