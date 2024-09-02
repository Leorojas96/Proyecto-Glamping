using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class Permiso
    {
        public Permiso()
        {
            RolesPermisos = new HashSet<RolesPermiso>();
        }

        public int IdPermisos { get; set; }
        public string NomPermiso { get; set; } = null!;
        public string EstadoPermiso { get; set; } = null!;

        public virtual ICollection<RolesPermiso> RolesPermisos { get; set; }
    }
}
