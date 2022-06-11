using System.Text.Json;

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

        private IActorRef _fsmActor;

        private const int MaxBatchSize = 300;

        public BatchActor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            ReceiveAsync<SetTarget>( async fsmActor => 
            {
                _fsmActor = fsmActor.Ref;
            });

            ReceiveAsync<int>( async count => 
            { 
                log.Info( "Queue:" + count );

                if(count > MaxBatchSize)
                {
                    if(_fsmActor != null)
                    {
                        _fsmActor.Tell(new Flush());
                    }                    
                }
            });

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
                    string processStep = "InQueue";

                    try
                    {
                        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var indexService = scope.ServiceProvider.GetRequiredService<IndexService>();                    

                        if(eventLogs.Count > 0)
                        {               
                        
                            processStep = "Try appDbContext.BulkInsertAsync";

                            //배치처리 GO
                            await appDbContext.BulkInsertAsync(eventLogs); 

                            processStep = "Try indexService.BulkInsertAsync";
                        
                            await indexService.BulkInsertAsync(eventLogs);
                        }
                        
                    }
                    catch( Exception ex )
                    {
                        var json = JsonSerializer.Serialize(eventLogs);
                        log.Error( processStep + " ==> " + json );
                    }
                }
            });

        }
    }
}
