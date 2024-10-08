﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Glamping2.Models;

namespace Glamping2.Controllers
{
    public class TipoHabitacionsController : Controller
    {
        private readonly GLAMPINGContext _context;

        public TipoHabitacionsController(GLAMPINGContext context)
        {
            _context = context;
        }
        // GET: TipoHabitacions
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

                // Obtener la lista de TipoHabitacions y pasarla a la vista
                var tipoHabitacions = await _context.TipoHabitacions.ToListAsync();

                return View(tipoHabitacions);
            }
            catch (Exception ex)
            {
                // Manejar errores y mostrar una vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                // Redirige al índice si la consulta está vacía
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Obtener todas las habitaciones desde la base de datos
                var habitaciones = await _context.TipoHabitacions.ToListAsync();

                // Convertir la consulta a minúsculas para comparación insensible a mayúsculas/minúsculas
                var lowerQuery = query.ToLower();

                // Filtrar las habitaciones en memoria
                var tipoHabitacion = habitaciones
                    .FirstOrDefault(h => h.Nombre != null && h.Nombre.ToLower().Contains(lowerQuery));

                if (tipoHabitacion == null)
                {
                    // Redirige al índice o muestra un mensaje si no se encuentra la habitación
                    TempData["SearchMessage"] = "Tipo de habitación no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Redirige a la vista de edición con el ID del tipo de habitación encontrado
                return RedirectToAction("Edit", new { id = tipoHabitacion.IdTipoHabita });
            }
            catch (Exception ex)
            {
                // Manejar errores y redirigir a una vista de error
                return RedirectToAction("Error", "Home", new { message = $"Ocurrió un error al intentar buscar el tipo de habitación: {ex.Message}" });
            }
        }

        // GET: TipoHabitacions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TipoHabitacions == null)
            {
                return NotFound();
            }

            var tipoHabitacion = await _context.TipoHabitacions
                .FirstOrDefaultAsync(m => m.IdTipoHabita == id);
            if (tipoHabitacion == null)
            {
                return NotFound();
            }

            return View(tipoHabitacion);
        }

        // GET: TipoHabitacions/Create
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

                // Retorna la vista de creación
                return View();
            }
            catch (Exception ex)
            {
                // Manejar errores y pasar el mensaje de error a la vista de error
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        // POST: TipoHabitacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTipoHabita,Nombre,Precio,Estado,NroPersonsa")] TipoHabitacion tipoHabitacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoHabitacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoHabitacion);
        }

        // GET: TipoHabitacions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TipoHabitacions == null)
            {
                return NotFound();
            }

            var tipoHabitacion = await _context.TipoHabitacions.FindAsync(id);
            if (tipoHabitacion == null)
            {
                return NotFound();
            }
            return View(tipoHabitacion);
        }

        // POST: TipoHabitacions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTipoHabita,Nombre,Precio,Estado,NroPersonsa")] TipoHabitacion tipoHabitacion)
        {
            if (id != tipoHabitacion.IdTipoHabita)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoHabitacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoHabitacionExists(tipoHabitacion.IdTipoHabita))
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
            return View(tipoHabitacion);
        }


        

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoHabitacion = await _context.TipoHabitacions.FindAsync(id);
            if (tipoHabitacion != null)
            {
                tipoHabitacion.Estado = "Inactivo"; // Cambiar el estado a Inactivo
                _context.Update(tipoHabitacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var tipoHabitacion = await _context.TipoHabitacions.FindAsync(id);

            if (tipoHabitacion == null)
            {
                return NotFound();
            }

            tipoHabitacion.Estado = "Activo"; // Cambiar el estado a Activo
            _context.Update(tipoHabitacion);
            await _context.SaveChangesAsync();

            TempData["Message"] = "El tipo de habitación ha sido activado.";
            return RedirectToAction(nameof(Index));
        }

        private bool TipoHabitacionExists(int id)
        {
          return (_context.TipoHabitacions?.Any(e => e.IdTipoHabita == id)).GetValueOrDefault();
        }
    }
}
