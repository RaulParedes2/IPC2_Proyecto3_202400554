using System.Text.RegularExpressions;

namespace ITGSA.API.Helpers
{
    public static class RegexHelper
    {
        // Extraer NIT de texto sucio (ej: "NIT: 123456-K" o "1234567890123-1")
        public static string ExtraerNIT(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            
            var patron = @"\b(\d{4,15}[-\s]?[Kk0-9]?)\b";
            var match = Regex.Match(texto, patron);
            return match.Success ? match.Groups[1].Value.Trim() : texto.Trim();
        }

        // Extraer fecha en formato dd/mm/yyyy (ignorando texto extra)
        public static string ExtraerFecha(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            
            var patron = @"(\d{2})[/-](\d{2})[/-](\d{4})";
            var match = Regex.Match(texto, patron);
            if (match.Success)
            {
                return $"{match.Groups[1].Value}/{match.Groups[2].Value}/{match.Groups[3].Value}";
            }
            return texto.Trim();
        }

        // Extraer valor numérico de texto (ej: "Q 100.00" o "100.00")
        public static decimal ExtraerValor(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return 0;
            
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