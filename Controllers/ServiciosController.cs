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
    public class ServiciosController : Controller
    {
        private readonly GLAMPINGContext _context;

        public ServiciosController(GLAMPINGContext context)
        {
            _context = context;
        }

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

                var servicios = await _context.Servicios
                    .ToListAsync();

                return View(servicios);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        public async Task<IActionResult> ServiciosDisponibles()
        {
            try
            {
                var servicios = await _context.Servicios
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

                // Devuelve la vista "Disponibles" en lugar de "ServiciosDisponibles"
                return View("Disponibles", servicios);
            }
            catch (Exception ex)
            {
                // Manejo de errores: muestra una vista de error con el mensaje de excepción
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }
        // GET: Servicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Servicios == null)
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

                // Obtener el servicio
                var servicio = await _context.Servicios
                    .FirstOrDefaultAsync(m => m.IdServicios == id);

                if (servicio == null)
                {
                    return NotFound();
                }

                return View(servicio);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        // GET: Servicios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servicios/Create
        [HttpPost]
        public async Task<IActionResult> Create(Servicio servicio, IFormFile ImagenFile)
        {
            if (ImagenFile != null && ImagenFile.Length > 0)
            {
                // Ruta para guardar la imagen en wwwroot/imagenes/servicios
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagenes/servicios");

                // Asegurarse de que la carpeta exista, si no, crearla
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Crear la ruta completa del archivo
                var filePath = Path.Combine(folderPath, ImagenFile.FileName);

                // Guardar la imagen en el servidor
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImagenFile.CopyToAsync(stream);
                }

                // Guardar la URL relativa de la imagen en el modelo
                servicio.ImagenUrl = "/imagenes/servicios/" + ImagenFile.FileName;
            }

            _context.Add(servicio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Servicios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            return View(servicio);
        }

        // POST: Servicios/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Servicio servicio)
        {
            _context.Update(servicio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Servicios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Servicios == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios
                .FirstOrDefaultAsync(m => m.IdServicios == id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var servicio = await _context.Servicios
                .Include(s => s.PaqueteServicios)
                .FirstOrDefaultAsync(s => s.IdServicios == id);

            if (servicio == null)
            {
                return NotFound();
            }

            // Primero eliminar las referencias en Paquetes
            var paquetes = await _context.Paquetes
                .Where(p => p.IdServicios == id)
                .ToListAsync();

            // Eliminar los paquetes que hacen referencia al servicio
            _context.Paquetes.RemoveRange(paquetes);

            // Luego eliminar el servicio
            _context.Servicios.Remove(servicio);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Servicios/Search
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                // Redirige al índice si la consulta está vacía
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Obtener todos los servicios desde la base de datos
                var servicios = await _context.Servicios.ToListAsync();

                // Convertir la consulta a minúsculas para comparación insensible a mayúsculas/minúsculas
                var lowerQuery = query.ToLower();

                // Filtrar los servicios en memoria
                var servicio = servicios
                    .FirstOrDefault(s => s.NomServicio != null && s.NomServicio.ToLower().Contains(lowerQuery));

                if (servicio == null)
                {
                    // Redirige al índice o muestra un mensaje si no se encuentra el servicio
                    TempData["SearchMessage"] = "Servicio no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Redirige a la vista de edición con el ID del servicio encontrado
                return RedirectToAction("Details", new { id = servicio.IdServicios });
            }
            catch (Exception ex)
            {
                // Manejar errores y redirigir a una vista de error
                return RedirectToAction("Error", "Home", new { message = $"Ocurrió un error al intentar buscar el servicio: {ex.Message}" });
            }
        }


        private bool ServicioExists(int id)
        {
            return (_context.Servicios?.Any(e => e.IdServicios == id)).GetValueOrDefault();
        }

    }
}
