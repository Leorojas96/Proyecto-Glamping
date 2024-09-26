using Glamping2.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Glamping2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GLAMPINGContext _context;

        public HomeController(ILogger<HomeController> logger, GLAMPINGContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Este método se ejecuta antes de cada acción para asegurar que ViewBag.IsAdmin esté disponible
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;
                var userRole = _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdRol)
                    .FirstOrDefault();

                var role = _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefault();

                ViewBag.IsAdmin = (role != null && role == "Administrador");
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
        }


        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated && ViewBag.IsAdmin == true)
            {
                // Redirigir al Index de Habitaciones si es Administrador
                return RedirectToAction("Index", "Habitaciones");
            }
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userEmail = User.Identity.Name;

                if (userEmail == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var userRole = await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdRol)
                    .FirstOrDefaultAsync();

                if (userRole == 0)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                var role = await _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefaultAsync();

                ViewBag.IsAdmin = (role != null && role == "Administrador");

                if (!ViewBag.IsAdmin)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
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
