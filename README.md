### Architecture

```
                       Client (Browser / Mobile / Video Player)
                                       |
                    +------------------+------------------+
                    |  REST / HTTP            WebSocket   |
                    v                                     v
        +-------------------------------------------------------------+
        |  Nginx  -  API Gateway . Load Balancer . CDN edge (HLS)     |
        |  TLS . rate limiting . path routing . Cache-Control headers |
        +------+--------------------------------------------+---------+
               | /api/*                                     | /hls/*  /static/*
               v                                            v
   +------------------------------------+            +----------------+
   |            Microservices           |            |     MinIO      |
   |   (Docker . Kubernetes . Helm)     |            |  HLS segments  |
   |                                    |            |  thumbnails    |
   |   user-service          (.NET)     |            |  VOD assets    |
   |   catalog-service       (.NET)     |            +----------------+
   |   notification-service  (.NET)     |
   |   chat-service          (.NET)     |     gRPC, service-to-service
   |   ingest-service        (Python)   | <----------------------------+
   |   search-service        (Python)   |                              |
   |   recommendation-service(Python)   |                              |
   |   analytics-service     (Python)   |                              |
   +---+----------+----------+----------+--------------------+----------+
       |          |          |                              |
   +---v----+ +---v----+ +---v-----+                 +-------v--------+
   |Postgres| | Redis  | | Kafka   |                 | Elasticsearch  |
   |users   | |sessions| |video-   |                 | video/channel  |
   |channels| |cache   | |events   |                 | search index   |
   |videos  | |viewer  | |watch-   |                 +----------------+
   |follows | |counts  | |events   |                 +----------------+
   |playlist| |ratelim | |channel- |                 |   Cassandra    |
   +--------+ |locks   | |events   |                 | watch events   |
             |pub/sub | +---------+                 | view telemetry  |
             +--------+                              +----------------+
                                  |
                    +-------------v--------------+
                    | Observability              |
                    | Prometheus . Grafana       |
                    | JSON logs + correlation_id |
                    +----------------------------+
```

### The eight services

| Service                | Language | Primary store                  | Cache / Redis use           | Produces events                      | Consumes events                      |
| ---------------------- | -------- | ------------------------------ | --------------------------- | ------------------------------------ | ------------------------------------ |
| user-service           | .NET     | PostgreSQL                     | sessions (JWT)              | —                                    | —                                    |
| catalog-service        | .NET     | PostgreSQL                     | catalog cache               | `video.published`, `channel.updated` | —                                    |
| ingest-service         | Python   | PostgreSQL                     | —                           | `video.uploaded`                     | —                                    |
| notification-service   | .NET     | —                              | online-user set             | —                                    | `video.published`, `channel.live`    |
| chat-service           | .NET     | —                              | pub/sub fan-out, rate limit | —                                    | —                                    |
| search-service         | Python   | Elasticsearch _(is the store)_ | —                           | —                                    | `video.published`, `channel.updated` |
| recommendation-service | Python   | —                              | recommendation cache        | —                                    | `watch-events`                       |
| analytics-service      | Python   | Cassandra                      | —                           | —                                    | all events                           |

### Database Design

```
users
─────────────────────────────────────────────────────────
user_id           INT              PRIMARY KEY
username          VARCHAR(100)     NOT NULL
email             VARCHAR(100)     NOT NULL  UNIQUE
password_hash     VARCHAR(100)     NOT NULL                   -- ASP.NET Core PasswordHasher output is ~84 chars, varchar(100) gives a safe margin
profile_url       VARCHAR(2000)    NULL
account_status    TINYINT          NOT NULL  DEFAULT 1        -- 0 = deactivated, 1 = active, 2 = banned
created_at        TIMESTAMPTZ      NOT NULL  DEFAULT NOW()
modified_at       TIMESTAMPTZ      NOT NULL  DEFAULT NOW()


follows
─────────────────────────────────────────────────────────
follower_id   INT          NOT NULL  FK → users(user_id)
followee_id   INT          NOT NULL  FK → users(user_id)
created_at    TIMESTAMPTZ  NOT NULL  DEFAULT NOW()

PRIMARY KEY (follower_id, followee_id)
INDEX idx_follows_followee_id ON follows(followee_id)

```
