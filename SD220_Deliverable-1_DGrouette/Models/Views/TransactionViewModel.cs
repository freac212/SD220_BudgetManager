using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Views
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool Void { get; set; }
        public int CategoryId { get; set; }
        public string CreatorId { get; set; }
        public int BankAccountId { get; set; }
    }
}