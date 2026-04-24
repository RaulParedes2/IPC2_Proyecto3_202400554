using ITGSA.API.Models;
using ITGSA.API.Helpers;

namespace ITGSA.API.Services
{
    public class DataService : IDataService
    {
        public void ResetAllData()
        {
            XmlHelper.LimpiarTodosLosDatos();
        }

        public (int, int, int, int) ProcesarConfiguracion(List<Cliente> clientes, List<Banco> bancos)
        {
            // Cargar datos existentes
            var clientesExistentes = XmlHelper.CargarClientes();
            var bancosExistentes = XmlHelper.CargarBancos();

            int clientesCreados = 0, clientesActualizados = 0;
            int bancosCreados = 0, bancosActualizados = 0;

            // Procesar clientes (UPSERT por NIT)
            foreach (var cliente in clientes)
            {
                var existente = clientesExistentes.FirstOrDefault(c => c.NIT == cliente.NIT);
                if (existente == null)
                {
                    clientesExistentes.Add(cliente);
                    clientesCreados++;
                }
                else
                {
                    existente.Nombre = cliente.Nombre;
                    clientesActualizados++;
                }
            }

            // Procesar bancos (UPSERT por código)
            foreach (var banco in bancos)
            {
                var existente = bancosExistentes.FirstOrDefault(b => b.Codigo == banco.Codigo);
                if (existente == null)
                {
                    bancosExistentes.Add(banco);
                    bancosCreados++;
                }
                else
                {
                    existente.Nombre = banco.Nombre;
                    bancosActualizados++;
                }
            }

            // Guardar cambios
            XmlHelper.GuardarClientes(clientesExistentes);
            XmlHelper.GuardarBancos(bancosExistentes);

            return (clientesCreados, clientesActualizados, bancosCreados, bancosActualizados);
        }

        public (int, int, int, int, int, int) ProcesarTransacciones(
            List<Factura> facturas, List<Pago> pagos)
        {
            // Cargar datos existentes
            var facturasExistentes = XmlHelper.CargarFacturas();
            var pagosExistentes = XmlHelper.CargarPagos();
            var saldosFavor = XmlHelper.CargarSaldosFavor();

            int nuevasFacturas = 0, facturasDuplicadas = 0, facturasError = 0;
            int nuevosPagos = 0, pagosDuplicados = 0, pagosError = 0;

            // Procesar facturas (solo insert, no update)
            foreach (var factura in facturas)
            {
                // Validar datos
                if (string.IsNullOrWhiteSpace(factura.NumeroFactura) ||
                    string.IsNullOrWhiteSpace(factura.NITcliente) ||
                    factura.Valor <= 0)
                {
                    facturasError++;
                    continue;
                }

                // Verificar si ya existe
                var existe = facturasExistentes.Any(f => f.NumeroFactura == factura.NumeroFactura);
                if (existe)
                {
                    facturasDuplicadas++;
                    continue;
                }

                // Nueva factura: el saldo pendiente es el valor total
                factura.SaldoPendiente = factura.Valor;
                facturasExistentes.Add(factura);
                nuevasFacturas++;
            }

            // Procesar pagos (solo insert)
            foreach (var pago in pagos)
            {
                // Validar datos
                if (string.IsNullOrWhiteSpace(pago.CodigoBanco) ||
                    string.IsNullOrWhiteSpace(pago.NITcliente) ||
                    pago.Valor <= 0)
                {
                    pagosError++;
                    continue;
                }

                // Verificar duplicado (mismo banco, cliente, fecha y valor)
                var existe = pagosExistentes.Any(p =>
                    p.CodigoBanco == pago.CodigoBanco &&
                    p.NITcliente == pago.NITcliente &&
                    p.Fecha == pago.Fecha &&
                    p.Valor == pago.Valor);

                if (existe)
                {
                    pagosDuplicados++;
                    continue;
                }

                pagosExistentes.Add(pago);
                nuevosPagos++;
            }

            // Guardar facturas y pagos antes de aplicar pagos
            XmlHelper.GuardarFacturas(facturasExistentes);
            XmlHelper.GuardarPagos(pagosExistentes);

            // Aplicar pagos a facturas (los pagos nuevos y los no aplicados)
            AplicarPagosAFacturas(facturasExistentes, pagosExistentes, saldosFavor);

            // Guardar todo actualizado
            XmlHelper.GuardarFacturas(facturasExistentes);
            XmlHelper.GuardarPagos(pagosExistentes);
            XmlHelper.GuardarSaldosFavor(saldosFavor);

            return (nuevasFacturas, facturasDuplicadas, facturasError,
                    nuevosPagos, pagosDuplicados, pagosError);
        }

        private void AplicarPagosAFacturas(List<Factura> facturas, List<Pago> pagos, List<SaldoFavor> saldosFavor)
        {
            // Agrupar pagos no aplicados por cliente
            var pagosNoAplicados = pagos.Where(p => !p.Aplicado).ToList();
            var pagosPorCliente = pagosNoAplicados.GroupBy(p => p.NITcliente);

            foreach (var grupo in pagosPorCliente)
            {
                var nitCliente = grupo.Key;
                var montoTotalPagos = grupo.Sum(p => p.Valor);

                // Obtener saldo a favor existente
                var saldoExistente = saldosFavor.FirstOrDefault(s => s.NITcliente == nitCliente);
                decimal saldoFavorCliente = saldoExistente?.Monto ?? 0;

                // Total disponible = pagos nuevos + saldo a favor
                decimal totalDisponible = montoTotalPagos + saldoFavorCliente;

                // Obtener facturas del cliente con saldo pendiente, ordenadas por fecha (más antigua primero)
                var facturasCliente = facturas
                    .Where(f => f.NITcliente == nitCliente && f.SaldoPendiente > 0)
                    .OrderBy(f => ConvertirFecha(f.Fecha))
                    .ToList();

                // Aplicar el monto a las facturas
                foreach (var factura in facturasCliente)
                {
                    if (totalDisponible <= 0) break;

                    if (totalDisponible >= factura.SaldoPendiente)
                    {
                        // Paga la factura completa
                        totalDisponible -= factura.SaldoPendiente;
                        factura.SaldoPendiente = 0;
                    }
                    else
                    {
                        // Paga parcialmente
                        factura.SaldoPendiente -= totalDisponible;
                        totalDisponible = 0;
                    }
                }

                // Actualizar saldo a favor
                if (totalDisponible > 0)
                {
                    if (saldoExistente != null)
                        saldoExistente.Monto = totalDisponible;
                    else
                        saldosFavor.Add(new SaldoFavor { NITcliente = nitCliente, Monto = totalDisponible });
                }
                else if (saldoExistente != null && totalDisponible <= 0)
                {
                    saldosFavor.Remove(saldoExistente);
                }

                // Marcar pagos como aplicados
                foreach (var pago in grupo)
                {
                    pago.Aplicado = true;
                }
            }
        }

        private DateTime ConvertirFecha(string fechaStr)
        {
            if (DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var fecha))
                return fecha;
            return DateTime.MinValue;
        }

        public List<Factura> GetFacturasPorCliente(string nit)
        {
            var facturas = XmlHelper.CargarFacturas();
            return facturas.Where(f => f.NITcliente == nit).OrderByDescending(f => ConvertirFecha(f.Fecha)).ToList();
        }

        public List<Pago> GetPagosPorCliente(string nit)
        {
            var pagos = XmlHelper.CargarPagos();
            return pagos.Where(p => p.NITcliente == nit).OrderByDescending(p => ConvertirFecha(p.Fecha)).ToList();
        }

        public decimal GetSaldoFavor(string nit)
        {
            var saldos = XmlHelper.CargarSaldosFavor();
            return saldos.FirstOrDefault(s => s.NITcliente == nit)?.Monto ?? 0;
        }

        public List<(string mes, decimal total)> GetIngresosPorMes(int anio, int mesInicio, int cantidadMeses)
        {
            var facturas = XmlHelper.CargarFacturas();
            var pagosAplicados = XmlHelper.CargarPagos().Where(p => p.Aplicado).ToList();

            var resultados = new List<(string mes, decimal total)>();

            for (int i = 0; i < cantidadMeses; i++)
            {
                int mesActual = mesInicio - i;
                int anioActual = anio;

                if (mesActual <= 0)
                {
                    mesActual += 12;
                    anioActual--;
                }

                // Sumar pagos aplicados en ese mes
                var pagosMes = pagosAplicados
                    .Where(p => ConvertirFecha(p.Fecha).Year == anioActual &&
                                ConvertirFecha(p.Fecha).Month == mesActual)
                    .Sum(p => p.Valor);

                var nombreMes = new DateTime(anioActual, mesActual, 1).ToString("MMMM/yyyy");
                resultados.Add((nombreMes, pagosMes));
            }

            return resultados;
        }
    }
}