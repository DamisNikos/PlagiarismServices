using Common.ResultsModel;
using FrontEndService.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEndService
{
    public class ResultsContext : DbContext
    {
        public DbSet<Comparison> Comparisons { get; set; }
        public DbSet<CommonPassage> Passages { get; set; }



        public ResultsContext()
        : base("Data Source=  85.75.108.128;Initial Catalog=ResultDatabase;Persist Security Info=True;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=True")
        {
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<ResultsContext>());
        }
    }
}
