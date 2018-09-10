using Common.DataModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace PlagiarismAlgorithmService.Algorithm
{
    internal class DataRetriever
    {
        public static ProcessDocument RetrieveDocument(string documentHash)
        {
            ProcessDocument processDocument = new ProcessDocument();

            using (var originalcontext = new DocumentContext())
            {
                processDocument.document = originalcontext.Documents
                    .AsNoTracking()
                    .Where(n => n.DocHash.Equals(documentHash))
                    .OrderByDescending(n => n.DocumentID).FirstOrDefault();

                processDocument.profiles = JsonConvert.DeserializeObject<List<Profile>>(processDocument.document.profiles);
                processDocument.words = JsonConvert.DeserializeObject<List<string>>(processDocument.document.words);
                foreach (Profile profile in processDocument.profiles)
                {
                    StopWordListGenerator.GenerateStopWordList(profile);
                }
            }

            return processDocument;
        }
    }
}