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
    public class AnticipoNominasController : Controller
    {
        private readonly NuevaAppContext _context;

        public AnticipoNominasController(NuevaAppContext context)
        {
            _context = context;
        }

        //// GET: AnticipoNominas
        //public async Task<IActionResult> Index()
        //{
        //    var nuevaAppContext = _context.AnticipoNominas.Include(a => a.IdEmpleadoNavigation);
        //    return View(await nuevaAppContext.ToListAsync());
        //}
        // GET: AnticipoNominas
        public async Task<IActionResult> Index(int id)
        {
            var nuevaAppContext = _context.AnticipoNominas
                .Include(b => b.IdEmpleadoNavigation)
                .Where(c => c.IdEmpleado == id);

            return View(await nuevaAppContext.ToListAsync());
        }


        // GET: AnticipoNominas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipoNomina = await _context.AnticipoNominas
                .Include(a => a.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipoNomina == id);
            if (anticipoNomina == null)
            {
                return NotFound();
            }

            return View(anticipoNomina);
        }

        // GET: AnticipoNominas/Create
        public IActionResult Create()
        {
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre");
            return View();
        }

        // POST: AnticipoNominas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAnticipoNomina,IdEmpleado,Monto,Fecha,Activo")] AnticipoNomina anticipoNomina)
        {
            ModelState.Remove(nameof(anticipoNomina.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(anticipoNomina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", anticipoNomina.IdEmpleado);
            return View(anticipoNomina);
        }

        // GET: AnticipoNominas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipoNomina = await _context.AnticipoNominas.FindAsync(id);
            if (anticipoNomina == null)
            {
                return NotFound();
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", anticipoNomina.IdEmpleado);
            return View(anticipoNomina);
        }

        // POST: AnticipoNominas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAnticipoNomina,IdEmpleado,Monto,Fecha,Activo")] AnticipoNomina anticipoNomina)
        {
            if (id != anticipoNomina.IdAnticipoNomina)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(anticipoNomina.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anticipoNomina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnticipoNominaExists(anticipoNomina.IdAnticipoNomina))
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", anticipoNomina.IdEmpleado);
            return View(anticipoNomina);
        }

        // GET: AnticipoNominas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipoNomina = await _context.AnticipoNominas
                .Include(a => a.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipoNomina == id);
            if (anticipoNomina == null)
            {
                return NotFound();
            }

            return View(anticipoNomina);
        }

        // POST: AnticipoNominas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anticipoNomina = await _context.AnticipoNominas.FindAsync(id);
            if (anticipoNomina != null)
            {
                _context.AnticipoNominas.Remove(anticipoNomina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnticipoNominaExists(int id)
        {
            return _context.AnticipoNominas.Any(e => e.IdAnticipoNomina == id);
        }
    }
}
