using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var pgDb = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("order-service-db");

builder.AddProject<Kawa_OrderService_Api>("api")
    .WithReference(pgDb)
    .WaitFor(pgDb);

builder.Build().Run();