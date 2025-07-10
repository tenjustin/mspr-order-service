using Kawa.OrderService.Api.Models;
using Kawa.OrderService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kawa.OrderService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageBrokerService _messageBroker;
    private readonly ILogger<MessageController> _logger;

    public MessageController(IMessageBrokerService messageBroker, ILogger<MessageController> logger)
    {
        _messageBroker = messageBroker;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult SendMessage([FromBody] OrderMessage message)
    {
        try
        {
            // Configuration d'échange et de clé de routage
            const string exchange = "client-service";
            var routingKey = $"client.{message.Action?.ToLower() ?? "default"}";

            _messageBroker.PublishMessage(exchange, routingKey, message);

            return Ok(new { success = true, messageId = message.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du message");
            return StatusCode(500, "Une erreur s'est produite lors de l'envoi du message");
        }
    }
}
