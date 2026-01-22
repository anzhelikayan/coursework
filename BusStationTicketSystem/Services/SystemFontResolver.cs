using PdfSharp.Fonts;

namespace BusStationTicketSystem.Services
{
    /// <summary>
    /// FontResolver для PdfSharp, який використовує системні шрифти Windows
    /// </summary>
    public class SystemFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Повертаємо інформацію про шрифт
            // PdfSharp на Windows автоматично знайде системні шрифти
            // Якщо шрифт не знайдено, використовуємо Arial як запасний варіант
            string fontName = familyName;
            
            // Перевірка популярних шрифтів
            if (string.IsNullOrEmpty(fontName) || 
                (fontName != "Arial" && fontName != "Times New Roman" && 
                 fontName != "Courier New" && fontName != "Calibri"))
            {
                fontName = "Arial"; // За замовчуванням використовуємо Arial
            }
            
            return new FontResolverInfo(fontName, isBold, isItalic);
        }

        public byte[]? GetFont(string faceName)
        {
            // Для Windows PdfSharp використовує системні шрифти через GDI+
            // Повертаємо null, щоб PdfSharp використав вбудований механізм
            return null;
        }
    }
}

