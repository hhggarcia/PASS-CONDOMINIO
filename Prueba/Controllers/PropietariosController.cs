using Microsoft.AspNetCore.Mvc;

namespace Prueba.Controllers
{
    public class PropietariosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Historial()
        {
            return View();
        }

        public IActionResult DashboardUsuario()
        {
            return View();
        }
        public IActionResult RegistrarPagos()
        {
            return View();
        }
    }
}
