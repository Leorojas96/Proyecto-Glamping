using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class Habitacione
    {
        public Habitacione()
        {
            Paquetes = new HashSet<Paquete>();
        }

        public int IdHabitacion { get; set; }
        public int NroHabitacion { get; set; }
        public string Descripcion { get; set; } = null!;
        public string EstadoHabitacion { get; set; } = null!;
        public int? IdTipoHabita { get; set; }
        public string? Comodidades { get; set; }

        public virtual TipoHabitacion? IdTipoHabitaNavigation { get; set; }
        public virtual ICollection<Paquete> Paquetes { get; set; }
    }
}
