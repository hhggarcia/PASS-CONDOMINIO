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

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class ProductosController : Controller
    {
        private readonly NuevaAppContext _context;

        public ProductosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Productos.Include(p => p.IdCondominioNavigation).Include(p => p.IdRetencionIslrNavigation).Include(p => p.IdRetencionIvaNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdRetencionIslrNavigation)
                .Include(p => p.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Concepto");
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion");
            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProducto,Nombre,IdCondominio,Precio,TipoProducto,Descripcion,Disponible,IdRetencionIva,IdRetencionIslr")] Producto producto)
        {
            ModelState.Remove(nameof(producto.IdRetencionIvaNavigation));
            ModelState.Remove(nameof(producto.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(producto.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", producto.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Concepto", producto.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", producto.IdRetencionIva);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", producto.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Concepto", producto.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", producto.IdRetencionIva);
            return View(producto);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,Nombre,IdCondominio,Precio,TipoProducto,Descripcion,Disponible,IdRetencionIva,IdRetencionIslr")] Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(producto.IdRetencionIvaNavigation));
            ModelState.Remove(nameof(producto.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(producto.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.IdProducto))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", producto.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Concepto", producto.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", producto.IdRetencionIva);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdRetencionIslrNavigation)
                .Include(p => p.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}