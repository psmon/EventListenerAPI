# EventListenerAPI


# 컨셉

![](./doc/concept.png)

불특정하게 발생하는 대용량 이벤트를 고성능 실시간 대응

성능고려
- 이벤트마다 Store커넥션을 1사용하는것은 커넥션비용 낭비
- 1번의 이벤트를 100번저장하는것보다, 100개의 데이터를 벌크인서트하는것이 성능 효율
- 불특정 발생하는 이벤트를 배치처리가 아닌, FSM Queue Actor를 활용한 세미실시간처리


# Local Infra

- Postgre : localhost:5432
- Elk : http://localhost:9200
- Kibana : http://localhost:9400/app/kibana



# 참고 링크
- FSM을 이용한 배치처리기:  https://wiki.webnori.com/display/AKKA/Finite+State+Machines
- Akka.net FSM Actor :  https://getakka.net/articles/actors/finite-state-machine.html
- BulkInsert : https://www.c-sharpcorner.com/article/bulk-operations-in-entity-framework-core/
- redshift : https://docs.aws.amazon.com/ko_kr/redshift/latest/dg/r_CREATE_TABLE_examples.html

