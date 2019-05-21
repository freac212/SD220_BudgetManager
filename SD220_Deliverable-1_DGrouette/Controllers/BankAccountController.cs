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
        [CreatorAuthorization]
        public IHttpActionResult Create(int? id, BankAccountBindingModel bankAccountBindingModel)
        {
            // Id being the id of the Household
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var household = DbContext.Households.FirstOrDefault(p => p.Id == id);
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
        // POST api/category/edit/2
        [HttpPost]
        [Route("edit/{id:int}")]
        [CategoryAuthorization]
        public IHttpActionResult Edit(int? id, BankAccountBindingModel bindingModel)
        {
            if (!ModelState.IsValid) // ++Q : Turn into filter.
                return BadRequest(ModelState);

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == id);
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
        [CategoryAuthorization]
        public IHttpActionResult Delete(int? id)
        {
            var removedBankAccount = DbContext.BankAccounts.Remove(DbContext.BankAccounts.FirstOrDefault(p => p.Id == id));

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

        // All users of a household can see a list of all accounts for that household
        // GET api/bankaccount/getall
        [HttpGet]
        [Route("getall")]
        public IHttpActionResult GetAll()
        {
            var userId = User.Identity.GetUserId();
            if (userId is null)
                return NotFound();

            var bankAccounts = DbContext.BankAccounts.Where(p => p.Household.Users.Any(i => i.Id == userId)).ToList();
            if (bankAccounts is null)
                return NotFound();

            return OkView(bankAccounts);
        }

        // The owner can update a bank accounts total at will (will just call bankaccountupdate, a universal method.)
        // GET api/bankaccount/updatebalance/2
        [HttpGet]
        [Route("updatebalance/{id:int}")]
        public IHttpActionResult UpdateBalance(int? Id)
        {
            if (Id is null)
                return BadRequest();

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return NotFound();

            bankAccount.UpdateBalance();

            return OkView(bankAccount);
        }

        // === Extras for debugging. ===
        // GET api/bankaccount/getbyid/2
        [HttpGet]
        [Route("getbyid/{id:int}", Name = "GetBankAccountById")]
        public IHttpActionResult GetById(int? Id)
        {
            if (Id is null)
                return BadRequest();

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return NotFound();

            return OkView(bankAccount);
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
