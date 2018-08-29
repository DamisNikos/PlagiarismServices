using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Common.DataModels
{
    public class OldWord
    {
        public int wordID { get; set; }
        public string word { get; set; }

        [ForeignKey("document")]
        public int documentID { get; set; }

        [IgnoreDataMember]
        public OldDocument document { get; set; }
    }
}