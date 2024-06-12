using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Prueba.Context;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;
using System.Net.Mail;
using System.Text;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class CorreoController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPDFServices _servicesPDF;
        private readonly IRelacionGastoRepository _repoRelacionGastos;
        private readonly IEmailService _servicesEmail;
        private readonly NuevaAppContext _context;

        public CorreoController(IWebHostEnvironment webHostEnvironment,
            IPDFServices servicesPDF,
            IRelacionGastoRepository repoRelacionGastos,
            IEmailService servicesEmail,
            NuevaAppContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _servicesPDF = servicesPDF;
            _repoRelacionGastos = repoRelacionGastos;
            _servicesEmail = servicesEmail;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Envia un correo al propietario con 
        /// el detalle del recibo seleccionado
        /// </summary>
        /// <param name="id">Id del recibo de cobro a enviar a correo</param>
        /// <returns></returns>
        public async Task<IActionResult> SendReciboCobro(int id)
        {
            var recibo = await _context.ReciboCobros.FindAsync(id);
            var modelo = new DetalleReciboTransaccionesVM();
            var email = new EmailAttachmentPdf();

            if (recibo != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);
                var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);

                modelo.Recibo = recibo;
                modelo.Propiedad = propiedad;
                modelo.GruposPropiedad = gruposPropiedad;
                modelo.RelacionGasto = rg;
                modelo.Transacciones = transacciones;

                var data = await _servicesPDF.DetalleReciboTransaccionesPDF(modelo);

                email.From = modelo.Transacciones.Condominio.Email;
                email.To = usuario.Email != null ? usuario.Email : "";
                email.Pdf = data;
                email.FileName = "Recibo" + "_" + recibo.Fecha.ToString("dd/MM/yyyy") + propiedad.Codigo.ToString();
                email.Password = modelo.Transacciones.Condominio.ClaveCorreo != null ? modelo.Transacciones.Condominio.ClaveCorreo : "";
                email.Subject = modelo.Transacciones.Condominio.Nombre + " Recibo " + recibo.Mes;


                var result = _servicesEmail.SendEmailRG(email);

                if (!result.Contains("OK"))
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = result
                    };

                    return View("Error", modeloError);
                }
            }

            return RedirectToAction("Index", "RelacionGastos");
        }

        /// <summary>
        /// Envia cada recibo a los propietarios
        /// por cada propiedad
        /// </summary>
        /// <param name="id">Id de la relacion de gastos para enviar todos sus recibos</param>
        /// <returns></returns>
        public async Task<IActionResult> SendAllRecibosCobro(int id)
        {
            var rg = await _context.RelacionGastos.FindAsync(id);

            if (rg != null)
            {
                var recibos = await _context.ReciboCobros.Where(c => c.IdRgastos == rg.IdRgastos).ToListAsync();

                if (recibos.Any())
                {
                    foreach (var recibo in recibos)
                    {
                        var modelo = new DetalleReciboTransaccionesVM();
                        var email = new EmailAttachmentPdf();
                        var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                        var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                        var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);
                        var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);

                        modelo.Recibo = recibo;
                        modelo.Propiedad = propiedad;
                        modelo.GruposPropiedad = gruposPropiedad;
                        modelo.RelacionGasto = rg;
                        modelo.Transacciones = transacciones;

                        var data = await _servicesPDF.DetalleReciboTransaccionesPDF(modelo);

                        email.From = modelo.Transacciones.Condominio.Email;
                        email.To = usuario.Email;
                        email.Pdf = data;
                        email.FileName = "Recibo" + "_" + recibo.Fecha.ToString("dd/MM/yyyy") + propiedad.Codigo.ToString();
                        email.Password = modelo.Transacciones.Condominio.ClaveCorreo != null ? modelo.Transacciones.Condominio.ClaveCorreo : "";
                        email.Subject = modelo.Transacciones.Condominio.Nombre + " Recibo " + recibo.Mes;

                        var result = _servicesEmail.SendEmailRG(email);

                        if (!result.Contains("OK"))
                        {
                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = result
                            };

                            return View("Error", modeloError);
                        }
                    }
                }
            }

            return RedirectToAction("Index", "RelacionGastos");
        }

        // enviar comprobante de pago propietario
        public async Task<IActionResult> SendCompPagoPropietario(int id)
        {
            var pagoPropiedad = await _context.PagoPropiedads.FindAsync(id);
            if (pagoPropiedad != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(pagoPropiedad.IdPropiedad);
                //var pago = await _context.PagoRecibidos.FindAsync(pagoPropiedad.IdPago);
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

                var result = _servicesEmail.SendEmailRG(email);

                if (!result.Contains("OK"))
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = result
                    };

                    return View("Error", modeloError);
                }
            }

            return RedirectToAction("PagosConfirmados", "PagoRecibidos");
        }

        // enviar correo general a todos los propietarios
        public IActionResult CorreoGlobal()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CorreoGlobal(EmailAttachmentPdf modelo, IFormFile file)
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var condominio = await _context.Condominios.FindAsync(idCondominio);

                if (condominio != null)
                {
                    modelo.From = condominio.Email;
                    modelo.Password = condominio.ClaveCorreo != null ? condominio.ClaveCorreo : "";
                    // pdf a enviar

                    modelo.Attachment = file;

                    // buscar usuarios del condominio si poseen propiedades
                    // recorrer a los usuarios y enviar un correo a cada uno
                    var usuarios = await (from p in _context.Propiedads.Where(c => c.IdCondominio == idCondominio)
                                          join c in _context.AspNetUsers
                                          on p.IdUsuario equals c.Id
                                          select c.Email
                                          ).ToListAsync();

                    foreach (var user in usuarios)
                    {
                        modelo.To = user ?? "";

                        var result = _servicesEmail.SendEmailAttachement(modelo);

                        if (!result.Contains("OK"))
                        {
                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = result
                            };

                            TempData.Keep();
                            return View("Error", modeloError);
                        }
                    }
                }

                TempData.Keep();
                return RedirectToAction("Dashboard", "Administrador", new { id = idCondominio });
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

        // enviar correo a todos los clientes

        // enviar correo con factura de venta a un cliente
    }
}
