using System.Collections.Generic;

namespace Glamping2.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Reserva> Reservas { get; set; }
        public decimal TotalGeneral { get; set; }
    }
}
