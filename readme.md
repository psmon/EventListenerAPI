# BypassQueueAPI

API 기능 : 이벤트 발생순서에의핸 API완료 순서보장을 보증합니다.

사용목적 : 이벤트 발생(주로 웹훅을 호출하는경우)지점 수정이 불가능하고
웹훅으로 연결된 스트리밍 웹훅 API 요청완료에 대해 순차완료보장이 필요한경우 (Lock사용 불필요)


## 컨셉

### 문제
![](./doc/concept0.png)

이벤트가 순차적으로 발생하고, 분산된 서버(또는 비동기로 처리되는 서버)가
다음과 같은 전화기 이벤트를 처리한다고 가정해봅시다.

발생이벤트 순서
- 1.전화 이벤트
- 2.전화받기 이벤트
- 3.전화 끊기 이벤트

이것을 차례로 처리한다고, 순차적으로 처리완료가 된다라고 착각할수 있습니다. 
하지만 각각의 이벤트의 완료순서가 다르기때문에 순차완료가 보장이 안됩니다.

이벤트를 처리하는 서버가 동기적(순서대로) 으로밖에 처리못하고 처리기가 단 하나일때
이 문제는 해결될수 있지만, 우리의 이벤트 처리기는 순서보장이 필요없는 다양한 API를
처리할수 있으며 결정적으로 단일스레드 요청순서대로만 처리하는 서버는 블락킹으로 
단일지점 병목및 메시지 유실의 가능성이 높습니다.


전화 이벤트가 처리중인 상태에서, 전화 끊기 이벤트가 수행되어 
순서에의해 동작오류가 발생할수 있습니다. 


동시성적으로 발생하는 충돌문제를 해결하게 위해 다음 전략을 이용할수 있습니다.

- 다양한 어플리케이션 Lock 모델사용 : 락으로 인해 대기상태에 빠질수 있으며, 이벤트유실을 포함 데드락이 걸릴수 있습니다. 결정적으로 분산된 환경에서 리소스 잠금은 구현이 어렵습니다.
- DB Lock 이해 : 멀티스레드 트랙젝션간 동일데이터 변경을 보호하기 위한 용도이며, 복잡한 쿼리에의해 동시적으로 Row를 변경하는 경우 DeadLock이 될가능성이 높으며 DB엔진및 격리수준에 따라 작동방식이 다르기때문에 성능을 제어하기가 어렵습니다.


#### 데드락 쿼리 예제

    MYSQL에서 다음 쿼리를 동시에 수행하면 데드락 발생~
    
    START TRANSACTION;
    SELECT * FROM testdb.tbl_locktest for update;
    do sleep(10);
    UPDATE `testdb`.`tbl_locktest2` SET `text` = 'test2' WHERE (`id` = '1');
    COMMIT;

    START TRANSACTION;
    SELECT * FROM testdb.tbl_locktest2 for update;
    UPDATE `testdb`.`tbl_locktest` SET `text` = 'test2' WHERE (`id` = '1');
    COMMIT;


### 순차적 완료를 보장

해결컨셉:
순차처리가 필요한 그룹을 지정하여, 해당 그룹에 소속된 그룹의 이벤트만 순차 처리가 가능합니다.
순차처리를 유지하고, 동시성을 높이려면 순차처리 단위 그룹을 논리적으로 분리하면 됩니다.
이 순차 그룹은 액터하나에 해당합니다.

TYPE A: 직접 WebHook 받을시
![](./doc/concept.png)


TYPE B: WebHook Target수정불가시 - ByPass+CallBack 방식으로 작동할시
![](./doc/concept2.png)



##
    완료시간이 제각각이여도, 요청 순서대로 완료보장~

    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          [REQNO-1] Done TestCallBack 2 , Completed Time 508
    [INFO][2022-05-20 오전 6:55:09][Thread 0018][akka://akka-universe/user/group1] Received PostSpec message: http://localhost:9000/api/ByPassCallBack/test
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          PostTodoItem
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          TestCallCount 6
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          PostTodoItem
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          TestCallCount 7
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          [REQNO-2] Done TestCallBack 3 , Completed Time 918
    [INFO][2022-05-20 오전 6:55:10][Thread 0008][akka://akka-universe/user/group1] Received PostSpec message: http://localhost:9000/api/ByPassCallBack/test
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          [REQNO-3] Done TestCallBack 4 , Completed Time 724
    [INFO][2022-05-20 오전 6:55:11][Thread 0004][akka://akka-universe/user/group1] Received PostSpec message: http://localhost:9000/api/ByPassCallBack/test
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          [REQNO-4] Done TestCallBack 5 , Completed Time 765
    [INFO][2022-05-20 오전 6:55:11][Thread 0028][akka://akka-universe/user/group1] Received PostSpec message: http://localhost:9000/api/ByPassCallBack/test
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          [REQNO-5] Done TestCallBack 6 , Completed Time 1061
    [INFO][2022-05-20 오전 6:55:13][Thread 0018][akka://akka-universe/user/group1] Received PostSpec message: http://localhost:9000/api/ByPassCallBack/test
    info: QueueByPassAPI.Controllers.ByPassCallBackController[0]
          [REQNO-6] Done TestCallBack 7 , Completed Time 1332

## Code 컨셉

    //액터구현
    public class QueueActor: ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();

        private ApiClient apiClient = new ApiClient();

        public QueueActor()
        {
            ReceiveAsync<PostSpec>(async message => {
                log.Info("Received PostSpec message: {0} {1}", message.host, message.path);
                var data = await apiClient.PostCallBack(message.reqId, message.host, message.path, message.data);
            });
        }
        
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new QueueActor());
        }

    }

    //액터 호출
    _bridge.Tell(id, new Model.PostSpec()
    { 
        reqId = _testCount.callCount,
        host = tryCallBackUrl, path = null, data = todoItem 
    });

액터는 기본적으로 순차처리 큐처럼 작동됩니다. 메시지를 보내면 액터메시지 큐에 적재되고
하나씩 꺼내어 처리를 합니다. 순서보장에의해 순차완료가 됩니다.
별도로 큐에적재하고 하나씩 꺼내는 과정을 구현할필요없이 처리부만 구현하면 그렇게 작동이됩니다.

순차성이 필요없다라고 하면 pipe로 액터처리를 멈추지않고 동시비동기 처리가 가능합니다.

## API DOC
![](./doc/api_help.png)

- CallBackURL : 이 API를 호출하면, 요청데이터를 큐에적재합니다. 그리고 순차완료를 보장하기위해서 하나씩 꺼내서 CallBack을 수행합니다.
- id : GroupID에 해당하며 여기에 소속된 그룹만 동시완료처리를 보장합니다. 동시처리를 높이려면 이 그룹을 논리적으로 구분합니다. 이것은 장비ID일수도있고, 도메인ID일수도 있습니다.
- body : 콜백시 요청을 해야할 실제 데이터입니다.

이벤트 발생 순서대비 완료순서를 보장하기위해, API간 CallBack(PingPong)이 이용됩니다.


## 추가 자료

이해를 돕기 위해, 순차처리기를 단일지점 API로 구성을 하였지만 (처리를 위임하기 때문에 단일지점으로 대부분 충분)

순차를 보장해야하는 그룹이 많아서 스케일아웃이 필요하다고 하면 액터를 클러스터 구성하여 분산처리 가능합니다.


추가 참고 링크 : 
- https://wiki.webnori.com/display/webfr/Cluster+with+Actor -액터를 클러스터화 할때
- https://petabridge.com/blog/akkadotnet-async-actors-using-pipeto/ -액터를 비동기적으로 중단없이 실행할때 (순차완료 보장은 안됨)
- https://getakka.net/articles/actors/mailboxes.html#unboundedprioritymailbox - 이벤트가 순차가아닌 아주 짧은시간(100ms이내) 동시에 발생한다고 하면 추가적으로 메일박스 우선순위 액터를 적용할수 있습니다.  주로 아주짧은시간 동시에 발생한 이벤트가, 네트워크의 다른 라우티 경로에의해 이벤트 수신순서가 변경될수 있으며 우선순위는 발생id및 메시지 type등을 선택할수 있습니다.
- https://www.letmecompile.com/mysql-innodb-lock-deadlock/ - Mysql DeadLock