﻿using System;
using System.Collections.Generic;

namespace ConsoleApp.Data.EfCore
{
    public partial class PurchaseOrderStatus
    {
        public PurchaseOrderStatus()
        {
            PurchaseOrders = new HashSet<PurchaseOrders>();
        }

        public int Id { get; set; }
        public string Status { get; set; }

        public virtual ICollection<PurchaseOrders> PurchaseOrders { get; set; }
    }
}
