using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;

namespace Prueba.Controllers
{
    public class EstacionamientosController : Controller
    {
        private readonly IEstacionamientoRepository _repoEstacionamiento;
        private readonly PruebaContext _context;

        public EstacionamientosController(IEstacionamientoRepository repoEstacionamiento,
            PruebaContext context)
        {
            _repoEstacionamiento = repoEstacionamiento;
            _context = context;
        }

        // GET: Estacionamientos
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.Estacionamientos.Include(e => e.IdInmuebleNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        // GET: Estacionamientos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Estacionamientos == null)
            {
                return NotFound();
            }

            var estacionamiento = await _context.Estacionamientos
                .Include(e => e.IdInmuebleNavigation)
                .FirstOrDefaultAsync(m => m.IdEstacionamiento == id);
            if (estacionamiento == null)
            {
                return NotFound();
            }

            return View(estacionamiento);
        }

        // GET: Estacionamientos/Create
        public IActionResult Create()
        {
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble");
            return View();
        }

        // POST: Estacionamientos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEstacionamiento,IdInmueble,Nombre,NumPuestos")] Estacionamiento estacionamiento)
        {
            if (ModelState.IsValid)
            {
                var result = await _repoEstacionamiento.Crear(estacionamiento);
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", estacionamiento.IdInmueble);
            return View(estacionamiento);
        }

        // GET: Estacionamientos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Estacionamientos == null)
            {
                return NotFound();
            }

            var estacionamiento = await _context.Estacionamientos.FindAsync(id);
            if (estacionamiento == null)
            {
                return NotFound();
            }
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", estacionamiento.IdInmueble);
            return View(estacionamiento);
        }

        // POST: Estacionamientos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEstacionamiento,IdInmueble,Nombre,NumPuestos")] Estacionamiento estacionamiento)
        {
            if (id != estacionamiento.IdEstacionamiento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _repoEstacionamiento.Editar(estacionamiento);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_repoEstacionamiento.EstacionamientoExists(estacionamiento.IdEstacionamiento))
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
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", estacionamiento.IdInmueble);
            return View(estacionamiento);
        }

        // GET: Estacionamientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Estacionamientos == null)
            {
                return NotFound();
            }

            var estacionamiento = await _context.Estacionamientos
                .Include(e => e.IdInmuebleNavigation)
                .FirstOrDefaultAsync(m => m.IdEstacionamiento == id);
            if (estacionamiento == null)
            {
                return NotFound();
            }

            return View(estacionamiento);
        }

        // POST: Estacionamientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Estacionamientos == null)
            {
                return Problem("Entity set 'PruebaContext.Estacionamientos'  is null.");
            }
            var result = await _repoEstacionamiento.Eliminar(id);
            
            return RedirectToAction(nameof(Index));
        }


    }
}
