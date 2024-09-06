using System.Security.Cryptography;

// Initialize CancellationToken
CancellationTokenSource tokenSource = new();
CancellationToken token = tokenSource.Token;

var lockObject = new Object();
var consoleLock = new Object();

var tasks = new List<Task<int[]>>();
// Insert the token to the factory for Tasks
var factory = new TaskFactory(token);

for (int taskCtr = 0; taskCtr <= 10; taskCtr++)
{
    int iterCount = taskCtr + 1;
    // Create a task and start it
    tasks.Add(factory.StartNew(() =>
    {
        int value;
        var values = new int[10];
        for (int ctr = 1; ctr <= 10; ctr++)
        {
            lock(lockObject)
            {
                value = RandomNumberGenerator.GetInt32(0, 5);
            }

            if (value == 0)
            {
                tokenSource.Cancel();
                Console.WriteLine($"Canceling at task {iterCount}");
                break;
            }
            values[ctr - 1] = value;
        }
        return values;
    }, token));

    try
    {
        // Start an aggregating task when all tasks generated above are done.
        var factoryTask = factory.ContinueWhenAll(tasks.ToArray(),
            (results) =>
            {
                Console.WriteLine("Calculating overall mean...");
                long sum = 0;
                int n = 0;
                foreach (var t in results)
                {
                    foreach (var r in t.Result)
                    {
                        sum += r;
                        n++;
                    }
                }
                return sum / (double)n;
            }, token);
        Console.WriteLine($"The mean is {factoryTask.Result}.");
    }
    catch (AggregateException age)
    {
        foreach (Exception e in age.InnerExceptions)
        {
            if (e is TaskCanceledException taskCanceledException)
            {
                Console.WriteLine($"Unable to compute mean: {taskCanceledException.Message}");
            }
            else
            {
                Console.WriteLine($"Exception: {e.GetType().Name}");
            }
        }
    }
    finally
    {
        tokenSource.Dispose();
    }
}
