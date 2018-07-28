using System.Runtime.Serialization;

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