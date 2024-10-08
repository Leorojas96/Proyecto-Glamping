﻿using System.Collections.Generic;
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
                var user = await _context.Usuarios
                    .Where(u => u.Correo == model.Correo && u.Contraseña == model.Contraseña)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Correo)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);
                    if (user.IdRol == 1)
                    {
                        return RedirectToAction("Index", "Habitaciones");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
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


        public IActionResult Perfil()
        {
            
            var correoUsuario = User.Identity.Name; 

            if (correoUsuario == null)
            {
                return RedirectToAction("Login", "Account"); 
            }

           
            var usuario = _context.Usuarios
                .Where(u => u.Correo == correoUsuario)
                .FirstOrDefault();

            if (usuario == null)
            {
                return NotFound(); 
            }
            var esAdmin = usuario.IdRol == 1;
            ViewBag.IsAdmin = esAdmin;

            var persona = _context.Personas
                .Where(p => p.IdPersona == usuario.IdPersona)
                .FirstOrDefault();

            var reservas = _context.Reservas
                .Where(r => r.IdUsuario == usuario.IdUsuario)
                .OrderByDescending(r => r.FechaReserva) 
                .Take(5) 
                .ToList();

            
            ViewBag.Usuario = usuario;
            ViewBag.Persona = persona;
            ViewBag.Reservas = reservas;

            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
