using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawa.OrderService.Api.Database.Models
{
    public class OrderLine
    {
        public string OrderId { get; set; } = default!;
        public string ProductId { get; set; } = default!;
        public int Quantite { get; set; }

        public Order Order { get; set; } = default!;
    }
}
