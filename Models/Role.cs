using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Glamping2.Models
{
    public partial class Role
    {
        public Role()
        {
            RolesPermisos = new HashSet<RolesPermiso>();
            Usuarios = new HashSet<Usuario>();
        }

        public int IdRol { get; set; }
        public string NomRol { get; set; } = null!;

        [Required(ErrorMessage = "El campo Estado es obligatorio.")] // Agrega esta línea
        public string Estado { get; set; }

        // Permisos para los módulos
        public bool PermisoDashboard { get; set; } = false;
        public bool PermisoRoles { get; set; } = false;
        public bool PermisoUsuarios { get; set; } = false;
        public bool PermisoHabitaciones { get; set; } = false;
        public bool PermisoServicios { get; set; } = false;
        public bool PermisoPaquetes { get; set; } = false;
        public bool PermisoClientes { get; set; } = false;
        public bool PermisoReservas { get; set; } = false;

        public virtual ICollection<RolesPermiso> RolesPermisos { get; set; }
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
