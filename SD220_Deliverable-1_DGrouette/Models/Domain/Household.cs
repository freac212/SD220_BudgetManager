using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Domain
{
    public class Household
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual ApplicationUser Creator { get; set; }
        public string CreatorId { get; set; }

        public virtual List<ApplicationUser> Users { get; set; }
        public virtual List<ApplicationUser> InvitedUsers { get; set; }
        public virtual List<Category> Categories { get; set; }
        public virtual List<BankAccount> BankAccounts { get; set; } // A bank account can only exist on one household, but a household can have many accounts

        public Household()
        {
            Users = new List<ApplicationUser>();
            InvitedUsers = new List<ApplicationUser>();
            Categories = new List<Category>();
            BankAccounts = new List<BankAccount>();
        }
    }
}