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

    public class EmpleadosController : Controller
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public EmpleadosController(IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: Empleados
        public async Task<IActionResult> Index()
        {
            return View(await _context.Empleados.ToListAsync());
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empleados/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEmpleado,Nombre,Apellido,Cedula,FechaIngreso,Estado,BaseSueldo,RefMonto")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                _context.Add(empleado);

                // registrar en nomina condomino
                var registro = new CondominioNomina
                {
                    IdCondominio = idCondominio,
                    IdEmpleado = empleado.IdEmpleado
                };

                _context.CondominioNominas.Add(registro);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEmpleado,Nombre,Apellido,Cedula,FechaIngreso,Estado,BaseSueldo,RefMonto")] Empleado empleado)
        {
            if (id != empleado.IdEmpleado)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.IdEmpleado))
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
            return View(empleado);
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                var deducciones = _context.Deducciones.Where(d => d.IdEmpleado == id).ToList();
                var percepciones = _context.Percepciones.Where(d => d.IdEmpleado == id).ToList();

                if (deducciones.Any())
                {
                    _context.Deducciones.RemoveRange(deducciones);
                }

                if (percepciones.Any())
                {
                    _context.Percepciones.RemoveRange(percepciones);
                }


                var recibos = _context.ReciboNominas.Where(d => d.IdEmpleado == id).ToList();

                if (recibos.Any())
                {
                    foreach (var recibo in recibos)
                    {
                        var pagosRecibo = _context.PagosNominas.Where(d => d.IdReciboNomina == recibo.IdReciboNomina).ToList();

                        _context.PagosNominas.RemoveRange(pagosRecibo);
                    }

                    _context.ReciboNominas.RemoveRange(recibos);
                }

                _context.Empleados.Remove(empleado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.IdEmpleado == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del empleado a consultar deducciones</param>
        /// <returns></returns>
        public IActionResult VerDeducciones(int id)
        {
            return RedirectToAction("Index", "Deducciones", new { id });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del empleado a consultar percepciones</param>
        /// <returns></returns>
        public IActionResult VerPercepciones(int id)
        {
            return RedirectToAction("Index", "Percepciones", new { id });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">Id del empleado a consultar recibos de nomina</param>
        /// <returns></returns>
        public IActionResult VerRecibosNomina(int id)
        {
            return RedirectToAction("Index", "ReciboNominas", new { id });
        }

        public async Task<IActionResult> PagoNomina()
        {
            // cargar formulario

            // usar el repositorio?

            return View();
        }
    }
}
