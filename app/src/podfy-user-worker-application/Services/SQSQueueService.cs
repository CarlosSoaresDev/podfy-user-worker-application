using Amazon.SQS;
using Amazon.SQS.Model;

namespace podfy_user_worker_application.Services;
public class SQSQueueService : ISQSQueueService
{
    public async Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request)
    {
        return await GetSQSCclient().ReceiveMessageAsync(request);
    }

    public async Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle)
    {
        return await GetSQSCclient().DeleteMessageAsync(queueUrl, receiptHandle);
    }

    private AmazonSQSClient GetSQSCclient()
    {
        if (Debugger.IsAttached)
            return new AmazonSQSClient();

        return new AmazonSQSClient(Environment.GetEnvironmentVariable("ACCESS_KEY"), Environment.GetEnvironmentVariable("SECRET_KEY"), Amazon.RegionEndpoint.USEast1);
    }
}