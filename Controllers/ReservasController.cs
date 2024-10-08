﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Glamping2.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Glamping2.Controllers
{
    public class ReservasController : Controller
    {
        private readonly GLAMPINGContext _context;

        public ReservasController(GLAMPINGContext context)
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

                // Obtener todas las reservas sin filtrar por EstadoReserva
                var reservas = await _context.Reservas
                    .Include(r => r.IdPaquetesNavigation)
                    .Include(r => r.IdUsuarioNavigation)
                        .ThenInclude(u => u.IdPersonaNavigation) // Asegúrate de incluir Persona
                    .ToListAsync();

                return View(reservas);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        // GET: Reservas/Create
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create(int paqueteId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Configura ViewBag.IsAdmin para la acción Create
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

                var paquetesActivos = await _context.Paquetes
                    .Where(p => p.Estado == "Activo")
                    .ToListAsync();
                ViewBag.Paquetes = paquetesActivos;

                var usuarioCorreo = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if (usuarioCorreo != null)
                {
                    var usuarioLogueado = await _context.Usuarios
                        .Include(u => u.IdPersonaNavigation)
                        .FirstOrDefaultAsync(u => u.Correo == usuarioCorreo);

                    if (usuarioLogueado != null)
                    {
                        ViewBag.UsuarioNombre = usuarioLogueado.IdPersonaNavigation?.NomPersona;
                        ViewBag.IdUsuario = usuarioLogueado.IdUsuario;
                    }
                }

                var reserva = new Reserva
                {
                    FechaReserva = DateTime.Now.Date,
                    FechaInicio = DateTime.Now.Date.AddDays(1),
                    FechaFin = DateTime.Now.Date.AddDays(2),
                    EstadoReserva = "Pendiente",
                    IdPaquetes = paqueteId
                };

                return View(reserva);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        // Método para buscar usuario por documento
        [HttpGet]
        public async Task<JsonResult> BuscarUsuarioPorDocumento(int docPersona)
        {
            var usuario = await _context.Personas
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.DocPersona == docPersona);

            if (usuario != null)
            {
                var usuarioInfo = usuario.Usuarios.FirstOrDefault();
                if (usuarioInfo != null)
                {
                    return Json(new
                    {
                        idUsuario = usuarioInfo.IdUsuario,
                        nombreCompleto = $"{usuario.NomPersona} {usuario.ApePersona}",
                        correo = usuarioInfo.Correo
                    });
                }
            }
            return Json(null);
        }

        // GET: Reservas/CheckPaqueteAvailability
        [HttpGet]
        public async Task<IActionResult> CheckPaqueteAvailability(int idPaquete, DateTime fechaInicio, DateTime fechaFin)
        {
            var paquete = await _context.Paquetes.FindAsync(idPaquete);

            if (paquete == null)
            {
                return Json(new { disponible = false, mensaje = "Paquete no encontrado." });
            }

            // Verificar si el paquete está reservado para las fechas seleccionadas
            bool paqueteReservado = await _context.Reservas
                .AnyAsync(r => r.IdPaquetes == idPaquete &&
                               ((fechaInicio >= r.FechaInicio && fechaInicio <= r.FechaFin) ||
                                (fechaFin >= r.FechaInicio && fechaFin <= r.FechaFin)) &&
                               (r.EstadoReserva != "Finalizada" && r.EstadoReserva != "Cancelada"));

            // Retorna true si el paquete está disponible, false si está reservado
            return Json(new { disponible = !paqueteReservado });
        }


        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReservas,FechaReserva,FechaInicio,FechaFin,NroPersonas,Abono,EstadoReserva,IdUsuario,IdPaquetes")] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                var paquete = await _context.Paquetes
                    .FirstOrDefaultAsync(p => p.IdPaquetes == reserva.IdPaquetes);

                if (paquete == null)
                {
                    ModelState.AddModelError("", "El paquete seleccionado no es válido.");
                }
                else
                {
                    // Verificar si el paquete está reservado para las fechas seleccionadas
                    bool paqueteReservado = await _context.Reservas
                        .AnyAsync(r => r.IdPaquetes == reserva.IdPaquetes &&
                                       ((reserva.FechaInicio >= r.FechaInicio && reserva.FechaInicio <= r.FechaFin) ||
                                        (reserva.FechaFin >= r.FechaInicio && reserva.FechaFin <= r.FechaFin)) &&
                                       (r.EstadoReserva != "Finalizada" && r.EstadoReserva != "Cancelada"));

                    if (paqueteReservado)
                    {
                        ModelState.AddModelError("IdPaquetes", "El paquete ya está reservado para las fechas seleccionadas.");
                    }
                    else if (reserva.FechaInicio > reserva.FechaFin)
                    {
                        ModelState.AddModelError("", "La fecha de inicio debe ser anterior a la fecha de fin.");
                    }
                    else
                    {
                        var diferenciaDias = (reserva.FechaFin - reserva.FechaInicio).Days;
                        decimal total = Convert.ToDecimal(paquete.Precio);

                        if (diferenciaDias > 2)
                        {
                            total += (diferenciaDias - 2) * 30000;
                        }

                        reserva.Total = total;
                        _context.Add(reserva);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Details", new { id = reserva.IdReservas });
                    }
                }
            }

            ViewBag.Paquetes = await _context.Paquetes.Where(p => p.Estado == "Activo").ToListAsync();
            return View(reserva);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Reservas == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdUsuarioNavigation)
                .ThenInclude(u => u.IdPersonaNavigation)// Asegúrate de incluir la navegación si necesitas el nombre del usuario
                .FirstOrDefaultAsync(r => r.IdReservas == id);

            if (reserva == null)
            {
                return NotFound();
            }

            
            ViewData["IdUsuario"] = reserva.IdUsuario;
            ViewData["NombreUsuario"] = reserva.IdUsuarioNavigation?.IdPersonaNavigation?.NomPersona;

            ViewData["IdPaquetes"] = new SelectList(_context.Paquetes, "IdPaquetes", "NomPaquete", reserva.IdPaquetes);
            return View(reserva);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReservas,FechaReserva,FechaInicio,FechaFin,NroPersonas,Total,Abono,EstadoReserva,IdUsuario,IdPaquetes")] Reserva reserva)
        {
            if (id != reserva.IdReservas)
            {
                return NotFound();
            }

            // Validar que la FechaInicio no sea anterior a la FechaReserva
            if (reserva.FechaInicio < reserva.FechaReserva)
            {
                ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser anterior a la fecha de reserva.");
            }

            // Validar que la FechaFin no sea anterior a la FechaInicio
            if (reserva.FechaFin < reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mantener el IdUsuario que estaba en la base de datos
                    var reservaExistente = await _context.Reservas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.IdReservas == id);

                    if (reservaExistente != null)
                    {
                        reserva.IdUsuario = reservaExistente.IdUsuario; // Mantener el IdUsuario original
                    }

                    // Obtén el paquete seleccionado para calcular el total
                    var paqueteSeleccionado = await _context.Paquetes
                        .FirstOrDefaultAsync(p => p.IdPaquetes == reserva.IdPaquetes);

                    if (paqueteSeleccionado != null)
                    {
                        // Calcula la diferencia de días entre FechaInicio y FechaFin
                        var diferenciaDias = (reserva.FechaFin - reserva.FechaInicio).Days;

                        // Calcula el total basado en la duración de la reserva
                        decimal total = Convert.ToDecimal(paqueteSeleccionado.Precio);

                        if (diferenciaDias > 2)
                        {
                            // Si la diferencia de días es superior a 2, suma los días extras multiplicados por 30,000 al total
                            int diasExtras = diferenciaDias - 2;
                            total += diasExtras * 30000;
                        }

                        // Asigna el total calculado a la propiedad Total
                        reserva.Total = total;
                    }

                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.IdReservas))
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

            ViewData["IdPaquetes"] = new SelectList(_context.Paquetes, "IdPaquetes", "NomPaquete", reserva.IdPaquetes);
            return View(reserva);
        }

        

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Reservas == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdPaquetesNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdReservas == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Reservas == null)
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

                var reserva = await _context.Reservas
                    .Include(r => r.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdPersonaNavigation) // Incluye la propiedad de navegación de Usuario
                    .Include(r => r.IdPaquetesNavigation) // Incluye la propiedad de navegación de Paquete
                    .FirstOrDefaultAsync(m => m.IdReservas == id);

                if (reserva == null)
                {
                    return NotFound();
                }

                // Verifica si el usuario tiene permiso para ver los detalles
                if (!ViewBag.IsAdmin && reserva.IdUsuario != (await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => u.IdUsuario)
                    .FirstOrDefaultAsync()))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View(reserva);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }


        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index");
            }

            // Encuentra el usuario que coincida con el nombre o identificador de persona
            var usuario = await _context.Usuarios
                .Include(u => u.IdPersonaNavigation)
                .FirstOrDefaultAsync(u =>
                    u.IdPersonaNavigation.NomPersona.Contains(searchTerm) ||
                    u.IdPersonaNavigation.DocPersona.ToString().Contains(searchTerm));

            if (usuario == null)
            {
                // Manejo si no se encuentra ningún usuario
                return RedirectToAction("Index");
            }

            // Encuentra la reserva más reciente del usuario con estado activo o pendiente
            var reserva = await _context.Reservas
                .Where(r => r.IdUsuario == usuario.IdUsuario && (r.EstadoReserva == "Activo" || r.EstadoReserva == "Pendiente"))
                .OrderByDescending(r => r.FechaReserva)
                .FirstOrDefaultAsync();

            if (reserva == null)
            {
                // Manejo si no se encuentra ninguna reserva activa o pendiente
                return RedirectToAction("Index");
            }

            // Redirige a la vista de detalles de la reserva encontrada
            return RedirectToAction("Details", new { id = reserva.IdReservas });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            // Cambiar el estado de la reserva a "Cancelada"
            reserva.EstadoReserva = "Cancelada";
            _context.Update(reserva);
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            TempData["Mensaje"] = "La reserva ha sido cancelada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel2(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            // Cambiar el estado de la reserva a "Cancelada"
            reserva.EstadoReserva = "Cancelada";
            _context.Update(reserva);
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            TempData["Mensaje"] = "La reserva ha sido cancelada exitosamente.";
            return RedirectToAction("Perfil", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            // Verificar que el estado sea "Pendiente" y la fecha de inicio coincida con la fecha actual
            if (reserva.EstadoReserva == "Pendiente")
            {
                if (reserva.FechaInicio.Date == DateTime.Now.Date)
                {
                    reserva.EstadoReserva = "Confirmada";
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();

                    // Mensaje de éxito
                    TempData["Mensaje"] = $"El estado de la reserva ha sido cambiado a {reserva.EstadoReserva}.";
                }
                else
                {
                    // Si la fecha no coincide, mostrar un mensaje de error
                    TempData["Mensaje"] = "La reserva solo puede confirmarse el día de inicio de la reserva.";
                }
            }
            else if (reserva.EstadoReserva == "Confirmada")
            {
                reserva.EstadoReserva = "Finalizada";
                _context.Update(reserva);
                await _context.SaveChangesAsync();

                // Mensaje de éxito
                TempData["Mensaje"] = $"El estado de la reserva ha sido cambiado a {reserva.EstadoReserva}.";
            }

            // Asegurar que los mensajes se conserven
            TempData.Keep();

            // Redirigir para mostrar el mensaje
            return RedirectToAction("Index");
        }



        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReservas == id);
        }
    }
}
