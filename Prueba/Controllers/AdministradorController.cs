using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Services;
using Prueba.Utils;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class AdministradorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailService _serviceEmail;
        private readonly IManageExcel _manageExcel;
        private readonly PruebaContext _context;

        public AdministradorController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            PruebaContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            //CON EL ID DEL USUARIO LOGEADO BUSCAR SUS CONDOMINIOS ASIGNADOS
            var idAdministrador = TempData.Peek("idUserLog").ToString();

            //CARGAR LIST DE CONDOMINIOS
            var condominios = from c in _context.Condominios
                              where c.IdAdministrador == idAdministrador
                              select c;

            foreach (var item in condominios)
            {
                var inmuebles = from i in _context.Inmuebles
                                where item.IdCondominio == i.IdCondominio
                                select i;

                item.Inmuebles = await inmuebles.ToListAsync();
            }

            var condominiosModel = await condominios.ToListAsync();

            TempData.Keep();

            return View(condominiosModel);
        }
        public IActionResult RegistrarPagos()
        {
            return View();
        }
        public IActionResult PagosRecibidos()
        {
            return View();
        }
        public IActionResult LibroDiario()
        {
            return View();
        }
        public IActionResult Deudores()
        {
            return View();
        }
        public IActionResult RelaciondeGastos()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
