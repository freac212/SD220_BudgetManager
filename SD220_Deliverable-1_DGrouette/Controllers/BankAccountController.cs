using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SD220_Deliverable_1_DGrouette.Models;
using SD220_Deliverable_1_DGrouette.Models.Bindings;
using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Filters;
using SD220_Deliverable_1_DGrouette.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SD220_Deliverable_1_DGrouette.Controllers
{
    [RoutePrefix("api/bankaccount")]
    [Authorize]
    public class BankAccountController : ApiController
    {
        // Using the same DB instance
        private ApplicationUserManager UserManager => ControllerContext.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        private ApplicationDbContext DbContext => ControllerContext.Request.GetOwinContext().Get<ApplicationDbContext>();

        // Owner of the household can create inf. bank accounts for a household
        // POST api/bankaccount/create
        [HttpPost]
        [Route("create/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdCreator))]
        public IHttpActionResult Create(int? Id, BankAccountBindingModel bankAccountBindingModel)
        {
            if (ModelState is null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);
            if (household is null)
                return NotFound();

            var bankAccount = new BankAccount
            {
                Name = bankAccountBindingModel.Name,
                Description = bankAccountBindingModel.Description,
                DateCreated = DateTime.Now,
                DateUpdated = null,
                Balance = 0.0m,
                Household = household
            };

            household.BankAccounts.Add(bankAccount);
            DbContext.SaveChanges();

            var bankAccountView = BankAccountHelpers.MapBankAccountToView(bankAccount);

            return Created(Url.Link(
                "GetBankAccountById",
                new { bankAccount.Id }),
                bankAccountView
            );
        }

        // Owner can edit an account
        // POST api/bankaccount/edit/2
        [HttpPost]
        [Route("edit/{id:int}")]
        [UserAuthorization(IdType = typeof(BankAccountCreator))] // only the owner of the household can edit the bank accounts
        public IHttpActionResult Edit(int? Id, BankAccountBindingModel bindingModel)
        {
            if (ModelState is null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return NotFound();

            bankAccount.Name = bindingModel.Name;
            bankAccount.Description = bindingModel.Description;
            bankAccount.DateUpdated = DateTime.Now;
            DbContext.SaveChanges();

            return OkView(bankAccount);
        }

        // Owner can delete an account
        // POST api/bankAccount/delete/2
        [HttpDelete]
        [Route("delete/{id:int}")]
        [UserAuthorization(IdType = typeof(BankAccountCreator))]
        public IHttpActionResult Delete(int? Id)
        {
            var removedBankAccount = DbContext.BankAccounts.Remove(DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id));

            if (removedBankAccount != null)
            {
                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        // All users of a household can see a list of all BankAccounts for that household
        // GET api/bankaccount/getall
        [HttpGet]
        [Route("getall/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdHouseMember))]
        public IHttpActionResult GetAll(int? Id)
        {
            // household Id
            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);
            if (household is null)
                return NotFound();

            var bankAccounts = household.BankAccounts.ToList();
            if (bankAccounts is null)
                return NotFound();

            return OkView(bankAccounts);
        }

        // The owner can update a bank accounts total at will
        // GET api/bankaccount/updatebalance/2
        [HttpGet] // Change to post ++Q -> Post because we're changing the database
        [Route("updatebalance/{id:int}")]
        [UserAuthorization(IdType = typeof(BankAccountCreator))]
        public IHttpActionResult UpdateBalance(int? Id)
        {
            if (Id is null)
                return NotFound();

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return NotFound();

            bankAccount.CalculateBalance();
            DbContext.SaveChanges();

            return OkView(bankAccount);
        }

        // >Getting the bank accounts by the household Id, user must be a member of the household
        // GET api/category/getallbyhousehold
        [HttpGet]
        [Route("getallbyhousehold/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdHouseMember))]
        public IHttpActionResult GetAllByHousehold(int? Id)
        {
            var userId = User.Identity.GetUserId();
            if (userId is null)
                return Unauthorized();

            var bankAccounts = DbContext.Households.FirstOrDefault(p => p.Id == Id).BankAccounts.ToList();
            if (bankAccounts is null)
                return BadRequest("No Accounts for this household");

            var viewModels = bankAccounts.Select(p => BankAccountHelpers.MapBankAccountToView(p));

            return Ok(viewModels);
        }

        // === Extras for debugging. ===
        // GET api/bankaccount/getbyid/2
        [HttpGet]
        [UserAuthorization(IdType = typeof(BankAccountHouseMember))]
        [Route("getbyid/{id:int}", Name = "GetBankAccountById")]
        public IHttpActionResult GetById(int? Id)
        {
            if (Id is null)
                return NotFound();

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return NotFound();

            return OkView(bankAccount);
        }

        // GET api/bankaccount/getName/2
        [HttpGet]
        [Route("getName/{id:int}")]
        [UserAuthorization(IdType = typeof(BankAccountHouseMember))]
        public IHttpActionResult GetName(int? Id)
        {
            if (Id is null)
                return BadRequest("Id is invalid");

            var bankaccountName = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id).Name;
            if (bankaccountName is null)
                return NotFound();

            return Ok(bankaccountName);
        }

        private IHttpActionResult OkView(BankAccount bankAccount)
        {
            var bankAccountView = BankAccountHelpers.MapBankAccountToView(bankAccount);
            return Ok(bankAccountView);
        }

        private IHttpActionResult OkView(List<BankAccount> bankAccounts)
        {
            var bankAccountViews = bankAccounts.Select(p => BankAccountHelpers.MapBankAccountToView(p));
            return Ok(bankAccountViews);
        }
    }
}
