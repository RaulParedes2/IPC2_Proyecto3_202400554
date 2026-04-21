namespace ITGSA.API.Models
{
    public class Factura
    {
        
        public string NITcliente { get; set; } = string.Empty;
        public string NumeroFactura { get; set; } = string.Empty;
        public string Fecha { get; set; } = string .Empty;
        public decimal Valor { get; set; }
        public decimal SaldoPendiente { get; set; }
        public bool Pagada => SaldoPendiente <= 0;
        
    }
}