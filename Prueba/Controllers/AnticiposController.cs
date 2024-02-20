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

    public class AnticiposController : Controller
    {
        private readonly NuevaAppContext _context;
        private readonly IFiltroFechaRepository _reposFiltroFecha;


        public AnticiposController(IFiltroFechaRepository filtroFechaRepository, NuevaAppContext context)
        {
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: Anticipos
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var listAnticipos = await (from a in _context.Anticipos.Include(f => f.IdProveedorNavigation)
                                      join c in _context.CodigoCuentasGlobals on a.IdCodCuenta equals c.IdCodCuenta
                                      where c.IdCondominio == IdCondominio
                                      select a).ToListAsync();

            //var nuevaAppContext = _context.Anticipos.Include(a => a.IdProveedorNavigation);
            return View(listAnticipos);
        }

        // GET: Anticipos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipo = await _context.Anticipos
                .Include(a => a.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipo == id);
            if (anticipo == null)
            {
                return NotFound();
            }

            return View(anticipo);
        }

        // GET: Anticipos/Create
        public IActionResult Create()
        {
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre");
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");
            return View();
        }

        // POST: Anticipos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAnticipo,Numero,Fecha,Saldo,Detalle,IdProveedor, IdCodCuenta")] Anticipo anticipo)
        {
            var idCuenta = _context.SubCuenta.Where(c => c.Id == anticipo.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
            anticipo.IdCodCuenta = idCodCuenta;
            anticipo.Activo = true;

            ModelState.Remove(nameof(anticipo.IdProveedorNavigation));
            ModelState.Remove(nameof(anticipo.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(anticipo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", anticipo.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

            return View(anticipo);
        }

        // GET: Anticipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipo = await _context.Anticipos.FindAsync(id);
            if (anticipo == null)
            {
                return NotFound();
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", anticipo.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

            return View(anticipo);
        }

        // POST: Anticipos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAnticipo,Numero,Fecha,Saldo,Detalle,IdProveedor")] Anticipo anticipo)
        {
            if (id != anticipo.IdAnticipo)
            {
                return NotFound();
            }

            var idCuenta = _context.SubCuenta.Where(c => c.Id == anticipo.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
            anticipo.IdCodCuenta = idCodCuenta;
            anticipo.Activo = true;

            ModelState.Remove(nameof(anticipo.IdProveedorNavigation));
            ModelState.Remove(nameof(anticipo.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anticipo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnticipoExists(anticipo.IdAnticipo))
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
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", anticipo.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

            return View(anticipo);
        }

        // GET: Anticipos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipo = await _context.Anticipos
                .Include(a => a.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipo == id);
            if (anticipo == null)
            {
                return NotFound();
            }

            return View(anticipo);
        }

        // POST: Anticipos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anticipo = await _context.Anticipos.FindAsync(id);
            if (anticipo != null)
            {
                var pagosAnticipo = await _context.PagoFacturas.Where(c => c.IdAnticipo.Equals(id)).ToListAsync();

                if (pagosAnticipo != null)
                {
                    _context.PagoFacturas.RemoveRange(pagosAnticipo);
                }

                _context.Anticipos.Remove(anticipo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnticipoExists(int id)
        {
            return _context.Anticipos.Any(e => e.IdAnticipo == id);
        }
        
        [HttpPost]
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var filtrarFecha = await _reposFiltroFecha.ObtenerAnticipos(filtrarFechaVM);
            return View("Index", filtrarFecha);
        }
    }
}
