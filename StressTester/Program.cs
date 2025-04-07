using CCNAssignment2Client;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Stress Test...");
        
        using var client = new RapidRequestClient(maxConcurrentRequests: 50);
        
        // Test adding multiple books
        Console.WriteLine("\nTesting AddBook with 500 concurrent requests...");
        var addBookResults = await client.SendBulkRequestsAsync(
            client.CreateAddBookRequest(),
            5000
        );
        PrintResults(addBookResults);

        // Test adding multiple members
        Console.WriteLine("\nTesting AddMember with 300 concurrent requests...");
        var addMemberResults = await client.SendBulkRequestsAsync(
            client.CreateAddMemberRequest(),
            300
        );
        PrintResults(addMemberResults);

        // Test finding books (this will fail for non-existent IDs, but that's okay for stress testing)
        Console.WriteLine("\nTesting FindBook with 400 concurrent requests...");
        var findBookResults = await client.SendBulkRequestsAsync(
            client.CreateFindBookRequest(1),
            400
        );
        PrintResults(findBookResults);

        Console.WriteLine("\nStress Test Completed!");
    }

    static void PrintResults(List<RequestResult> results)
    {
        var successful = results.Count(r => r.IsSuccessful);
        var failed = results.Count - successful;
        var avgResponseTime = results.Average(r => r.ResponseTime);
        var maxResponseTime = results.Max(r => r.ResponseTime);
        var minResponseTime = results.Min(r => r.ResponseTime);

        Console.WriteLine($"Total Requests: {results.Count}");
        Console.WriteLine($"Successful: {successful}");
        Console.WriteLine($"Failed: {failed}");
        Console.WriteLine($"Average Response Time: {avgResponseTime:F2}ms");
        Console.WriteLine($"Min Response Time: {minResponseTime}ms");
        Console.WriteLine($"Max Response Time: {maxResponseTime}ms");

        if (failed > 0)
        {
            Console.WriteLine("\nFailed Requests:");
            foreach (var result in results.Where(r => !r.IsSuccessful))
            {
                Console.WriteLine($"Request {result.RequestId}: {result.ErrorMessage}");
            }
        }
    }
} 