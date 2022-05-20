using System;
using System.Collections.Generic;

namespace ConsoleApp.Data.EfCore
{
    public partial class EmployeePerformance
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public int? OrderAmount { get; set; }
    }
}
