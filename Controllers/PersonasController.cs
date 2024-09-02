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
    public class PersonasController : Controller
    {
        private readonly GLAMPINGContext _context;

        public PersonasController(GLAMPINGContext context)
        {
            _context = context;
        }

        // GET: Personas
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

                // Obtener la lista de Personas y pasarla a la vista
                var personas = await _context.Personas.ToListAsync();

                return View(personas);
            }
            catch (Exception ex)
            {
                // Manejar errores y mostrar una vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult BuscarPorDocumento(int documento)
        {
            if (documento <= 0)
            {
                return RedirectToAction("Index");
            }

            var persona = _context.Personas.FirstOrDefault(p => p.DocPersona == documento);

            if (persona != null)
            {
                return RedirectToAction("Details", new { id = persona.IdPersona });
            }
            else
            {
                TempData["Error"] = "No se encontró ninguna persona con ese número de documento.";
                return RedirectToAction("Index");
            }
        }


        // GET: Personas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Personas == null)
            {
                return NotFound();
            }

            try
            {
                // Verificar si el usuario está autenticado
                var userEmail = User.Identity?.Name;

                if (userEmail == null)
                {
                    return RedirectToAction("Login", "Account"); // Redirigir si no está autenticado
                }

                // Obtener el rol del usuario
                var userRole = await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdRol)
                    .FirstOrDefaultAsync();

                if (userRole == 0)
                {
                    return RedirectToAction("AccessDenied", "Account"); // Redirigir si el rol no está definido
                }

                // Verificar el nombre del rol
                var role = await _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefaultAsync();

                ViewBag.IsAdmin = role == "Administrador";

                // Obtener los detalles de la persona
                var persona = await _context.Personas
                    .FirstOrDefaultAsync(m => m.IdPersona == id);

                if (persona == null)
                {
                    return NotFound();
                }

                return View(persona);
            }
            catch (Exception ex)
            {
                // Manejar la excepción y mostrar un mensaje de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }



        // GET: Personas/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPersona,TipoDoc,DocPersona,NomPersona,ApePersona,FechaNaci,Tel,Direcion,Nacionalidad,Ciudad")] Persona persona)
        {
            if (ModelState.IsValid)
            {
                // Configurar la hora a medianoche si solo necesitas la fecha
                persona.FechaNaci = persona.FechaNaci.Date;

                // Calcular la edad
                var today = DateTime.Today;
                var age = today.Year - persona.FechaNaci.Year;
                if (persona.FechaNaci.Date > today.AddYears(-age)) age--;

                // Si deseas almacenar la edad en la base de datos
                persona.Edad = age;

                _context.Add(persona);
                await _context.SaveChangesAsync();

                // Redirigir a la acción Create del controlador Usuarios
                return RedirectToAction("Create", "Usuarios");
            }
            return View(persona);
        }



        // GET: Personas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Personas == null)
            {
                return NotFound();
            }

            var persona = await _context.Personas.FindAsync(id);
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
        }

        // POST: Personas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPersona,TipoDoc,DocPersona,NomPersona,ApePersona,Edad,Tel,FechaNaci,Direcion,Nacionalidad,Ciudad")] Persona persona)
        {
            if (id != persona.IdPersona)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(persona);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonaExists(persona.IdPersona))
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
            return View(persona);
        }

        // GET: Personas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Personas == null)
            {
                return NotFound();
            }

            var persona = await _context.Personas
                .FirstOrDefaultAsync(m => m.IdPersona == id);
            if (persona == null)
            {
                return NotFound();
            }

            return View(persona);
        }

        // POST: Personas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Personas == null)
            {
                return Problem("Entity set 'GLAMPINGContext.Personas'  is null.");
            }
            var persona = await _context.Personas.FindAsync(id);
            if (persona != null)
            {
                _context.Personas.Remove(persona);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteLast()
        {
            var lastPersona = await _context.Personas.OrderByDescending(p => p.IdPersona).FirstOrDefaultAsync();

            if (lastPersona != null)
            {
                _context.Personas.Remove(lastPersona);
                await _context.SaveChangesAsync();
            }

            // Redirigir a la acción de creación de usuarios para que puedan reintentar o ser redirigidos.
            return RedirectToAction("Index", "Home");
        }




        private bool PersonaExists(int id)
        {
          return (_context.Personas?.Any(e => e.IdPersona == id)).GetValueOrDefault();
        }
    }
}
