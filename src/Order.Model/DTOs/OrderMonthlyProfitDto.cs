using System;
using System.Collections.Generic;

namespace Order.Data.Entities
{
    public class OrderMonthlyProfitDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Profit { get; set; }
    }
}
