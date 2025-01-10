using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Prueba.Controllers
{
    public class CasosController : Controller
    {
        // GET: CasosController1
        public ActionResult Index()
        {
            return View();
        }

        // GET: CasosController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CasosController1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CasosController1/Create
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

        // GET: CasosController1/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CasosController1/Edit/5
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

        // GET: CasosController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CasosController1/Delete/5
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
