using System;
using System.Collections.Generic;
using System.Text;

namespace SincronizadorPqfLegacy.Application.DTOs
{
    public class CotizacionesDto
    {
        public Guid IdCoizacion { get; set; }
        public string Folio { get; set; }
        public decimal Total { get; set; }
    }
}
