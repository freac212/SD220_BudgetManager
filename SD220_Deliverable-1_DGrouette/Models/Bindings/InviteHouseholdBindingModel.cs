using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SD220_Deliverable_1_DGrouette.Models.Bindings
{
    public class InviteHouseholdBindingModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string CallbackUrl { get; set; }
    }
}