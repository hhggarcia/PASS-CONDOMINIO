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

    public class PuestoEsController : Controller
    {
        private readonly IEstacionamientoRepository _repoEstacionamiento;
        private readonly NuevaAppContext _context;

        public PuestoEsController(IEstacionamientoRepository repoEstacionamiento,
            NuevaAppContext context)
        {
            _repoEstacionamiento = repoEstacionamiento;
            _context = context;
        }

        // GET: PuestoEs
        public async Task<IActionResult> Index(int id)
        {
            //var NuevaAppContext = _context.PuestoEs.Include(p => p.IdEstacionamientoNavigation).Include(p => p.IdPropiedadNavigation);
            var puestos = await _repoEstacionamiento.PuestosEsta(id);
            return View(puestos);
        }

        // GET: PuestoEs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PuestoEs == null)
            {
                return NotFound();
            }

            var puestoE = await _context.PuestoEs
                .Include(p => p.IdEstacionamientoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPuestoE == id);
            if (puestoE == null)
            {
                return NotFound();
            }

            return View(puestoE);
        }

        // GET: PuestoEs/Create
        public IActionResult Create()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var inmuebles = _context.Inmuebles.Where(c => c.IdCondominio == idCondominio);
            //var NuevaAppContext = _context.Estacionamientos.Include(e => e.IdInmuebleNavigation);
            if (inmuebles != null && inmuebles.Any())
            {
                var propiedades = _context.Propiedads.Where(c => c.IdInmueble == inmuebles.First().IdInmueble);
                ViewData["IdPropiedad"] = new SelectList(propiedades, "IdPropiedad", "Codigo");
            }
            else
            {
                ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo");
            }

            TempData.Keep();
            ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "Nombre");
            return View();
        }

        // POST: PuestoEs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPuestoE,IdEstacionamiento,IdPropiedad,Codigo,Alicuota")] PuestoE puestoE)
        {
            //if (ModelState.IsValid)
            //{

            //}
            //ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "IdEstacionamiento", puestoE.IdEstacionamiento);
            //ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", puestoE.IdPropiedad);
            //return View(puestoE);

            try
            {
                var result = await _repoEstacionamiento.CrearPuestoEst(puestoE);

                var puestos = await _repoEstacionamiento.PuestosEsta(puestoE.IdEstacionamiento);
                return View(nameof(Index), puestos);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }

        // GET: PuestoEs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PuestoEs == null)
            {
                return NotFound();
            }

            var puestoE = await _context.PuestoEs.FindAsync(id);
            if (puestoE == null)
            {
                return NotFound();
            }
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var inmuebles = _context.Inmuebles.Where(c => c.IdCondominio == idCondominio);
            //var NuevaAppContext = _context.Estacionamientos.Include(e => e.IdInmuebleNavigation);
            if (inmuebles != null && inmuebles.Any())
            {
                var propiedades = _context.Propiedads.Where(c => c.IdInmueble == inmuebles.First().IdInmueble);
                ViewData["IdPropiedad"] = new SelectList(propiedades, "IdPropiedad", "Codigo", puestoE.IdPropiedad);
            }
            else
            {
                ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", puestoE.IdPropiedad);
            }

            ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "Nombre", puestoE.IdEstacionamiento);
            TempData.Keep();

            return View(puestoE);
        }

        // POST: PuestoEs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPuestoE,IdEstacionamiento,IdPropiedad,Codigo,Alicuota")] PuestoE puestoE)
        {
            if (id != puestoE.IdPuestoE)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                var result = await _repoEstacionamiento.EditarPuestoEst(puestoE);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repoEstacionamiento.PuestoEExists(puestoE.IdPuestoE))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "IdEstacionamiento", puestoE.IdEstacionamiento);
            //ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", puestoE.IdPropiedad);
            //return View(puestoE);
        }

        // GET: PuestoEs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PuestoEs == null)
            {
                return NotFound();
            }

            var puestoE = await _context.PuestoEs
                .Include(p => p.IdEstacionamientoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPuestoE == id);
            if (puestoE == null)
            {
                return NotFound();
            }

            return View(puestoE);
        }

        // POST: PuestoEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PuestoEs == null)
            {
                return Problem("Entity set 'NuevaAppContext.PuestoEs'  is null.");
            }
            var result = await _repoEstacionamiento.EliminarPuestoEst(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
