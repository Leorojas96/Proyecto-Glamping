using System;
using System.Collections.Generic;

namespace Glamping2.Models
{
    public partial class Reserva
    {
        public int IdReservas { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int NroPersonas { get; set; }
        public decimal Total { get; set; } 
        public decimal Abono { get; set; }  
        public string EstadoReserva { get; set; } 
        public int? IdUsuario { get; set; }
        public int? IdPaquetes { get; set; }

        public virtual Paquete? IdPaquetesNavigation { get; set; }
        public virtual Usuario? IdUsuarioNavigation { get; set; }
    }
}
