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

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class ReferenciaDolarsController : Controller
    {
        private readonly IReferenciaDolarRepository _repoDolar;
        private readonly PruebaContext _context;

        public ReferenciaDolarsController(IReferenciaDolarRepository repoDolar,
            PruebaContext context)
        {
            _repoDolar = repoDolar;
            _context = context;
        }

        // GET: ReferenciaDolars
        public async Task<IActionResult> Index()
        {
              return _context.ReferenciaDolars != null ? 
                          View(await _context.ReferenciaDolars.ToListAsync()) :
                          Problem("Entity set 'PruebaContext.ReferenciaDolars'  is null.");
        }

        // GET: ReferenciaDolars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ReferenciaDolars == null)
            {
                return NotFound();
            }

            var referenciaDolar = await _context.ReferenciaDolars
                .FirstOrDefaultAsync(m => m.IdReferencia == id);
            if (referenciaDolar == null)
            {
                return NotFound();
            }

            return View(referenciaDolar);
        }

        // GET: ReferenciaDolars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReferenciaDolars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReferencia,Valor,Fecha")] ReferenciaDolar referenciaDolar)
        {
            if (ModelState.IsValid)
            {
                var result = await _repoDolar.Crear(referenciaDolar);
                return RedirectToAction(nameof(Index));
            }
            return View(referenciaDolar);
        }

        // GET: ReferenciaDolars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ReferenciaDolars == null)
            {
                return NotFound();
            }

            var referenciaDolar = await _context.ReferenciaDolars.FindAsync(id);
            if (referenciaDolar == null)
            {
                return NotFound();
            }
            return View(referenciaDolar);
        }

        // POST: ReferenciaDolars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReferencia,Valor,Fecha")] ReferenciaDolar referenciaDolar)
        {
            if (id != referenciaDolar.IdReferencia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _repoDolar.Editar(referenciaDolar);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_repoDolar.ReferenciaDolarExists(referenciaDolar.IdReferencia))
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
            return View(referenciaDolar);
        }

        // GET: ReferenciaDolars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ReferenciaDolars == null)
            {
                return NotFound();
            }

            var referenciaDolar = await _context.ReferenciaDolars
                .FirstOrDefaultAsync(m => m.IdReferencia == id);
            if (referenciaDolar == null)
            {
                return NotFound();
            }

            return View(referenciaDolar);
        }

        // POST: ReferenciaDolars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ReferenciaDolars == null)
            {
                return Problem("Entity set 'PruebaContext.ReferenciaDolars'  is null.");
            }
            var referenciaDolar = await _context.ReferenciaDolars.FindAsync(id);
            if (referenciaDolar != null)
            {
                _context.ReferenciaDolars.Remove(referenciaDolar);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
