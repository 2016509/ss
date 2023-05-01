using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace StockAppliance.Methods
{
    internal class PDFData
    {
        public static string PdfText(string path)
        {
            PdfReader reader = new(path);
            string text = string.Empty;
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                text += PdfTextExtractor.GetTextFromPage(reader, page);
            }
            reader.Close();
            return text;
        }
    }
}
