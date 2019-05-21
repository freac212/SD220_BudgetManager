using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Domain
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public decimal Balance { get; set; } // Default value is already 0.0d

        // Relationships
        public virtual Household Household { get; set; }
        public int HouseholdId { get; set; }

        public void UpdateBalance()
        {
            // Get all transactions and set the balance from them
        }

        //public decimal AddTransaction(decimal transactionValue)
        //{
        //    return Balance += transactionValue;
        //}
    }
}