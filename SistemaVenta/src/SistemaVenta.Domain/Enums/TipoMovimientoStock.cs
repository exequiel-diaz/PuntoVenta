using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Domain.Enums
{
    public enum TipoMovimientoStock
    {
        IngresoMercaderia,
        Venta,
        AjusteManual,
        CorreccionInventario
        //no agrego lo de transferencia, eso lo dejo por si queremos hacer para multiplesdepositos
    }
}
