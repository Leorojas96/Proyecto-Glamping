using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class Servicio
    {
        public Servicio()
        {
            PaqueteServicios = new HashSet<PaqueteServicio>(); // Relación muchos a muchos
        }

        public int IdServicios { get; set; }
        public string NomServicio { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string TipoServicio { get; set; } = null!;
        public double Precio { get; set; }

        // Colección para la relación muchos a muchos a través de PaqueteServicios
        public virtual ICollection<PaqueteServicio> PaqueteServicios { get; set; }
        public virtual ICollection<Paquete> Paquetes { get; set; }
    }
}
