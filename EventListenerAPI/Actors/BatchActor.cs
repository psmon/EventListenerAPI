using Akka.Actor;
using Akka.Event;

using EFCore.BulkExtensions;

using EventListenerAPI.Models;
using EventListenerAPI.Services;

namespace EventListenerAPI.Actors
{
    public class BatchActor : ReceiveActor
    {
        private ILoggingAdapter log = Context.GetLogger();  //기본탑재 로그

        private readonly IServiceScopeFactory _scopeFactory;

        public BatchActor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            ReceiveAsync<Batch>( async message =>
            {
                log.Info( "GetMessageCnt:" + message.Obj.Count.ToString() );

                List<EventLog> eventLogs = new();
                foreach( object data in message.Obj )
                {
                    if(data is EventLog) eventLogs.Add((EventLog)data);
                }

                using(var scope = _scopeFactory.CreateScope())
                {
                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var indexService = scope.ServiceProvider.GetRequiredService<IndexService>();

                    if(eventLogs.Count > 0)
                    {                        
                        //배치처리 GO
                        await appDbContext.BulkInsertAsync(eventLogs); 
                        
                        await indexService.BulkInsertAsync(eventLogs);
                    }
                }
            });

        }
    }
}
