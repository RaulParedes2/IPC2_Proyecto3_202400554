using System.Text.RegularExpressions;

namespace ITGSA.API.Helpers
{
    public static class RegexHelper
    {
        // NIT de texto sucio (ej: "NIT: 123456-K" o "1234567890123-1")
        public static string ExtractNIT(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            var patron = @"\b(\d{4,15}[-\s]?[Kk0-9]?)\b";
            var match = Regex.Match(texto, patron);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        // Extraer fecha en formato dd/mm/yyyy (ignorando texto extra)
        public static string ExtraerFecha(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            /*var patron = @"\b(0?[1-9]|[12][0-9]|3[01])/(0?[1-9]|1[0-2])/\d{4}\b";*/
            var patron = @"(\d{2})[/-](\d{2})[/-](\d{4})";
            var match = Regex.Match(texto, patron);
            if (match.Success)
            {
                return $"{match.Groups[1].Value}/{match.Groups[2].Value}/{match.Groups[3].Value}";
            }
            return texto.Trim();
        }

        // Extraer valor numérico de texto (ej: "Q 100.00" o "100.00")
        public static decimal ExtrerValor(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return 0;

            /*var patron = @"\bQ?\s?(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)\b";*/
            var patron = @"\d+(?:[.,]\d+)?";
            var match = Regex.Match(texto, patron);
            if (match.Success)
            {
                var valorStr = match.Value.Replace('.', ',');
                if (decimal.TryParse(valorStr, out decimal resultado))
                    return resultado;
            }
            return 0;
        }

        
    }
}