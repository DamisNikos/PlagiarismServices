using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.DataModels
{
    public class Document
    {
        public string DocName { get; set; }
        public int DocumentID { get; set; }
        public byte[] DocContent { get; set; }

        [StringLength(450)]
        [Index(IsUnique = true)]
        public string DocHash { get; set; }

        public string DocUser { get; set; }

        [Required]
        public string words { get; set; }

        [Required]
        public string profiles { get; set; }
    }
}