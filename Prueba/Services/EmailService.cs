using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using MimeKit;
using MimeKit.Text;
using Prueba.Models;
using Prueba.ViewModels;
using System.Linq.Expressions;
using System.Text;

namespace Prueba.Services
{
    public interface IEmailService
    {
        void SendEmail(RegisterConfirm request);
        string RectificarPago(string EmailFrom, string EmailTo, string password, PagoRecibido pago, ReferenciasPr referencia);
        void ConfirmacionPagoCuota(String EmailFrom, String EmailTo, CuotasEspeciale cuotasEspeciale, ReciboCuota reciboCobro, PagoRecibido pagoRecibido, String password);
        void RectificarPagoCuotaEspecial(String EmailFrom, String EmailTo, CuotasEspeciale cuotasEspeciale, PagoRecibido pago, String password);
        void EmailGastosCuotas(String EmailFrom, IList<GastosCuotasEmailVM> relacionGastosEmailVM, String password);
        string SendEmailRG(EmailAttachmentPdf model);
        string SendEmailAttachement(EmailAttachmentPdf model);
        string SendEmailAList(EmailAttachmentPdf model, IList<string> receptores);
        string ConfirmacionPago(string EmailFrom, string EmailTo, Propiedad propiedad, IList<ReciboCobro> recibos, PagoRecibido pago, ReferenciasPr referencia, string password);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public void SendEmail(RegisterConfirm request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="EmailFrom"></param>
        /// <param name="EmailTo"></param>
        /// <param name="propiedad"></param>
        /// <param name="recibos"></param>
        /// <param name="pago"></param>
        /// <param name="referencia"></param>
        /// <param name="password"></param>
        public string ConfirmacionPago(String EmailFrom, String EmailTo, Propiedad propiedad, IList<ReciboCobro> recibos, PagoRecibido pago, ReferenciasPr referencia, String password)
        {

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(EmailFrom));
                email.To.Add(MailboxAddress.Parse(EmailTo));
                email.Subject = "Confirmación de pago " + propiedad.Codigo;
                var result = string.Empty;

                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = $@"
                    <html>
                        <body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 5rem; padding: 2rem;"">
                            
                                <div style="" margin: 1rem;"">
                                    <h3 style=""color: #3950a2;"">{email.Subject}</h3>
                                    <h4>¡Gracias por realizar su pago!</h4>
                                    <p>Su pago fue confirmado con éxito. Se detallan los datos:</p>
                                </div>
                            
                                <div
                                    style="" margin: 0 auto; width: 100%;
                                      overflow-x: auto;
                                    ""
                                  >
                                    <table border='1' style='border-collapse: collapse; width: 100%'>
                                        <tr>
                                            <th style='background-color: #3950a2; color: white; text-align: center; padding: 10px;'>Fecha</th>
                                            <th style='background-color: #3950a2; color: white; text-align: center; padding: 10px;'>Método de Pago</th>
                                            <th style='background-color: #3950a2; color: white; text-align: center; padding: 10px;'>Referencia #</th>
                                            <th style='background-color: #3950a2; color: white; text-align: center; padding: 10px;'>Banco</th>
                                            <th style='background-color: #3950a2; color: white; text-align: center; padding: 10px;'>Monto</th>
                                        </tr>
                                        <tr>
                                            <td style='text-align: center; padding: 10px;'>{pago.Fecha.ToString("dd/MM/yyyy")}</td>
                                            <td style='text-align: center; padding: 10px;'>{(pago.FormaPago ? "Transferencia" : "Efectivo")}</td>
                                            <td style='text-align: center; padding: 10px;'>{referencia.NumReferencia}</td>
                                            <td style='text-align: center; padding: 10px;'>{referencia.Banco}</td>
                                            <td style='text-align: center; padding: 10px;'>{pago.Monto.ToString("N")} Bs</td>
                                        </tr>
                                    </table>
                                    <hr/>                                    
                                </div>                                

                                <div class=""footer"" style='color: #3950a2; padding: 10px; text-align: center; position: fixed; bottom: 0; width: 100%;'>
                                    Desarrollado por: Password Technology C.A.
                                </div>
                                                      
                        </body>
                    </html>"
                };

                using var smtp2 = new SmtpClient();
                smtp2.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtp2.Authenticate(EmailFrom, password);
                result = smtp2.Send(email);
                smtp2.Disconnect(true);

                return result;
            }
            catch (Exception ex)
            {
                return $"Error al enviar el correo: {ex.Message}";
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="EmailFrom"></param>
        /// <param name="EmailTo"></param>
        /// <param name="password"></param>
        /// <param name="pago"></param>
        /// <param name="referencia"></param>
        public string RectificarPago(string EmailFrom, string EmailTo, string password, PagoRecibido pago, ReferenciasPr referencia)
        {
            try
            {
                var result = string.Empty;

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(EmailFrom));
                email.To.Add(MailboxAddress.Parse(EmailTo));
                email.Subject = "Rectificación de pago";

                email.Body = new TextPart(TextFormat.Html)
                {
                    Text =
                       $@"
                    <html>
                        <body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 5rem; padding: 2rem;"">
                            <h3 style=""color: #3950a2;"">{email.Subject}</h3>
                            <h4 style=""color: #3950a2;"">NO RESPONDA ESTE CORREO SIMPLEMENTE NOTIFIQUE NUEVAMENTE POR LA APLICACIÓN WEB</h4>
                            <h4>Su pago no fue aceptado</h4>
                            <p>Su pago fue verificado y no aparece en los estados de cuenta de la admistración. A continuación, se detallan los datos:</p>

                            <table border='1' style='border-collapse: collapse; width: 100%;'>
                                <tr>
                                    <th style='background-color: #3950a2; color: white;'>Fecha</th>
                                    <th style='background-color: #3950a2; color: white;'>Método de Pago</th>
                                    <th style='background-color: #3950a2; color: white;'>Referencia #</th>
                                    <th style='background-color: #3950a2; color: white;'>Banco</th>
                                    <th style='background-color: #3950a2; color: white;'>Monto</th>
                                </tr>
                                <tr>
                                    <td>{pago.Fecha.ToString("dd/MM/yyyy")}</td>
                                    <td>{(pago.FormaPago ? "Transferencia" : "Efectivo")}</td>
                                    <td>{referencia.NumReferencia}</td>
                                    <td>{referencia.Banco}</td>
                                    <td>{pago.Monto.ToString("N")} Bs</td>
                                </tr>
                            </table>
                            <hr/>
                            <p>Te invitamos a revisar sus transacciones y notificar nuevamente el pago, ya que este será eliminado del sistema</p>

                            <div class=""footer"" style='color: #3950a2; padding: 10px; text-align: center; position: fixed; bottom: 0; width: 100%;'>
                                Desarrollado por: Password Technology C.A.
                            </div>
                        </body>
                    </html>"
                };

                using var smtp = new SmtpClient();
                smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(EmailFrom, password);
                result = smtp.Send(email);
                smtp.Disconnect(true);

                return result;
            }
            catch (Exception ex)
            {
                return $"Error al enviar el correo: {ex.Message}";
            }
        }
        public void ConfirmacionPagoCuota(String EmailFrom, String EmailTo, CuotasEspeciale cuotasEspeciale, ReciboCuota reciboCobro, PagoRecibido pagoRecibido, String password)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(EmailTo));
            email.Subject = "Conformación de pago Cuota Especial";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text =
                    $@"
                <html>
                <body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;"">

                    <h3 style=""color: #3950a2;"">{{email.Subject}}</h3>
                    <h4>¡Gracias por realizar su pago!</h4>
                    <p>Confirmamos la recepción del pago realizado por la cuota {{cuotasEspeciale.Descripcion}}. A continuación, se detallan los datos:</p>

                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <th style='background-color: #3950a2; color: white;'>Recibo</th>
                            <th style='background-color: #3950a2; color: white;'>Fecha</th>
                            <th style='background-color: #3950a2; color: white;'>Cuota Especial</th>
                            <th style='background-color: #3950a2; color: white;'>Cuota Pagadas</th>
                            <th style='background-color: #3950a2; color: white;'>Cuota Faltantes</th>
                            <th style='background-color: #3950a2; color: white;'>Deuda</th>
                            <th style='background-color: #3950a2; color: white;'>Abonado</th>
                            <th style='background-color: #3950a2; color: white;'>Monto</th>
                        </tr>
                        <tr>
                            <td>{{reciboCobro.IdReciboCuotas}}</td>
                            <td>{{DateTime.Now}}</td>
                            <td>{{cuotasEspeciale.Descripcion}} Bs</td>
                            <td>{{reciboCobro.CuotasPagadas}} Bs</td>
                            <td>{{reciboCobro.CuotasFaltantes}} Bs</td>
                            <td>{{reciboCobro.SubCuotas}} Bs</td>
                            <td>{{reciboCobro.Abonado}} Bs</td>
                            <td>{{pagoRecibido.MontoRef}} Bs</td>
                        </tr>
                    </table>

                    <div class=""footer"" style='background-color: #333; color: #fff; padding: 10px; text-align: center; position: fixed; bottom: 0; width: 100%;'>
                        Desarrollado por Password Tecnology
                    </div>

                </body>
                </html>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(EmailFrom, password);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
        public void RectificarPagoCuotaEspecial(String EmailFrom, String EmailTo, CuotasEspeciale cuotasEspeciale, PagoRecibido pago, String password)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(EmailFrom));
            email.To.Add(MailboxAddress.Parse(EmailTo));
            email.Subject = "Rectificación de pago Cuota Especial";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text =
                   $@"
                <html>
                <body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;"">

                    <h3 style=""color: #3950a2;"">{{email.Subject}}</h3>
                    <h4>Su pago no fue aceptado</h4>
                    <p>Lamentamos informarle que su pago no ha sido aceptado. A continuación, se detallan los datos:</p>

                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <th style='background-color: #3950a2; color: white;'>Fecha</th>
                            <th style='background-color: #3950a2; color: white;'>Cuota Especial</th>
                            <th style='background-color: #3950a2; color: white;'>Método de Pago</th>
                            <th style='background-color: #3950a2; color: white;'>Monto</th>
                        </tr>
                        <tr>
                            <td>{{pago.Fecha}}</td>
                            <td>{{cuotasEspeciale.Descripcion}}</td>
                            <td>{{{{pago.FormaPago ? 'Transferencia' : 'Efectivo'}}}}</td>
                            <td>{{pago.Monto}} Bs</td>
                        </tr>
                    </table>

                    <div class=""footer"" style='background-color: #333; color: #fff; padding: 10px; text-align: center; position: fixed; bottom: 0; width: 100%;'>
                        Desarrollado por Password Tecnology
                    </div>

                </body>
                </html>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            //smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
            smtp.Authenticate(EmailFrom, password);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
        public void EmailGastosCuotas(String EmailFrom, IList<GastosCuotasEmailVM> relacionGastosEmailVM, String password)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(EmailFrom));

            foreach (var item in relacionGastosEmailVM)
            {
                email.To.Add(MailboxAddress.Parse(item.Email));
                if (item.CuotasEspeciale == null)
                {
                    email.Subject = "Recibo de Cobro para la propiedad " + item.Propiedad.Codigo;
                    email.Body = new TextPart(TextFormat.Html)
                    {
                        Text =
                       $@"
                        <html>
                       <body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;"">

                            <h3 style=""color: #3950a2;"">{{email.Subject}}</h3>
                            <p>A continuación, se detallan los datos:</p>

                            <table border='1' style='border-collapse: collapse; width: 100%;'>
                                <tr>
                                    <th style='background-color: #3950a2; color: white;'>Fecha</th>
                                    <th style='background-color: #3950a2; color: white;'>Propiedad</th>
                                    <th style='background-color: #3950a2; color: white;'>Monto</th>
                                </tr>
                                <tr>
                                    <td>{{item.ReciboCobro.Fecha}}</td>
                                    <td>{{item.Propiedad.Codigo}}</td>
                                    <td>{{item.ReciboCobro.MontoRef}}</td>
                                </tr>
                            </table>

                            <div class=""footer"" style='background-color: #333; color: #fff; padding: 10px; text-align: center; position: fixed; bottom: 0; width: 100%;'>
                                Desarrollado por Password Tecnology
                            </div>

                        </body>
                        </html>"
                    };
                }
                else
                {
                    email.Subject = "Recibo de Cobro para la Cuota Especial " + item.CuotasEspeciale.Descripcion;
                    email.Body = new TextPart(TextFormat.Html)
                    {
                        Text =
                       $@"
                        <html>
                        <body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;"">

                            <h3 style=""color: #3950a2;"">{{email.Subject}}</h3>
                            <p>A continuación, se detallan los datos:</p>

                            <table style=""border-collapse: collapse; width: 100%;"">
                                <tr>
                                    <th style=""background-color: #3950a2; color: white;"">Fecha</th>
                                    <th style=""background-color: #3950a2; color: white;"">Propiedad</th>
                                    <th style=""background-color: #3950a2; color: white;"">Cantidad de cuotas</th>
                                    <th style=""background-color: #3950a2; color: white;"">Monto de cuotas</th>
                                    <th style=""background-color: #3950a2; color: white;"">Monto Total</th>
                                </tr>
                                <tr>
                                    <td>{{item.CuotasEspeciale.FechaInicio}}</td>
                                    <td>{{item.Propiedad.Codigo}}</td>
                                    <td>{{item.CuotasEspeciale.CantidadCuotas}}</td>
                                    <td>{{item.CuotasEspeciale.SubCuotas / item.CuotasEspeciale.CantidadCuotas}} Bs</td>
                                    <td>{{item.CuotasEspeciale.SubCuotas}} Bs</td>
                                </tr>
                            </table>

                            <div class=""footer"" style=""background-color: #333; color: #fff; padding: 10px; text-align: center; position: fixed; bottom: 0; width: 100%;"">
                                Desarrollado por Password Tecnology
                            </div>

                        </body>

                        </html>"
                    };
                }

                using var smtp = new SmtpClient();
                smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                //smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
                smtp.Authenticate(EmailFrom, password);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }

        /// <summary>
        /// Envia un correo con un archivo adjunto
        /// </summary>
        /// <param name="model">contiene la descripcion del correo</param>
        /// <returns></returns>
        public string SendEmailRG(EmailAttachmentPdf model)
        {
            try
            {
                var result = string.Empty;
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(model.From));
                email.To.Add(MailboxAddress.Parse(model.To));
                email.Subject = model.Subject;

                var pdfAttachment = new MimePart("application", "pdf")
                {
                    Content = new MimeContent(new MemoryStream(model.Pdf)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = model.FileName + ".pdf"
                };

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = model.Body;
                bodyBuilder.Attachments.Add(pdfAttachment);
                email.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(model.From, model.Password);
                result = smtpClient.Send(email);
                smtpClient.Disconnect(true);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                // Maneja el error según tus necesidades (registra, notifica, etc.)
                return $"Error al enviar el correo: {ex.Message}";
            }
        }

        public string SendEmailAttachement(EmailAttachmentPdf model)
        {
            try
            {
                var result = string.Empty;
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(model.From));
                email.To.Add(MailboxAddress.Parse(model.To));
                email.Subject = model.Subject;

                var pdfAttachment = new MimePart("application", "pdf")
                {
                    Content = new MimeContent(model.Attachment.OpenReadStream()),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = model.FileName + ".pdf"
                };

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = model.Body;
                bodyBuilder.Attachments.Add(pdfAttachment);
                email.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(model.From, model.Password);
                result = smtpClient.Send(email);
                smtpClient.Disconnect(true);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                // Maneja el error según tus necesidades (registra, notifica, etc.)
                return $"Error al enviar el correo: {ex.Message}";
            }
        }

        public string SendEmailAList(EmailAttachmentPdf model, IList<string> receptores)
        {
            try
            {
                var result = string.Empty;
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(model.From));
                foreach (var item in receptores)
                {
                    email.To.Add(MailboxAddress.Parse(item));
                }
                email.Subject = model.Subject;

                var pdfAttachment = new MimePart("application", "pdf")
                {
                    Content = new MimeContent(model.Attachment.OpenReadStream()),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = model.FileName + ".pdf"
                };

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = model.Body;
                bodyBuilder.Attachments.Add(pdfAttachment);
                email.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(model.From, model.Password);
                result = smtpClient.Send(email);
                smtpClient.Disconnect(true);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                // Maneja el error según tus necesidades (registra, notifica, etc.)
                return $"Error al enviar el correo: {ex.Message}";
            }
        }
    }
}
