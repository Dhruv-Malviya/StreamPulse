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
account_status    TINYINT          NOT NULL  DEFAULT 1        -- 0 = deactivated, 1 = active, 2 = banned, 3 = soft deleted
created_at        TIMESTAMPTZ      NOT NULL  DEFAULT NOW()
modified_at       TIMESTAMPTZ      NOT NULL  DEFAULT NOW()

INDEX: idx_users_email ON users(email)
CONSTRAINT: CK_User_AccountStatus (account_status >= 0 AND account_status <= 3)


follows
─────────────────────────────────────────────────────────
follower_id   INT          NOT NULL  FK → users(user_id)
followee_id   INT          NOT NULL  FK → users(user_id)
created_at    TIMESTAMPTZ  NOT NULL  DEFAULT NOW()

PRIMARY KEY (follower_id, followee_id)
INDEX: idx_follows_followee_id ON follows(followee_id)


channels
─────────────────────────────────────────────────────────
channel_id          INT              PRIMARY KEY
user_id             INT              NOT NULL  UNIQUE
channel_name        VARCHAR(100)     NOT NULL  UNIQUE
channel_description VARCHAR(2000)    NULL
channel_profile_url VARCHAR(2000)    NULL
channel_status      SMALLINT         NOT NULL  DEFAULT 1       -- 0 = deactivated, 1 = active, 2 = banned, 3 = soft deleted
created_at          TIMESTAMPTZ      NOT NULL  DEFAULT NOW()
modified_at         TIMESTAMPTZ      NOT NULL  DEFAULT NOW()

INDEXES (automatic from constraints):
  PRIMARY KEY → channel_id
  UNIQUE      → user_id
  UNIQUE      → channel_name

CONSTRAINT: CK_Channel_ChannelStatus  (channel_status >= 0 AND channel_status <= 3)


videos
─────────────────────────────────────────────────────────
video_id               INT              PRIMARY KEY
channel_id             INT              NOT NULL  FK → channels(channel_id)
video_title            VARCHAR(100)     NOT NULL
video_description      VARCHAR(2000)    NULL
video_thumbnail_url    VARCHAR(2000)    NULL
video_status           SMALLINT         NOT NULL  DEFAULT 0 -- 0 = uploading, 1 = processing, 2 = published-- 3 = unlisted, 4 = deleted
video_size             BIGINT           NOT NULL
video_duration_seconds INT              NOT NULL  -- seconds
created_at             TIMESTAMPTZ      NOT NULL  DEFAULT NOW()
modified_at            TIMESTAMPTZ      NOT NULL  DEFAULT NOW()

INDEXES:
  PRIMARY KEY → video_id
  INDEX       → channel_id

CONSTRAINT: "CK_Video_VideoStatus" (video_status >= 0 AND video_status <= 4)

video_assets
─────────────────────────────────────────────────────────
video_asset_id      INT              PRIMARY KEY
video_id            INT              NOT NULL  FK → videos(video_id)
video_quality       INT              NOT NULL  -- vertical pixels: 360, 720, 1080
manifest_url        VARCHAR(2000)    NOT NULL
video_asset_size    BIGINT           NOT NULL  -- bytes
video_asset_bitrate INT              NOT NULL  -- kbps
video_asset_status  SMALLINT         NOT NULL  DEFAULT 0 -- 0 = initiated, 1 = success, 2 = failed, 3 = deleted
created_at          TIMESTAMPTZ      NOT NULL  DEFAULT NOW()

INDEXES:
  PRIMARY KEY → video_asset_id
  INDEX       → video_id

CONSTRAINT: "CK_VideoAsset_VideoAssetStatus" (video_asset_status >= 0 AND video_asset_status <= 3)


upload_jobs
─────────────────────────────────────────────────────────
upload_job_id            INT              PRIMARY KEY
channel_id               INT              NOT NULL             -- FK → channels(channel_id), not enforced (cross-service boundary)
upload_job_status        SMALLINT         NOT NULL  DEFAULT 0  -- 0 = pending, 1 = uploading, 2 = uploaded, 3 = processing, 4 = completed, 5 = failed
upload_job_blob_path     VARCHAR(2000)    NOT NULL             -- object storage key, not a presigned URL
upload_job_title         VARCHAR(100)     NOT NULL
upload_job_description   VARCHAR(2000)    NULL
upload_job_thumbnail_url VARCHAR(2000)   NULL
created_at               TIMESTAMPTZ      NOT NULL  DEFAULT NOW()
modified_at              TIMESTAMPTZ      NOT NULL  DEFAULT NOW()

INDEX:      idx_upload_jobs_modified_at ON upload_jobs(modified_at) WHERE upload_job_status IN (1, 2, 3)
CONSTRAINT: CK_UploadJob_UploadJobStatus (upload_job_status >= 0 AND upload_job_status <= 5)

```
