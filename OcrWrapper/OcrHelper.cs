using System;
using Tesseract;

namespace OcrWrapper
{
    // trong project khác
    public class OcrWrapper
    {
        public string GetText(string imagePath)
        {
            using var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }
    }
}
