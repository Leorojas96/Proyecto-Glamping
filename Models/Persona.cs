using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Glamping2.Models
{
    public partial class Persona
    {
        public Persona()
        {
            Usuarios = new HashSet<Usuario>();
        }

        public int IdPersona { get; set; }
        public string TipoDoc { get; set; } = null!;
        public int DocPersona { get; set; }
        public string NomPersona { get; set; } = null!;
        public string ApePersona { get; set; } = null!;
        public int Edad { get; set; }
        public int Tel { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNaci { get; set; }
        public string Direcion { get; set; } = null!;
        public string Nacionalidad { get; set; } = null!;
        public string Ciudad { get; set; } = null!;

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
