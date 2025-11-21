using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Application.DTOs
{
    public class Cotizaciones
    {
        public Guid IdCoizacion { get; set; }
        public string Folio { get; set; }
        public decimal Total { get; set; }
    }
}
