using DividendTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DividendTracker.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public PortfolioController(ApplicationDbContext context)
        {
            _context = context; 
        }

        public IActionResult Index()
        {
            return View();
        }

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
                    // calculate the new weighted average cost per share
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

            // handle invalid model state
            return RedirectToAction("Index", "Stocks");
        }

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
                
            }

            return RedirectToAction(nameof(UserPortfolio));
        }


    }
}
