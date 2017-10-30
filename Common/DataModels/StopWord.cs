using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModels
{
    public class StopWord
    {
        public int stopWordID { get; set; }
        public int index { get; set; }
        public string word { get; set; }

        public int stopNgramID { get; set; }
        [IgnoreDataMember]
        public StopNGram stopNGram { get; set; }
    }
}
