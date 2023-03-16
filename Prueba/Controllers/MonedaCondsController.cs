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
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class MonedaCondsController : Controller
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly PruebaContext _context;

        public MonedaCondsController(IMonedaRepository repoMoneda,
            PruebaContext context)
        {
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: MonedaConds
        public async Task<IActionResult> Index()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var pruebaContext = _context.MonedaConds
                .Where(m => m.IdCondominio == idCondominio)
                .Include(m => m.IdCondominioNavigation)
                .Include(m => m.IdMonedaNavigation);

            TempData.Keep();

            return View(await pruebaContext.ToListAsync());
        }

        // GET: MonedaConds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MonedaConds == null)
            {
                return NotFound();
            }

            var monedaCond = await _context.MonedaConds
                .Include(m => m.IdCondominioNavigation)
                .Include(m => m.IdMonedaNavigation)
                .FirstOrDefaultAsync(m => m.IdMonedaCond == id);
            if (monedaCond == null)
            {
                return NotFound();
            }

            return View(monedaCond);
        }

        // GET: MonedaConds/Create
        public async Task<IActionResult> Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var condominio = await _context.Condominios.FindAsync(idCondominio);

            if (condominio != null)
            {
                ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == condominio.IdCondominio), "IdCondominio", "Nombre");
                ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "Nombre");

                TempData.Keep();

                return View();
            }

            return RedirectToAction("Index");
        }

        // POST: MonedaConds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMonedaCond,IdCondominio,IdMoneda,Princinpal,Simbolo,ValorDolar")] MonedaCond monedaCond)
        {
            try
            {
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var existMoneda = _context.MonedaConds.Where(c => c.Simbolo == monedaCond.Simbolo && c.IdCondominio == idCondominio);
                var existPrincipal = _context.MonedaConds.Where(c => c.Princinpal && c.IdCondominio == idCondominio);
                if (existMoneda != null && existMoneda.Any())
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una moneda con este símbolo!"
                    };
                    TempData.Keep();
                    return View("Error", modeloError);
                }
                else if (existPrincipal != null && existPrincipal.Any() && monedaCond.Princinpal)
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una moneda Principal!"
                    };
                    TempData.Keep();

                    return View("Error", modeloError);
                }
                var result = await _repoMoneda.Crear(monedaCond);

                TempData.Keep();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                TempData.Keep();

                return View("Error", modeloError);
            }
            //if (ModelState.IsValid)
            //{

            //}
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", monedaCond.IdCondominio);
            //ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "Nombre", monedaCond.IdMoneda);
            //return View(monedaCond);
        }

        // GET: MonedaConds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MonedaConds == null)
            {
                return NotFound();
            }

            var monedaCond = await _context.MonedaConds.FindAsync(id);
            if (monedaCond == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", monedaCond.IdCondominio);
            ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "Nombre", monedaCond.IdMoneda);
            return View(monedaCond);
        }

        // POST: MonedaConds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMonedaCond,IdCondominio,IdMoneda,Princinpal,Simbolo,ValorDolar")] MonedaCond monedaCond)
        {
            if (id != monedaCond.IdMonedaCond)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                var principalExist = _context.MonedaConds.Where(c => c.Princinpal).ToList();

                if (principalExist.Any() && monedaCond.Princinpal)
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una moneda Principal!"
                    };

                    return View("Error", modeloError);
                }
                var result = await _repoMoneda.Editar(monedaCond);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repoMoneda.MonedaCondExists(monedaCond.IdMonedaCond))
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
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", monedaCond.IdCondominio);
            //ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "Nombre", monedaCond.IdMoneda);
            //return View(monedaCond);
        }

        // GET: MonedaConds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MonedaConds == null)
            {
                return NotFound();
            }

            var monedaCond = await _context.MonedaConds
                .Include(m => m.IdCondominioNavigation)
                .Include(m => m.IdMonedaNavigation)
                .FirstOrDefaultAsync(m => m.IdMonedaCond == id);
            if (monedaCond == null)
            {
                return NotFound();
            }

            return View(monedaCond);
        }

        // POST: MonedaConds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MonedaConds == null)
            {
                return Problem("Entity set 'PruebaContext.MonedaConds'  is null.");
            }
            var result = await _repoMoneda.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }


    }
}
