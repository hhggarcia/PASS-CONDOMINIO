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
    public class ProveedoresController : Controller
    {
        private readonly NuevaAppContext _context;

        public ProveedoresController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Proveedores
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Proveedors.Include(p => p.IdCondominioNavigation).Include(p => p.IdRetencionIslrNavigation).Include(p => p.IdRetencionIvaNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Proveedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdRetencionIslrNavigation)
                .Include(p => p.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // GET: Proveedores/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id");
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id");
            return View();
        }

        // POST: Proveedores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProveedor,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", proveedor.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id", proveedor.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id", proveedor.IdRetencionIva);
            return View(proveedor);
        }

        // GET: Proveedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", proveedor.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id", proveedor.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id", proveedor.IdRetencionIva);
            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProveedor,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.IdProveedor))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", proveedor.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id", proveedor.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id", proveedor.IdRetencionIva);
            return View(proveedor);
        }

        // GET: Proveedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdRetencionIslrNavigation)
                .Include(p => p.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor != null)
            {
                _context.Proveedors.Remove(proveedor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedors.Any(e => e.IdProveedor == id);
        }
    }
}
