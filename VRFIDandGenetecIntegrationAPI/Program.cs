using App.Bal.Repositories;
using App.Bal.Services;
using Microsoft.EntityFrameworkCore;
using VRFIDandGenetecIntegrationAPI.Controllers;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.OpenApi.Models;
using App.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<CardHolderController>();
builder.Services.AddTransient<GroupController>();
builder.Services.AddTransient<PassCreatorAPIController>();

// Configure DbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<BookingProcessor>();
builder.Services.AddHostedService<BookingPollingService>();
builder.Services.AddSingleton<ISMSService, SMSService>();
builder.Services.AddSingleton<IGenetecServices, GenetecServices>();
builder.Services.AddSingleton<IMailService, MailService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new CustomerAuthorizeAttribute("customerid"));
});

// Configure Hangfire
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))); // Connection string for Hangfire database

builder.Services.AddHangfireServer(); // Add Hangfire server

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("VRFIDAPIPolicy", policyBuilder =>
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VRFID API", Version = "v1" });
    c.OperationFilter<AddCustomerIdHeaderParameter>(); // Register filter
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("VRFIDAPIPolicy"); // Enable the CORS policy here

app.UseAuthorization();

app.UseHangfireDashboard(); // Enable Hangfire Dashboard (optional)

app.MapControllers();

app.Run();
