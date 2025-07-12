using System;
using System.Collections.Generic;

namespace Order.Data.Entities
{
    public class OrderCreateDto
    {

        public Guid ResellerId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StatusId { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
