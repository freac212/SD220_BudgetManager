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
        public decimal Balance { get; set; } // Default value is already 0.0m

        // Relationships
        public virtual Household Household { get; set; }
        public int HouseholdId { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public BankAccount()
        {
            Balance = 0.0m; // Even though the value defaults to 0, we ensure it is 0 by setting it here.
            // Can also add DateCreated here. ++Q
            Transactions = new List<Transaction>();
        }

        public void CalculateBalance()
        {
            Balance = Transactions.Where(p => !p.IsVoid).Sum(p => p.Amount);
        }

        public void AddTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);

            if(!transaction.IsVoid)
                Balance += transaction.Amount;
        }
        public void RemoveTransaction(Transaction transaction)
        {
            Transactions.Remove(transaction);

            if (!transaction.IsVoid)
                Balance -= transaction.Amount;
        }
    }
}