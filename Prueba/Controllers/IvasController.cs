using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    public class IvasController : Controller
    {
        private readonly NuevaAppContext _context;

        public IvasController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Ivas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ivas.ToListAsync());
        }

        // GET: Ivas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iva = await _context.Ivas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (iva == null)
            {
                return NotFound();
            }

            return View(iva);
        }

        // GET: Ivas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ivas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,Porcentaje,Activo,Principal")] Iva iva)
        {
            if (ModelState.IsValid)
            {
                _context.Add(iva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(iva);
        }

        // GET: Ivas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iva = await _context.Ivas.FindAsync(id);
            if (iva == null)
            {
                return NotFound();
            }
            return View(iva);
        }

        // POST: Ivas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,Porcentaje,Activo,Principal")] Iva iva)
        {
            if (id != iva.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(iva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IvaExists(iva.Id))
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
            return View(iva);
        }

        // GET: Ivas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iva = await _context.Ivas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (iva == null)
            {
                return NotFound();
            }

            return View(iva);
        }

        // POST: Ivas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var iva = await _context.Ivas.FindAsync(id);
            if (iva != null)
            {
                _context.Ivas.Remove(iva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IvaExists(int id)
        {
            return _context.Ivas.Any(e => e.Id == id);
        }
    }
}
