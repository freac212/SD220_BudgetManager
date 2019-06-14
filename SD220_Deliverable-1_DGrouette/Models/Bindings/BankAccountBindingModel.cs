using System.ComponentModel.DataAnnotations;

namespace SD220_Deliverable_1_DGrouette.Models.Bindings
{
    public class BankAccountBindingModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}