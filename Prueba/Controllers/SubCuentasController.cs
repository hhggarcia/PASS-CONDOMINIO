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
    public class SubCuentasController : Controller
    {
        private readonly NuevaAppContext _context;

        public SubCuentasController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: SubCuentas
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubCuenta.ToListAsync());
        }

        // GET: SubCuentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCuenta == null)
            {
                return NotFound();
            }

            return View(subCuenta);
        }

        // GET: SubCuentas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubCuentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descricion,Codigo")] SubCuenta subCuenta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subCuenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subCuenta);
        }

        // GET: SubCuentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta.FindAsync(id);
            if (subCuenta == null)
            {
                return NotFound();
            }
            return View(subCuenta);
        }

        // POST: SubCuentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descricion,Codigo")] SubCuenta subCuenta)
        {
            if (id != subCuenta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subCuenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubCuentaExists(subCuenta.Id))
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
            return View(subCuenta);
        }

        // GET: SubCuentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCuenta == null)
            {
                return NotFound();
            }

            return View(subCuenta);
        }

        // POST: SubCuentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCuenta = await _context.SubCuenta.FindAsync(id);
            if (subCuenta != null)
            {
                _context.SubCuenta.Remove(subCuenta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubCuentaExists(int id)
        {
            return _context.SubCuenta.Any(e => e.Id == id);
        }
    }
}
