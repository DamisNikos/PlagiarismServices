using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ResultsModel
{
    public class ResultsContext : DbContext
    {
        public DbSet<Comparison> Comparisons { get; set; }
        public DbSet<CommonPassage> Passages { get; set; }



        public ResultsContext()
        : base("Data Source=  94.64.96.46;Initial Catalog=ResultDatabase;Persist Security Info=True;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=True")
        {
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<ResultsContext>());
        }


    }
}
