using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual Household Household { get; set; }
        public int HouseholdId { get; set; }
        public virtual List<Transaction> Transactions { get; set; }

        public Category()
        {
            // Can also set DateCreated here ++Q
            Transactions = new List<Transaction>();
        }
    }
}