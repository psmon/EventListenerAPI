using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.DependencyInjection;

using EventListenerAPI.Actors;
using EventListenerAPI.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using AkkaConfig = Akka.Configuration.Config;

namespace EventListenerAPI.Services
{
    public interface IActorBridge
    {
        void Tell(EventLog message);
    }
    public class AkkaService : IHostedService, IActorBridge
    {
        private ActorSystem _actorSystem;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        
        
        private IActorRef _fsmActor;

        private readonly IHostApplicationLifetime _applicationLifetime;

        public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = appLifetime;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var bootstrap = BootstrapSetup.Create();

            // enable DI support inside this ActorSystem, if needed
            var diSetup = DependencyResolverSetup.Create(_serviceProvider);
            

            // merge this setup (and any others) together into ActorSystemSetup
            var actorSystemSetup = 
                bootstrap.And(diSetup);
            

            // start ActorSystem
            _actorSystem = ActorSystem.Create("akka-universe", actorSystemSetup);


            _applicationLifetime.ApplicationStarted.Register(async () =>
            {
                var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();                
                IActorRef fsmActor = _actorSystem.ActorOf<FSMActor>("fsmActor");
                IActorRef batchActor = _actorSystem.ActorOf(Props.Create(() => new BatchActor(serviceScopeFactory)), "batchActor");
                        
                //배치처리기 지정..
                fsmActor.Tell(new SetTarget(batchActor));

                batchActor.Tell(new SetTarget(fsmActor));

                _fsmActor = fsmActor;

            });


            // add a continuation task that will guarantee shutdown of application if ActorSystem terminates
            //await _actorSystem.WhenTerminated.ContinueWith(tr => {
            //   _applicationLifetime.StopApplication();
            //});
            _actorSystem.WhenTerminated.ContinueWith(tr => {
                _applicationLifetime.StopApplication();
              });
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // strictly speaking this may not be necessary - terminating the ActorSystem would also work
            // but this call guarantees that the shutdown of the cluster is graceful regardless
            await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }

        public void Tell(EventLog message)
        {
            _fsmActor.Tell(new Queue(message));
        }

    }
}
