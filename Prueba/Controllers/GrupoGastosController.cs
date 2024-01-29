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
    public class GrupoGastosController : Controller
    {
        private readonly NuevaAppContext _context;

        public GrupoGastosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: GrupoGastos
        public async Task<IActionResult> Index()
        {
            return View(await _context.GrupoGastos.ToListAsync());
        }

        // GET: GrupoGastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoGasto = await _context.GrupoGastos
                .FirstOrDefaultAsync(m => m.IdGrupoGasto == id);
            if (grupoGasto == null)
            {
                return NotFound();
            }

            return View(grupoGasto);
        }

        // GET: GrupoGastos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GrupoGastos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdGrupoGasto,NumGrupo,NombreGrupo")] GrupoGasto grupoGasto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grupoGasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grupoGasto);
        }

        // GET: GrupoGastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoGasto = await _context.GrupoGastos.FindAsync(id);
            if (grupoGasto == null)
            {
                return NotFound();
            }
            return View(grupoGasto);
        }

        // POST: GrupoGastos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdGrupoGasto,NumGrupo,NombreGrupo")] GrupoGasto grupoGasto)
        {
            if (id != grupoGasto.IdGrupoGasto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grupoGasto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrupoGastoExists(grupoGasto.IdGrupoGasto))
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
            return View(grupoGasto);
        }

        // GET: GrupoGastos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoGasto = await _context.GrupoGastos
                .FirstOrDefaultAsync(m => m.IdGrupoGasto == id);
            if (grupoGasto == null)
            {
                return NotFound();
            }

            return View(grupoGasto);
        }

        // POST: GrupoGastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grupoGasto = await _context.GrupoGastos.FindAsync(id);
            if (grupoGasto != null)
            {
                _context.GrupoGastos.Remove(grupoGasto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrupoGastoExists(int id)
        {
            return _context.GrupoGastos.Any(e => e.IdGrupoGasto == id);
        }
    }
}
