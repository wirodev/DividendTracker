//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace DividendTracker.Models
//{
//    public class UserPortfolio
//    {
//        [Key]
//        public int UserPortfolioId { get; set; }

//        [Required]
//        [ForeignKey("Stock")] // added to test
//        public string Ticker { get; set; }
//        public virtual Stock Stock { get; set; } // added for dash

//        [Column(TypeName = "decimal(18, 2)")]
//        public decimal AverageCostPerShare { get; set; }

//        public int AmountOfSharesOwned { get; set; }

//        // Removed the Stock navigation property because we're only binding the Ticker.
//    }
//}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DividendTracker.Models
{
    public class UserPortfolio
    {
        [Key]
        public int UserPortfolioId { get; set; }

        [Required] // Ensures Ticker is not null
        [ForeignKey("Stock")]
        public string Ticker { get; set; }

        // The virtual keyword is used for lazy loading of the related Stock entity
        public virtual Stock Stock { get; set; } // Navigation property for related Stock

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AverageCostPerShare { get; set; }

        public int AmountOfSharesOwned { get; set; }

        // You can remove the Stock property if it's just a string and not an object reference
        // The ForeignKey attribute is not required if you're following EF conventions and the property names match
    }
}
