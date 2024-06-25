using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;
using SQLitePCL;

namespace Prueba.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IReportesRepository _repoReportes;
        private readonly IPdfReportesServices _servicesPDF;
        private readonly NuevaAppContext _context;

        public ReportesController(IReportesRepository repoReportes,
            IPdfReportesServices servicesPDF,
            NuevaAppContext context)
        {
            _repoReportes = repoReportes;
            _servicesPDF = servicesPDF;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> EstadoCuentas()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var condominio = await _context.Condominios.FindAsync(IdCondominio);

            if (condominio != null)
            {
                var propiedades = await _context.Propiedads.Where(c => c.IdCondominio == IdCondominio).ToListAsync();
                var modelo = new List<EstadoCuentasVM>();

                if (propiedades != null && propiedades.Any())
                {
                    foreach (var propiedad in propiedades)
                    {
                        var usuario = await _context.AspNetUsers.FirstAsync(c => c.Id == propiedad.IdUsuario);
                        var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == propiedad.IdPropiedad && !c.Pagado).ToListAsync();

                        modelo.Add(new EstadoCuentasVM()
                        {
                            Condominio = condominio,
                            Propiedad = propiedad,
                            User = usuario,
                            ReciboCobro = recibos
                        });
                    }

                    TempData.Keep();
                    var data = _servicesPDF.EstadoCuentas(modelo);
                    Stream stream = new MemoryStream(data);
                    return File(stream, "application/pdf", "EstadoCuentasOficina_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
                }
                
            }

            return RedirectToAction("Dashboard", "Administrator");
        }

        [HttpGet]
        public async Task<IActionResult> DeudoresPDF()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

            if (modelo.Propiedades != null && modelo.Propiedades.Any()
                && modelo.Propietarios != null && modelo.Propietarios.Any()
                && modelo.Recibos != null && modelo.Recibos.Any())
            {
                var data = await _servicesPDF.Deudores(modelo, idCondominio);
                Stream stream = new MemoryStream(data);
                TempData.Keep();
                return File(stream, "application/pdf", "Deudores_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }

            TempData.Keep();

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> DeudoresResumenPDF()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

            if (modelo.Propiedades != null && modelo.Propiedades.Any()
                && modelo.Propietarios != null && modelo.Propietarios.Any()
                && modelo.Recibos != null && modelo.Recibos.Any())
            {
                var data = await _servicesPDF.DeudoresResumen(modelo, idCondominio);
                Stream stream = new MemoryStream(data);
                TempData.Keep();
                return File(stream, "application/pdf", "Deudores_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }

            TempData.Keep();

            return RedirectToAction("Index");

        }
    }
}
