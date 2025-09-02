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

    public class ReciboCobrosController : Controller
    {
        private readonly NuevaAppContext _context;

        public ReciboCobrosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: ReciboCobros
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.ReciboCobros.Include(r => r.IdPropiedadNavigation).Include(r => r.IdRgastosNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: ReciboCobros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboCobro = await _context.ReciboCobros
                .Include(r => r.IdPropiedadNavigation)
                .Include(r => r.IdRgastosNavigation)
                .FirstOrDefaultAsync(m => m.IdReciboCobro == id);
            if (reciboCobro == null)
            {
                return NotFound();
            }

            return View(reciboCobro);
        }

        // GET: ReciboCobros/Create
        public IActionResult Create()
        {
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo");
            ViewData["IdRgastos"] = new SelectList(_context.RelacionGastos, "IdRgastos", "Mes");
            return View();
        }

        // POST: ReciboCobros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReciboCobro,IdPropiedad,IdRgastos,Monto,Fecha,Pagado,EnProceso,Abonado,MontoRef,ValorDolar,SimboloMoneda,SimboloRef,MontoMora,MontoIndexacion,Acumulado,Mes,ReciboActual,TotalPagar,Diferencial,MontoRefTotalPagar,AbonadoRef")] ReciboCobro reciboCobro)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reciboCobro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", reciboCobro.IdPropiedad);
            ViewData["IdRgastos"] = new SelectList(_context.RelacionGastos, "IdRgastos", "Mes", reciboCobro.IdRgastos);
            return View(reciboCobro);
        }

        // GET: ReciboCobros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboCobro = await _context.ReciboCobros.FindAsync(id);
            if (reciboCobro == null)
            {
                return NotFound();
            }
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", reciboCobro.IdPropiedad);
            ViewData["IdRgastos"] = new SelectList(_context.RelacionGastos, "IdRgastos", "Mes", reciboCobro.IdRgastos);
            return View(reciboCobro);
        }

        // POST: ReciboCobros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReciboCobro,IdPropiedad,IdRgastos,Monto,Fecha,Pagado,EnProceso,Abonado,MontoRef,ValorDolar,SimboloMoneda,SimboloRef,MontoMora,MontoIndexacion,Acumulado,Mes,ReciboActual,TotalPagar,Diferencial,MontoRefTotalPagar,AbonadoRef")] ReciboCobro reciboCobro)
        {
            if (id != reciboCobro.IdReciboCobro)
            {
                return NotFound();
            }

            ModelState.Remove("IdPropiedadNavigation");
            ModelState.Remove("IdRgastosNavigation");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reciboCobro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReciboCobroExists(reciboCobro.IdReciboCobro))
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
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", reciboCobro.IdPropiedad);
            ViewData["IdRgastos"] = new SelectList(_context.RelacionGastos, "IdRgastos", "Mes", reciboCobro.IdRgastos);
            return View(reciboCobro);
        }

        // GET: ReciboCobros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboCobro = await _context.ReciboCobros
                .Include(r => r.IdPropiedadNavigation)
                .Include(r => r.IdRgastosNavigation)
                .FirstOrDefaultAsync(m => m.IdReciboCobro == id);
            if (reciboCobro == null)
            {
                return NotFound();
            }

            return View(reciboCobro);
        }

        // POST: ReciboCobros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reciboCobro = await _context.ReciboCobros.FindAsync(id);
            if (reciboCobro != null)
            {
                _context.ReciboCobros.Remove(reciboCobro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReciboCobroExists(int id)
        {
            return _context.ReciboCobros.Any(e => e.IdReciboCobro == id);
        }
    }
}
