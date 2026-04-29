using ITGSA.API.Models;

namespace ITGSA.API.Services
{
    public interface IDataService
    {
        // Reset
        void ResetAllData();

        // Configuración
        (int clientesCreados, int clientesActualizados, int bancosCreados, int bancosActualizados)
            ProcesarConfiguracion(List<Cliente> clientes, List<Banco> bancos);

        // Transacciones
        (int nuevasFacturas, int facturasDuplicadas, int facturasError,
          int nuevosPagos, int pagosDuplicados, int pagosError)
            ProcesarTransacciones(List<Factura> facturas, List<Pago> pagos);

        // Consultas
        List<Factura> GetFacturasPorCliente(string nit);
        List<Pago> GetPagosPorCliente(string nit);
        decimal GetSaldoFavor(string nit);
        List<(string mes, decimal total)> GetIngresosPorMes(int anio, int mesInicio, int cantidadMeses);
        //-------------------------------------------------------------------------
        (List<string> bancos, List<Dictionary<string, decimal>> datos) GetIngresosPorBanco(int mes, int anio, int cantidadMeses);
    }
}