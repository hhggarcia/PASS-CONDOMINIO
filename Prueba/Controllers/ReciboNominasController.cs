using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Services;

namespace Prueba.Controllers
{
    public class ReciboNominasController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly NuevaAppContext _context;

        public ReciboNominasController(IPDFServices servicesPDF,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
            _context = context;
        }

        //// GET: ReciboNominas
        //public async Task<IActionResult> Index()
        //{
        //    var nuevaAppContext = _context.ReciboNominas.Include(r => r.IdEmpleadoNavigation);
        //    return View(await nuevaAppContext.ToListAsync());
        //}

        // GET: ReciboNominas
        public async Task<IActionResult> Index(int id)
        {
            var nuevaAppContext = _context.ReciboNominas.Include(r => r.IdEmpleadoNavigation).Where(c => c.IdEmpleado == id);
            return View(await nuevaAppContext.ToListAsync());
        }
        // GET: ReciboNominas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboNomina = await _context.ReciboNominas
                .Include(r => r.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdReciboNomina == id);
            if (reciboNomina == null)
            {
                return NotFound();
            }

            return View(reciboNomina);
        }

        // GET: ReciboNominas/Create
        public IActionResult Create()
        {
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado");
            return View();
        }

        // POST: ReciboNominas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReciboNomina,Entregado,IdEmpleado,Fecha,Periodo,Concepto,PagoTotal,RefMonto")] ReciboNomina reciboNomina)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reciboNomina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado", reciboNomina.IdEmpleado);
            return View(reciboNomina);
        }

        // GET: ReciboNominas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboNomina = await _context.ReciboNominas.FindAsync(id);
            if (reciboNomina == null)
            {
                return NotFound();
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado", reciboNomina.IdEmpleado);
            return View(reciboNomina);
        }

        // POST: ReciboNominas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReciboNomina,Entregado,IdEmpleado,Fecha,Periodo,Concepto,PagoTotal,RefMonto")] ReciboNomina reciboNomina)
        {
            if (id != reciboNomina.IdReciboNomina)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reciboNomina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReciboNominaExists(reciboNomina.IdReciboNomina))
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado", reciboNomina.IdEmpleado);
            return View(reciboNomina);
        }

        // GET: ReciboNominas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboNomina = await _context.ReciboNominas
                .Include(r => r.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdReciboNomina == id);
            if (reciboNomina == null)
            {
                return NotFound();
            }

            return View(reciboNomina);
        }

        // POST: ReciboNominas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reciboNomina = await _context.ReciboNominas.FindAsync(id);
            if (reciboNomina != null)
            {
                var pagosRecibos = await _context.PagosNominas.Where(c => c.IdReciboNomina == id).ToListAsync();

                if (pagosRecibos != null)
                {
                    _context.PagosNominas.RemoveRange(pagosRecibos);
                }

                _context.ReciboNominas.Remove(reciboNomina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReciboPDF(int id)
        {
            var reciboNomina = await _context.ReciboNominas.FindAsync(id);
            if (reciboNomina != null)
            {
                var data = await _servicesPDF.ReciboNominaPDF(reciboNomina);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "ReciboNomina.pdf");
            }

            return RedirectToAction("Index", "Empleados");
        }

        private bool ReciboNominaExists(int id)
        {
            return _context.ReciboNominas.Any(e => e.IdReciboNomina == id);
        }
    }
}
