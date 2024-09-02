using Glamping2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Glamping2.Controllers
{
    public class HomeController2 : Controller
    {
        private readonly GLAMPINGContext _context;

        public HomeController2(GLAMPINGContext context)
        {
            _context = context;
        }
        // GET: HomeController2
        public ActionResult Index()
        {
            return View();
        }

        // GET: HomeController2/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeController2/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeController2/Create
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

        // GET: HomeController2/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeController2/Edit/5
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

        // GET: HomeController2/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController2/Delete/5
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
