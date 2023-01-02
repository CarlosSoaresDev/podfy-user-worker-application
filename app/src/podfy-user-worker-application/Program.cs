using podfy_user_worker_application.Services;
using podfy_user_worker_application.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ISQSQueueService, SQSQueueService>();
builder.Services.AddTransient<ISESService, SESService>();
builder.Services.AddHostedService<WorkerScheduler>();

var app = builder.Build();

app.Run();