using System.Text;
using System.Text.Json;
using Kawa.OrderService.Api.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Kawa.OrderService.Api.Services;

/// <summary>
/// Interface pour le traitement des messages reçus
/// </summary>
public interface IMessageHandler<in T>
{
    Task HandleMessageAsync(T message, string routingKey);
}

/// <summary>
/// Service consommateur pour RabbitMQ
/// </summary>
public class MessageConsumerService : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string _queueName = string.Empty;

    public MessageConsumerService(
        IConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<MessageConsumerService> logger)
    {
        _connectionFactory = connectionFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Démarrage du service consommateur de messages");
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            // Configurez l'échange
            const string exchangeName = "order-service";
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // Créer une file d'attente avec un nom généré par le serveur
            _queueName = _channel.QueueDeclare().QueueName;

            // Associez la file d'attente à l'échange avec un modèle de routage
            // Par exemple, écoutez tous les messages commençant par "order."
            _channel.QueueBind(_queueName, exchangeName, "order.*");

            // Configurez le consommateur
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                try
                {
                    _logger.LogInformation("Message reçu avec la clé de routage {RoutingKey}: {Message}",
                        routingKey, message);

                    // Traiter le message en fonction de la clé de routage
                    if (routingKey.StartsWith("order."))
                    {
                        var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message);
                        if (orderMessage != null)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<OrderMessage>>();
                            await handler.HandleMessageAsync(orderMessage, routingKey);
                        }
                    }

                    // Accusé de réception
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors du traitement du message");
                    // Selon la politique souhaitée, vous pourriez rejeter le message 
                    // ou le renvoyer dans la file d'attente
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            // Commencer à consommer les messages
            _channel.BasicConsume(_queueName, false, consumer);

            _logger.LogInformation("En attente de messages sur la file {QueueName}", _queueName);

            // Maintenir le service en vie
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation du consommateur de messages");

            // Réessayez après un délai si le service n'est pas en cours d'arrêt
            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
                await ExecuteAsync(stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Arrêt du service consommateur de messages");

        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        await base.StopAsync(cancellationToken);
    }
}