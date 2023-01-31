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
    [Authorize(Policy = "RequireSuperAdmin")]

    public class InmueblesController : Controller
    {
        private readonly PruebaContext _context;

        public InmueblesController(PruebaContext context)
        {
            _context = context;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.Inmuebles.Include(i => i.IdCondominioNavigation).Include(i => i.IdInmuebleNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Inmuebles == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles
                .Include(i => i.IdCondominioNavigation)
                .Include(i => i.IdInmuebleNavigation)
                .FirstOrDefaultAsync(m => m.IdInmueble == id);
            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdInmueble"] = new SelectList(_context.Zonas, "IdZona", "Zona1");
            return View();
        }

        // POST: Inmuebles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdInmueble,IdZona,Nombre,TotalPropiedad,IdCondominio")] Inmueble inmueble)
        {
            //if (ModelState.IsValid)
            //{
            _context.Add(inmueble);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", inmueble.IdCondominio);
            //ViewData["IdInmueble"] = new SelectList(_context.Zonas, "IdZona", "Zona1", inmueble.IdInmueble);
            //return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Inmuebles == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", inmueble.IdCondominio);
            ViewData["IdInmueble"] = new SelectList(_context.Zonas, "IdZona", "Zona1", inmueble.IdInmueble);
            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInmueble,IdZona,Nombre,TotalPropiedad,IdCondominio")] Inmueble inmueble)
        {
            if (id != inmueble.IdInmueble)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                _context.Update(inmueble);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InmuebleExists(inmueble.IdInmueble))
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
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", inmueble.IdCondominio);
            //ViewData["IdInmueble"] = new SelectList(_context.Zonas, "IdZona", "Zona1", inmueble.IdInmueble);
            //return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Inmuebles == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles
                .Include(i => i.IdCondominioNavigation)
                .Include(i => i.IdInmuebleNavigation)
                .FirstOrDefaultAsync(m => m.IdInmueble == id);
            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Inmuebles == null)
            {
                return Problem("Entity set 'PruebaContext.Inmuebles'  is null.");
            }
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                _context.Inmuebles.Remove(inmueble);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InmuebleExists(int id)
        {
            return (_context.Inmuebles?.Any(e => e.IdInmueble == id)).GetValueOrDefault();
        }
    }
}
