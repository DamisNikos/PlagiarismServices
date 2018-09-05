using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.DataModels
{
    public class StopNGram
    {
        public string stopWordsInString { get; set; }

        public int lower { get; set; }

        public int upper { get; set; }

        [NotMapped]
        public List<string> stopWordsList { get; set; }

        public StopNGram()
        {
            stopWordsList = new List<string>();
        }
    }
}