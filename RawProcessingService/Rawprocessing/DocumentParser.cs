using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DataModels;
using System.IO;
using Toxy;
using System.Fabric;

namespace RawProcessingService.Rawprocessing
{
    public class DocumentParser
    {
        public static List<Word> GetText(string docPath, StatelessServiceContext serviceContext)
        {
            string text = null;
            string extension = Path.GetExtension(docPath);
            try
            {
                ParserContext context = new ParserContext(docPath);
                if (extension.Equals(".txt"))
                {
                    ITextParser parser = ParserFactory.CreateText(context);
                    text = parser.Parse().ToString().ToLower().Replace('\n', ' ').Replace('\r', ' ')
                .Replace('\t', ' ');
                }
                else if (extension.Equals(".pdf") || extension.Equals(".docx") || extension.Equals(".doc"))
                {
                    IDocumentParser parser = ParserFactory.CreateDocument(context);
                    text = parser.Parse().ToString().ToLower().Replace('\n', ' ').Replace('\r', ' ')
                .Replace('\t', ' ');
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(serviceContext, $"Exception found at GetText() : {e.Message}");     
            }
            text = RemovePunctuation(text);
            string[] words = text.Split(default(Char[]), StringSplitOptions.RemoveEmptyEntries);

            List<Word> listOfWords = new List<Word>();
            foreach (string word in words)
            {
                listOfWords.Add(new Word() { word = word });
            }
            return listOfWords;
        }

        //Removes punctuation
        private static string RemovePunctuation(string s)
        {
            var sb = new StringBuilder();

            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
            }

            s = sb.ToString();
            return s;
        }
    }
}
