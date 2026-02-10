// HealthCheckEndpoint.cs
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ClaudeCodeInstaller.Core
{
    [SupportedOSPlatform("windows")]
    public class HealthCheckEndpoint
    {
        private readonly HealthCheckService _healthCheckService;
        private HttpListener? _listener;
        private bool _isRunning;

        public HealthCheckEndpoint(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public async Task StartAsync(string url = "http://localhost:8080/health")
        {
            if (_isRunning)
                return;

            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _listener.Start();
            _isRunning = true;

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        _ = Task.Run(async () => await HandleRequestAsync(context));
                    }
                    catch (Exception ex)
                    {
                        if (_isRunning)
                        {
                            Console.WriteLine($"Health check endpoint error: {ex.Message}");
                        }
                    }
                }
            });
            await Task.CompletedTask;
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            _listener?.Close();
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/health")
                {
                    var healthResult = await _healthCheckService.CheckHealthAsync();
                    
                    var json = SerializeHealthResult(healthResult);

                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.StatusCode = healthResult.IsHealthy ? (int)HttpStatusCode.OK : (int)HttpStatusCode.ServiceUnavailable;
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var error = Encoding.UTF8.GetBytes(SerializeError(ex.Message));
                await response.OutputStream.WriteAsync(error, 0, error.Length);
            }
            finally
            {
                response.Close();
            }
        }

        [RequiresUnreferencedCode("JSON serialization may require types that cannot be statically analyzed")]
        private static string SerializeHealthResult(HealthCheckResult healthResult)
        {
            return JsonSerializer.Serialize(new
            {
                status = healthResult.IsHealthy ? "healthy" : "unhealthy",
                timestamp = healthResult.Timestamp,
                checks = new
                {
                    claudeCodeInstalled = healthResult.IsClaudeCodeInstalled,
                    prerequisites = healthResult.HasPrerequisites,
                    windows11 = healthResult.IsWindows11,
                    adminRights = healthResult.HasAdminRights
                },
                error = healthResult.ErrorMessage
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        [RequiresUnreferencedCode("JSON serialization may require types that cannot be statically analyzed")]
        private static string SerializeError(string message)
        {
            return JsonSerializer.Serialize(new { error = message });
        }
    }
}
