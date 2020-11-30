﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace goodmaji
{
    public class Shipment
    {
        public string ST69 { get; set; }
        public int ST12 { get; set; }
    }

    public enum GoodMajiStatus
    {
        ReturnedToHub = -4,
        DeliveryCompleted = 6,
        ArrivedAtDistributionPoint = 7
    }
}
