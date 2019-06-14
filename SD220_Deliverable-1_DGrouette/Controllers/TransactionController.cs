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
using System.Web.Http;

namespace SD220_Deliverable_1_DGrouette.Controllers
{
    [RoutePrefix("api/transaction")]
    [Authorize]
    public class TransactionController : ApiController
    {
        // Using the same DB instance
        private ApplicationUserManager UserManager => ControllerContext.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        private ApplicationDbContext DbContext => ControllerContext.Request.GetOwinContext().Get<ApplicationDbContext>();

        // All users of the household can create inf. bank accounts for the selected household
        // POST api/transaction/create
        [HttpPost]
        [Route("create/{id:int}")]
        [UserAuthorization(IdType = typeof(BankAccountHouseMember))]
        public IHttpActionResult Create(int? Id, TransactionBindingModel transactionBinding)
        {
            // Id being the id of the bankAccount
            if (ModelState is null || !ModelState.IsValid || Id is null)
                return BadRequest(ModelState);

            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return BadRequest("No bank account with that Id");

            // Ensure that the bank account and category are in the same houeshold.
            if (!bankAccount.Household.Categories.Any(p => p.Id == transactionBinding.CategoryId))
                return BadRequest("No category with that Id");

            var transaction = new Transaction
            {
                Title = transactionBinding.Title,
                Description = transactionBinding.Description,
                CategoryId = transactionBinding.CategoryId,
                Date = transactionBinding.Date,
                Amount = transactionBinding.Amount,
                CreatorId = User.Identity.GetUserId(),
                DateCreated = DateTime.Now,
                DateUpdated = null,
                BankAccountId = (int)Id,
                IsVoid = false
            };

            bankAccount.AddTransaction(transaction);
            DbContext.SaveChanges();

            var transactionView = TransactionHelpers.MapToView(transaction);

            return Created(Url.Link(
                "GetTransactionById",
                new { transaction.Id }),
                transactionView
            );
        }

        // Method to void transactions, owner/ creator auth
        [HttpPost]
        [Route("switchvoid/{id:int}")]
        [UserAuthorization(IdType = typeof(TransactionCreator))]
        public IHttpActionResult SwitchVoidState(int? Id)
        {
            var transaction = DbContext.Transactions.FirstOrDefault(p => p.Id == Id);
            if (transaction is null)
                return NotFound();

            if (transaction.IsVoid)
            {
                transaction.BankAccount.Balance += transaction.Amount;
                transaction.IsVoid = false;
            } else
            {
                transaction.BankAccount.Balance -= transaction.Amount;
                transaction.IsVoid = true;
            }
            DbContext.SaveChanges();

            return OkView(transaction);
        }

        // Owner can edit an all, reg users can only edit what they've created.
        // POST api/transaction/edit/2
        [HttpPost]
        [Route("edit/{id:int}")]
        [UserAuthorization(IdType = typeof(TransactionCreator))]
        public IHttpActionResult Edit(int? Id, TransactionBindingModel bindingModel)
        {
            if (ModelState is null || !ModelState.IsValid) // ++Q : Turn into filter.
                return BadRequest(ModelState);

            var transaction = DbContext.Transactions.FirstOrDefault(p => p.Id == Id);
            if (transaction is null)
                return BadRequest("No Transaction with that Id");

            if (!transaction.BankAccount.Household.Categories.Any(p => p.Id == bindingModel.CategoryId))
                return BadRequest("No Category with that Id");

            if (!transaction.IsVoid)
            {
                transaction.BankAccount.Balance -= transaction.Amount; // Removing the old transaction amount from the old bank account
                transaction.BankAccount.Balance += bindingModel.Amount; // Adding the new amounts to the new account
            }

            transaction.Title = bindingModel.Title;
            transaction.Description = bindingModel.Description;
            transaction.CategoryId = bindingModel.CategoryId;
            transaction.Amount = bindingModel.Amount;
            transaction.Date = bindingModel.Date;
            transaction.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();

            return OkView(transaction);
        }

        // Owner can delete all in the household, reg users can only delete transactions they've created
        // POST api/transaction/delete/2
        [HttpDelete]
        [Route("delete/{id:int}")]
        [UserAuthorization(IdType = typeof(TransactionCreator))]
        public IHttpActionResult Delete(int? Id)
        {
            var transaction = DbContext.Transactions.FirstOrDefault(p => p.Id == Id);
            var bankAccount = transaction.BankAccount;
            bankAccount.RemoveTransaction(transaction);
            var removedTransaction = DbContext.Transactions.Remove(transaction);
            DbContext.SaveChanges();

            if (removedTransaction != null)
            {
                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        // All Users of a household can list all transactions of a BankAccount.
        // GET api/transaction/getall/2
        [HttpGet]
        [Route("getall/{id:int}")]
        [UserAuthorization(IdType = typeof(BankAccountHouseMember))]
        public IHttpActionResult GetAll(int? Id)
        {
            // BankAccount Id
            var bankAccount = DbContext.BankAccounts.FirstOrDefault(p => p.Id == Id);
            if (bankAccount is null)
                return NotFound();

            var transactions = bankAccount.Transactions.ToList();
            if (transactions is null)
                return NotFound();

            return OkView(transactions);
        }

        // === Extras for debugging/ front end. ===
        // >Getting the transactions by the household Id, user must be a member of the household
        // GET api/transaction/getallbyhousehold
        [HttpGet]
        [Route("getallbyhousehold/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdHouseMember))]
        public IHttpActionResult GetAllByHousehold(int? Id)
        {
            var userId = User.Identity.GetUserId();
            if (userId is null)
                return Unauthorized();

            var transactions = DbContext.Households.FirstOrDefault(p => p.Id == Id).BankAccounts.SelectMany(p => p.Transactions).ToList();
            if (transactions is null)
                return BadRequest("No Transactions for this household");

            var transactionsView = transactions.Select(p => TransactionHelpers.MapToView(p));

            return Ok(transactionsView);
        }

        // GET api/transaction/getbyid/2
        [HttpGet]
        [Route("getbyid/{id:int}", Name = "GetTransactionById")]
        [UserAuthorization(IdType = typeof(TransactionHouseMember))]
        public IHttpActionResult GetById(int? Id)
        {
            if (Id is null)
                return BadRequest("Id is invalid");

            var transaction = DbContext.Transactions.FirstOrDefault(p => p.Id == Id);
            if (transaction is null)
                return NotFound();

            return OkView(transaction);
        }

        // GET api/transaction/getTitle/2
        [HttpGet]
        [Route("getTitle/{id:int}")]
        [UserAuthorization(IdType = typeof(TransactionHouseMember))]
        public IHttpActionResult GetTitle(int? Id)
        {
            if (Id is null)
                return BadRequest("Id is invalid");

            var transactionTitle = DbContext.Transactions.FirstOrDefault(p => p.Id == Id).Title;
            if (transactionTitle is null)
                return NotFound();

            return Ok(transactionTitle);
        }

        // GET api/transaction/isusercreator
        [HttpGet]
        [Route("isusercreator/{id:int}")]
        [UserAuthorization(IdType = typeof(TransactionHouseMember))]
        public IHttpActionResult IsUserCreator(int? Id)
        {
            // refering to creator of household or creator of transaction. 
            // If the user is either of them, return true, as they can manipulate the transaction.

            // Doesn't matter if the transaction is on another household, as long as the user has the rights.

            if (Id is null)
                return BadRequest("Id is invalid.");

            var userId = User.Identity.GetUserId();

            var transaction = DbContext.Transactions.FirstOrDefault(p => p.Id == Id);
            if (transaction is null)
                return NotFound();

            var isCreatorView = new IsCreatorViewModel()
            {
                IsCreator = transaction.CreatorId == userId || transaction.BankAccount.Household.CreatorId == userId
            };

            return Ok(isCreatorView);
        }

        private IHttpActionResult OkView(Transaction transaction)
        {
            var transactionView = TransactionHelpers.MapToView(transaction);
            return Ok(transactionView);
        }

        private IHttpActionResult OkView(List<Transaction> transactions)
        {
            var transactionViews = transactions.Select(p => TransactionHelpers.MapToView(p));
            return Ok(transactionViews);
        }
    }
}
