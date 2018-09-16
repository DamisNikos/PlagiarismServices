using Common.DataModels;
using Common.Interfaces;
using Common.ResultsModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Fabric;
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
                           .GroupBy(n => n.SuspiciousDocumentName)
                           .Select(f => f.OrderBy(n => n.comparisonID)
                           .FirstOrDefault()
                           )
                           .OrderByDescending(n => n.comparisonID)
                           .ToList();

                if (!String.IsNullOrEmpty(searchString))
                {
                    comparisons = comparisons.Where(n => n.SuspiciousDocumentName.Contains(searchString)).ToList();
                }
            }

            int pageSize = 2;
            return View(PaginatedList<Comparison>.CreateAsync(comparisons.AsQueryable(), page ?? 1, pageSize));
        }

        public async Task<IActionResult> ComparedDocuments(string suspiciousName, string sortOrder, string currentNameFilter
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

            if (suspiciousName != null)
            {
                ViewData["CurrentNameFilter"] = suspiciousName;
            }
            else
            {
                suspiciousName = currentNameFilter;
            }

            using (var context = new DocumentContext())
            {
                comparisons = context.Comparisons.AsNoTracking().Include(n => n.CommonPassages).Where(n => n.ComparisonUser.Equals(User.Identity.Name))
                                .Where(n => n.SuspiciousDocumentName.Equals(suspiciousName)).OrderByDescending(n => n.comparisonID).ToList();

                if (!String.IsNullOrEmpty(searchString))
                {
                    var comparisonSearch = comparisons.Where(n => n.OriginalDocumentName.Contains(searchString)).ToList();
                    if (comparisonSearch.Count > 0)
                    {
                        comparisons = comparisonSearch;
                    }
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

        public async Task<IActionResult> Delete(string name)
        {
            using (var context = new DocumentContext())
            {
                context.Comparisons.RemoveRange(context.Comparisons
                    .Include(n => n.CommonPassages)
                    .Where(n => n.ComparisonUser.Equals(User.Identity.Name))
                    .Where(n => n.SuspiciousDocumentName.Equals(name))
                    .ToList());

                context.Documents.Remove(context.Documents
                    .Where(n => n.DocUser.Equals(User.Identity.Name))
                    .Where(n => n.DocName.Equals(name)).FirstOrDefault());
                context.SaveChanges();
            }

            return RedirectToAction(nameof(HistoryController.Index), "History");
        }

        public async Task<IActionResult> Rescan(string name)
        {
            Document document;

            using (var context = new DocumentContext())
            {
                document = context.Documents.Where(n => n.DocName.Equals(name))
                    .Where(n => n.DocUser.Equals(User.Identity.Name))
                    .FirstOrDefault();

                context.Comparisons.RemoveRange(context.Comparisons
                   .Include(n => n.CommonPassages)
                   .Where(n => n.ComparisonUser.Equals(User.Identity.Name))
                   .Where(n => n.SuspiciousDocumentName.Equals(name))
                   .ToList());
                context.SaveChanges();
            }

            var serviceName = new Uri("fabric:/PlagiarismServices/ManagerService");
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

                var partitionInformation = (Int64RangePartitionInformation)partitions.FirstOrDefault().PartitionInformation;
                IManagement managementClient = ServiceProxy.Create<IManagement>(serviceName, new ServicePartitionKey(partitionInformation.LowKey));

                managementClient.DocumentHashReceivedAsync(document.DocHash, document.DocUser);
            }
            return RedirectToAction(nameof(HistoryController.Index), "History");
        }
    }
}