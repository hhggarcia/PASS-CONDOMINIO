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
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

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
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Proveedors
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdRetencionIslrNavigation)
                .Include(p => p.IdRetencionIvaNavigation)
                .Where(p => p.IdCondominio == IdCondominio);

            TempData.Keep();

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
        public async Task<IActionResult> Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == IdCondominio), "IdCondominio", "Nombre");
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion");

            var selectIslrs = await (from c in _context.Islrs
                              where c.Tarifa > 0
                              select new
                              {
                                  DataValue = c.Id,
                                  DataText =  c.Concepto 
                                  + ((c.Pjuridica) ? " PJ" : "") 
                                  + ((c.Pnatural) ? " PN" : "") 
                                  + ((c.Domiciliada) ? " Domiciliado" : "") 
                                  + ((c.NoDomiciliada) ? " No Domiciliado" : "") 
                                  + ((c.Residenciada) ? " Residenciada" : "")
                                  + ((c.NoResidenciada) ? " No Residenciada" : "")
                                  + " " + c.Tarifa + "%"

                              }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");

            TempData.Keep();

            return View();
        }

        // POST: Proveedores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("IdProveedor,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Proveedor proveedor)
        //{
        //    ModelState.Remove(nameof(proveedor.IdCondominioNavigation));
        //    ModelState.Remove(nameof(proveedor.IdRetencionIslrNavigation));
        //    ModelState.Remove(nameof(proveedor.IdRetencionIvaNavigation));

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(proveedor);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", proveedor.IdCondominio);
        //    ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descrpicion", proveedor.IdRetencionIva);

        //    var selectIslrs = await (from c in _context.Islrs
        //                             where c.Tarifa > 0
        //                             select new
        //                             {
        //                                 DataValue = c.Id,
        //                                 DataText = c.Concepto
        //                                 + ((c.Pjuridica) ? " PJ" : "")
        //                                 + ((c.Pnatural) ? " PN" : "")
        //                                 + ((c.Domiciliada) ? " Domiciliado" : "")
        //                                 + ((c.NoDomiciliada) ? " No Domiciliado" : "")
        //                                 + ((c.Residenciada) ? " Residenciada" : "")
        //                                 + ((c.NoResidenciada) ? " No Residenciada" : "")
        //                                 + " " + c.Tarifa + "%"

        //                             }).ToListAsync();

        //    ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText", proveedor.IdRetencionIslr);

        //    return View(proveedor);
        //}

        // POST: Proveedores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProveedor,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Proveedor proveedor, bool check)
        {
            ModelState.Remove(nameof(proveedor.IdCondominioNavigation));
            ModelState.Remove(nameof(proveedor.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(proveedor.IdRetencionIvaNavigation));

            if (ModelState.IsValid)
            {
                if (check)
                {

                    proveedor.IdRetencionIslr = null;
                    //proveedor.IdRetencionIva = null;
                    _context.Add(proveedor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                    
                }
                else
                {
                    _context.Add(proveedor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", proveedor.IdCondominio);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descrpicion", proveedor.IdRetencionIva);

            var selectIslrs = await (from c in _context.Islrs
                                     where c.Tarifa > 0
                                     select new
                                     {
                                         DataValue = c.Id,
                                         DataText = c.Concepto
                                         + ((c.Pjuridica) ? " PJ" : "")
                                         + ((c.Pnatural) ? " PN" : "")
                                         + ((c.Domiciliada) ? " Domiciliado" : "")
                                         + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                         + ((c.Residenciada) ? " Residenciada" : "")
                                         + ((c.NoResidenciada) ? " No Residenciada" : "")
                                         + " " + c.Tarifa + "%"

                                     }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText", proveedor.IdRetencionIslr);

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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", proveedor.IdCondominio);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion");

            var selectIslrs = await (from c in _context.Islrs
                                     where c.Tarifa > 0
                                     select new
                                     {
                                         DataValue = c.Id,
                                         DataText = c.Concepto
                                         + ((c.Pjuridica) ? " PJ" : "")
                                         + ((c.Pnatural) ? " PN" : "")
                                         + ((c.Domiciliada) ? " Domiciliado" : "")
                                         + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                         + ((c.Residenciada) ? " Residenciada" : "")
                                         + ((c.NoResidenciada) ? " No Residenciada" : "")
                                         + " " + c.Tarifa + "%"

                                     }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");

            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProveedor,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Proveedor proveedor, bool check)
        {
            if (id != proveedor.IdProveedor)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(proveedor.IdCondominioNavigation));
            ModelState.Remove(nameof(proveedor.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(proveedor.IdRetencionIvaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    if (check)
                    {

                        proveedor.IdRetencionIslr = null;
                        proveedor.IdRetencionIva = null;
                        _context.Update(proveedor);
                        await _context.SaveChangesAsync();

                    }
                    else
                    {
                        _context.Update(proveedor);
                        await _context.SaveChangesAsync();
                    }
                    
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", proveedor.IdCondominio);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion");

            var selectIslrs = await (from c in _context.Islrs
                                     where c.Tarifa > 0
                                     select new
                                     {
                                         DataValue = c.Id,
                                         DataText = c.Concepto
                                         + ((c.Pjuridica) ? " PJ" : "")
                                         + ((c.Pnatural) ? " PN" : "")
                                         + ((c.Domiciliada) ? " Domiciliado" : "")
                                         + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                         + ((c.Residenciada) ? " Residenciada" : "")
                                         + ((c.NoResidenciada) ? " No Residenciada" : "")
                                         + " " + c.Tarifa + "%"

                                     }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");

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
