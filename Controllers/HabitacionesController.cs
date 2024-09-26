using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Glamping2.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Glamping2.Controllers
{
    public class HabitacionesController : Controller
    {
        private readonly GLAMPINGContext _context;

        public HabitacionesController(GLAMPINGContext context)
        {
            _context = context;
        }

        // GET: Habitaciones
        public async Task<IActionResult> Index()
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

                ViewBag.IsAdmin = role == "Administrador";

                var habitaciones = await _context.Habitaciones
                    .Include(h => h.IdTipoHabitaNavigation)
                    .ToListAsync();

                return View(habitaciones);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        // GET: Habitaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Habitaciones == null)
            {
                return NotFound();
            }

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

                ViewBag.IsAdmin = role == "Administrador";

                var habitacione = await _context.Habitaciones
                    .Include(h => h.IdTipoHabitaNavigation)
                    .FirstOrDefaultAsync(m => m.IdHabitacion == id);

                if (habitacione == null)
                {
                    return NotFound();
                }

                return View(habitacione);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        // GET: Habitaciones/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var userEmail = User.Identity.Name;

                if (userEmail == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener el rol del usuario actual
                var userRole = await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdRol)
                    .FirstOrDefaultAsync();

                if (userRole == 0)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Obtener el nombre del rol
                var role = await _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefaultAsync();

                // Determina si el usuario es administrador
                ViewBag.IsAdmin = role == "Administrador";

                // Obtener lista de tipos de habitación activos (estado = "Activo")
                var tipoHabitacions = _context.TipoHabitacions
                    .Where(t => t.Estado == "Activo")
                    .ToList();

                // Configurar la lista desplegable para tipos de habitación
                ViewData["IdTipoHabita"] = new SelectList(tipoHabitacions, "IdTipoHabita", "Nombre");

                // Convertir la lista a JSON para pasarla a la vista
                ViewData["TipoHabitacionData"] = tipoHabitacions;

                // Crear un nuevo modelo y establecer el estado predeterminado
                var model = new Habitacione
                {
                    EstadoHabitacion = "Disponible" // Valor predeterminado
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Manejar errores y pasar el mensaje de error a la vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }



        // POST: Habitaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdHabitacion,NroHabitacion,Descripcion,EstadoHabitacion,IdTipoHabita,Comodidades")] Habitacione habitacione)
        {
            if (ModelState.IsValid)
            {
                // El estado de la habitación ya está establecido en el modelo como "Disponible", no es necesario asignarlo aquí.

                // Recoge comodidades desde el formulario
                habitacione.Comodidades = Request.Form["Comodidades"].ToString();

                _context.Add(habitacione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdTipoHabita"] = new SelectList(_context.TipoHabitacions, "IdTipoHabita", "Nombre", habitacione.IdTipoHabita);


            return View(habitacione);
        }



        // GET: Habitaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Habitaciones == null)
            {
                return NotFound();
            }

            var habitacione = await _context.Habitaciones.FindAsync(id);
            if (habitacione == null)
            {
                return NotFound();
            }

            // Definir una lista estática de estados
            var estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "Disponible", Text="Disponible" },
        new SelectListItem { Value = "Inactiva", Text="Inactiva"}
       
    };
            ViewBag.EstadoHabitaciones = estados;

            // Configura ViewBag.IdTipoHabita
            var tipoHabitacions = await _context.TipoHabitacions.ToListAsync();
            ViewBag.IdTipoHabita = new SelectList(tipoHabitacions, "IdTipoHabita", "Nombre");

            return View(habitacione);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,NroHabitacion,Descripcion,EstadoHabitacion,IdTipoHabita,Comodidades")] Habitacione habitacione)
        {
            if (id != habitacione.IdHabitacion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(habitacione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HabitacioneExists(habitacione.IdHabitacion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { message = "Ocurrió un error al intentar editar la habitación." });
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Re-cargar los datos para la vista si el modelo no es válido
            // Re-configura ViewBag.EstadoHabitaciones
            var estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "Disponible", Text="Disponible" },
        new SelectListItem { Value = "Inactiva", Text="Inactiva"}

    };
            ViewBag.EstadoHabitaciones = estados;

            // Re-configura ViewBag.IdTipoHabita
            var tipoHabitacions = await _context.TipoHabitacions.ToListAsync();
            ViewBag.IdTipoHabita = new SelectList(tipoHabitacions, "IdTipoHabita", "Nombre", habitacione.IdTipoHabita);

            return View(habitacione);
        }

        



        // GET: Habitaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Habitaciones == null)
            {
                return NotFound();
            }

            var habitacione = await _context.Habitaciones
                .Include(h => h.IdTipoHabitaNavigation)
                .FirstOrDefaultAsync(m => m.IdHabitacion == id);
            if (habitacione == null)
            {
                return NotFound();
            }

            return View(habitacione);
        }

        // POST: Habitaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) // Cambié el tipo de retorno a Task<ActionResult>
        {
            var habitacion = await _context.Habitaciones.FindAsync(id); // Usar FindAsync para la búsqueda asíncrona

            if (habitacion == null)
            {
                return NotFound();
            }

            var paqueteRelacionado = await _context.Paquetes.AnyAsync(p => p.IdHabitacion == id); // Hacerlo asíncrono

            if (paqueteRelacionado)
            {
                // Cambiar estado de la habitación si está relacionada a un paquete
                habitacion.EstadoHabitacion = "Inactiva";
                _context.Habitaciones.Update(habitacion); // Asegúrate de usar "habitacion" en vez de "habitacione"
                await _context.SaveChangesAsync();

                TempData["Message"] = "La habitación está relacionada con un paquete, por lo que ha sido marcada como 'Inactiva'.";
            }
            else
            {
                // Eliminar la habitación si no está relacionada a ningún paquete
                _context.Habitaciones.Remove(habitacion);
                await _context.SaveChangesAsync(); // Cambia a SaveChangesAsync para ser consistente

                TempData["Message"] = "La habitación ha sido eliminada correctamente.";
            }

            return RedirectToAction("Index");
        }


        // POST: Habitaciones/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            // Cambiar el estado a "Disponible"
            habitacion.EstadoHabitacion = "Disponible";
            _context.Habitaciones.Update(habitacion);
            await _context.SaveChangesAsync();

            TempData["Message"] = "La habitación ha sido activada y está disponible.";

            return RedirectToAction("Index");
        }


        private bool HabitacioneExists(int id)
        {
            return (_context.Habitaciones?.Any(e => e.IdHabitacion == id)).GetValueOrDefault();
        }
    }
}

