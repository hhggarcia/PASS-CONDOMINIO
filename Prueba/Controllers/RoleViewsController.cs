using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prueba.Core;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    public class RoleViewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Policy = "RequirePropietario")]
        public IActionResult Propietario()
        {
            return View();
        }

        [Authorize(Policy = "RequireAdmin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Policy = "RequireSuperAdmin")]
        public IActionResult SuperAdmin()
        {
            return View();
        }
    }
}