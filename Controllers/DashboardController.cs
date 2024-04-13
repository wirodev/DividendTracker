using DividendTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DividendTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // fetch UserPortfolio data
            var userPortfolios = await _context.UserPortfolios
                .Include(up => up.Stock) // changed from stock to ticker
                .ThenInclude(stock => stock.Dividend) 
                .ToListAsync();

            // calculate total dividend
            var totalDividend = userPortfolios
                .Where(up => up.Stock != null && up.Stock.Dividend != null) // Check for null
                .Sum(up => up.Stock.Dividend.DividendAmount * up.AmountOfSharesOwned);



            // calculate dividend yield and yield on cost for each stock and average them
            var totalValue = userPortfolios.Sum(up => up.AverageCostPerShare * up.AmountOfSharesOwned);
            var totalDividendYield = userPortfolios.Sum(up => up.Stock.Dividend.DividendAmount / up.Stock.Dividend.CurrentStockPrice) / userPortfolios.Count;
            var totalYieldOnCost = userPortfolios.Sum(up => up.Stock.Dividend.DividendAmount / up.AverageCostPerShare) / userPortfolios.Count;

            var dashboardModel = new DashboardModel
            {
                TotalDividend = totalDividend,
                TotalDividendYield = totalDividendYield,
                TotalYieldOnCost = totalYieldOnCost
            };

            return View(dashboardModel);
        }
    }

}
