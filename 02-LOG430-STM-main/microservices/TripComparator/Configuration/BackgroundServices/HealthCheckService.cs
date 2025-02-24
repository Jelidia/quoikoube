using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

public class HealthCheckService : IHostedService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthCheckService> _logger;
    private Timer? _timer;
    private static readonly string ServicesAdress = Environment.GetEnvironmentVariable("SERVICES_ADDRESS");
    private static readonly string NodeControllerPort = Environment.GetEnvironmentVariable("NODE_CONTROLLER_PORT");
    private string port = "32772";

    public HealthCheckService(HttpClient httpClient, ILogger<HealthCheckService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_timer == null)
        {
            // Lancer la vérification toutes les X secondes
            //_timer = new Timer(ExecuteHealthCheck, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        return Task.CompletedTask;
    }

    private async void ExecuteHealthCheck(object state)
    {
        var url = $"http://{ServicesAdress}:{port}/RouteTime/IsAlive";

        try
        {
            await _httpClient.GetAsync(url);
        }
        catch (Exception _)
        {
            _logger.LogInformation("Route Time Provider n'est pas en vie.");
            url = $"http://{ServicesAdress}:{NodeControllerPort}/routing/routeByServiceType/RouteTimeProvider?caller=TripComparator&mode=0";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonArray = JArray.Parse(jsonResponse);
                    var newPort = jsonArray[0]?["port"]?.ToString();

                    _logger.LogInformation("New port for RouteTimeProvider: {Response}", newPort);
                    port = newPort;
                }
                else
                {
                    _logger.LogWarning("They are currently no RouteTimeProvider container. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting the new RouteTimeProvider port at {Url}: {Error}", url, ex.Message);
            }

        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
