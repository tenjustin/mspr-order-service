using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawa.OrderService.Api.Database.Models
{
    public class Order
    {
        public string Id { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public DateTime DateCommande { get; set; }
        public ICollection<OrderLine> Lignes { get; set; } = new List<OrderLine>();
    }
}
