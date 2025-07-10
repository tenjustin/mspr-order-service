using Kawa.OrderService.Api.Models;


namespace Kawa.OrderService.Api.Services;

/// <summary>
/// Implémentation du gestionnaire de messages clients
/// </summary>
public class OrderMessageHandler : IMessageHandler<OrderMessage>
{
    private readonly ILogger<OrderMessageHandler> _logger;

    public OrderMessageHandler(ILogger<OrderMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleMessageAsync(OrderMessage message, string routingKey)
    {
        _logger.LogInformation("Traitement du message: ID={Id}, Action={Action}, Contenu={Content}",
            message.Id, message.Action, message.Content);

        // Implémentez votre logique métier ici
        // Par exemple:
        switch (message.Action?.ToLower())
        {
            case "create":
                _logger.LogInformation("Création d'un nouveau client");
                break;
            case "update":
                _logger.LogInformation("Mise à jour d'un client");
                break;
            case "delete":
                _logger.LogInformation("Suppression d'un client");
                break;
            default:
                _logger.LogWarning("Action inconnue: {Action}", message.Action);
                break;
        }

        return Task.CompletedTask;
    }
}
