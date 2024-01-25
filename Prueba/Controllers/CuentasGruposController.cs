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
    public class CuentasGruposController : Controller
    {
        private readonly NuevaAppContext _context;

        public CuentasGruposController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: CuentasGrupos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.CuentasGrupos.Include(c => c.IdCodCuentaNavigation).Include(c => c.IdGrupoGastoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CuentasGrupos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasGrupo = await _context.CuentasGrupos
                .Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdGrupoGastoNavigation)
                .FirstOrDefaultAsync(m => m.IdCuentaGrupos == id);
            if (cuentasGrupo == null)
            {
                return NotFound();
            }

            return View(cuentasGrupo);
        }

        // GET: CuentasGrupos/Create
        public IActionResult Create()
        {
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "Codigo");
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "Nombre");
            return View();
        }

        // POST: CuentasGrupos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCuentaGrupos,IdCodCuenta,IdGrupoGasto")] CuentasGrupo cuentasGrupo)
        {
            ModelState.Remove(nameof(cuentasGrupo.IdCodCuentaNavigation));
            ModelState.Remove(nameof(cuentasGrupo.IdGrupoGastoNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(cuentasGrupo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "Codigo", cuentasGrupo.IdCodCuenta);
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "Nombre", cuentasGrupo.IdGrupoGasto);
            return View(cuentasGrupo);
        }

        // GET: CuentasGrupos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasGrupo = await _context.CuentasGrupos.FindAsync(id);
            if (cuentasGrupo == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "Codigo", cuentasGrupo.IdCodCuenta);
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "Nombre", cuentasGrupo.IdGrupoGasto);
            return View(cuentasGrupo);
        }

        // POST: CuentasGrupos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCuentaGrupos,IdCodCuenta,IdGrupoGasto")] CuentasGrupo cuentasGrupo)
        {
            if (id != cuentasGrupo.IdCuentaGrupos)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(cuentasGrupo.IdCodCuentaNavigation));
            ModelState.Remove(nameof(cuentasGrupo.IdGrupoGastoNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentasGrupo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentasGrupoExists(cuentasGrupo.IdCuentaGrupos))
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
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "Codigo", cuentasGrupo.IdCodCuenta);
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "Nombre", cuentasGrupo.IdGrupoGasto);
            return View(cuentasGrupo);
        }

        // GET: CuentasGrupos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasGrupo = await _context.CuentasGrupos
                .Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdGrupoGastoNavigation)
                .FirstOrDefaultAsync(m => m.IdCuentaGrupos == id);
            if (cuentasGrupo == null)
            {
                return NotFound();
            }

            return View(cuentasGrupo);
        }

        // POST: CuentasGrupos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuentasGrupo = await _context.CuentasGrupos.FindAsync(id);
            if (cuentasGrupo != null)
            {
                _context.CuentasGrupos.Remove(cuentasGrupo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentasGrupoExists(int id)
        {
            return _context.CuentasGrupos.Any(e => e.IdCuentaGrupos == id);
        }
    }
}
