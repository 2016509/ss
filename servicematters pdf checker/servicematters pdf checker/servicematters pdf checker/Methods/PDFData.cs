using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace servicematters_pdf_checker.Methods
{
    internal class PDFData
    {
        public static string pdfText(Stream stream)
        {

            PdfReader reader = new PdfReader(stream);
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
