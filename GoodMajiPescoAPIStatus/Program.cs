using goodmaji;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodMajiPescoAPIStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new PrescoService();
            var shipstatusRequest = new List<ShipStatusRequest>(); //service.GetShipStatusRequests();
            shipstatusRequest.Add(new ShipStatusRequest { ShipNo = "GMJI7110052847001" });
            var updateresult = service.UpdateShipmentStatus(shipstatusRequest);

            Console.WriteLine(updateresult);
            Console.ReadLine();
        }
    }
}
