using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.Xml.Linq;
using ITGSA.API.Services;
using ITGSA.API.Models;
using ITGSA.API.Helpers;

namespace ITGSA.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly IDataService _dataService;
        
        public SystemController(IDataService dataService)
        {
            _dataService = dataService;
        }
        
        // GET: /System/limpiarDatos
        [HttpGet("limpiarDatos")]
        public IActionResult LimpiarDatos()
        {
            try
            {
                _dataService.ResetAllData();
                
                var response = new XDocument(
                    new XElement("respuesta",
                        new XElement("estado", "exito"),
                        new XElement("mensaje", "Sistema reiniciado correctamente")
                    )
                );
                
                return Content(response.ToString(), "application/xml");
            }
            catch (Exception ex)
            {
                return Content($@"<?xml version=""1.0""?><respuesta><estado>error</estado><mensaje>{ex.Message}</mensaje></respuesta>", 
                    "application/xml");
            }
        }
        
        // POST: /System/grabarConfiguracion
        [HttpPost("grabarConfiguracion")]
        public async Task<IActionResult> GrabarConfiguracion(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No se recibió ningún archivo");
                
                // Leer y parsear XML
                using var stream = file.OpenReadStream();
                var doc = XDocument.Load(stream);
                
                var clientes = new List<Cliente>();
                var bancos = new List<Banco>();
                
                // Extraer clientes con limpieza de datos
                foreach (var clienteElem in doc.Descendants("cliente"))
                {
                    var nitRaw = clienteElem.Element("NIT")?.Value ?? "";
                    var nombreRaw = clienteElem.Element("nombre")?.Value ?? "";
                    
                    clientes.Add(new Cliente
                    {
                        NIT = RegexHelper.ExtraerNIT(nitRaw),
                        Nombre = nombreRaw.Trim()
                    });
                }
                
                // Extraer bancos
                foreach (var bancoElem in doc.Descendants("banco"))
                {
                    var codigoRaw = bancoElem.Element("codigo")?.Value ?? "";
                    var nombreRaw = bancoElem.Element("nombre")?.Value ?? "";
                    
                    bancos.Add(new Banco
                    {
                        Codigo = RegexHelper.ExtraerNIT(codigoRaw),
                        Nombre = nombreRaw.Trim()
                    });
                }
                
                // Procesar configuración
                var (clientesCreados, clientesActualizados, bancosCreados, bancosActualizados) = 
                    _dataService.ProcesarConfiguracion(clientes, bancos);
                
                // Generar respuesta XML según especificación
                var response = new XDocument(
                    new XElement("respuesta",
                        new XElement("clientes",
                            new XElement("creados", clientesCreados),
                            new XElement("actualizados", clientesActualizados)
                        ),
                        new XElement("bancos",
                            new XElement("creados", bancosCreados),
                            new XElement("actualizados", bancosActualizados)
                        )
                    )
                );
                
                return Content(response.ToString(), "application/xml");
            }
            catch (Exception ex)
            {
                var errorResponse = new XDocument(
                    new XElement("respuesta",
                        new XElement("estado", "error"),
                        new XElement("mensaje", ex.Message)
                    )
                );
                return Content(errorResponse.ToString(), "application/xml");
            }
        }
        
        // POST: /System/grabarTransaccion
        [HttpPost("grabarTransaccion")]
        public async Task<IActionResult> GrabarTransaccion(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No se recibió ningún archivo");
                
                // Leer y parsear XML
                using var stream = file.OpenReadStream();
                var doc = XDocument.Load(stream);
                
                var facturas = new List<Factura>();
                var pagos = new List<Pago>();
                
                // Extraer facturas
                foreach (var facturaElem in doc.Descendants("factura"))
                {
                    var numeroRaw = facturaElem.Element("numeroFactura")?.Value ?? "";
                    var nitRaw = facturaElem.Element("NITcliente")?.Value ?? "";
                    var fechaRaw = facturaElem.Element("fecha")?.Value ?? "";
                    var valorRaw = facturaElem.Element("valor")?.Value ?? "";
                    
                    facturas.Add(new Factura
                    {
                        NumeroFactura = RegexHelper.ExtraerNIT(numeroRaw),
                        NITcliente = RegexHelper.ExtraerNIT(nitRaw),
                        Fecha = RegexHelper.ExtraerFecha(fechaRaw),
                        Valor = RegexHelper.ExtraerValor(valorRaw),
                        SaldoPendiente = 0 // Se calculará después
                    });
                }
                
                // Extraer pagos
                foreach (var pagoElem in doc.Descendants("pago"))
                {
                    var codigoRaw = pagoElem.Element("codigoBanco")?.Value ?? "";
                    var fechaRaw = pagoElem.Element("fecha")?.Value ?? "";
                    var nitRaw = pagoElem.Element("NITcliente")?.Value ?? "";
                    var valorRaw = pagoElem.Element("valor")?.Value ?? "";
                    
                    pagos.Add(new Pago
                    {
                        CodigoBanco = RegexHelper.ExtraerNIT(codigoRaw),
                        Fecha = RegexHelper.ExtraerFecha(fechaRaw),
                        NITcliente = RegexHelper.ExtraerNIT(nitRaw),
                        Valor = RegexHelper.ExtraerValor(valorRaw),
                        Aplicado = false
                    });
                }
                
                // Procesar transacciones
                var (nuevasFacturas, facturasDuplicadas, facturasError, 
                     nuevosPagos, pagosDuplicados, pagosError) = 
                    _dataService.ProcesarTransacciones(facturas, pagos);
                
                // Generar respuesta XML según especificación
                var response = new XDocument(
                    new XElement("transacciones",
                        new XElement("facturas",
                            new XElement("nuevasFacturas", nuevasFacturas),
                            new XElement("facturasDuplicadas", facturasDuplicadas),
                            new XElement("facturasConError", facturasError)
                        ),
                        new XElement("pagos",
                            new XElement("nuevosPagos", nuevosPagos),
                            new XElement("pagosDuplicados", pagosDuplicados),
                            new XElement("pagosConError", pagosError)
                        )
                    )
                );
                
                return Content(response.ToString(), "application/xml");
            }
            catch (Exception ex)
            {
                var errorResponse = new XDocument(
                    new XElement("transacciones",
                        new XElement("estado", "error"),
                        new XElement("mensaje", ex.Message)
                    )
                );
                return Content(errorResponse.ToString(), "application/xml");
            }
        }
        
        // GET: /System/devolverEstadoCuenta?nit=123
        // GET: /System/devolverEstadoCuenta?todos=true
        [HttpGet("devolverEstadoCuenta")]
        public IActionResult DevolverEstadoCuenta([FromQuery] string? nit, [FromQuery] bool todos = false)
        {
            try
            {
                if (todos)
                {
                    // Obtener todos los clientes
                    var clientes = XmlHelper.CargarClientes();
                    var resultados = new List<object>();
                    
                    foreach (var cliente in clientes.OrderBy(c => c.NIT))
                    {
                        var estado = ObtenerEstadoCuentaCliente(cliente.NIT);
                        resultados.Add(estado);
                    }
                    
                    return Ok(resultados);
                }
                else if (!string.IsNullOrEmpty(nit))
                {
                    var estado = ObtenerEstadoCuentaCliente(RegexHelper.ExtraerNIT(nit));
                    return Ok(estado);
                }
                else
                {
                    return BadRequest("Debe especificar nit o todos=true");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        private object ObtenerEstadoCuentaCliente(string nit)
        {
            var clientes = XmlHelper.CargarClientes();
            var cliente = clientes.FirstOrDefault(c => c.NIT == nit);
            
            if (cliente == null)
                return new { error = $"Cliente con NIT {nit} no encontrado" };
            
            var facturas = _dataService.GetFacturasPorCliente(nit);
            var pagos = _dataService.GetPagosPorCliente(nit);
            var saldoFavor = _dataService.GetSaldoFavor(nit);
            
            var deudaTotal = facturas.Where(f => !f.Pagada).Sum(f => f.SaldoPendiente);
            var saldoActual = saldoFavor - deudaTotal;
            
            // Construir transacciones combinadas
            var transacciones = new List<object>();
            
            foreach (var factura in facturas)
            {
                transacciones.Add(new
                {
                    fecha = factura.Fecha,
                    cargo = factura.SaldoPendiente < factura.Valor ? factura.Valor - factura.SaldoPendiente : factura.Valor,
                    abono = 0,
                    descripcion = $"Factura #{factura.NumeroFactura}"
                });
            }
            
            foreach (var pago in pagos)
            {
                transacciones.Add(new
                {
                    fecha = pago.Fecha,
                    cargo = 0,
                    abono = pago.Valor,
                    descripcion = $"Pago - Banco: {pago.CodigoBanco}"
                });
            }
            
            var transaccionesOrdenadas = transacciones
                .OrderByDescending(t => ConvertirFecha(t.GetType().GetProperty("fecha")?.GetValue(t)?.ToString() ?? ""))
                .ToList();
            
            return new
            {
                nit = cliente.NIT,
                nombre = cliente.Nombre,
                saldoActual = saldoActual,
                transacciones = transaccionesOrdenadas
            };
        }
        
        // GET: /System/devolverResumenPagos?mes=3&anio=2024
        [HttpGet("devolverResumenPagos")]
        public IActionResult DevolverResumenPagos([FromQuery] int mes, [FromQuery] int anio)
        {
            try
            {
                var ingresos = _dataService.GetIngresosPorMes(anio, mes, 3);
                
                return Ok(new
                {
                    mesElegido = new DateTime(anio, mes, 1).ToString("MMMM/yyyy"),
                    ultimosTresMeses = ingresos.Select(i => new { mes = i.mes, total = i.total })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        private DateTime ConvertirFecha(string fechaStr)
        {
            if (DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var fecha))
                return fecha;
            return DateTime.MinValue;
        }
    }
}