using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Helpers
{
    public class TransactionHelpers
    {
        internal static object MapToView(Transaction transaction)
        {
            return new TransactionViewModel()
            {
                Id = transaction.Id,
                Title = transaction.Title,
                Description = transaction.Description,
                DateCreated = transaction.DateCreated,
                DateUpdated = transaction.DateUpdated,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Void = transaction.isVoid,
                CategoryId = transaction.CategoryId,
                CreatorId = transaction.CreatorId,
                BankAccountId = transaction.BankAccountId
            };
        }
    }
}