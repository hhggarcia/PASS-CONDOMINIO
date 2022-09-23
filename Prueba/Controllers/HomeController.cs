using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prueba.Areas.Identity.Data;
using Prueba.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace Prueba.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SignInManager<ApplicationUser> signInManager, ILogger<HomeController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // REVISAR USUARIO LOG -> DEPENDIENDO DE SU ROL REDIRECCIONAR
            if (_signInManager.Context.User.Identity.IsAuthenticated)
            {
                var idClaim = _signInManager.Context.User
                    .Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = idClaim.Value;

                var user = await _signInManager.UserManager.FindByIdAsync(id);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
                else
                {
                    var claims = new List<Claim>
                    {
                        new Claim("amr", "pwd"),
                    };
                    var roles = await _signInManager.UserManager.GetRolesAsync(user);
                    if (roles.Any())
                    {
                        //"Manager,User"
                        var roleClaim = string.Join(",", roles);
                        claims.Add(new Claim("Roles", roleClaim));

                        TempData["idUserLog"] = user.Id;

                        switch (roles.FirstOrDefault())
                        {
                            case "Propietario":
                                //GUARDAR EN TEMPDATA EL ID DEL PROPIETARIO LOGEADO
                                return RedirectToAction("Index", "Propietarios");
                            case "Administrador":
                                //GUARDAR EN TEMPDATA EL ID DEL ADMINISTRADOR LOGEADO
                                return RedirectToAction("Index", "Administrador");
                            case "SuperAdmin":
                                //GUARDAR EN TEMPDATA EL ID DEL SUPERADMIN LOGEADO
                                return RedirectToAction("Dashboard", "Admin");
                            default:
                                return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}