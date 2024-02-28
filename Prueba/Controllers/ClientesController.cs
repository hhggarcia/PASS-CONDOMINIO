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
            var nuevaAppContext = _context.Clientes.Include(c => c.IdCondominioNavigation).Include(c => c.IdRetencionIslrNavigation).Include(c => c.IdRetencionIvaNavigation);
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
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id");
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id");
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", cliente.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id", cliente.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id", cliente.IdRetencionIva);
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", cliente.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id", cliente.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id", cliente.IdRetencionIva);
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,IdCondominio,Nombre,Direccion,Telefono,Rif,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", cliente.IdCondominio);
            ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Id", cliente.IdRetencionIslr);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Id", cliente.IdRetencionIva);
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
