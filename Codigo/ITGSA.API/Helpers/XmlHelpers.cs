using System.Xml;
using System.Xml.Linq;
using ITGSA.API.Models;

namespace ITGSA.API.Helpers
{
    public static class XmlHelper
    {
        private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        
        // Asegurar que la carpeta Data existe
        public static void EnsureDataDirectory()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
        }

        // ========== CLIENTES ==========
        public static List<Cliente> CargarClientes()
        {
            var path = Path.Combine(DataPath, "clientes.xml");
            if (!File.Exists(path)) return new List<Cliente>();
            
            var doc = XDocument.Load(path);
            return doc.Descendants("cliente").Select(c => new Cliente
            {
                NIT = c.Element("NIT")?.Value ?? "",
                Nombre = c.Element("nombre")?.Value ?? ""
            }).ToList();
        }

        public static void GuardarClientes(List<Cliente> clientes)
        {
            var doc = new XDocument(
                new XElement("clientes",
                    clientes.Select(c => new XElement("cliente",
                        new XElement("NIT", c.NIT),
                        new XElement("nombre", c.Nombre)
                    ))
                )
            );
            doc.Save(Path.Combine(DataPath, "clientes.xml"));
        }

        // ========== BANCOS ==========
        public static List<Banco> CargarBancos()
        {
            var path = Path.Combine(DataPath, "bancos.xml");
            if (!File.Exists(path)) return new List<Banco>();
            
            var doc = XDocument.Load(path);
            return doc.Descendants("banco").Select(b => new Banco
            {
                Codigo = b.Element("codigo")?.Value ?? "",
                Nombre = b.Element("nombre")?.Value ?? ""
            }).ToList();
        }

        public static void GuardarBancos(List<Banco> bancos)
        {
            var doc = new XDocument(
                new XElement("bancos",
                    bancos.Select(b => new XElement("banco",
                        new XElement("codigo", b.Codigo),
                        new XElement("nombre", b.Nombre)
                    ))
                )
            );
            doc.Save(Path.Combine(DataPath, "bancos.xml"));
        }

        // ========== FACTURAS ==========
        public static List<Factura> CargarFacturas()
        {
            var path = Path.Combine(DataPath, "facturas.xml");
            if (!File.Exists(path)) return new List<Factura>();
            
            var doc = XDocument.Load(path);
            return doc.Descendants("factura").Select(f => new Factura
            {
                NumeroFactura = f.Element("numeroFactura")?.Value ?? "",
                NITcliente = f.Element("NITcliente")?.Value ?? "",
                Fecha = f.Element("fecha")?.Value ?? "",
                Valor = decimal.Parse(f.Element("valor")?.Value ?? "0"),
                SaldoPendiente = decimal.Parse(f.Element("saldoPendiente")?.Value ?? "0")
            }).ToList();
        }

        public static void GuardarFacturas(List<Factura> facturas)
        {
            var doc = new XDocument(
                new XElement("facturas",
                    facturas.Select(f => new XElement("factura",
                        new XElement("numeroFactura", f.NumeroFactura),
                        new XElement("NITcliente", f.NITcliente),
                        new XElement("fecha", f.Fecha),
                        new XElement("valor", f.Valor),
                        new XElement("saldoPendiente", f.SaldoPendiente)
                    ))
                )
            );
            doc.Save(Path.Combine(DataPath, "facturas.xml"));
        }

        // ========== PAGOS ==========
        public static List<Pago> CargarPagos()
        {
            var path = Path.Combine(DataPath, "pagos.xml");
            if (!File.Exists(path)) return new List<Pago>();
            
            var doc = XDocument.Load(path);
            return doc.Descendants("pago").Select(p => new Pago
            {
                CodigoBanco = p.Element("codigoBanco")?.Value ?? "",
                Fecha = p.Element("fecha")?.Value ?? "",
                NITcliente = p.Element("NITcliente")?.Value ?? "",
                Valor = decimal.Parse(p.Element("valor")?.Value ?? "0"),
                Aplicado = bool.Parse(p.Element("aplicado")?.Value ?? "false")
            }).ToList();
        }

        public static void GuardarPagos(List<Pago> pagos)
        {
            var doc = new XDocument(
                new XElement("pagos",
                    pagos.Select(p => new XElement("pago",
                        new XElement("codigoBanco", p.CodigoBanco),
                        new XElement("fecha", p.Fecha),
                        new XElement("NITcliente", p.NITcliente),
                        new XElement("valor", p.Valor),
                        new XElement("aplicado", p.Aplicado)
                    ))
                )
            );
            doc.Save(Path.Combine(DataPath, "pagos.xml"));
        }

        // ========== SALDOS FAVOR ==========
        public static List<SaldoFavor> CargarSaldosFavor()
        {
            var path = Path.Combine(DataPath, "saldosfavor.xml");
            if (!File.Exists(path)) return new List<SaldoFavor>();
            
            var doc = XDocument.Load(path);
            return doc.Descendants("saldo").Select(s => new SaldoFavor
            {
                NITcliente = s.Element("NITcliente")?.Value ?? "",
                Monto = decimal.Parse(s.Element("monto")?.Value ?? "0")
            }).ToList();
        }

        public static void GuardarSaldosFavor(List<SaldoFavor> saldos)
        {
            var doc = new XDocument(
                new XElement("saldosFavor",
                    saldos.Select(s => new XElement("saldo",
                        new XElement("NITcliente", s.NITcliente),
                        new XElement("monto", s.Monto)
                    ))
                )
            );
            doc.Save(Path.Combine(DataPath, "saldosfavor.xml"));
        }

        // Limpiar todos los datos
        public static void LimpiarTodosLosDatos()
        {
            EnsureDataDirectory();
            
            var archivos = new[] { "clientes.xml", "bancos.xml", "facturas.xml", "pagos.xml", "saldosfavor.xml" };
            foreach (var archivo in archivos)
            {
                var path = Path.Combine(DataPath, archivo);
                if (File.Exists(path))
                    File.Delete(path);
            }
            
            // Crear archivos vacíos
            GuardarClientes(new List<Cliente>());
            GuardarBancos(new List<Banco>());
            GuardarFacturas(new List<Factura>());
            GuardarPagos(new List<Pago>());
            GuardarSaldosFavor(new List<SaldoFavor>());
        }
    }
}