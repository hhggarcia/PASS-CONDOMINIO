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
    public class CuentasGlobalController : Controller
    {
        private readonly NuevaAppContext _context;

        public CuentasGlobalController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: CuentasGlobal
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.CodigoCuentasGlobals.Include(c => c.IdClaseNavigation).Include(c => c.IdCondominioNavigation).Include(c => c.IdCuentaNavigation).Include(c => c.IdGrupoNavigation).Include(c => c.IdSubCuentaNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CuentasGlobal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals
                .Include(c => c.IdClaseNavigation)
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdCuentaNavigation)
                .Include(c => c.IdGrupoNavigation)
                .Include(c => c.IdSubCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdCodCuenta == id);
            if (codigoCuentasGlobal == null)
            {
                return NotFound();
            }

            return View(codigoCuentasGlobal);
        }

        // GET: CuentasGlobal/Create
        public IActionResult Create()
        {
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id");
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id");
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id");
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id");
            return View();
        }

        // POST: CuentasGlobal/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCodCuenta,IdSubCuenta,IdCuenta,IdGrupo,IdClase,Codigo,Saldo,SaldoInicial,IdCondominio")] CodigoCuentasGlobal codigoCuentasGlobal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(codigoCuentasGlobal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id", codigoCuentasGlobal.IdClase);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", codigoCuentasGlobal.IdCondominio);
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", codigoCuentasGlobal.IdCuenta);
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id", codigoCuentasGlobal.IdGrupo);
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id", codigoCuentasGlobal.IdSubCuenta);
            return View(codigoCuentasGlobal);
        }

        // GET: CuentasGlobal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals.FindAsync(id);
            if (codigoCuentasGlobal == null)
            {
                return NotFound();
            }
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id", codigoCuentasGlobal.IdClase);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", codigoCuentasGlobal.IdCondominio);
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", codigoCuentasGlobal.IdCuenta);
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id", codigoCuentasGlobal.IdGrupo);
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id", codigoCuentasGlobal.IdSubCuenta);
            return View(codigoCuentasGlobal);
        }

        // POST: CuentasGlobal/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCodCuenta,IdSubCuenta,IdCuenta,IdGrupo,IdClase,Codigo,Saldo,SaldoInicial,IdCondominio")] CodigoCuentasGlobal codigoCuentasGlobal)
        {
            if (id != codigoCuentasGlobal.IdCodCuenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(codigoCuentasGlobal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CodigoCuentasGlobalExists(codigoCuentasGlobal.IdCodCuenta))
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
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id", codigoCuentasGlobal.IdClase);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", codigoCuentasGlobal.IdCondominio);
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", codigoCuentasGlobal.IdCuenta);
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id", codigoCuentasGlobal.IdGrupo);
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id", codigoCuentasGlobal.IdSubCuenta);
            return View(codigoCuentasGlobal);
        }

        // GET: CuentasGlobal/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals
                .Include(c => c.IdClaseNavigation)
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdCuentaNavigation)
                .Include(c => c.IdGrupoNavigation)
                .Include(c => c.IdSubCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdCodCuenta == id);
            if (codigoCuentasGlobal == null)
            {
                return NotFound();
            }

            return View(codigoCuentasGlobal);
        }

        // POST: CuentasGlobal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals.FindAsync(id);
            if (codigoCuentasGlobal != null)
            {
                _context.CodigoCuentasGlobals.Remove(codigoCuentasGlobal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CodigoCuentasGlobalExists(int id)
        {
            return _context.CodigoCuentasGlobals.Any(e => e.IdCodCuenta == id);
        }
    }
}
