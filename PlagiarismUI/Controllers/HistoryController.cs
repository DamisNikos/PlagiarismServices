using Common.DataModels;
using Common.ResultsModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlagiarismUI.Controllers
{
    public class HistoryController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            List<Comparison> comparisons;

            ViewData["CurrentSort"] = sortOrder;
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            using (var context = new DocumentContext())
            {
                comparisons = context.Comparisons
                           .Where(n => n.ComparisonUser.Equals(User.Identity.Name))
                           .GroupBy(n => n.OriginalDocumentName)
                           .Select(f => f.OrderBy(n => n.comparisonID)
                           .FirstOrDefault()
                           )
                           .OrderByDescending(n => n.comparisonID)
                           .ToList();

                if (!String.IsNullOrEmpty(searchString))
                {
                    comparisons = comparisons.Where(n => n.OriginalDocumentName.Contains(searchString)).ToList();
                }
            }

            int pageSize = 2;
            return View(PaginatedList<Comparison>.CreateAsync(comparisons.AsQueryable(), page ?? 1, pageSize));
        }

        public async Task<IActionResult> ComparedDocuments(string originalName, string sortOrder, string currentNameFilter
                                                         , string currentFilter, string searchString, int? page)
        {
            List<Comparison> comparisons;

            ViewData["CurrentSort"] = sortOrder;
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            if (originalName != null)
            {
                ViewData["CurrentNameFilter"] = originalName;
            }
            else
            {
                originalName = currentNameFilter;
            }

            using (var context = new DocumentContext())
            {
                comparisons = context.Comparisons.AsNoTracking().Include(n => n.CommonPassages).Where(n => n.ComparisonUser.Equals(User.Identity.Name))
                                .Where(n => n.OriginalDocumentName.Equals(originalName)).OrderByDescending(n => n.comparisonID).ToList();

                if (!String.IsNullOrEmpty(searchString))
                {
                    comparisons = comparisons.Where(n => n.SuspiciousDocumentName.Contains(searchString)).ToList();
                }
            }

            int pageSize = 10;
            return View(PaginatedList<Comparison>.CreateAsync(comparisons.AsQueryable(), page ?? 1, pageSize));
        }

        public async Task<IActionResult> Passages(int comparisonID)
        {
            Comparison comparison;

            using (var context = new DocumentContext())
            {
                comparison = context.Comparisons.AsNoTracking().Include(n => n.CommonPassages).Where(n => n.ComparisonUser.Equals(User.Identity.Name))
                                .Where(n => n.comparisonID.Equals(comparisonID)).FirstOrDefault();
            }

            return View(comparison);
        }
    }
}