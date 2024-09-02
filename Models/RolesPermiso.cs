using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class RolesPermiso
    {
        public int IdRolesPermisos { get; set; }
        public string Estado { get; set; } = null!;
        public int? IdPermisos { get; set; }
        public int? IdRol { get; set; }

        public virtual Permiso? IdPermisosNavigation { get; set; }
        public virtual Role? IdRolNavigation { get; set; }
    }
}
