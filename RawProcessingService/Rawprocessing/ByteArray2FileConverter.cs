using Common.DataModels;
using System.IO;
using System.Linq;

namespace RawProcessingService
{
    internal class ByteArray2FileConverter
    {
        //recovering a file (given correct fileName path) from a byte array
        public static void ByteArray2File(Document document)
        {
            using (Stream file = File.OpenWrite(document.DocName))
            {
                file.Write(document.DocContent, 0, document.DocContent.Length);
            }
        }

        public static void DeleteFile(Document document)
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] file = directory.GetFiles(document.DocName);
            file.First().Delete();
        }
    }
}