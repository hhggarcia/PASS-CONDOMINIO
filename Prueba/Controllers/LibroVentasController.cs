using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class LibroVentasController : Controller
    {
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public LibroVentasController(IFiltroFechaRepository filtroFechaRepository,NuevaAppContext context)
        {
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: LibroVentas
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.LibroVentas.Include(l => l.IdCondominioNavigation).Include(l => l.IdFacturaNavigation);
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
