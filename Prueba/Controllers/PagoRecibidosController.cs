using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    public class PagoRecibidosController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly IPagosRecibidosRepository _repoPagosRecibidos;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IEmailService _serviceEmail;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly NuevaAppContext _context;

        public PagoRecibidosController(IPDFServices servicesPDF,
            IPagosRecibidosRepository repoPagosRecibidos,
            IWebHostEnvironment webHostEnvironment,
            ICuentasContablesRepository repoCuentas,
            IEmailService serviceEmail,
            SignInManager<ApplicationUser> signInManager,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
            _repoPagosRecibidos = repoPagosRecibidos;
            _webHostEnvironment = webHostEnvironment;
            _repoCuentas = repoCuentas;
            _serviceEmail = serviceEmail;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: PagoRecibidos
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.PagoRecibidos.Include(p => p.IdCondominioNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: PagoRecibidos/Details/5
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoRecibido = await _context.PagoRecibidos
                .Include(p => p.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoRecibido == id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }

            return View(pagoRecibido);
        }

        // GET: PagoRecibidos/Create
        [Authorize(Policy = "RequireAdmin")]

        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            return View();
        }

        // POST: PagoRecibidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> Create([Bind("IdPagoRecibido,IdCondominio,FormaPago,Monto,Fecha,IdSubCuenta,Concepto,Confirmado,ValorDolar,MontoRef,SimboloMoneda,SimboloRef,Imagen")] PagoRecibido pagoRecibido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagoRecibido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoRecibido.IdCondominio);
            return View(pagoRecibido);
        }

        // GET: PagoRecibidos/Edit/5
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoRecibido = await _context.PagoRecibidos.FindAsync(id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoRecibido.IdCondominio);
            return View(pagoRecibido);
        }

        // POST: PagoRecibidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> Edit(int id, [Bind("IdPagoRecibido,IdCondominio,FormaPago,Monto,Fecha,IdSubCuenta,Concepto,Confirmado,ValorDolar,MontoRef,SimboloMoneda,SimboloRef,Imagen")] PagoRecibido pagoRecibido)
        {
            if (id != pagoRecibido.IdPagoRecibido)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagoRecibido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoRecibidoExists(pagoRecibido.IdPagoRecibido))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoRecibido.IdCondominio);
            return View(pagoRecibido);
        }

        // GET: PagoRecibidos/Delete/5
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoRecibido = await _context.PagoRecibidos
                .Include(p => p.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoRecibido == id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }

            return View(pagoRecibido);
        }

        // POST: PagoRecibidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pagoRecibido = await _context.PagoRecibidos.FindAsync(id);
            if (pagoRecibido != null)
            {
                _context.PagoRecibidos.Remove(pagoRecibido);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> PagosConfirmados()
        {
            var model = await _context.PagoPropiedads
                .Include(c => c.IdPagoNavigation)
                .Include(c => c.IdPropiedadNavigation)
                .Where(c => c.Confirmado)
                .ToListAsync();

            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<JsonResult> AjaxCargarRecibos(int valor)
        {
            PagoRecibidoVM modelo = new PagoRecibidoVM();

            if (valor > 0)
            {
                var propiedad = await _context.Propiedads.FindAsync(valor);

                if (propiedad != null)
                {
                    //var inmueble = await _context.Inmuebles.FindAsync(propiedad.IdInmueble);
                    var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);

                    //CARGAR SELECT DE SUB CUENTAS DE BANCOS
                    var subcuentasBancos = await _repoCuentas.ObtenerBancos(condominio.IdCondominio);
                    var subcuentasCaja = await _repoCuentas.ObtenerCaja(condominio.IdCondominio);


                    var recibos = await (from c in _context.ReciboCobros
                                         where c.IdPropiedad == valor
                                         where !c.Pagado
                                         select c).ToListAsync();

                    modelo.Interes = propiedad.MontoIntereses;
                    modelo.Indexacion = propiedad.MontoMulta != null ? (decimal)propiedad.MontoMulta : 0;
                    modelo.Credito = propiedad.Creditos != null ? (decimal)propiedad.Creditos : 0;
                    modelo.Saldo = propiedad.Saldo;
                    modelo.Deuda = propiedad.Deuda;
                    modelo.Recibos = recibos;
                    //modelo.Abonado = modelo.Recibos[0].Abonado;

                    if (modelo.Recibos.Any())
                    {
                        modelo.RecibosModel = recibos.Where(c => !c.EnProceso && !c.Pagado)
                            .Select(c => new SelectListItem { Text = c.Fecha.ToString("dd/MM/yyyy"), Value = c.IdReciboCobro.ToString() })
                            .ToList();
                        modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() })
                            .ToList();
                        modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() })
                            .ToList();
                        modelo.ListRecibos = recibos.Select(recibo => new SelectListItem
                        {
                            Text = recibo.Mes + " " + (recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.TotalPagar).ToString("N") + "Bs",
                            Value = recibo.IdReciboCobro.ToString(),
                            Selected = false,
                        }).ToList();
                    }
                }
            }

            return Json(modelo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> RegistrarPagosAdmin()
        {
            try
            {
                var modelo = new PagoRecibidoVM();

                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var propiedades = from c in _context.Propiedads
                                  where c.IdCondominio == idCondominio
                                  select c;

                modelo.Propiedades = await propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToListAsync();

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

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> RegistrarPagosAdmin(PagoRecibidoVM modelo)
        {
            try
            {
                if (modelo.Pagoforma == FormaPago.Transferencia)
                {
                    var existPagoTransferencia = from p in _context.PagoRecibidos
                                                 join referencia in _context.ReferenciasPrs
                                                 on p.IdPagoRecibido equals referencia.IdPagoRecibido
                                                 where referencia.NumReferencia == modelo.NumReferencia
                                                 select new { p, referencia };

                    if (existPagoTransferencia != null && existPagoTransferencia.Any())
                    {
                        var modeloError = new ErrorViewModel()
                        {
                            RequestId = "Ya existe un pago registrado con este número de Referencia!"
                        };

                        return View("Error", modeloError);
                    }
                }

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo.IdCondominio = idCondominio;

                var resultado = await _repoPagosRecibidos.RegistrarPagoPropietarioAdmin(modelo);

                // cargar comprobante si fue exitoso
                if (resultado == "exito")
                {
                    var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
                    //var recibo = await _context.ReciboCobros.FindAsync(modelo.IdRecibo);
                    var condominio = await _context.Condominios.FindAsync(modelo.IdCondominio);

                    var comprobante = new ComprobanteVM()
                    {
                        Propiedad = propiedad,
                        // Inmueble = inmueble,
                        Condominio = condominio,
                        FechaComprobante = DateTime.Today,

                        PagoRecibido = new PagoRecibido()
                        {
                            IdCondominio = modelo.IdCondominio,
                            Fecha = modelo.Fecha,
                            FormaPago = modelo.FormaPago,
                            Monto = modelo.Monto
                        },
                        Mensaje = "Gracias por su pago!"
                    };

                    if (modelo.IdCodigoCuentaBanco > 0)
                    {
                        var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);

                        comprobante.Referencias = new ReferenciasPr()
                        {
                            Banco = banco.Descricion,
                            NumReferencia = modelo.NumReferencia
                        };
                    }

                    TempData.Keep();

                    return View("Comprobante", comprobante);
                }

                string uniqueFileName = null;
                if (modelo.Imagen != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "ComprobantesPU");
                    uniqueFileName = Encoding.UTF8.GetString(modelo.Imagen);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    System.IO.File.Delete(filePath);
                }

                var propiedades = from c in _context.Propiedads
                                  where c.IdCondominio == idCondominio
                                  select c;

                modelo.Propiedades = await propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToListAsync();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = resultado;

                TempData.Keep();

                return View("RegistrarPagosAdmin", modelo);
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
        }



        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> ConfirmarPago(PagoRecibidoVM modelo)
        {
            if (modelo.ListRecibos != null && modelo.ListRecibos.Any())
            {
                foreach (var item in modelo.ListRecibos)
                {
                    if (item.Selected)
                    {
                        var idRecibo = Convert.ToInt32(item.Value);
                        var recibo = await _context.ReciboCobros.FindAsync(idRecibo);
                        if (recibo != null)
                        {
                            modelo.ListRecibosIDs.Add(idRecibo);
                            //modelo.Monto += recibo.Monto;
                        }
                    }
                }
            }

            return View(modelo);
        }


        /// <summary>
        /// accion de administrsdor para
        /// registrar un pago de una propiedad directamente
        /// el pago se considera ya confirmado
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> RegistrarPagos(PagoRecibidoVM modelo, IFormFile file)
        {
            try
            {
                if (modelo.Pagoforma == FormaPago.Transferencia)
                {
                    var existPagoTransferencia = from p in _context.PagoRecibidos
                                                 join referencia in _context.ReferenciasPrs
                                                 on p.IdPagoRecibido equals referencia.IdPagoRecibido
                                                 where referencia.NumReferencia == modelo.NumReferencia
                                                 select new { p, referencia };

                    if (existPagoTransferencia != null && existPagoTransferencia.Any())
                    {
                        var modeloError = new ErrorViewModel()
                        {
                            RequestId = "Ya existe un pago registrado con este número de Referencia!"
                        };

                        return View("Error", modeloError);
                    }
                }

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo.IdCondominio = idCondominio;

                string uniqueFileName = null;  //to contain the filename
                if (file != null)  //handle iformfile
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "ComprobantesPU");
                    uniqueFileName = file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                }

                modelo.Imagen = Encoding.UTF8.GetBytes(uniqueFileName); //fill the image property

                var resultado = await _repoPagosRecibidos.RegistrarPagoPropietarioAdmin(modelo);

                // cargar comprobante si fue exitoso
                if (resultado == "exito")
                {
                    var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
                    var condominio = await _context.Condominios.FindAsync(modelo.IdCondominio);

                    var comprobante = new ComprobanteVM()
                    {
                        Propiedad = propiedad,
                        // Inmueble = inmueble,
                        Condominio = condominio,
                        FechaComprobante = DateTime.Today,

                        PagoRecibido = new PagoRecibido()
                        {
                            IdCondominio = modelo.IdCondominio,
                            Fecha = modelo.Fecha,
                            FormaPago = modelo.FormaPago,
                            Monto = modelo.Monto
                        },
                        Mensaje = "Gracias por su pago!"
                    };

                    if (modelo.IdCodigoCuentaBanco > 0)
                    {
                        var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);

                        comprobante.Referencias = new ReferenciasPr()
                        {
                            Banco = banco.Descricion,
                            NumReferencia = modelo.NumReferencia
                        };
                    }

                    TempData.Keep();

                    return View("Comprobante", comprobante);
                }

                // eliminar archivo si no se registro efectivamente el pago
                if (file != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "ComprobantesPU");
                    uniqueFileName = file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    System.IO.File.Delete(filePath);
                }

                string idPropietario = TempData.Peek("idUserLog").ToString();

                var propiedades = from c in _context.Propiedads
                                  where c.IdUsuario == idPropietario
                                  select c;

                modelo.Propiedades = await propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToListAsync();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = resultado;

                TempData.Keep();

                return View("RegistrarPagos", modelo);
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
        }

        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> PagosRecibidosAdmin()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                // propietarios
                IList<ApplicationUser> listaPropietarios = await _signInManager.UserManager.Users.ToListAsync();
                // propiedades
                var propiedadesPorUsuario = new Dictionary<ApplicationUser, List<Propiedad>>();
                // pagos recibidos
                var pagosPorPropiedad = new Dictionary<Propiedad, List<PagoRecibido>>();

                foreach (var user in listaPropietarios)
                {
                    var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == user.Id).ToListAsync();
                    if (propiedades != null && propiedades.Any())
                    {
                        propiedadesPorUsuario.Add(user, propiedades);
                        foreach (var propiedad in propiedades)
                        {

                            var pagos = await (from c in _context.PagoRecibidos
                                               join d in _context.PagoPropiedads
                                               on c.IdPagoRecibido equals d.IdPago
                                               where c.IdCondominio == idCondominio && d.IdPropiedad == propiedad.IdPropiedad && !d.Confirmado
                                               select c).ToListAsync();

                            if (pagos != null && pagos.Any())
                            {
                                pagosPorPropiedad.Add(propiedad, pagos);
                            }
                        }
                    }
                }
                IndexPagoRecibdioVM modelo = new IndexPagoRecibdioVM()
                {
                    UsuariosPropiedad = propiedadesPorUsuario,
                    PropiedadPagos = pagosPorPropiedad,
                    //ReferenciasDolar = await _context.ReferenciaDolars.ToListAsync()
                };

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


        public async Task<IActionResult> DownloadImage(int id)
        {
            var pagoRecibido = await _context.PagoRecibidos.FindAsync(id);
            if (pagoRecibido == null)
            {
                return NotFound();
            }

            // Forzar la descarga y codificar el nombre del archivo
            return File(pagoRecibido.Imagen, "image/jpeg", "referencia.jpg");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del pago recibido</param>
        /// <returns></returns>
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> ConfirmarPagoRecibidoPropietario(int id)
        {
            // buscar pago recibido 
            var pagoRecibido = await _context.PagoRecibidos.FindAsync(id);

            if (pagoRecibido != null)
            {
                var referencia = await _context.ReferenciasPrs.FirstOrDefaultAsync(c => c.IdPagoRecibido == pagoRecibido.IdPagoRecibido);
                // buscar pagoPropiedad
                var pagoPropiedad = await _context.PagoPropiedads.FirstOrDefaultAsync(c => c.IdPago == pagoRecibido.IdPagoRecibido);
                // buscar pagoRecibos
                var pagoRecibos = await _context.PagosRecibos.Where(c => c.IdPago == pagoRecibido.IdPagoRecibido).ToListAsync();
                // buscar recibos relacionados al pago
                List<ReciboCobro> recibos = new List<ReciboCobro>();
                if (pagoRecibos != null && pagoRecibos.Any())
                {
                    foreach (var relacion in pagoRecibos)
                    {
                        var recibo = await _context.ReciboCobros.FindAsync(relacion.IdRecibo);
                        if (recibo != null)
                        {
                            recibos.Add(recibo);
                        }
                    }
                }
                // actualizar recibos.Abonados, deuda y saldo
                var result = await _repoPagosRecibidos.ConfirmarPagoPropietario(recibos, pagoRecibido, pagoPropiedad);

                if (result == "exito")
                {
                    // propiedad y condominio
                    if (pagoPropiedad != null)
                    {
                        var propiedad = await _context.Propiedads
                            .Include(c => c.IdUsuarioNavigation)
                            .Include(c => c.IdCondominioNavigation)
                            .Where(c => c.IdPropiedad == pagoPropiedad.IdPropiedad)
                            .FirstOrDefaultAsync();

                        if (propiedad != null)
                        {
                            var bancoSubcuenta = await _context.SubCuenta.FindAsync(Convert.ToInt32(referencia.Banco));

                            referencia.Banco = bancoSubcuenta.Descricion;

                            var envioCorreo = _serviceEmail.ConfirmacionPago(propiedad.IdCondominioNavigation.Email,
                                propiedad.IdUsuarioNavigation.Email,
                                propiedad,
                                recibos,
                                pagoRecibido,
                                referencia,
                                propiedad.IdCondominioNavigation.ClaveCorreo);

                            // si se envia el correo 
                            if (envioCorreo.Contains("OK"))
                            {
                                return RedirectToAction("PagosRecibidosAdmin");

                            }
                            else
                            {
                                var modeloError = new ErrorViewModel()
                                {
                                    RequestId = envioCorreo
                                };

                                return View("Error", modeloError);
                            }

                        }
                    }

                    TempData.Keep();

                    return RedirectToAction("PagosRecibidosAdmin");

                }
                else
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = result
                    };

                    return View("Error", modeloError);
                }

            }

            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult RectificarPago(int id)
        {
            try
            {
                TempData["idPagoConfirmar"] = id.ToString();
                return View();
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

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> ConfirmarRectificarPago()
        {
            try
            {
                int id = Convert.ToInt32(TempData.Peek("idPagoConfirmar").ToString());

                // buscar pago
                var pago = await _context.PagoRecibidos.FindAsync(id);

                if (pago != null)
                {

                    // buscar pagoPropiedad
                    var pagoPropiedad = await _context.PagoPropiedads.FirstAsync(c => c.IdPago == id);
                    var condominio = await _context.Condominios.FindAsync(pago.IdCondominio);
                    // hacer Rectificado true
                    if (pagoPropiedad != null && condominio != null)
                    {
                        var propiedad = await _context.Propiedads.FindAsync(pagoPropiedad.IdPropiedad);
                        var usuario = await _signInManager.UserManager.FindByIdAsync(propiedad.IdUsuario);
                        var referencia = new ReferenciasPr();

                        if (pago.FormaPago)
                        {
                            referencia = await _context.ReferenciasPrs.FirstAsync(c => c.IdPagoRecibido == pago.IdPagoRecibido);
                        }

                        // enviar correo
                        var resultado = _serviceEmail.RectificarPago(condominio.Email, usuario.Email, condominio.ClaveCorreo != null ? condominio.ClaveCorreo : "", pago, referencia);

                        // si se envia el correo 
                        if (resultado.Contains("OK"))
                        {
                            // -> eliminar pago propiedad
                            _context.PagoPropiedads.Remove(pagoPropiedad);
                            // -> eliminar pago recibos
                            var pagoRecibo = await _context.PagosRecibos.FirstAsync(c => c.IdPago == pago.IdPagoRecibido);
                            _context.PagosRecibos.Remove(pagoRecibo);
                            // -> eliminar referencia
                            _context.ReferenciasPrs.Remove(referencia);
                            // -> eliminar pago
                            _context.PagoRecibidos.Remove(pago);

                            await _context.SaveChangesAsync();
                        }
                    }
                }

                TempData.Keep();

                return RedirectToAction("PagosRecibidosAdmin");

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

        //[HttpPost]
        //[Authorize(Policy = "RequireAdmin")]
        //public IActionResult UsuarioContraseña(string contrasena)
        //{
        //    TempData["Contrasena"] = contrasena;

        //    return Json(new { success = true, message = "Datos almacenados correctamente" });
        //}

        [Authorize(Policy = "RequirePropietario")]
        public async Task<IActionResult> PagosRecibidosPropietario()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                //llamar a pagos realizados
                // pasar diccionario de propiedades y por cada propiedad una lista de pagos realizados
                var modelo = new Dictionary<Propiedad, List<PagoRecibido>>();
                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();
                if (propiedades != null && propiedades.Any())
                {
                    foreach (var item in propiedades)
                    {
                        //var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();

                        var pagos = await (from pago in _context.PagoPropiedads
                                           join pagoRecibido in _context.PagoRecibidos
                                           on pago.IdPago equals pagoRecibido.IdPagoRecibido
                                           where pago.IdPropiedad == item.IdPropiedad
                                           select pagoRecibido).ToListAsync();

                        modelo.Add(item, pagos);
                    }
                }

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
        /// <param name="id">Id del pago de la propiedad</param>
        /// <returns></returns>
        public async Task<IActionResult> ComprobantePDF(int id)
        {

            var pagoPropiedad = await _context.PagoPropiedads.FindAsync(id);
            if (pagoPropiedad != null)
            {
                var data = await _servicesPDF.ComprobantePagoRecibidoPDF(pagoPropiedad);

                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "ComprobantePago.pdf");
            }

            return View("PagosConfirmados");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpPost]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SendEmailCompPago(int id)
        {
            var pagoPropiedad = await _context.PagoPropiedads.FindAsync(id);
            if (pagoPropiedad != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(pagoPropiedad.IdPropiedad);
                var pago = await _context.PagoRecibidos.FindAsync(pagoPropiedad.IdPago);
                var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);
                var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);
                var data = await _servicesPDF.ComprobantePagoRecibidoPDF(pagoPropiedad);

                EmailAttachmentPdf email = new EmailAttachmentPdf()
                {
                    From = condominio.Email,
                    To = usuario.Email,
                    Pdf = data,
                    FileName = "ComprobantePago_" + propiedad.Codigo + "_" + DateTime.Today.ToString("dd/MM/yyyy"),
                    Subject = "Comprobante de Pago - " + condominio.Nombre,
                    Password = condominio.ClaveCorreo != null ? condominio.ClaveCorreo : ""
                };

                var result = _serviceEmail.SendEmailRG(email);

                if (!result.Contains("OK"))
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = result
                    };

                    return View("Error", modeloError);
                }
            }

            return RedirectToAction("PagosConfirmados");
        }

        private bool PagoRecibidoExists(int id)
        {
            return _context.PagoRecibidos.Any(e => e.IdPagoRecibido == id);
        }
    }
}
