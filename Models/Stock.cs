using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace DividendTracker.Models
{
    public class Stock
    {
        [Key]
        public string Ticker { get; set; } // Primary key
        public virtual Dividend Dividend { get; set; } // added for dash
        public string CompanyName { get; set; }
        public string Sector { get; set; }

        public virtual ICollection<UserPortfolio> UserPortfolios { get; set; }
    }
}
