using Amazon.SQS.Model;

namespace podfy_user_worker_application.Services;
public interface ISQSQueueService
{
    Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request);

    Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle);
}

