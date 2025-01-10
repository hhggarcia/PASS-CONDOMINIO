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
    public class ImpresorasController : Controller
    {
        private readonly NuevaAppContext _context;

        public ImpresorasController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Impresoras
        public async Task<IActionResult> Index()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Impresoras.Where(c => c.IdCondominio == idCondominio).Include(i => i.IdCondominioNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Impresoras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var impresora = await _context.Impresoras
                .Include(i => i.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdImpresora == id);
            if (impresora == null)
            {
                return NotFound();
            }

            return View(impresora);
        }

        // GET: Impresoras/Create
        public IActionResult Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == idCondominio), "IdCondominio", "Nombre");

            TempData.Keep();
            return View();
        }

        // POST: Impresoras/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdImpresora,Nombre,IdCondominio")] Impresora impresora)
        {
            ModelState.Remove(nameof(impresora.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(impresora);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == idCondominio), "IdCondominio", "Nombre", impresora.IdCondominio);
            TempData.Keep();

            return View(impresora);
        }

        // GET: Impresoras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var impresora = await _context.Impresoras.FindAsync(id);
            if (impresora == null)
            {
                return NotFound();
            }
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == idCondominio), "IdCondominio", "Nombre", impresora.IdCondominio);
            TempData.Keep();

            return View(impresora);
        }

        // POST: Impresoras/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdImpresora,Nombre,IdCondominio")] Impresora impresora)
        {
            if (id != impresora.IdImpresora)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(impresora.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(impresora);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImpresoraExists(impresora.IdImpresora))
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
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == idCondominio), "IdCondominio", "Nombre", impresora.IdCondominio);
            TempData.Keep();
            return View(impresora);
        }

        // GET: Impresoras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var impresora = await _context.Impresoras
                .Include(i => i.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdImpresora == id);
            if (impresora == null)
            {
                return NotFound();
            }

            return View(impresora);
        }

        // POST: Impresoras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var impresora = await _context.Impresoras.FindAsync(id);
            if (impresora != null)
            {
                _context.Impresoras.Remove(impresora);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImpresoraExists(int id)
        {
            return _context.Impresoras.Any(e => e.IdImpresora == id);
        }
    }
}
