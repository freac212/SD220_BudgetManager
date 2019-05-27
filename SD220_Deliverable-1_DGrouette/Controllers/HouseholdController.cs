using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SD220_Deliverable_1_DGrouette.Models;
using SD220_Deliverable_1_DGrouette.Models.Bindings;
using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Filters;
using SD220_Deliverable_1_DGrouette.Models.Helpers;
using SD220_Deliverable_1_DGrouette.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SD220_Deliverable_1_DGrouette.Controllers
{
    [RoutePrefix("api/household")]
    [Authorize]
    public class HouseholdController : ApiController
    {
        // Using the same DB instance
        private ApplicationUserManager UserManager => ControllerContext.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => ControllerContext.Request.GetOwinContext().Get<ApplicationDbContext>();
        //public RoleManager<IdentityRole> RoleManager => new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));

        // >Registered Users Can Create inf households
        // POST api/household/create
        [HttpPost]
        [Route("create")]
        public IHttpActionResult Create(HouseholdBindingModel householdBinding)
        {
            if (ModelState is null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UserManager.FindById(User.Identity.GetUserId());

            var household = new Household
            {
                DateCreated = DateTime.Now,
                DateUpdated = null,
                Name = householdBinding.Name,
                Description = householdBinding.Description,
                Creator = user,
                Members = new List<ApplicationUser>()
                {
                    user // adding the user so they show up in the user list on requests
                }
            };

            DbContext.Households.Add(household);
            DbContext.SaveChanges();

            var householdView = HouseholdHelpers.MapHouseholdToView(household);

            return Created(Url.Link(
                "GetHouseholdById",
                new { household.Id }),
                householdView
            );
        }

        // >The owner/ creator of a household should be able to edit the household information.
        // POST api/household/edit/2
        [HttpPost]
        [Route("edit/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdCreator))]
        public IHttpActionResult Edit(int? Id, HouseholdBindingModel householdBinding)
        {
            // Two strings so that you can edit only one string? or you have to edit both?
            // Asking here because I can use required tags on the bindingModel

            if (ModelState is null || !ModelState.IsValid) // ++Q : Turn into filter.
                return BadRequest(ModelState);

            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);

            if (household is null)
                return NotFound();

            household.DateUpdated = DateTime.Now;
            household.Name = householdBinding.Name;
            household.Description = householdBinding.Description;
            DbContext.SaveChanges();

            var householdView = HouseholdHelpers.MapHouseholdToView(household);

            return Ok(householdView);
        }

        // >The owner of a household should be able to invite users by e-mail to participate in their households. 
        // POST api/household/invite
        [HttpPost]
        [Route("invite/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdCreator))]
        public IHttpActionResult Invite(int? Id, InviteHouseholdBindingModel bindingModel)
        {
            // Invited user must be registered in the app
            // api takes an email and sends a link to that email.
            // the link allows the user to add themselves to the household, but only if they're invited! (on the invite list)

            if (String.IsNullOrEmpty(bindingModel.Email))
                return BadRequest("Email is invalid.");

            var Inviteduser = UserManager.FindByEmail(bindingModel.Email);
            if (Inviteduser is null)
                return NotFound();

            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);
            if (household is null)
                return NotFound();

            if (household.CreatorId == Inviteduser.Id)
                return BadRequest("Cannot invite Household creator.");

            if (household.InvitedUsers.Any(p => p.Id == Inviteduser.Id))
                return BadRequest("User is already invited.");

            if (household.Members.Any(p => p.Id == Inviteduser.Id))
                return BadRequest("User is already in the household.");

            household.InvitedUsers.Add(Inviteduser);
            DbContext.SaveChanges();

            var callbackUrl = Url.Link("Default", new { Controller = "Household", Action = "Join", Id = household.Id });

            var emailService = new EmailService();
            emailService.Send(Inviteduser.Email, $@"
                    <div>
                        <h2>Link to join '{household.Name}' Household</h2>
                        <p>Invited by {household.Creator.Email}</p>
                        <a href='{callbackUrl}'>{callbackUrl}</a>
                    </div>"
                , $"You've been invited to {household.Name} - BudgetManager");

            return Ok();

        }

        // >Registered users should be able to join households they were invited to. 
        // POST api/household/join
        [HttpPost]
        [Route("join/{id:int}")]
        public IHttpActionResult Join(int Id)
        {
            // Check to see if the user making this request in the on the household and flagged as invited.
            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);
            if (household is null)
                return NotFound();

            // The user making the request is the user trying to join.
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user is null)
                return NotFound();

            // Ensure user is in the invited list
            if (household.InvitedUsers.Any(p => p.Id == user.Id))
            {
                household.InvitedUsers.Remove(user);
                household.Members.Add(user);
                DbContext.SaveChanges();

                return Ok();
            }
            else
            {
                return BadRequest("This user hasn't been invited to this household");
            }
        }

        // >Owners can delete households
        // DELETE api/household/1
        [HttpDelete]
        [Route("delete/{id:int}")] // Not required but might aswell
        [UserAuthorization(IdType = typeof(HouseholdCreator))]
        public IHttpActionResult Delete(int? Id)
        {
            // Inorder to avoid getting the error for cascade deleting, we need to change something in the onModelCreating, adding .WillCascadeOnDelete(false);
            var removedHousehold = DbContext.Households.Remove(DbContext.Households.FirstOrDefault(p => p.Id == Id));

            if (removedHousehold != null)
            {
                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        // > Registered users can list users of a household
        // GET api/household/4/listusers
        [HttpGet]
        [Route("getallusers/{id:int}")]
        public IHttpActionResult GetAllUsers(int? Id)
        {
            var householdUsers = DbContext.Households.FirstOrDefault(p => p.Id == Id).Members;
            if (householdUsers is null)
                return NotFound();

            var householdUsersView = householdUsers.Select(p => new HouseholdUserViewModel()
            {
                Email = p.Email,
                Id = p.Id,
                Username = p.UserName
            }).ToList();

            return Ok(householdUsersView);
        }

        // >List all households that a users in ::Extra::
        // GET api/household/usershouseholds
        [HttpGet]
        [Route("user")]
        public IHttpActionResult GetAllUserHouseholds()
        {
            var userHouseholds = UserManager.FindById(User.Identity.GetUserId()).HouseholdMembers;
            if (userHouseholds is null)
                return NotFound();

            var userHouseholdsView = userHouseholds.Select(p => HouseholdHelpers.MapHouseholdToView(p));

            return Ok(userHouseholdsView);
        }

        // >Registered users should be able to leave households they belong to. (But not owners)
        // GET api/household/2/leave
        [HttpGet]
        [Route("leave/{id:int}")]
        public IHttpActionResult Leave(int? Id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user is null)
                return NotFound();

            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);
            if (household is null)
                return NotFound();

            // If user is in the household and is not the creator, allow them to leave.
            if (household.Members.Any(p => p.Id == user.Id && household.Creator.Id != user.Id))
            {
                household.Members.Remove(user);
                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        // === Extras for debugging. ===
        // GET api/household/getall
        [HttpGet]
        [Route("getall")]
        public IHttpActionResult GetAll()
        {
            var households = DbContext.Households.ToList();
            if (households is null)
                return NotFound();

            var householdsView = households.Select(p => HouseholdHelpers.MapHouseholdToView(p));

            return Ok(householdsView);
        }

        // GET api/household/getbyid/23
        [HttpGet]
        [Route("getbyid/{id:int}", Name = "GetHouseholdById")]
        public IHttpActionResult GetById(int? Id)
        {
            if (Id is null)
                return BadRequest("Id is invalid");

            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);

            if (household is null)
                return NotFound();

            var householdView = HouseholdHelpers.MapHouseholdToView(household);

            return Ok(householdView);
        }
    }
}
