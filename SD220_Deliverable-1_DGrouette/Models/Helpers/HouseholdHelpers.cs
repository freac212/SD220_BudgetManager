using Microsoft.AspNet.Identity;
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
        public static HouseholdViewModel MapHouseholdToView(Domain.Household household, string userId)
        {
            return new HouseholdViewModel()
            {
                Id = household.Id,
                DateCreated = household.DateCreated,
                DateUpdated = household.DateUpdated,
                Name = household.Name,
                Description = household.Description,
                Creator = household.Creator.Email,
                IsCreator = household.CreatorId == userId,
                IsMember = household.Members.Any(p => p.Id == userId),
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
                }).ToList(),
                BankAccounts = household.BankAccounts.Select(p => new BankAccountViewModel()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Balance = p.Balance,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    HouseholdId = p.HouseholdId
                }).ToList(),
                Transactions = household.BankAccounts
                .SelectMany(p => p.Transactions)
                .Select(p => new TransactionViewModel()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    Date = p.Date,
                    Amount = p.Amount,
                    IsVoid = p.IsVoid,
                    CategoryId = p.CategoryId,
                    CreatorId = p.CreatorId,
                    BankAccountId = p.BankAccountId
                }).ToList()
             
            };
        }

        public static InviteViewModel MapInviteToView(Domain.Household household)
        {
            return new InviteViewModel()
            {
                Id = household.Id,
                Name = household.Name,
                Creator = household.Creator.Email
            };
        }

        public static CategoryViewModel MapCategoryToView(Category category)
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

        public enum ListTypes
        {
            HouseHolds,
            Transactions,
            BankAccounts,
            Categories
        }
    }
}