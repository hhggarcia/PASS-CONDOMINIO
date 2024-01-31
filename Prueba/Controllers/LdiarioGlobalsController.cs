using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class LdiarioGlobalsController : Controller
    {
        private readonly ILibroDiarioRepository _repoLibroDiario;
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;
        private readonly IPDFServices _servicePDF;

        public LdiarioGlobalsController(ILibroDiarioRepository repoLibroDiario,
          IPDFServices PDFService,
          IFiltroFechaRepository filtroFechaRepository,
            NuevaAppContext context)
        {
            _reposFiltroFecha=filtroFechaRepository;
            _repoLibroDiario = repoLibroDiario;
            _servicePDF = PDFService;
            _context = context;
        }

        // GET: LdiarioGlobals
        //public async Task<IActionResult> Index()
        //{
        //    var NuevaAppContext = _context.LdiarioGlobals.Include(l => l.IdCodCuentaNavigation);
        //    return View(await NuevaAppContext.ToListAsync());
        //}

        // GET: LdiarioGlobals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.LdiarioGlobals == null)
            {
                return NotFound();
            }

            var ldiarioGlobal = await _context.LdiarioGlobals
                .Include(l => l.IdCodCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdAsiento == id);
            if (ldiarioGlobal == null)
            {
                return NotFound();
            }

            return View(ldiarioGlobal);
        }

        // GET: LdiarioGlobals/Create
        public IActionResult Create()
        {
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            //ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor");
            return View();
        }

        // POST: LdiarioGlobals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAsiento,IdCodCuenta,Fecha,Concepto,Monto,TipoOperacion,NumAsiento, IdDolar")] LdiarioGlobal ldiarioGlobal)
        {
            if (ModelState.IsValid)
            {
                var result = await _repoLibroDiario.Crear(ldiarioGlobal);
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", ldiarioGlobal.IdCodCuenta);
            //ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor", ldiarioGlobal.IdDolar);

            return View(ldiarioGlobal);
        }

        // GET: LdiarioGlobals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.LdiarioGlobals == null)
            {
                return NotFound();
            }

            var ldiarioGlobal = await _context.LdiarioGlobals.FindAsync(id);
            if (ldiarioGlobal == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", ldiarioGlobal.IdCodCuenta);
            //ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor", ldiarioGlobal.IdDolar);
            return View(ldiarioGlobal);
        }

        // POST: LdiarioGlobals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAsiento,IdCodCuenta,Fecha,Concepto,Monto,TipoOperacion,NumAsiento,IdDolar")] LdiarioGlobal ldiarioGlobal)
        {
            if (id != ldiarioGlobal.IdAsiento)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                var result = await _repoLibroDiario.Editar(ldiarioGlobal);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repoLibroDiario.LdiarioGlobalExists(ldiarioGlobal.IdAsiento))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", ldiarioGlobal.IdCodCuenta);
            //ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor", ldiarioGlobal.IdDolar);

            //return View(ldiarioGlobal);
        }

        // GET: LdiarioGlobals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.LdiarioGlobals == null)
            {
                return NotFound();
            }

            var ldiarioGlobal = await _context.LdiarioGlobals
                .Include(l => l.IdCodCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdAsiento == id);
            if (ldiarioGlobal == null)
            {
                return NotFound();
            }

            return View(ldiarioGlobal);
        }

        // POST: LdiarioGlobals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.LdiarioGlobals == null)
            {
                return Problem("Entity set 'NuevaAppContext.LdiarioGlobals'  is null.");
            }

            var result = await _repoLibroDiario.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                //traer subcuentas del condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                LibroDiarioVM modelo = _repoLibroDiario.LibroDiario(idCondominio);

                TempData.Keep();

                return View(modelo);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }
       
        [HttpPost]
        public ContentResult LibroDiarioPDF([FromBody] LibroDiarioVM ldiarioGlobal)
        {
            try
            {
                if(ldiarioGlobal== null)
                {

                }
                var data = _servicePDF.LibroDiarioPDF(ldiarioGlobal);
                var base64 = Convert.ToBase64String(data);
                return Content(base64, "application/pdf");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generando PDF: {e.Message}");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var filtrarFecha = await _reposFiltroFecha.ObtenerLdiarioGlobals(idCondominio, filtrarFechaVM);
            return View("Index", filtrarFecha);
        }
        //[HttpGet]
        //public async Task<IActionResult> LibroDiarioPDF2()
        //{
        //    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
        //    LibroDiarioVM modelo = _repoLibroDiario.LibroDiario(idCondominio);
        //    var data = _servicePDF.LibroDiarioPDF(modelo);
        //    Stream stream = new MemoryStream(data);
        //    return File(stream, "application/pdf", "LibroDiario.pdf");
        //}
    }
}
