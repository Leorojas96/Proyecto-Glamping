using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class PaqueteServicio
    {
        public int IdPaqueteServicio { get; set; }
        public int? IdServicios { get; set; }
        public int? IdPaquetes { get; set; }

        public virtual Paquete? IdPaquetesNavigation { get; set; }
        public virtual Servicio? IdServiciosNavigation { get; set; }
    }
}
