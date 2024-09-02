using Glamping2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glamping2.Controllers
{
    public class DashboardController : Controller
    {

        private readonly GLAMPINGContext _context;

        public DashboardController(GLAMPINGContext context)
        {
            _context = context;
        }
        // GET: DashboardController
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener el correo electrónico del usuario autenticado
                var userEmail = User.Identity.Name;

                // Redirigir al login si el usuario no está autenticado
                if (userEmail == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener el rol del usuario basado en su correo electrónico
                var userRole = await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdRol)
                    .FirstOrDefaultAsync();

                // Redirigir a la página principal si el rol no está definido o no es "Administrador"
                if (userRole == 0)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Obtener el nombre del rol basado en el IdRol del usuario
                var role = await _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefaultAsync();

                // Determinar si el usuario es administrador
                if (role != "Administrador")
                {
                    return RedirectToAction("Index", "Home");
                }

                // Obtener todas las reservas
                var reservas = await _context.Reservas.ToListAsync();

                // Calcular el total general
                var totalGeneral = reservas.Sum(r => r.Total); // Asegúrate de que `Total` es una propiedad válida

                // Preparar el modelo
                var viewModel = new DashboardViewModel
                {
                    TotalGeneral = totalGeneral,
                    Reservas = reservas
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Manejar errores y mostrar una vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        // GET: DashboardController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DashboardController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DashboardController/Create
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

        // GET: DashboardController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DashboardController/Edit/5
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

        // GET: DashboardController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DashboardController/Delete/5
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
