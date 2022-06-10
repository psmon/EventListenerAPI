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

<pre>
version: '3.4'
 
services:

  postgres:
    image: postgres:9.6
    container_name: postgres
    environment:
      - POSTGRES_USER=docker
      - POSTGRES_PASSWORD=docker        
    ports:
      - '5432:5432'
    volumes:
      - postgre-data:/usr/local/psql/data
       
  adminer:
    image: adminer
    restart: always
    ports:
      - 13307:8080
    volumes:
      - "/etc/timezone:/etc/timezone:ro"
      - "/etc/localtime:/etc/localtime:ro"
    
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.4.0
    container_name: elasticsearch
    environment:
      - xpack.security.enabled=false
      - discovery.type=single-node
      - "ES_JAVA_OPTS=-Xms1500m -Xmx3000m"
    ulimits:
      memlock:
        soft: -1
        hard: -1
      nofile:
        soft: 65536
        hard: 65536
    cap_add:
      - IPC_LOCK
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    ports:
      - 9200:9200
      - 9300:9300
      
  kibana:
    container_name: kibana
    image: docker.elastic.co/kibana/kibana:7.4.0
    restart: always
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    ports:
      - 9400:5601    
    depends_on:
      - elasticsearch

volumes:
  elasticsearch-data:
    driver: local
  postgre-data:
    driver: local
</pre>


# 참고 링크
- FSM을 이용한 배치처리기:  https://wiki.webnori.com/display/AKKA/Finite+State+Machines
- Akka.net FSM Actor :  https://getakka.net/articles/actors/finite-state-machine.html
- BulkInsert : https://www.c-sharpcorner.com/article/bulk-operations-in-entity-framework-core/
- redshift : https://docs.aws.amazon.com/ko_kr/redshift/latest/dg/r_CREATE_TABLE_examples.html

