using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Helpers
{
    public static class BankAccountHelpers
    {
        internal static object MapBankAccountToView(BankAccount bankAccount)
        {
            return new BankAccountViewModel()
            {
                Id = bankAccount.Id,
                Name = bankAccount.Name,
                Description = bankAccount.Description,
                DateCreated = bankAccount.DateCreated,
                DateUpdated = bankAccount.DateUpdated,
                HouseholdId = bankAccount.HouseholdId
            };
        }
    }
}