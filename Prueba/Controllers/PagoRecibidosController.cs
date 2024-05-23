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

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class PagoRecibidosController : Controller
    {
        private readonly NuevaAppContext _context;

        public PagoRecibidosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: PagoRecibidos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.PagoRecibidos.Include(p => p.IdCondominioNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: PagoRecibidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoRecibido = await _context.PagoRecibidos
                .Include(p => p.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoRecibido == id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }

            return View(pagoRecibido);
        }

        // GET: PagoRecibidos/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            return View();
        }

        // POST: PagoRecibidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPagoRecibido,IdCondominio,FormaPago,Monto,Fecha,IdSubCuenta,Concepto,Confirmado,ValorDolar,MontoRef,SimboloMoneda,SimboloRef,Imagen")] PagoRecibido pagoRecibido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagoRecibido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoRecibido.IdCondominio);
            return View(pagoRecibido);
        }

        // GET: PagoRecibidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoRecibido = await _context.PagoRecibidos.FindAsync(id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoRecibido.IdCondominio);
            return View(pagoRecibido);
        }

        // POST: PagoRecibidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPagoRecibido,IdCondominio,FormaPago,Monto,Fecha,IdSubCuenta,Concepto,Confirmado,ValorDolar,MontoRef,SimboloMoneda,SimboloRef,Imagen")] PagoRecibido pagoRecibido)
        {
            if (id != pagoRecibido.IdPagoRecibido)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagoRecibido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoRecibidoExists(pagoRecibido.IdPagoRecibido))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoRecibido.IdCondominio);
            return View(pagoRecibido);
        }

        // GET: PagoRecibidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoRecibido = await _context.PagoRecibidos
                .Include(p => p.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoRecibido == id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }

            return View(pagoRecibido);
        }

        // POST: PagoRecibidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pagoRecibido = await _context.PagoRecibidos.FindAsync(id);
            if (pagoRecibido != null)
            {
                _context.PagoRecibidos.Remove(pagoRecibido);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoRecibidoExists(int id)
        {
            return _context.PagoRecibidos.Any(e => e.IdPagoRecibido == id);
        }
    }
}
