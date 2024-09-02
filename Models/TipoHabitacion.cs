using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class TipoHabitacion
    {
        public TipoHabitacion()
        {
            Habitaciones = new HashSet<Habitacione>();
        }

        public int IdTipoHabita { get; set; }
        public string Nombre { get; set; } = null!;
        public int Precio { get; set; }
        public string? Estado { get; set; }
        public int NroPersonsa { get; set; }

        public virtual ICollection<Habitacione> Habitaciones { get; set; }
    }
}
