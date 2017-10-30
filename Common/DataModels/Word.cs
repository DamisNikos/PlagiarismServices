using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModels
{
    public class Word
    {
        public int wordID { get; set; }
        public string word { get; set; }

        [ForeignKey("document")]
        public int documentID { get; set; }
        [IgnoreDataMember]
        public Document document { get; set; }
    }
}
