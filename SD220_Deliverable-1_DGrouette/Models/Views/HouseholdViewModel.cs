using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Views
{
    public class HouseholdViewModel
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public bool IsCreator { get; set; }
        public bool IsMember { get; set; }
        public List<HouseholdUserViewModel> HouseholdUsers { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
    }
}