using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace goodmaji
{
   public class ShipStatusRequest
    {
        public string ShipNo { get; set; }
    }

    public class PrescoShipment {
        public string  GMShipID { get; set; }
        public string  PrescoShipID { get; set; }

        public string ParcelNo { get; set; }
    }
}
