using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Prueba.Controllers
{
    public class TipoCasosController : Controller
    {
        // GET: TipoCasosController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TipoCasosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TipoCasosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoCasosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TipoCasosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TipoCasosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TipoCasosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TipoCasosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
