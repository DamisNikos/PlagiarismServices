using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.DataModels
{
    public class OldDocument
    {
        public string DocName { get; set; }
        public int documentID { get; set; }
        public byte[] DocContent { get; set; }

        [StringLength(450)]
        [Index(IsUnique = true)]
        public string DocHash { get; set; }

        public string DocUser { get; set; }

        [Required]
        public List<OldWord> words { get; set; }

        [Required]
        public List<OldProfileStopWord> profiles { get; set; }

        public OldDocument()
        {
            profiles = new List<OldProfileStopWord>();
            words = new List<OldWord>();
        }
    }
}