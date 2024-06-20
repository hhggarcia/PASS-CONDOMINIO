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

    public class LibroVentasController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public LibroVentasController(IPDFServices servicesPDF,
            IFiltroFechaRepository filtroFechaRepository,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: LibroVentas
        public async Task<IActionResult> Index()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.LibroVentas
                .Include(l => l.IdCondominioNavigation)
                .Include(l => l.IdFacturaNavigation)
                .Where(c => c.IdCondominio == idCondominio);

            TempData.Keep();

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: LibroVentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libroVenta = await _context.LibroVentas
                .Include(l => l.IdCondominioNavigation)
                .Include(l => l.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (libroVenta == null)
            {
                return NotFound();
            }

            return View(libroVenta);
        }

        // GET: LibroVentas/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura");
            return View();
        }

        // POST: LibroVentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCondominio,IdFactura,BaseImponible,Iva,Total,RetIva,RetIslr,Monto,NumComprobanteRet,FormaPago")] LibroVenta libroVenta)
        {
            ModelState.Remove(nameof(libroVenta.IdCondominioNavigation));
            ModelState.Remove(nameof(libroVenta.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(libroVenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", libroVenta.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", libroVenta.IdFactura);
            return View(libroVenta);
        }

        // GET: LibroVentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libroVenta = await _context.LibroVentas.FindAsync(id);
            if (libroVenta == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", libroVenta.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", libroVenta.IdFactura);
            return View(libroVenta);
        }

        // POST: LibroVentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCondominio,IdFactura,BaseImponible,Iva,Total,RetIva,RetIslr,Monto,NumComprobanteRet,FormaPago")] LibroVenta libroVenta)
        {
            if (id != libroVenta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(libroVenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibroVentaExists(libroVenta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", libroVenta.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", libroVenta.IdFactura);
            return View(libroVenta);
        }

        // GET: LibroVentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libroVenta = await _context.LibroVentas
                .Include(l => l.IdCondominioNavigation)
                .Include(l => l.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (libroVenta == null)
            {
                return NotFound();
            }

            return View(libroVenta);
        }

        // POST: LibroVentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libroVenta = await _context.LibroVentas.FindAsync(id);
            if (libroVenta != null)
            {
                _context.LibroVentas.Remove(libroVenta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ContentResult> LibroPDF([FromBody] IEnumerable<LibroVenta> listaLibroVenta)
        //{
        //    try
        //    {
        //        var modelo = new List<LibroVentasVM>();

        //        foreach (var item in listaLibroVenta)
        //        {
        //            var factura = await _context.FacturaEmitida.FindAsync(item.IdFactura);
        //            if (factura != null)
        //            {
        //                var producto = await _context.Productos.FindAsync(factura.IdProducto);
        //                var cliente  = await _context.Clientes.FindAsync(factura.IdCliente);

        //                modelo.Add(new LibroVentasVM
        //                {
        //                    libroVenta = item,
        //                    FacturaEmitida = factura,
        //                    Producto = producto,
        //                    cliente = cliente
        //                });
        //            }
        //        }                

        //        var data = _servicesPDF.LibroVentas(modelo);
        //        var base64 = Convert.ToBase64String(data);
        //        return Content(base64, "application/pdf");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Error generando PDF: {e.Message}");
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
        //    }
        //}

        public async Task<IActionResult> LibroVentaPDF()
        {

            try
            {
                var modelo = new List<LibroVentasVM>();

                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var listaLibroVenta = _context.LibroVentas
                    .Include(l => l.IdCondominioNavigation)
                    .Include(l => l.IdFacturaNavigation)
                    .Where(c => c.Activo)
                    .Where(c => c.IdCondominio == idCondominio);

                foreach (var item in listaLibroVenta)
                {
                    var factura = await _context.FacturaEmitida.FindAsync(item.IdFactura);
                    if (factura != null)
                    {
                        var producto = await _context.Productos.FindAsync(factura.IdProducto);
                        var cliente = await _context.Clientes.FindAsync(factura.IdCliente);

                        modelo.Add(new LibroVentasVM
                        {
                            libroVenta = item,
                            FacturaEmitida = factura,
                            Producto = producto,
                            cliente = cliente
                        });
                    }
                }

                TempData.Keep();

                var data = _servicesPDF.LibroVentas(modelo);
                //var base64 = Convert.ToBase64String(data);
                //return Content(base64, "application/pdf");
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "LibroVenta" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error generando PDF: {e.Message}");
                //Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }

        private bool LibroVentaExists(int id)
        {
            return _context.LibroVentas.Any(e => e.Id == id);
        }
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var LibroCompras = await _reposFiltroFecha.ObtenerLibroVentas(filtrarFechaVM);
            return View("Index", LibroCompras);
        }
    }
}
