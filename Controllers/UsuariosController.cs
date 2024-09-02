using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Glamping2.Models;

namespace Glamping2.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly GLAMPINGContext _context;

        public UsuariosController(GLAMPINGContext context)
        {
            _context = context;
        }

        // GET: Usuarios
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

                // Redirigir a la página de acceso denegado si el rol no está definido o es 0
                if (userRole == 0)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Obtener el nombre del rol basado en el IdRol del usuario
                var role = await _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefaultAsync();

                // Determinar si el usuario es administrador
                ViewBag.IsAdmin = role == "Administrador";

                // Obtener la lista de Usuarios y pasarla a la vista
                var usuarios = await _context.Usuarios
                    .Include(u => u.IdPersonaNavigation)
                    .Include(u => u.IdRolNavigation)
                    .ToListAsync();

                return View(usuarios);
            }
            catch (Exception ex)
            {
                // Manejar errores y mostrar una vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdPersonaNavigation)
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            // Verifica si el usuario es administrador
            bool isAdmin = User.IsInRole("Admin"); // Asegúrate de que el rol "Admin" existe en tu sistema

            ViewBag.IsAdmin = isAdmin;
            ViewBag.IdPersona = new SelectList(_context.Personas, "IdPersona", "NomPersona");
            ViewBag.IdRol = new SelectList(_context.Roles, "IdRol", "NomRol");

            return View();
        }


        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuario,Correo,Contraseña,IdPersona,IdRol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Si el usuario no es administrador, establecer valores predeterminados
                bool isAdmin = User.IsInRole("Admin");
                if (!isAdmin)
                {
                    // Obtener la última persona creada
                    var lastPersona = await _context.Personas.OrderByDescending(p => p.IdPersona).FirstOrDefaultAsync();
                    usuario.IdPersona = lastPersona?.IdPersona;

                    // Establecer el IdRol a 2 para clientes
                    usuario.IdRol = 2;
                }

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            // Si hay un error en el modelo, vuelve a mostrar la vista Create con datos de selección
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IdPersona = new SelectList(_context.Personas, "IdPersona", "NomPersona", usuario.IdPersona);
            ViewBag.IdRol = new SelectList(_context.Roles, "IdRol", "NomRol", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuarios/Buscar
        public IActionResult Buscar(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                // Redirige al índice o muestra un mensaje de error si no se proporciona correo
                return RedirectToAction(nameof(Index));
            }

            var usuario = _context.Usuarios
                                  .Include(u => u.IdPersonaNavigation)
                                  .Include(u => u.IdRolNavigation)
                                  .FirstOrDefault(u => u.Correo == correo);

            if (usuario == null)
            {
                // Redirige a una vista que indique que no se encontró el usuario, o muestra un mensaje de error
                return NotFound(); // O puedes usar RedirectToAction(nameof(Index)) y mostrar un mensaje
            }

            // Redirige a la acción Edit con el Id del usuario
            return RedirectToAction(nameof(Edit), new { id = usuario.IdUsuario });
        }


        // GET: Usuarios/Edit/5
        public IActionResult Edit(int id)
        {
            var usuario = _context.Usuarios
                                  .Include(u => u.IdPersonaNavigation)
                                  .Include(u => u.IdRolNavigation)
                                  .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            ViewBag.IdRol = new SelectList(
                _context.Roles,
                "IdRol",
                "NomRol",
                usuario.IdRol);

            ViewBag.NomPersona = usuario.IdPersonaNavigation?.NomPersona;
            ViewBag.NomRol = usuario.IdRolNavigation?.NomRol;

            return View(usuario);
        }



        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Correo,Contraseña,IdPersona,IdRol")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtén el usuario existente para mantener el IdPersona original
                    var usuarioExistente = await _context.Usuarios
                                                         .AsNoTracking()
                                                         .FirstOrDefaultAsync(u => u.IdUsuario == id);

                    if (usuarioExistente != null)
                    {
                        usuario.IdPersona = usuarioExistente.IdPersona; // Mantener el IdPersona original
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Repopular los select lists si el modelo no es válido
            ViewData["IdPersona"] = new SelectList(_context.Personas, "IdPersona", "NomPersona", usuario.IdPersona);
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NomRol", usuario.IdRol);
            return View(usuario);
        }



        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdPersonaNavigation)
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Usuarios == null)
            {
                return Problem("Entity set 'GLAMPINGContext.Usuarios'  is null.");
            }
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
          return (_context.Usuarios?.Any(e => e.IdUsuario == id)).GetValueOrDefault();
        }
    }
}
