using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

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