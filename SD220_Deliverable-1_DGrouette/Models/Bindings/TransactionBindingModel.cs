using System;
using System.ComponentModel.DataAnnotations;

namespace SD220_Deliverable_1_DGrouette.Models.Bindings
{
    public class TransactionBindingModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }
}