using Common.DataModels;
using System.Collections.Generic;

namespace PlagiarismAlgorithmService.Algorithm
{
    internal class ProcessDocument
    {
        public Document document;
        public List<Profile> profiles = new List<Profile>();
        public List<string> words = new List<string>();
    }
}