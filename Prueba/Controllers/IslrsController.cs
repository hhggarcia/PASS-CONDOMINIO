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
    public class IslrsController : Controller
    {
        private readonly NuevaAppContext _context;

        public IslrsController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Islrs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Islrs.ToListAsync());
        }

        // GET: Islrs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var islr = await _context.Islrs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (islr == null)
            {
                return NotFound();
            }

            return View(islr);
        }

        // GET: Islrs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Islrs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UnidadTributaria,Factor,Concepto,Literal,TipoReceptor,Residente,Domiciliada,BaseImponible,Tarifa,MontoSujeto,PagosMayores,Sustraendo")] Islr islr)
        {
            if (ModelState.IsValid)
            {
                _context.Add(islr);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(islr);
        }

        // GET: Islrs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var islr = await _context.Islrs.FindAsync(id);
            if (islr == null)
            {
                return NotFound();
            }
            return View(islr);
        }

        // POST: Islrs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UnidadTributaria,Factor,Concepto,Literal,TipoReceptor,Residente,Domiciliada,BaseImponible,Tarifa,MontoSujeto,PagosMayores,Sustraendo")] Islr islr)
        {
            if (id != islr.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(islr);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IslrExists(islr.Id))
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
            return View(islr);
        }

        // GET: Islrs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var islr = await _context.Islrs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (islr == null)
            {
                return NotFound();
            }

            return View(islr);
        }

        // POST: Islrs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var islr = await _context.Islrs.FindAsync(id);
            if (islr != null)
            {
                _context.Islrs.Remove(islr);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IslrExists(int id)
        {
            return _context.Islrs.Any(e => e.Id == id);
        }
    }
}
