using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModels
{
    public class Document
    {
        public string DocName { get; set; }
        public int documentID { get; set; }
        public byte[] DocContent { get; set; }

        [StringLength(450)]
        [Index(IsUnique = true)]
        public string DocHash { get; set; }
        public string DocUser { get; set; }

        [Required]
        public List<Word> words { get; set; }
        [Required]
        public List<ProfileStopWord> profiles { get; set; }

        public Document()
        {
            profiles = new List<ProfileStopWord>();
            words = new List<Word>();
        }
    }
}

