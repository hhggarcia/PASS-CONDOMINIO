﻿using System;
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

    public class PagosEmitidosController : Controller
    {
        private readonly IPagosEmitidosRepository _repoPagosEmitidos;
        private readonly PruebaContext _context;

        public PagosEmitidosController(IPagosEmitidosRepository repoPagosEmitidos,
            PruebaContext context)
        {

            _repoPagosEmitidos = repoPagosEmitidos;
            _context = context;
        }

        // GET: PagosEmitidos
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.PagoEmitidos.Include(p => p.IdCondominioNavigation).Include(p => p.IdDolarNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        public async Task<IActionResult> IndexPagosEmitidos()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await _repoPagosEmitidos.GetPagosEmitidos(idCondominio);

            TempData.Keep();

            return View(modelo);
        }

        // GET: PagosEmitidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PagoEmitidos == null)
            {
                return NotFound();
            }

            var pagoEmitido = await _context.PagoEmitidos
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdDolarNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoEmitido == id);
            if (pagoEmitido == null)
            {
                return NotFound();
            }

            return View(pagoEmitido);
        }

        // GET: PagosEmitidos/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor");

            return View();
        }

        // POST: PagosEmitidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPagoEmitido,IdCondominio,IdProveedor,Fecha,Monto,FormaPago,IdDolar")] PagoEmitido pagoEmitido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagoEmitido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoEmitido.IdCondominio);
            ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor", pagoEmitido.IdDolar);

            return View(pagoEmitido);
        }

        // GET: PagosEmitidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PagoEmitidos == null)
            {
                return NotFound();
            }

            var pagoEmitido = await _context.PagoEmitidos.FindAsync(id);
            if (pagoEmitido == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoEmitido.IdCondominio);
            ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor", pagoEmitido.IdDolar);

            return View(pagoEmitido);
        }

        // POST: PagosEmitidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPagoEmitido,IdCondominio,IdProveedor,Fecha,Monto,FormaPago,IdDolar")] PagoEmitido pagoEmitido)
        {
            if (id != pagoEmitido.IdPagoEmitido)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                _context.Update(pagoEmitido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repoPagosEmitidos.PagoEmitidoExists(pagoEmitido.IdPagoEmitido))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(IndexPagosEmitidos));
            //}
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoEmitido.IdCondominio);
            //return View(pagoEmitido);
        }

        // GET: PagosEmitidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PagoEmitidos == null)
            {
                return NotFound();
            }

            var pagoEmitido = await _context.PagoEmitidos
                .Include(p => p.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoEmitido == id);
            if (pagoEmitido == null)
            {
                return NotFound();
            }

            return View(pagoEmitido);
        }

        // POST: PagosEmitidos/Delete/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.PagoEmitidos == null)
                {
                    return Problem("Entity set 'PruebaContext.PagoEmitidos'  is null.");
                }
                var result = await _repoPagosEmitidos.Delete(id);

                return RedirectToAction(nameof(IndexPagosEmitidos));
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult RegistrarPagos()
        {
            try
            {
                //traer subcuentas del condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = _repoPagosEmitidos.FormRegistrarPago(idCondominio);

                TempData.Keep();

                return View(modelo);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult RegistrarPagosPost(RegistroPagoVM modelo)
        {
            try
            {
                modelo.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                if (ModelState.IsValid)
                {
                    var resultado = _repoPagosEmitidos.RegistrarPago(modelo);

                    if (resultado)
                    {
                        return RedirectToAction("IndexPagosEmitidos");
                    }

                }
                return RedirectToAction("RegistrarPagos");
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }
    }
}