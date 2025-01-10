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

    public class PropiedadesGruposController : Controller
    {
        private readonly NuevaAppContext _context;

        public PropiedadesGruposController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: PropiedadesGrupos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.PropiedadesGrupos.Include(p => p.IdGrupoGastoNavigation).Include(p => p.IdPropiedadNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: PropiedadesGrupos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedadesGrupo = await _context.PropiedadesGrupos
                .Include(p => p.IdGrupoGastoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedadGrupo == id);
            if (propiedadesGrupo == null)
            {
                return NotFound();
            }

            return View(propiedadesGrupo);
        }

        // GET: PropiedadesGrupos/Create
        public IActionResult Create()
        {
            int idPropiedad = Convert.ToInt32(TempData.Peek("IDPropiedad").ToString());


            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo");
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads.Where(c => c.IdPropiedad.Equals(idPropiedad)), "IdPropiedad", "Codigo");
            return View();
        }

        // POST: PropiedadesGrupos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPropiedadGrupo,IdGrupoGasto,IdPropiedad,Alicuota")] PropiedadesGrupo propiedadesGrupo)
        {
            ModelState.Remove(nameof(propiedadesGrupo.IdGrupoGastoNavigation));
            ModelState.Remove(nameof(propiedadesGrupo.IdPropiedadNavigation));
            if (ModelState.IsValid)
            {
                _context.Add(propiedadesGrupo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Propiedades");
            }
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo", propiedadesGrupo.IdGrupoGasto);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", propiedadesGrupo.IdPropiedad);
            return View(propiedadesGrupo);
        }

        // GET: PropiedadesGrupos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedadesGrupo = await _context.PropiedadesGrupos.FindAsync(id);
            if (propiedadesGrupo == null)
            {
                return NotFound();
            }
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo", propiedadesGrupo.IdGrupoGasto);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", propiedadesGrupo.IdPropiedad);
            return View(propiedadesGrupo);
        }

        // POST: PropiedadesGrupos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPropiedadGrupo,IdGrupoGasto,IdPropiedad,Alicuota")] PropiedadesGrupo propiedadesGrupo)
        {
            if (id != propiedadesGrupo.IdPropiedadGrupo)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(propiedadesGrupo.IdGrupoGastoNavigation));
            ModelState.Remove(nameof(propiedadesGrupo.IdPropiedadNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(propiedadesGrupo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropiedadesGrupoExists(propiedadesGrupo.IdPropiedadGrupo))
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
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "NombreGrupo", propiedadesGrupo.IdGrupoGasto);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", propiedadesGrupo.IdPropiedad);
            return View(propiedadesGrupo);
        }

        // GET: PropiedadesGrupos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedadesGrupo = await _context.PropiedadesGrupos
                .Include(p => p.IdGrupoGastoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedadGrupo == id);
            if (propiedadesGrupo == null)
            {
                return NotFound();
            }

            return View(propiedadesGrupo);
        }

        // POST: PropiedadesGrupos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propiedadesGrupo = await _context.PropiedadesGrupos.FindAsync(id);
            if (propiedadesGrupo != null)
            {
                _context.PropiedadesGrupos.Remove(propiedadesGrupo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropiedadesGrupoExists(int id)
        {
            return _context.PropiedadesGrupos.Any(e => e.IdPropiedadGrupo == id);
        }
    }
}
