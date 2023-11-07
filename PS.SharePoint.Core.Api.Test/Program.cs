using Unity;
using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Api.Test;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var container = new UnityContainer();
//container.AddSharePointConfiguration(new SharePointConfiguration("http://icp054dd.dev.sg.info/Statehome/"));
//container.RegisterSharePointRepository<PackageUserRole>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
