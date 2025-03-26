using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Repositories;
using Prueba.ViewModels;
using Prueba.ViewModels.FormaPagoVM;
using SQLitePCL;

namespace Prueba.Controllers
{
    public class BncController : Controller
    {
        private readonly NuevaAppContext _context;
        private readonly ICondominioRepository _repoCondominio;
        private readonly ICuentasContablesRepository _repoCuentas;

        public BncController(NuevaAppContext context,
            ICondominioRepository repoCondominio,
            ICuentasContablesRepository repoCuentas)
        {
            _context = context;
            _repoCondominio = repoCondominio;
            _repoCuentas = repoCuentas;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AjaxCargarRecibos(int valor)
        {
            PagoC2PVM modelo = new PagoC2PVM();

            if (valor > 0)
            {
                var propiedad = await _context.Propiedads.FindAsync(valor);

                if (propiedad != null)
                {
                    var recibos = await (from c in _context.ReciboCobros
                                         where c.IdPropiedad == valor
                                         where !c.Pagado
                                         select c).ToListAsync();

                    modelo.Interes = propiedad.MontoIntereses;
                    modelo.Indexacion = propiedad.MontoMulta != null ? (decimal)propiedad.MontoMulta : 0;
                    modelo.Credito = propiedad.Creditos != null ? (decimal)propiedad.Creditos : 0;
                    modelo.Saldo = propiedad.Saldo;
                    modelo.Deuda = propiedad.Deuda;
                    modelo.Recibos = recibos;

                    if (modelo.Recibos.Any())
                    {
                        modelo.RecibosModel = recibos.Where(c => !c.EnProceso && !c.Pagado)
                            .Select(c => new SelectListItem { Text = c.Fecha.ToString("dd/MM/yyyy"), Value = c.IdReciboCobro.ToString() })
                            .ToList();
                        modelo.ListRecibos = recibos.Select(recibo => new SelectListItem
                        {
                            Text = recibo.Mes + " " + (recibo.ReciboActual ? recibo.Monto : (recibo.Monto + recibo.MontoMora + recibo.MontoIndexacion - recibo.Abonado)).ToString("N") + "Bs",
                            Value = recibo.IdReciboCobro.ToString(),
                            Selected = false,
                        }).ToList();

                    }
                }
            }

            return Json(modelo);
        }

        public IActionResult ObtenerOperacionesPorBanco(int idBanco)
        {
            // Lógica para obtener las operaciones del banco
            var operaciones = new List<SelectListItem>
            {
                new SelectListItem { Value = "RF", Text = "RF" },
                new SelectListItem { Value = "TC", Text = "TC" },
                new SelectListItem { Value = "OTP", Text = "OTP" }
            };

            return Json(operaciones);
        }
        public async Task<IActionResult> C2PForm()
        {
            string idPropietario = TempData.Peek("idUserLog").ToString();

            var inquilino = await _context.Inquilinos.FirstOrDefaultAsync(c => c.IdUsuario == idPropietario);
            var model = new PagoC2PVM();
            if (inquilino != null)
            {
                var propiedades = await _context.Propiedads.Where(c => c.IdPropiedad == inquilino.IdPropiedad).ToListAsync();
                model.Propiedades = propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToList();
            }
            else
            {
                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();
                model.Propiedades = propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToList();
            }

            // BUSCAR LLAMANDO A LA API

            model.Bancos = new List<SelectListItem>
            {
                new SelectListItem { Value = "0105", Text = "Banco de Venezuela" },
                new SelectListItem { Value = "0102", Text = "Mercantil" },
                new SelectListItem { Value = "0172", Text = "Bancamiga" }
            };

            TempData.Keep();
            return View(model);
        }
    }
}
