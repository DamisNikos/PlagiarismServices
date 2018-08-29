using System.Runtime.Serialization;

namespace Common.DataModels
{
    public class OldStopWord
    {
        public int stopWordID { get; set; }
        public int index { get; set; }
        public string word { get; set; }

        public int stopNgramID { get; set; }

        [IgnoreDataMember]
        public OldStopNGram stopNGram { get; set; }
    }
}