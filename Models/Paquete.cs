using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class Paquete
    {
        public Paquete()
        {
            PaqueteServicios = new HashSet<PaqueteServicio>();
            Reservas = new HashSet<Reserva>();
        }

        public int IdPaquetes { get; set; }
        public string NomPaquete { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public double Precio { get; set; }
        public int? IdServicios { get; set; }
        public int? IdHabitacion { get; set; }

        public virtual Habitacione? IdHabitacionNavigation { get; set; }
        public virtual Servicio? IdServiciosNavigation { get; set; }
        public virtual ICollection<PaqueteServicio> PaqueteServicios { get; set; }
        public virtual ICollection<Reserva> Reservas { get; set; }
    }
}
