namespace ITGSA.API.Models
{
    public class Pago
    {
        public string CodigoBanco { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string NITcliente { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public bool Aplicado { get; set; } = false;
    }
}