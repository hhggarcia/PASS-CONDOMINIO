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
    public class ClientesController : Controller
    {
        private readonly NuevaAppContext _context;

        public ClientesController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Clientes
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdRetencionIslrNavigation)
                .Include(c => c.IdRetencionIvaNavigation)
                .Where(c => c.IdCondominio == IdCondominio);

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdRetencionIslrNavigation)
                .Include(c => c.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public async Task<IActionResult> Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == IdCondominio), "IdCondominio", "Nombre");
            
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion");

            var selectIslrs = await(from c in _context.Islrs
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

            TempData.Keep();

            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,IdCondominio,Nombre,Direccion,Telefono,Rif,Email,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Cliente cliente, bool checkIslr, bool checkIva)
        {
            ModelState.Remove(nameof(cliente.IdCondominioNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIvaNavigation));
            if (ModelState.IsValid)
            {
                if (checkIslr)
                {

                    cliente.IdRetencionIslr = null;                  

                }

                if (checkIva)
                {

                    cliente.IdRetencionIva = null;

                }

                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cliente.IdCondominio);
            
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", cliente.IdRetencionIva);

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

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cliente.IdCondominio);
            //ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Concepto", cliente.IdRetencionIslr);
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
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", cliente.IdRetencionIva);

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,IdCondominio,Nombre,Direccion,Telefono,Rif,Email,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Cliente cliente, bool checkIslr, bool checkIva)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(cliente.IdCondominioNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIvaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    if (checkIslr)
                    {

                        cliente.IdRetencionIslr = null;

                    }

                    if (checkIva)
                    {

                        cliente.IdRetencionIva = null;

                    }
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.IdCliente))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cliente.IdCondominio);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", cliente.IdRetencionIva);

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

            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdRetencionIslrNavigation)
                .Include(c => c.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}
