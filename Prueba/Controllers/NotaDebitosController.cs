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

    public class NotaDebitosController : Controller
    {
        private readonly NuevaAppContext _context;

        public NotaDebitosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: NotaDebitos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.NotaDebitos.Include(n => n.IdProveedorNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: NotaDebitos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaDebito = await _context.NotaDebitos
                .Include(n => n.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdNotaDebito == id);
            if (notaDebito == null)
            {
                return NotFound();
            }

            return View(notaDebito);
        }

        // GET: NotaDebitos/Create
        public IActionResult Create()
        {
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: NotaDebitos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdNotaDebito,NumNotaDebito,Concepto,IdProveedor,Abonado")] NotaDebito notaDebito)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notaDebito);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", notaDebito.IdProveedor);
            return View(notaDebito);
        }

        // GET: NotaDebitos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaDebito = await _context.NotaDebitos.FindAsync(id);
            if (notaDebito == null)
            {
                return NotFound();
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", notaDebito.IdProveedor);
            return View(notaDebito);
        }

        // POST: NotaDebitos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdNotaDebito,NumNotaDebito,Concepto,IdProveedor,Abonado")] NotaDebito notaDebito)
        {
            if (id != notaDebito.IdNotaDebito)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notaDebito);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotaDebitoExists(notaDebito.IdNotaDebito))
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
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", notaDebito.IdProveedor);
            return View(notaDebito);
        }

        // GET: NotaDebitos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaDebito = await _context.NotaDebitos
                .Include(n => n.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdNotaDebito == id);
            if (notaDebito == null)
            {
                return NotFound();
            }

            return View(notaDebito);
        }

        // POST: NotaDebitos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notaDebito = await _context.NotaDebitos.FindAsync(id);
            if (notaDebito != null)
            {
                _context.NotaDebitos.Remove(notaDebito);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotaDebitoExists(int id)
        {
            return _context.NotaDebitos.Any(e => e.IdNotaDebito == id);
        }
    }
}
