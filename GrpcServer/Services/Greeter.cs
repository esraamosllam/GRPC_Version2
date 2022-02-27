using Microsoft.Extensions.Logging;

namespace Server
{
    public class Greeter : IGreeter
    {
        private readonly ILogger<Greeter> _logger;

        public Greeter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Greeter>();
        }

        public string Greet(string name)
        {
            _logger.LogInformation($"Creating greeting to {name}");
            return $"Hello {name}";
        }
    }
}
