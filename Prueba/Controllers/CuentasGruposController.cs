using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

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
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo");
            return View();
        }

        // POST: CuentasGrupos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCuentaGrupos,IdCodCuenta,IdGrupoGasto")] CuentasGrupo cuentasGrupo)
        {
            ModelState.Remove(nameof(cuentasGrupo.IdGrupoGastoNavigation));
            ModelState.Remove(nameof(cuentasGrupo.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                var subcuenta = await _context.SubCuenta.FindAsync(cuentasGrupo.IdCodCuenta);
                var cc = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == subcuenta.Id).ToListAsync();
                cuentasGrupo.IdCodCuenta = cc.First().IdCodCuenta;

                _context.Add(cuentasGrupo);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "GrupoGastos");
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion", cuentasGrupo.IdCodCuenta);
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", cuentasGrupo.IdCodCuenta);

            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo", cuentasGrupo.IdGrupoGasto);
            
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
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion", cuentasGrupo.IdCodCuenta);
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", cuentasGrupo.IdCodCuenta);

            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo", cuentasGrupo.IdGrupoGasto);
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

            ModelState.Remove(nameof(cuentasGrupo.IdGrupoGastoNavigation));
            ModelState.Remove(nameof(cuentasGrupo.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    var subcuenta = await _context.SubCuenta.FindAsync(cuentasGrupo.IdCodCuenta);
                    var cc = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == subcuenta.Id).ToListAsync();
                    cuentasGrupo.IdCodCuenta = cc.First().IdCodCuenta;

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
                return RedirectToAction("Index", "GrupoGastos");

            }
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion", cuentasGrupo.IdCodCuenta);
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", cuentasGrupo.IdCodCuenta);

            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo", cuentasGrupo.IdGrupoGasto);
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
