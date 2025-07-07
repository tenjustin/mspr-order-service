using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var pgDb = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("order-service-db");

var username = builder.AddParameter("username", "username");
var password = builder.AddParameter("password", "password");

var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
                      .WithManagementPlugin();

builder.AddProject<Kawa_OrderService_Api>("api")
    .WithReference(pgDb)
    .WaitFor(pgDb);

builder.Build().Run();