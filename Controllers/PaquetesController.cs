using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Glamping2.Models;

namespace Glamping2.Controllers
{
    public class PaquetesController : Controller
    {
        private readonly GLAMPINGContext _context;

        public PaquetesController(GLAMPINGContext context)
        {
            _context = context;
        }

        // GET: Paquetes
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

                var paquetes = await _context.Paquetes
                    .Include(p => p.IdHabitacionNavigation)
                    .Include(p => p.IdServiciosNavigation)
                    .ToListAsync();

                return View(paquetes);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        public async Task<IActionResult> PaquetesDisponibles()
        {
            try
            {
                var paquetes = await _context.Paquetes
                    .Include(p => p.IdHabitacionNavigation)
                    .Include(p => p.IdServiciosNavigation)
                    .Where(p => p.Estado == "Activo")
                    .ToListAsync();

                // Configura ViewBag.IsAdmin en esta acción también
                var userEmail = User.Identity.Name;
                if (userEmail != null)
                {
                    var userRole = await _context.Usuarios
                        .Where(u => u.Correo == userEmail)
                        .Select(u => u.IdRol)
                        .FirstOrDefaultAsync();

                    var role = await _context.Roles
                        .Where(r => r.IdRol == userRole)
                        .Select(r => r.NomRol)
                        .FirstOrDefaultAsync();

                    ViewBag.IsAdmin = role == "Administrador";
                }
                else
                {
                    ViewBag.IsAdmin = false;
                }

                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;

                return View(paquetes);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


            // GET: Paquetes/Details/5
            public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Paquetes == null)
            {
                return NotFound();
            }

            try
            {
                // Verificar si el usuario está autenticado
                var userEmail = User.Identity.Name;
                if (userEmail == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener el rol del usuario
                var userRole = await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdRol)
                    .FirstOrDefaultAsync();

                // Verificar si el usuario tiene un rol válido
                if (userRole == 0)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Obtener el nombre del rol
                var role = await _context.Roles
                    .Where(r => r.IdRol == userRole)
                    .Select(r => r.NomRol)
                    .FirstOrDefaultAsync();

                // Determinar si el usuario es administrador
                ViewBag.IsAdmin = role == "Administrador";

                // Obtener el paquete
                var paquete = await _context.Paquetes
                    .Include(p => p.IdHabitacionNavigation)
                    .Include(p => p.IdServiciosNavigation)
                    .FirstOrDefaultAsync(m => m.IdPaquetes == id);

                if (paquete == null)
                {
                    return NotFound();
                }

                return View(paquete);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }




        // GET: Paquetes/Create
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

                // Obtener los servicios
                var servicios = _context.Servicios.ToList();

                // Obtener habitaciones activas (disponibles)
                var habitacionesActivas = _context.Habitaciones
                    .Where(h => h.EstadoHabitacion == "Disponible")
                    .Select(h => new { h.IdHabitacion, h.NroHabitacion }) // Solo selecciona los campos necesarios
                    .ToList();

                // Asignar la lista de habitaciones activas y servicios a ViewBag
                ViewBag.IdHabitacion = new SelectList(habitacionesActivas, "IdHabitacion", "NroHabitacion");
                ViewBag.IdServicios = new SelectList(servicios, "IdServicios", "NomServicio");

                return View();
            }
            catch (Exception ex)
            {
                // Manejar errores y pasar el mensaje de error a la vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        // POST: Paquetes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPaquetes,NomPaquete,Descripcion,Estado,IdServicios,IdHabitacion")] Paquete paquete)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el servicio seleccionado
                    var servicio = await _context.Servicios.FindAsync(paquete.IdServicios);
                    if (servicio == null)
                    {
                        ModelState.AddModelError("", "El servicio seleccionado no existe.");
                        throw new Exception("El servicio seleccionado no existe.");
                    }

                    // Obtener la habitación seleccionada
                    var habitacion = await _context.Habitaciones
                        .Include(h => h.IdTipoHabitaNavigation) // Incluyendo la navegación a TipoHabitacion
                        .FirstOrDefaultAsync(h => h.IdHabitacion == paquete.IdHabitacion);
                    if (habitacion == null)
                    {
                        ModelState.AddModelError("", "La habitación seleccionada no existe.");
                        throw new Exception("La habitación seleccionada no existe.");
                    }

                    // Obtener el tipo de habitación
                    var tipoHabitacion = habitacion.IdTipoHabitaNavigation;
                    if (tipoHabitacion == null)
                    {
                        ModelState.AddModelError("", "El tipo de habitación seleccionado no existe.");
                        throw new Exception("El tipo de habitación seleccionado no existe.");
                    }

                    // Calcular el precio del paquete sumando el precio del servicio y del tipo de habitación
                    paquete.Precio = servicio.Precio + tipoHabitacion.Precio;

                    // Guardar el paquete con el precio calculado
                    _context.Add(paquete);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al guardar los datos: {ex.Message}");
                }
            }

            // Si algo falla, recargar las listas de selección
            var servicios = _context.Servicios.ToList();
            var habitaciones = _context.Habitaciones.ToList();
            ViewBag.IdServicios = new SelectList(servicios, "IdServicios", "NomServicio", paquete.IdServicios);
            ViewBag.IdHabitacion = new SelectList(habitaciones, "IdHabitacion", "NroHabitacion", paquete.IdHabitacion);

            return View(paquete);
        }






        // GET: Paquetes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Paquetes == null)
            {
                return NotFound();
            }

            var paquete = await _context.Paquetes.FindAsync(id);
            if (paquete == null)
            {
                return NotFound();
            }

            ViewBag.IdServicios = new SelectList(_context.Servicios, "IdServicios", "NomServicio", paquete.IdServicios);
            ViewBag.IdHabitacion = new SelectList(_context.Habitaciones, "IdHabitacion", "NroHabitacion", paquete.IdHabitacion);

            return View(paquete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPaquetes,NomPaquete,Descripcion,Estado,Precio,IdServicios,IdHabitacion,ImagenUrl")] Paquete paquete)
        {
            if (id != paquete.IdPaquetes)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paquete);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaqueteExists(paquete.IdPaquetes))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.IdServicios = new SelectList(_context.Servicios, "IdServicios", "NomServicio", paquete.IdServicios);
            ViewBag.IdHabitacion = new SelectList(_context.Habitaciones, "IdHabitacion", "NroHabitacion", paquete.IdHabitacion);

            return View(paquete);
        }


        // GET: Paquetes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Paquetes == null)
            {
                return NotFound();
            }

            var paquete = await _context.Paquetes
                .Include(p => p.IdHabitacionNavigation)
                .Include(p => p.IdServiciosNavigation)
                .FirstOrDefaultAsync(m => m.IdPaquetes == id);

            if (paquete == null)
            {
                return NotFound();
            }

            return View(paquete);
        }

        // POST: Paquetes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Paquetes == null)
            {
                return Problem("Entity set 'GLAMPINGContext.Paquetes'  is null.");
            }

            var paquete = await _context.Paquetes.FindAsync(id);
            if (paquete != null)
            {
                _context.Paquetes.Remove(paquete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchByName(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return RedirectToAction(nameof(Index));
            }

            var paquetes = await _context.Paquetes.ToListAsync();
            var paquete = paquetes.FirstOrDefault(p => p.NomPaquete.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (paquete != null)
            {
                return RedirectToAction("Details", new { id = paquete.IdPaquetes });
            }

            TempData["ErrorMessage"] = "No se encontró ningún paquete con ese nombre.";
            return RedirectToAction(nameof(Index));
        }


        private bool PaqueteExists(int id)
        {
            return _context.Paquetes.Any(e => e.IdPaquetes == id);
        }
    }
}
