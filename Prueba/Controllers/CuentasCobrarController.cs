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

    public class CuentasCobrarController : Controller
    {
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public CuentasCobrarController(IFiltroFechaRepository filtroFechaRepository,NuevaAppContext context)
        {
            _reposFiltroFecha=filtroFechaRepository;
            _context = context;
        }

        // GET: CuentasCobrar
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.CuentasCobrars.Include(c => c.IdCondominioNavigation).Include(c => c.IdFacturaNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CuentasCobrar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasCobrar = await _context.CuentasCobrars
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentasCobrar == null)
            {
                return NotFound();
            }

            return View(cuentasCobrar);
        }

        // GET: CuentasCobrar/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura");
            return View();
        }

        // POST: CuentasCobrar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCondominio,IdFactura,Monto,Status")] CuentasCobrar cuentasCobrar)
        {
            ModelState.Remove(nameof(cuentasCobrar.IdCondominioNavigation));
            ModelState.Remove(nameof(cuentasCobrar.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(cuentasCobrar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasCobrar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", cuentasCobrar.IdFactura);
            return View(cuentasCobrar);
        }

        // GET: CuentasCobrar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasCobrar = await _context.CuentasCobrars.FindAsync(id);
            if (cuentasCobrar == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasCobrar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", cuentasCobrar.IdFactura);
            return View(cuentasCobrar);
        }

        // POST: CuentasCobrar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCondominio,IdFactura,Monto,Status")] CuentasCobrar cuentasCobrar)
        {
            if (id != cuentasCobrar.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(cuentasCobrar.IdCondominioNavigation));
            ModelState.Remove(nameof(cuentasCobrar.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentasCobrar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentasCobrarExists(cuentasCobrar.Id))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasCobrar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", cuentasCobrar.IdFactura);
            return View(cuentasCobrar);
        }

        // GET: CuentasCobrar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasCobrar = await _context.CuentasCobrars
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentasCobrar == null)
            {
                return NotFound();
            }

            return View(cuentasCobrar);
        }

        // POST: CuentasCobrar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuentasCobrar = await _context.CuentasCobrars.FindAsync(id);
            if (cuentasCobrar != null)
            {
                _context.CuentasCobrars.Remove(cuentasCobrar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentasCobrarExists(int id)
        {
            return _context.CuentasCobrars.Any(e => e.Id == id);
        }
        [HttpPost]
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var cuotas = await _reposFiltroFecha.ObtenerCuentaCobrar(filtrarFechaVM);
            return View("Index", cuotas);
        }
    }
}
