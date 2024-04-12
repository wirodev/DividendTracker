using DividendTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DividendTracker.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _context; // Add this line

        // Update your constructor to accept ApplicationDbContext
        public PortfolioController(ApplicationDbContext context)
        {
            _context = context; // Initialize the _context field
        }

        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddStockToPortfolio(UserPortfolio userPortfolio)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(userPortfolio);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("UserPortfolio"); // Redirect to the UserPortfolio action
        //    }

        //    // If ModelState is not valid, return to a view where the user can correct their input.
        //    // For example, you might want to redirect them back to the Index view with the form displayed.
        //    // Pass the model back to the view to display validation errors.
        //    return RedirectToAction("Index", "Stocks"); // or return View("SomeView", model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStockToPortfolio(UserPortfolio userPortfolio)
        {
            if (ModelState.IsValid)
            {
                var existingPortfolioEntry = await _context.UserPortfolios
                    .FirstOrDefaultAsync(p => p.Ticker == userPortfolio.Ticker);

                // check if exists
                if (existingPortfolioEntry != null)
                {
                    // Calculate the new weighted average cost per share
                    var totalCostOfExistingShares = existingPortfolioEntry.AverageCostPerShare * existingPortfolioEntry.AmountOfSharesOwned;
                    var totalCostOfNewShares = userPortfolio.AverageCostPerShare * userPortfolio.AmountOfSharesOwned;
                    var totalShares = existingPortfolioEntry.AmountOfSharesOwned + userPortfolio.AmountOfSharesOwned;

                    existingPortfolioEntry.AverageCostPerShare = (totalCostOfExistingShares + totalCostOfNewShares) / totalShares;
                    existingPortfolioEntry.AmountOfSharesOwned = totalShares;

                    _context.Update(existingPortfolioEntry);
                }

                // if does not exist, then add
                else
                {
                    _context.Add(userPortfolio);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(UserPortfolio));
            }

            // Handle invalid model state
            // This should redirect back to the form with validation errors
            // Here, redirecting to the Index action of the StocksController
            return RedirectToAction("Index", "Stocks");
        }




        //public async Task<IActionResult> UserPortfolio()
        //{
        //    var portfolioData = await _context.UserPortfolios
        //        .Join(_context.Stocks, // Assumes you have DbSet<Stock> Stocks in your context
        //              portfolio => portfolio.Ticker,
        //              stock => stock.Ticker,
        //              (portfolio, stock) => new { portfolio, stock })
        //        .Join(_context.Dividends, // Assumes you have DbSet<Dividend> Dividends in your context
        //              combined => combined.stock.Ticker,
        //              dividend => dividend.Ticker,
        //              (combined, dividend) => new UserPortfolioViewModel
        //              {
        //                  Ticker = combined.portfolio.Ticker,
        //                  CompanyName = combined.stock.CompanyName,
        //                  AverageCostPerShare = combined.portfolio.AverageCostPerShare,
        //                  AmountOfSharesOwned = combined.portfolio.AmountOfSharesOwned,
        //                  DividendAmount = dividend.DividendAmount
        //              })
        //        .ToListAsync();

        //    return View(portfolioData);
        //}

        public async Task<IActionResult> UserPortfolio()
        {
            var portfolioData = await _context.UserPortfolios
                .Join(_context.Stocks, portfolio => portfolio.Ticker, stock => stock.Ticker,
                    (portfolio, stock) => new { portfolio, stock })
                .Join(_context.Dividends, combined => combined.stock.Ticker, dividend => dividend.Ticker,
                    (combined, dividend) => new UserPortfolioViewModel
                    {
                        Ticker = combined.portfolio.Ticker,
                        CompanyName = combined.stock.CompanyName,
                        AverageCostPerShare = combined.portfolio.AverageCostPerShare,
                        AmountOfSharesOwned = combined.portfolio.AmountOfSharesOwned,
                        DividendAmount = dividend.DividendAmount,
                        CurrentStockPrice = dividend.CurrentStockPrice,
                        // Make sure to handle possible division by zero
                        DividendYield = dividend.CurrentStockPrice > 0 ? dividend.DividendAmount / dividend.CurrentStockPrice : 0,
                        YieldOnCost = combined.portfolio.AverageCostPerShare > 0 ? dividend.DividendAmount / combined.portfolio.AverageCostPerShare : 0
                    })
                .ToListAsync();

            return View(portfolioData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SellStock(string ticker, int amountOfSharesToSell)
        {
            var userPortfolioEntry = await _context.UserPortfolios
                .FirstOrDefaultAsync(p => p.Ticker == ticker);

            if (userPortfolioEntry != null && amountOfSharesToSell > 0 && amountOfSharesToSell <= userPortfolioEntry.AmountOfSharesOwned)
            {
                userPortfolioEntry.AmountOfSharesOwned -= amountOfSharesToSell;

                // If all shares are sold, you could choose to delete the entry or keep it with 0 shares.
                if (userPortfolioEntry.AmountOfSharesOwned == 0)
                {
                    _context.UserPortfolios.Remove(userPortfolioEntry);
                }
                else
                {
                    _context.Update(userPortfolioEntry);
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // Handle the case where there's an attempt to sell more shares than owned
                // You could add a model error here and return the view with a message
            }

            return RedirectToAction(nameof(UserPortfolio));
        }


    }
}
