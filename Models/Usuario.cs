using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            Reservas = new HashSet<Reserva>();
        }

        public int IdUsuario { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public int? IdPersona { get; set; }
        public int? IdRol { get; set; }

        public virtual Persona? IdPersonaNavigation { get; set; }
        public virtual Role? IdRolNavigation { get; set; }
        public virtual ICollection<Reserva> Reservas { get; set; }
    }
}
