using Marten;
using Profile.Api.Application.Projections;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(marten =>
{
    marten.Connection(builder.Configuration.GetConnectionString("DefaultDatabase"));
    marten.AutoCreateSchemaObjects = AutoCreate.CreateOnly;

    marten.Projections.Add<CustomerInfoProjection>();
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
