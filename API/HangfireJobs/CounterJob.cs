using Hangfire;
using Nummy.CodeLogger.Data.Entitites;
using Nummy.CodeLogger.Data.Services;

namespace API.HangfireJobs;

public class CounterJob(INummyCodeLoggerService service)
{
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Run(IJobCancellationToken hangfireToken)
    {
        try
        {
            await service.LogAsync(NummyCodeLogLevel.Info, "I am running");
        }
        catch (OperationCanceledException)
        {
            await service.LogAsync(NummyCodeLogLevel.Debug, "job cancelled");
            throw; // let Hangfire see the cancellation
        }
        catch (Exception ex)
        {
            await service.LogAsync(NummyCodeLogLevel.Fatal, "job crashed");
            throw; // Hangfire will retry according to attributes
        }
    }
}