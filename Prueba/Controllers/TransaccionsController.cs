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
    public class TransaccionsController : Controller
    {
        private readonly NuevaAppContext _context;

        public TransaccionsController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Transaccions
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Transaccions.Include(t => t.IdCodCuentaBancoNavigation).Include(t => t.IdCodCuentaNavigation).Include(t => t.IdGrupoGastoNavigation).Include(t => t.IdPropiedadNavigation).Include(t => t.IdProveedorNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Transaccions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaccion = await _context.Transaccions
                .Include(t => t.IdCodCuentaBancoNavigation)
                .Include(t => t.IdCodCuentaNavigation)
                .Include(t => t.IdGrupoGastoNavigation)
                .Include(t => t.IdPropiedadNavigation)
                .Include(t => t.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdTransaccion == id);
            if (transaccion == null)
            {
                return NotFound();
            }

            return View(transaccion);
        }

        // GET: Transaccions/Create
        public IActionResult Create()
        {
            ViewData["IdCodCuentaBanco"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "IdGrupoGasto");
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: Transaccions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTransaccion,TipoTransaccion,IdPropiedad,IdCodCuenta,Descripcion,IdProveedor,Documento,MontoTotal,Cancelado,FormaPago,IdCodCuentaBanco,NumDocumento,IdGrupoGasto")] Transaccion transaccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCodCuentaBanco"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", transaccion.IdCodCuentaBanco);
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", transaccion.IdCodCuenta);
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "IdGrupoGasto", transaccion.IdGrupoGasto);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", transaccion.IdPropiedad);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", transaccion.IdProveedor);
            return View(transaccion);
        }

        // GET: Transaccions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaccion = await _context.Transaccions.FindAsync(id);
            if (transaccion == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuentaBanco"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", transaccion.IdCodCuentaBanco);
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", transaccion.IdCodCuenta);
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "IdGrupoGasto", transaccion.IdGrupoGasto);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", transaccion.IdPropiedad);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", transaccion.IdProveedor);
            return View(transaccion);
        }

        // POST: Transaccions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTransaccion,TipoTransaccion,IdPropiedad,IdCodCuenta,Descripcion,IdProveedor,Documento,MontoTotal,Cancelado,FormaPago,IdCodCuentaBanco,NumDocumento,IdGrupoGasto")] Transaccion transaccion)
        {
            if (id != transaccion.IdTransaccion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransaccionExists(transaccion.IdTransaccion))
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
            ViewData["IdCodCuentaBanco"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", transaccion.IdCodCuentaBanco);
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", transaccion.IdCodCuenta);
            ViewData["IdGrupoGasto"] = new SelectList(_context.GrupoGastos, "IdGrupoGasto", "IdGrupoGasto", transaccion.IdGrupoGasto);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", transaccion.IdPropiedad);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", transaccion.IdProveedor);
            return View(transaccion);
        }

        // GET: Transaccions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaccion = await _context.Transaccions
                .Include(t => t.IdCodCuentaBancoNavigation)
                .Include(t => t.IdCodCuentaNavigation)
                .Include(t => t.IdGrupoGastoNavigation)
                .Include(t => t.IdPropiedadNavigation)
                .Include(t => t.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdTransaccion == id);
            if (transaccion == null)
            {
                return NotFound();
            }

            return View(transaccion);
        }

        // POST: Transaccions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaccion = await _context.Transaccions.FindAsync(id);
            if (transaccion != null)
            {
                _context.Transaccions.Remove(transaccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransaccionExists(int id)
        {
            return _context.Transaccions.Any(e => e.IdTransaccion == id);
        }
    }
}
