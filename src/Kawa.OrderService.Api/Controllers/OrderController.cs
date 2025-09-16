using Kawa.OrderService.Api.Database;
using Kawa.OrderService.Api.Database.Models;
using Kawa.OrderService.Api.Models;
using Kawa.OrderService.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kawa.OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly CommandesDbContext _context;
        private readonly IMessageBrokerService _messageBrokerService;

        public OrderController(CommandesDbContext context, IMessageBrokerService messageBrokerService)
        {
            _context = context;
            _messageBrokerService = messageBrokerService;
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.Orders.Include(x => x.Lignes).ToList();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(string id)
        {
            var order = _context.Orders.Include(x => x.Lignes).FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            if (order == null || order.Lignes == null || !order.Lignes.Any())
            {
                return BadRequest("Order or order lines cannot be null or empty.");
            }

            order.Id = Guid.NewGuid().ToString();
            order.DateCommande = DateTime.UtcNow;

            _context.Orders.Add(order);
            _context.SaveChanges();

            var message = new OrderMessage
            {
                Action = "OrderCreated",
                Content = $"Order {order.Id} created for client {order.ClientId} with {order.Lignes.Count} lines."
            };

            // Publish event to message broker
            _messageBrokerService.PublishMessage("kawa-exchange", "kawa-order", message);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(string id)
        {
            var order = _context.Orders.Include(x => x.Lignes).FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            var message = new OrderMessage
            {
                Action = "OrderDeleted",
                Content = $"Order {order.Id} deleted."
            };

            // Publish event to message broker
            _messageBrokerService.PublishMessage("kawa-exchange", "kawa-order", message);

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrder(string id, [FromBody] Order updatedOrder)
        {
            if (updatedOrder == null || updatedOrder.Lignes == null || !updatedOrder.Lignes.Any())
            {
                return BadRequest("Updated order or order lines cannot be null or empty.");
            }

            var existingOrder = _context.Orders.Include(x => x.Lignes).FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            existingOrder.ClientId = updatedOrder.ClientId;
            existingOrder.Lignes = updatedOrder.Lignes;
            existingOrder.DateCommande = DateTime.UtcNow;

            _context.SaveChanges();

            var message = new OrderMessage
            {
                Action = "OrderUpdated",
                Content = $"Order {existingOrder.Id} updated for client {existingOrder.ClientId} with {existingOrder.Lignes.Count} lines."
            };

            // Publish event to message broker
            _messageBrokerService.PublishMessage("kawa-exchange", "kawa-order", message);

            return NoContent();
        }
    }
}
