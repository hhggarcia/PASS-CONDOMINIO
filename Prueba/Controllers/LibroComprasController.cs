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

    public class LibroComprasController : Controller
    {
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public LibroComprasController(IFiltroFechaRepository filtroFechaRepository, NuevaAppContext context)
        {
            _reposFiltroFecha=  filtroFechaRepository;
            _context = context;
        }

        // GET: LibroCompras
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.LibroCompras.Include(l => l.IdCondominioNavigation).Include(l => l.IdFacturaNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: LibroCompras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libroCompra = await _context.LibroCompras
                .Include(l => l.IdCondominioNavigation)
                .Include(l => l.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (libroCompra == null)
            {
                return NotFound();
            }

            return View(libroCompra);
        }

        // GET: LibroCompras/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura");
            return View();
        }

        // POST: LibroCompras/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCondominio,IdFactura,BaseImponible,ExentoIva,Iva,Igtf,RetIva,RetIslr,Monto,NumComprobanteRet,FechaComprobanteRet,FormaPago")] LibroCompra libroCompra)
        {
            ModelState.Remove(nameof(libroCompra.IdCondominioNavigation));
            ModelState.Remove(nameof(libroCompra.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(libroCompra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", libroCompra.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", libroCompra.IdFactura);
            return View(libroCompra);
        }

        // GET: LibroCompras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libroCompra = await _context.LibroCompras.FindAsync(id);
            if (libroCompra == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", libroCompra.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", libroCompra.IdFactura);
            return View(libroCompra);
        }

        // POST: LibroCompras/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCondominio,IdFactura,BaseImponible,ExentoIva,Iva,Igtf,RetIva,RetIslr,Monto,NumComprobanteRet,FechaComprobanteRet,FormaPago")] LibroCompra libroCompra)
        {
            if (id != libroCompra.Id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(libroCompra.IdCondominioNavigation));
            ModelState.Remove(nameof(libroCompra.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(libroCompra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibroCompraExists(libroCompra.Id))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", libroCompra.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", libroCompra.IdFactura);
            return View(libroCompra);
        }

        // GET: LibroCompras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libroCompra = await _context.LibroCompras
                .Include(l => l.IdCondominioNavigation)
                .Include(l => l.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (libroCompra == null)
            {
                return NotFound();
            }

            return View(libroCompra);
        }

        // POST: LibroCompras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libroCompra = await _context.LibroCompras.FindAsync(id);
            if (libroCompra != null)
            {
                _context.LibroCompras.Remove(libroCompra);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LibroCompraExists(int id)
        {
            return _context.LibroCompras.Any(e => e.Id == id);
        }
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var LibroCompras = await _reposFiltroFecha.ObtenerLibroCompras(filtrarFechaVM);
            return View("Index", LibroCompras);
        }
    }
}
