USE [PConnectProquifaDotNet]
GO
CREATE OR ALTER VIEW vETLCotizacionesPendietes
AS
WITH UltimoRegistro AS (
    SELECT 
        s.IdentificadorRegistro,
        s.Estado,
        s.FechaRegistro,
        ROW_NUMBER() OVER (
            PARTITION BY s.IdentificadorRegistro 
            ORDER BY s.FechaRegistro DESC
        ) AS rn
    FROM PConnectProquifaDotNet.dbo.SyncJobLog s
),
FallosPersistentes AS (
    SELECT IdentificadorRegistro
    FROM UltimoRegistro
    WHERE rn = 1
      AND Estado = 'Fallo persistente'
)
SELECT 
    cc.IdCotCotizacion
FROM ProquifaDotNet.dbo.vcotCotizacion cc
INNER JOIN ProquifaDotNet.dbo.catEstadoCotizacion AS Estado 
    ON Estado.IdCatEstadoCotizacion = cc.IdCatEstadoCotizacion 
   AND Estado.Clave IN ('finalizada','enviada')
INNER JOIN ProquifaDotNet.dbo.Empresa e 
    ON e.IdEmpresa = cc.IdEmpresa 
   AND ISNULL(e.FacturaServicios,0) = 0
LEFT JOIN PConnectProquifaDotNet.dbo.Cotizaciones AS Cotizaciones 
    ON Cotizaciones.CotizacionPQF = cc.IdCotCotizacion
LEFT JOIN FallosPersistentes fp 
    ON fp.IdentificadorRegistro = cc.IdCotCotizacion
WHERE cc.Folio <> '' 
  AND cc.Folio IS NOT NULL
  AND cc.TotalProductos > 0
  AND cc.Activo = 1
  AND Cotizaciones.CotizacionPQF IS NULL
  AND fp.IdentificadorRegistro IS NULL;  -- << excluir los últimos fallos persistentes
  GO
