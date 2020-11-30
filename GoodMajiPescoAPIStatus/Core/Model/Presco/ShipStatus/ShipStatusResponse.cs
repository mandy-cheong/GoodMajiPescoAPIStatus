using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace goodmaji
{
    public class ShipStatusResponse
    {
        public ShipStatusResponse()
        {
            ShipStatuses = new List<ShipStatus>();
        }
        public List<ShipStatus> ShipStatuses { get; set; }
    }

    public class ShipStatus
    {
        public ShipStatus()
        {
            Tracking = new List<Tracking>();
        }
        public string ShipNo { get; set; }
        public List<Tracking> Tracking { get; set; }
    }
    public class Tracking {
        public DateTime Date { get; set; }
        public string  Status { get; set; }
        public string Description { get; set; }
    }

    public class PrescoShipStatus {
        public const string ArrivedAtDistributionPoint = "已到門市 Arrived at Distribution Point";
        public const string DeliveryCompleted = "取件完成 Delivery Complete";
        public const string RTS = "退件包裹 RTS";
    }

}
