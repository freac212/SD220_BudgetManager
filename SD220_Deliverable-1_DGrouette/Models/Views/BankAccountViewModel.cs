using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Views
{
    public class BankAccountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int HouseholdId { get; set; }
    }
}