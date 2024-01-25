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
    public class PropiedadesController : Controller
    {
        private readonly NuevaAppContext _context;

        public PropiedadesController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Propiedades
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Propiedads.Include(p => p.IdCondominioNavigation).Include(p => p.IdUsuarioNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Propiedades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == id);
            if (propiedad == null)
            {
                return NotFound();
            }

            return View(propiedad);
        }

        // GET: Propiedades/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: Propiedades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPropiedad,IdCondominio,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda,MontoIntereses")] Propiedad propiedad)
        {
            if (ModelState.IsValid)
            {
                _context.Add(propiedad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", propiedad.IdCondominio);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Id", propiedad.IdUsuario);
            return View(propiedad);
        }

        // GET: Propiedades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads.FindAsync(id);
            if (propiedad == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", propiedad.IdCondominio);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Id", propiedad.IdUsuario);
            return View(propiedad);
        }

        // POST: Propiedades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPropiedad,IdCondominio,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda,MontoIntereses")] Propiedad propiedad)
        {
            if (id != propiedad.IdPropiedad)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", propiedad.IdCondominio);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Id", propiedad.IdUsuario);
            return View(propiedad);
        }

        // GET: Propiedades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == id);
            if (propiedad == null)
            {
                return NotFound();
            }

            return View(propiedad);
        }

        // POST: Propiedades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
            return _context.Propiedads.Any(e => e.IdPropiedad == id);
        }
    }
}
