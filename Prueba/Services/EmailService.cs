using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Prueba.Models;
using Prueba.ViewModels;
using System.Linq.Expressions;

namespace Prueba.Services
{
    public interface IEmailService
    {
        void SendEmail(RegisterConfirm request);
        void ConfirmacionPago(String EmailTo, Propiedad propiedad, ReciboCobro reciboCobro, PagoRecibido pagoRecibido);
        void RectificarPago(String EmailTo, PagoRecibido pago);
        void ConfirmacionPagoCuota(String EmailTo, CuotasEspeciale cuotasEspeciale, ReciboCuota reciboCobro, PagoRecibido pagoRecibido);
        void RectificarPagoCuotaEspecial(String EmailTo, CuotasEspeciale cuotasEspeciale, PagoRecibido pago);
    }
    public class EmailService: IEmailService
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
        public void ConfirmacionPago(String EmailTo, Propiedad propiedad, ReciboCobro reciboCobro, PagoRecibido pagoRecibido)
        {

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(EmailTo));
            email.Subject = "Conformación de pago";
            email.Body = new TextPart(TextFormat.Html) {
            Text =
                    $@"
                <html>
                <body>
                    <h3>{email.Subject}</h3>
                     <h4>¡Gracias por realizar su pago!</h4>
                     <p>Su pago fue confirmado con éxito. Se detallan los datos:</p>
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <th>Recibo</th>
                            <th>Fecha</th>
                            <th>Deuda</th>
                            <th>Abonado</th>
                            <th>Monto</th>
                        </tr>
                        <tr>
                            <td>{reciboCobro.IdReciboCobro}</td>
                            <td>{DateTime.Now}</td>
                            <td>{propiedad.Saldo + propiedad.Deuda} Bs</td>
                            <td>{reciboCobro.Abonado} Bs</td>
                            <td>{pagoRecibido.MontoRef} Bs</td>
                        </tr>
                    </table>
                </body>
                </html>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void RectificarPago(String EmailTo, PagoRecibido pago)
        {

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(EmailTo));
            email.Subject = "Rectificación de pago";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text =
                   $@"
                <html>
                <body>
                    <h3>{email.Subject}</h3>
                     <h4>Su pago no fue aceptado</h4>
                     <p>Lamentamos informarle que su pago no ha sido aceptado. A continuación, se detallan los datos:</p>
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <th>Fecha</th>
                            <th>Método de Pago</th>
                            <th>Monto</th>
                        </tr>
                        <tr>
                            <td>{pago.Fecha}</td>
                            <td>{{pago.FormaPago ? 'Transferencia' : 'Efectivo'}}</td>
                            <td>{pago.Monto} Bs</td>
                        </tr>
                    </table>
                </body>
                </html>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
        public void ConfirmacionPagoCuota(String EmailTo, CuotasEspeciale cuotasEspeciale, ReciboCuota reciboCobro, PagoRecibido pagoRecibido)
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
                <body>
                    <h3>{email.Subject}</h3>
                     <h4>¡Gracias por realizar su pago!</h4>
                     <p>Confirmamos la recepción del pago realizado por la cuota {cuotasEspeciale.Descripcion}. A continuación, se detallan los datos:</p>
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <th>Recibo</th>
                            <th>Fecha</th>
                            <th>Cuota Especial</th>
                            <th>Cuota Pagadas</th>
                            <th>Cuota Faltantes</th>
                            <th>Deuda</th>
                            <th>Abonado</th>
                            <th>Monto</th>
                        </tr>
                        <tr>
                            <td>{reciboCobro.IdReciboCuotas}</td>
                            <td>{DateTime.Now}</td>
                            <td>{cuotasEspeciale.Descripcion} Bs</td>
                            <td>{reciboCobro.CuotasPagadas} Bs</td>
                            <td>{reciboCobro.CuotasFaltantes} Bs</td>
                            <td>{reciboCobro.SubCuotas} Bs</td>
                            <td>{reciboCobro.Abonado} Bs</td>
                            <td>{pagoRecibido.MontoRef} Bs</td>
                        </tr>
                    </table>
                </body>
                </html>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void RectificarPagoCuotaEspecial(String EmailTo,CuotasEspeciale cuotasEspeciale, PagoRecibido pago)
        {

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(EmailTo));
            email.Subject = "Rectificación de pago Cuota Especial";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text =
                   $@"
                <html>
                <body>
                    <h3>{email.Subject}</h3>
                     <h4>Su pago no fue aceptado</h4>
                     <p>Lamentamos informarle que su pago no ha sido aceptado. A continuación, se detallan los datos:</p>
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <th>Fecha</th>
                            <th>Cuota Especial</th>
                            <th>Método de Pago</th>
                            <th>Monto</th>
                        </tr>
                        <tr>
                            <td>{pago.Fecha}</td>
                            <td>{cuotasEspeciale.Descripcion}</td>
                            <td>{{pago.FormaPago ? 'Transferencia' : 'Efectivo'}}</td>
                            <td>{pago.Monto} Bs</td>
                        </tr>
                    </table>
                </body>
                </html>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

    }
}
