using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnectProquifaDotNet.Entities;

[Keyless]
public partial class vETLCotizacionesPendiete
{
    public Guid IdCotCotizacion { get; set; }
}
