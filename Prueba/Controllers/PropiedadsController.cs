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

    public class PropiedadsController : Controller
    {
        private readonly PruebaContext _context;

        public PropiedadsController(PruebaContext context)
        {
            _context = context;
        }

        // GET: Propiedads
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.Propiedads.Include(p => p.IdInmuebleNavigation).Include(p => p.IdUsuarioNavigation);
            return View(await pruebaContext.OrderBy(c => c.IdUsuarioNavigation.Email).ToListAsync());
        }

        // GET: Propiedads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Propiedads == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads
                .Include(p => p.IdInmuebleNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == id);
            if (propiedad == null)
            {
                return NotFound();
            }

            return View(propiedad);
        }

        // GET: Propiedads/Create
        public IActionResult Create()
        {
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "Nombre");
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers.OrderBy(c => c.Email), "Id", "Email");
            return View();
        }

        // POST: Propiedads/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPropiedad,IdInmueble,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda")] Propiedad propiedad)
        {
            //if (ModelState.IsValid)
            //{
                _context.Add(propiedad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "Nombre", propiedad.IdInmueble);
            //ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", propiedad.IdUsuario);
            //return View(propiedad);
        }

        // GET: Propiedads/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Propiedads == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads.FindAsync(id);
            if (propiedad == null)
            {
                return NotFound();
            }
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "Nombre", propiedad.IdInmueble);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", propiedad.IdUsuario);
            return View(propiedad);
        }

        // POST: Propiedads/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPropiedad,IdInmueble,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda")] Propiedad propiedad)
        {
            if (id != propiedad.IdPropiedad)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
                try
                {
                    _context.Update(propiedad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropiedadExists(propiedad.IdPropiedad))
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
            //ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", propiedad.IdInmueble);
            //ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Id", propiedad.IdUsuario);
            //return View(propiedad);
        }

        // GET: Propiedads/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Propiedads == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads
                .Include(p => p.IdInmuebleNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == id);
            if (propiedad == null)
            {
                return NotFound();
            }

            return View(propiedad);
        }

        // POST: Propiedads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Propiedads == null)
            {
                return Problem("Entity set 'PruebaContext.Propiedads'  is null.");
            }
            var propiedad = await _context.Propiedads.FindAsync(id);
            if (propiedad != null)
            {
                _context.Propiedads.Remove(propiedad);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropiedadExists(int id)
        {
          return (_context.Propiedads?.Any(e => e.IdPropiedad == id)).GetValueOrDefault();
        }
    }
}
