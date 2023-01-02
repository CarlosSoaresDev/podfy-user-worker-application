using Amazon.SQS;
using Amazon.SQS.Model;
using podfy_user_worker_application.Services;

namespace podfy_user_worker_application.Worker;

public class WorkerScheduler : BackgroundService
{
    private readonly ISESService _sESService;
    private readonly ILogger<WorkerScheduler> _logger;
    private readonly ISQSQueueService _sQSQueueService;

    public WorkerScheduler(ISESService sESService
        , ILogger<WorkerScheduler> logger
        , ISQSQueueService sQSQueueService)
    {
        _sESService = sESService ?? throw new ArgumentNullException(nameof(sESService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sQSQueueService = sQSQueueService ?? throw new ArgumentNullException(nameof(sQSQueueService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Start job worker");
            var queueUrl = Environment.GetEnvironmentVariable("SQS_URL");

            while (!stoppingToken.IsCancellationRequested)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    WaitTimeSeconds = 2,
                    MaxNumberOfMessages = 10
                };

                var response = await _sQSQueueService.ReceiveMessageAsync(request);

                if (response.Messages.Any())
                {
                    foreach (var message in response.Messages)
                    {
                        if (await _sESService.SendEmailAsync(message.Body))
                        {
                            var responseDeletedQueue = await _sQSQueueService.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
                            if (responseDeletedQueue.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                _logger.LogInformation($"Item {message.MessageId} is deleted");
                            else
                                _logger.LogError($"Item {message.MessageId} is not deleted");
                        }
                    }
                }
            }
        }
        catch (AmazonSQSException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw ex;
        }
    }
}
