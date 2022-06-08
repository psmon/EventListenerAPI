
using EventListenerAPI.Models;

using Nest;

namespace EventListenerAPI.Services
{
    public interface IElasticEngine
    {
        IElasticClient GetClientForEventLog();
    }

    public class ElasticEngine : IElasticEngine
    {
        private IElasticClient clientForEventLog { get; set; }

        private string ElkUrl { get; set; }        

        public ElasticEngine(IConfiguration configuration)
        {
            var url = configuration["elasticsearch:url"];
            ElkUrl = url;

            var eventlogIdx = configuration["elasticsearch:index-eventlog"];

            var defaultSettings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(eventlogIdx)
                .DefaultMappingFor<EventLog>(m => m                    
                    .IdProperty(p => p.uuid)
                );

            clientForEventLog = new ElasticClient(defaultSettings);

        }

        public IElasticClient GetClientForEventLog()
        {
            return clientForEventLog;
        }

    }

    public class IndexService
    {
        private readonly IElasticEngine _elasticEngine;

        private readonly ILogger _logger;

        public IndexService(IElasticEngine elasticEngine, ILogger<IndexService> logger)
        {
            _elasticEngine = elasticEngine;

            _logger = logger;

        }

        public async Task BulkInsertAsync(List<EventLog> eventLogs)
        {
            var bulkDesc = new BulkDescriptor();

            foreach(var log in eventLogs)
            {
                bulkDesc.Index<EventLog>(op => op
                    .Document(log)
                );
            }

            var bulkResult_descriptorAutokeyword = await _elasticEngine.GetClientForEventLog().
                BulkAsync(bulkDesc).ConfigureAwait(false);

            if (!bulkResult_descriptorAutokeyword.ApiCall.Success)
            {
                _logger.LogError("bulkResult_descriptorAutokeyword");                
            }

            _logger.LogInformation($"Elk:IndexingAutoCompleted");
        }
    }
}
