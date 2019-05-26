using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Helpers
{
    public static class HouseholdHelpers
    {
        public static HouseholdViewModel MapHouseholdToView(Domain.Household household)
        {
            return new HouseholdViewModel()
            {
                Id = household.Id,
                DateCreated = household.DateCreated,
                DateUpdated = household.DateUpdated,
                Name = household.Name,
                Description = household.Description,
                Creator = household.Creator.Email,
                HouseholdUsers = household.Members.Select(p => new HouseholdUserViewModel()
                {
                    Id = p.Id,
                    Email = p.Email,
                    Username = p.UserName
                }).ToList(),
                Categories = household.Categories.Select(p => new CategoryViewModel()
                {
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    Description = p.Description,
                    Id = p.Id,
                    Name = p.Name,
                    HouseholdId = p.HouseholdId
                }).ToList()
             };
        }

        internal static object MapCategoryToView(Category category)
        {
            return new CategoryViewModel()
            {
                Id = category.Id,
                DateCreated = category.DateCreated,
                DateUpdated = category.DateUpdated,
                Name = category.Name,
                Description = category.Description,
                HouseholdId = category.HouseholdId
            };
        }

        internal enum ListTypes
        {
            HouseHolds,
            Transactions,
            BankAccounts,
            Categories
        }
    }
}