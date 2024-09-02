using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLAMPING.Models;
using Glamping2.Models;

namespace Glamping2.Controllers
{
    public class AccountController : Controller
    {
        private readonly GLAMPINGContext _context;

        // Constructor: Inyecta el contexto de base de datos
        public AccountController(GLAMPINGContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                // Busca el usuario en la base de datos
                var user = await _context.Usuarios
                    .Where(u => u.Correo == model.Correo && u.Contraseña == model.Contraseña)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Crea las claims para el usuario autenticado
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Correo)
                        // Agrega más claims si es necesario
                    };

                    // Crea la identidad con las claims y especifica el esquema de autenticación
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        // Puedes agregar propiedades de autenticación si es necesario
                    };

                    // Inicia la sesión del usuario
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Redirige a la página principal después de iniciar sesión
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Si las credenciales no son válidas, muestra un mensaje de error
                    ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                }
            }

            // Si el modelo no es válido, vuelve a mostrar la vista de login
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Cierra la sesión del usuario
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige a la página principal después de cerrar sesión
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
