# StreamPulse — The SDE2 Build

_A 6-month, hands-on roadmap to interview-ready depth across backend infrastructure and distributed systems._

This is your roadmap and mine. You build, I guide. Everything here is designed around how you said you learn best: practical, with assistance, one concrete checkpoint at a time, with the "why" attached to every decision.

---

## 0. Mentor's note — how to read this file

This file is your single source of truth for the next ~24 weeks. There is one companion file, `StreamPulse-Tracker.md`, where you log what you do. That's the whole system: a plan and a logbook.

A few things up front:

- **The plan is calibrated to you.** ~1–2 focused hours on weekdays for the project, weekends for the DSA track and system design theory. Modest 8GB machine, so the plan never asks you to run everything at once. You've touched most of this stack before, so the pace is brisk and the emphasis is on _tradeoffs and failure modes_, not syntax.
- **Phases 0–4 are written day-by-day in full detail.** From Phase 5 onward, each week is broken into day-sized checkpoints but kept lighter. When you reach a new phase, ping me and I'll expand that week into the same rich daily detail — calibrated to what you actually struggled with in the previous phase. That's the point of doing this with a mentor instead of a static PDF.
- **Don't binge the file.** Read the phase you're on. Skim the rest. The skill tree and the rules, though — internalise those now.
- **When you're stuck, blocked, want a boss check graded, want a week expanded, or want a mock interview — just ask.** That's the on-demand mentoring loop. The roadmap is self-contained enough to run solo, but you'll go faster if you use me.

---

## 1. The mission

You are not building a product. You are building a **vehicle for learning** — a system realistic enough that every technology in the stack is _demanded_ by a real problem, not bolted on for show. By the end you have:

1. A distributed system you built yourself, end to end, that you can whiteboard from memory.
2. Genuine depth — you can explain _why_ every choice was made and _what breaks_ if you change it.
3. A portfolio piece you can walk an interviewer through for any system design round.
4. ~140 algorithm problems solved across every major pattern.
5. A library of system design talking points drawn from things you actually did, not things you read.

The interview goal is deliberately **company-agnostic**. The plan prepares you for big-tech system design rigour, startup "can you actually ship it" pragmatism, and everything in between.

---

## 2. The system — StreamPulse

**StreamPulse is a live video streaming platform.** Think a backend-focused slice of Twitch or YouTube Live: creators publish video, viewers watch and search and chat in real time, and the platform tracks everything that happens.

Why streaming instead of a shopping cart: streaming _forces_ the interesting problems to the surface. Object storage and the CDN pattern stop being an afterthought and become the spine of the product. Real-time (chat, viewer counts, live notifications) is a first-class concern, which is exactly why WebSockets joined the scope. Write-heavy analytics (every watch heartbeat from every viewer) is a genuine firehose, so Cassandra earns its place. And "design YouTube" is one of the most-asked system design interview prompts on earth — you'll have _built a small one_.

### What the system does

1. A creator uploads a video (or registers a live stream) — `ingest-service` stores HLS segments to object storage.
2. The video is published — `catalog-service` writes metadata to Postgres and emits a `video.published` event.
3. Search picks it up — `search-service` consumes the event and indexes it into Elasticsearch.
4. Followers get notified — `notification-service` consumes the event and pushes a WebSocket notification to online followers.
5. A viewer searches, finds the video, and hits play — traffic enters through Nginx, which serves HLS segments from object storage with CDN-style cache headers.
6. While watching, the viewer's player sends watch heartbeats — these stream through Kafka into Cassandra via `analytics-service`.
7. The viewer opens live chat — `chat-service` handles the WebSocket connection; messages fan out across chat-service instances via Redis.
8. Live viewer counts update in real time — backed by Redis sorted sets, pushed to clients over WebSockets.
9. `recommendation-service` consumes the watch stream and keeps a "what to watch next" list warm in Redis.

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

`catalog-service` also owns playback: serving HLS manifests and minting presigned URLs. It's folded in rather than split out to keep the service count honest for an 8GB machine.

### Why polyglot — some .NET, some Python

This is a deliberate design choice, and a strong interview talking point. The split is along the grain of each language's strengths:

- **.NET (ASP.NET Core)** owns the _transactional and real-time API surface_: `user-service`, `catalog-service`, `notification-service`, `chat-service`. Strong typing, mature EF Core migrations, excellent first-party WebSocket story via SignalR, great performance for request/response APIs.
- **Python (FastAPI)** owns the _data and pipeline plane_: `ingest-service`, `search-service`, `recommendation-service`, `analytics-service`. First-class Elasticsearch and Cassandra clients, `boto3` for S3-compatible storage, and the natural home for any future ML-adjacent recommendation work.

The moment you have two languages, you have a real problem to solve: **how do services that don't share a runtime talk to each other reliably?** That's the entire reason gRPC + Protobuf is in the plan as its own phase, and it's why Kafka events need explicit schemas. "We were polyglot, so the contract had to live outside the code" is a sentence that makes interviewers lean in.

### Technology stack

| Technology           | Role in StreamPulse                                   | The concept you must own                                                 |
| -------------------- | ----------------------------------------------------- | ------------------------------------------------------------------------ |
| Docker               | Containerise all eight services + infra               | Multi-stage builds (compiled vs interpreted), layer cache, health checks |
| PostgreSQL           | Authoritative relational store                        | Schema design, indexing, transactions, isolation, pooling, MVCC          |
| Nginx                | API gateway, load balancer, CDN edge for HLS          | Upstream groups, rate limiting, Cache-Control, range requests            |
| gRPC + Protobuf      | Service-to-service comms across the language boundary | Contracts, schema evolution, deadlines, streaming RPC types              |
| Redis                | Cache, locks, rate limiter, live counts, chat fan-out | Cache patterns, stampede, Redlock, sorted sets, pub/sub                  |
| Kafka                | Event streaming + event sourcing the video lifecycle  | Consumer groups, idempotency, exactly-once, consumer lag                 |
| WebSockets           | Live chat, live notifications, viewer count push      | Backplane scaling, sticky sessions, presence, backpressure               |
| Elasticsearch        | Video & channel search (read model)                   | Inverted index, analyzers, CQRS, eventual consistency                    |
| MinIO                | S3-compatible object storage for video assets         | Presigned URLs, bucket policies, the CDN pattern, multipart              |
| Cassandra            | Write-heavy analytics & watch telemetry sink          | Query-first modelling, partition keys, CAP in practice                   |
| Kubernetes           | Orchestrate the whole platform                        | StatefulSet, probes, HPA, requests vs limits, Ingress, RBAC              |
| Helm                 | Package & deploy the platform in one command          | Charts, per-env values, hooks, umbrella charts                           |
| Prometheus + Grafana | Observability                                         | Metrics, dashboards, consumer lag, p99 latency, alerting                 |

---

## 3. The skill tree

Your progress is tracked as a skill tree. **Thirteen branches.** Each branch has three tiers. You unlock a tier by completing the checkpoints tied to it; you complete a branch by reaching Tier III _and_ passing that phase's Boss Check, which earns you the branch's **Mastery Badge**.

Tier meanings are the same across every branch:

- **Tier I — Foundations.** You can use the technology. You know what it is and what problem it solves.
- **Tier II — Practitioner.** You can build real things with it and articulate the tradeoffs you chose.
- **Tier III — Master.** You can explain every major failure mode, you've deliberately broken it and watched what happens, and you can defend your design choices under questioning.

```
STREAMPULSE SKILL TREE
======================

[CONTAINERS]                  Tier I -> Tier II -> Tier III  => Badge: Container Captain
   built on: nothing (start here)

[RELATIONAL DATA]             Tier I -> Tier II -> Tier III  => Badge: Schema Smith
   built on: Containers I

[EDGE & NETWORKING]           Tier I -> Tier II -> Tier III  => Badge: Edge Warden
   built on: Containers II

[SERVICE CONTRACTS / gRPC]    Tier I -> Tier II -> Tier III  => Badge: Contract Broker
   built on: Edge & Networking I

[CACHING & COORDINATION]      Tier I -> Tier II -> Tier III  => Badge: Cache Conjurer
   built on: Relational Data II

[EVENT STREAMING]             Tier I -> Tier II -> Tier III  => Badge: Stream Lord
   built on: Caching & Coordination II, Service Contracts I

[REAL-TIME / WEBSOCKETS]      Tier I -> Tier II -> Tier III  => Badge: Realtime Ranger
   built on: Caching & Coordination II, Event Streaming I

[SEARCH]                      Tier I -> Tier II -> Tier III  => Badge: Index Oracle
   built on: Event Streaming II

[OBJECT STORAGE]              Tier I -> Tier II -> Tier III  => Badge: Object Keeper
   built on: Edge & Networking III

[WIDE-COLUMN NOSQL]           Tier I -> Tier II -> Tier III  => Badge: Wide-Column Warden
   built on: Event Streaming III

[ORCHESTRATION]               Tier I -> Tier II -> Tier III  => Badge: Cluster Commander
   built on: every "built on" branch above at Tier II+

[PACKAGING / HELM]            Tier I -> Tier II -> Tier III  => Badge: Chart Captain
   built on: Orchestration III

[OBSERVABILITY]               Tier I -> Tier II -> Tier III  => Badge: Signal Seer
   built on: Packaging II

[ALGORITHMS]   (parallel weekend branch, leveled separately)
   Apprentice -> Journeyman -> Master  => Badge: Pattern Master

[SYSTEM DESIGN]   (the capstone meta-branch)
   unlocks only when ALL 13 technology branches are at Tier III
   => Final Badge: StreamPulse Architect
```

Mark tier unlocks in `StreamPulse-Tracker.md` as you go. The tree is the map; the badges are the proof.

---

## 4. Mastery badges

Fifteen badges. Each technology badge is earned by passing that phase's **Boss Check** — a set of hard questions you must be able to answer _out loud_, plus a **Break-It Challenge** where you deliberately break the system and observe the failure. You don't earn the badge by making it run. You earn it by understanding why it runs, and what happens when it doesn't.

| #   | Badge                     | Branch                   | Earned by                                      |
| --- | ------------------------- | ------------------------ | ---------------------------------------------- |
| 1   | Container Captain         | Containers               | Phase 1 Boss Check                             |
| 2   | Schema Smith              | Relational Data          | Phase 2 Boss Check                             |
| 3   | Edge Warden               | Edge & Networking        | Phase 3 Boss Check                             |
| 4   | Contract Broker           | Service Contracts / gRPC | Phase 4 Boss Check                             |
| 5   | Cache Conjurer            | Caching & Coordination   | Phase 5 Boss Check                             |
| 6   | Stream Lord               | Event Streaming          | Phase 6 Boss Check                             |
| 7   | Realtime Ranger           | Real-Time / WebSockets   | Phase 7 Boss Check                             |
| 8   | Index Oracle              | Search                   | Phase 8 Boss Check                             |
| 9   | Object Keeper             | Object Storage           | Phase 9 Boss Check                             |
| 10  | Wide-Column Warden        | Wide-Column NoSQL        | Phase 10 Boss Check                            |
| 11  | Cluster Commander         | Orchestration            | Phase 11 Boss Check                            |
| 12  | Chart Captain             | Packaging / Helm         | Phase 12 Boss Check                            |
| 13  | Signal Seer               | Observability            | Phase 13 Boss Check                            |
| 14  | Pattern Master            | Algorithms               | Finishing all DSA patterns + a timed mixed set |
| 15  | **StreamPulse Architect** | System Design            | Phase 14 — the final boss                      |

You can send me your Boss Check answers any time and I'll grade them honestly, push on the weak spots, and only then tell you the badge is earned. Self-certifying is allowed, but a graded badge is worth more.

---

## 5. The rules of the game

1. **One phase at a time.** Do not start Phase N+1 until the Phase N Boss Check passes. The system compounds; a shaky foundation collapses three phases later.
2. **Read before you build.** The first checkpoint of every phase is reading. Spend it understanding _what problem this technology solves_ before you write a line of code.
3. **Break things on purpose.** Every Boss Check has a Break-It Challenge. Kill the broker. Disconnect Redis. Fill the partition. Production fails — understanding _how_ it fails is the actual job of an SDE2.
4. **Revisit earlier phases.** Each new phase improves an old one. When you learn Redis, go back and cache `catalog-service`. When you learn Kafka, event-source the video lifecycle. When you learn observability, instrument everything. The platform should be _better_ after every phase, not just bigger.
5. **Write down the why.** Keep a `DECISIONS.md` in the repo. Every non-obvious choice — a partition key, a cache TTL, a consistency level, an index — gets a short entry: what you chose, what you rejected, what breaks if you're wrong. These entries _are_ your interview answers.
6. **Explain it out loud.** Every checkpoint ends with an "Explain it" prompt. Say the answer aloud, as if to an interviewer. If you can't, you haven't finished the checkpoint.
7. **Log every day.** Missing a build day is fine — life happens. Missing the _log entry_ is not. The streak in your tracker is the one number that predicts whether you finish.
8. **Weekdays build. Weekends sharpen.** Project work Monday–Friday, the DSA track and system design theory on weekends. Don't blur them; the context switch is a feature.

---

## 6. The 8GB survival guide

Your machine has ~8GB of RAM. The full StreamPulse stack — Postgres, Redis, Kafka, Elasticsearch, Cassandra, MinIO, eight services, plus a Kubernetes cluster — will not fit in memory at once, and it doesn't need to. Learning happens one slice at a time anyway.

**The slice rule: never run more than one infra "slice" at a time.**

| Slice       | Contains                                                | Run during            |
| ----------- | ------------------------------------------------------- | --------------------- |
| `core`      | Postgres + the 3–4 app services you're touching + Nginx | Phases 1–4, most days |
| `cache`     | Redis + the services using it                           | Phase 5               |
| `messaging` | Kafka (KRaft mode, no Zookeeper) + producers/consumers  | Phase 6               |
| `realtime`  | Redis + chat-service + notification-service             | Phase 7               |
| `search`    | Elasticsearch + search-service                          | Phase 8               |
| `storage`   | MinIO + ingest-service + Nginx                          | Phase 9               |
| `nosql`     | Cassandra (single node) + analytics-service             | Phase 10              |
| `cluster`   | a `kind` Kubernetes cluster (its own memory beast)      | Phases 11–13          |

Practical rules baked into the plan:

- **Use Kafka in KRaft mode.** No Zookeeper means one less JVM eating ~300MB.
- **Single-node everything in dev.** Replication factor 1 by default. You'll _briefly_ bump replication only in the specific checkpoints that teach replication, then drop it back.
- **Set a memory limit on every Compose service.** This is also good practice you'll need for Kubernetes resource limits later — you're learning the discipline early.
- **Tear down between phases.** `make down` is muscle memory. A stopped container still holds its volume; a running one holds RAM.
- **For Kubernetes, use `kind`, not Minikube.** `kind` runs the cluster as containers and is lighter. During Phases 11–13, run infra _subsets_ inside the cluster, not the whole stack.
- **Escape hatch:** if the Kubernetes weeks get genuinely painful, a cheap cloud VM (a 4-vCPU / 8GB box for a few weeks) is a reasonable spend. Not required — `kind` with subsets works — but allowed. Ping me and we'll size it.

A `Makefile` with `make up-<slice>`, `make down`, `make ps` targets is part of Phase 0. You'll thank yourself in week 16.

---

## 7. Timeline at a glance

Twenty-four weeks. Weekday build, weekend DSA. No fixed interview date, so this is the full steady track — if a date appears, ping me and we'll compress by marking a core path.

| Weeks | Phase                                              | Branch                 | Badge                 |
| ----- | -------------------------------------------------- | ---------------------- | --------------------- |
| 1     | Phase 0 — Orientation & repo restructure           | —                      | —                     |
| 1–2   | Phase 1 — Docker & the polyglot skeleton           | Containers             | Container Captain     |
| 3     | Phase 2 — PostgreSQL                               | Relational Data        | Schema Smith          |
| 4     | Phase 3 — Nginx: gateway, load balancer, CDN       | Edge & Networking      | Edge Warden           |
| 5     | Phase 4 — gRPC & service contracts                 | Service Contracts      | Contract Broker       |
| 6–7   | Phase 5 — Redis: caching & coordination            | Caching & Coordination | Cache Conjurer        |
| 8–10  | Phase 6 — Kafka & event sourcing                   | Event Streaming        | Stream Lord           |
| 11–12 | Phase 7 — WebSockets & real-time                   | Real-Time              | Realtime Ranger       |
| 13    | Phase 8 — Elasticsearch & CQRS                     | Search                 | Index Oracle          |
| 14    | Phase 9 — MinIO & HLS delivery                     | Object Storage         | Object Keeper         |
| 15–16 | Phase 10 — Cassandra & analytics                   | Wide-Column NoSQL      | Wide-Column Warden    |
| 17–20 | Phase 11 — Kubernetes                              | Orchestration          | Cluster Commander     |
| 21    | Phase 12 — Helm                                    | Packaging              | Chart Captain         |
| 22–23 | Phase 13 — Observability, integration & CI/CD      | Observability          | Signal Seer           |
| 24    | Phase 14 — System design mastery & mock interviews | System Design          | StreamPulse Architect |

Phase 0 overlaps the first two days of Week 1; Phase 1 starts mid-Week 1.

---

## 8. The phases

Each phase below lists its goal, the topics it covers, its weekday daily checkpoints, the deliverable, and the Boss Check (questions + Break-It Challenge) that earns the badge.

Each daily checkpoint has the same shape:

- **Build** — the concrete thing you make today.
- **Read first** — what to understand before building (this _is_ the work, not a warm-up).
- **Done when** — the objective completion criterion.
- **Explain it** — the out-loud question. Answer it before you log the day.

---

### Phase 0 — Orientation & repo restructure

**Weeks: Week 1, Days 1–2 · Branch: none · Goal: StreamPulse exists as a repo and you can see the whole map before touching a service.**

**Day 1 — Restructure the repo**

- Build: Keep `pi-estimator/` exactly as it is — it's your working Kubernetes + Helm reference and you'll come back to it. Create `streampulse/` with the target folder structure (see Section 9). Write `streampulse/README.md` containing the architecture diagram and the service table from this file.
- Read first: Re-open your own `pi-estimator` Helm charts and RBAC manifests. You'll reuse those exact patterns in Phase 11–12 — this time understanding every line.
- Done when: the new structure is committed; `streampulse/README.md` renders correctly.
- Explain it: "Why keep `pi-estimator` around instead of deleting it now that I have a bigger project?"

**Day 2 — Dev environment & the survival kit**

- Build: Verify your toolchain — Docker, .NET 8 SDK, Python 3.12, `kind`, `helm`, `kubectl`. Create the `Makefile` skeleton with `make up-core`, `make down`, `make ps` targets (stubs for now). Write `streampulse/RUNBOOK.md` containing the slice table from Section 6.
- Read first: Section 6 of this file. Internalise the slice rule.
- Done when: `docker --version`, `dotnet --info`, `python --version`, `kind --version` all succeed; `Makefile` and `RUNBOOK.md` committed.
- Explain it: "Which infra components will I never run simultaneously on this machine, and why?"

_No Boss Check for Phase 0 — it's setup. The game starts at Phase 1._

---

### Phase 1 — Docker & the polyglot skeleton

**Weeks: Week 1 Days 3–5 + Week 2 · Branch: Containers · Goal: three services (two .NET, one Python) plus PostgreSQL, all running in Compose, all healthy.**

Topics: images, layers, build cache, `.dockerignore`, non-root users; multi-stage builds for a _compiled_ runtime (.NET SDK image → ASP.NET runtime image) versus an _interpreted_ one (Python builder installs deps → slim runtime); Compose networking, named volumes, `env_file`, health checks, `depends_on: condition: service_healthy`; image scanning with `trivy`; memory limits on every service.

**Week 1**

- **Day 3 — Scaffold `user-service` (.NET).** Build: an ASP.NET Core minimal API with a `/health` endpoint and a stub `/users`. Single-stage Dockerfile, runs in a container. Read first: Docker docs on images and layers. Done when: `docker run` serves `/health`. Explain it: "What is a layer, and what invalidates the build cache?"
- **Day 4 — Multi-stage build for `user-service`.** Build: convert the Dockerfile to multi-stage (SDK build stage → `aspnet` runtime stage). Add a non-root user. Compare image sizes before/after. Read first: multi-stage build docs. Done when: runtime image is dramatically smaller and runs as non-root. Explain it: "Why is the SDK image a security and size liability in production?"
- **Day 5 — Scaffold `catalog-service` (.NET).** Build: second .NET minimal API, multi-stage Dockerfile from the start, `/health` + stub `/videos`. Add `.dockerignore` to both services. Done when: both images build clean. Explain it: "What does `.dockerignore` actually save me — name two costs of a fat build context?"

**Week 2**

- **Day 1 — Scaffold `ingest-service` (Python/FastAPI).** Build: a FastAPI app, `/health` + stub `/upload`. Multi-stage Dockerfile: a builder stage that installs dependencies into a virtualenv, a slim runtime stage that copies only the venv. Read first: how Python multi-stage differs from compiled-language multi-stage. Done when: the image runs and is slim. Explain it: "There's no compile step in Python — so what is the builder stage even _for_?"
- **Day 2 — `docker-compose.yml`.** Build: wire all three services onto one named network. Done when: `docker compose up` brings all three up and they can resolve each other by name. Explain it: "How does service-name DNS resolution work inside a Compose network?"
- **Day 3 — Add PostgreSQL.** Build: add Postgres to Compose with a named volume, `env_file` for credentials, and a health check. Make the services `depends_on: condition: service_healthy`. Done when: services wait for Postgres to be genuinely ready, not just started. Explain it: "What's the difference between a container being _running_ and being _ready_?"
- **Day 4 — Health checks & memory limits.** Build: a real health check on each service (not just `/health` returning 200 — check the DB connection). Add a `mem_limit` to every Compose service. Done when: `docker stats` shows every container capped. Explain it: "What happens when a container hits its memory limit? What happens when its health check fails?"
- **Day 5 — Scan & Boss Check.** Build: run `trivy` against all three images, triage the findings. Then take the Boss Check. Done when: Container Captain badge earned. Explain it: the Boss Check questions below.

**Deliverable:** `user-service` + `catalog-service` + `ingest-service` + PostgreSQL running in Docker Compose, all multi-stage, all non-root, all health-checked, all memory-capped.

**Boss Check — Container Captain**

- Questions: Why multi-stage builds? What invalidates a layer cache, and how do you order a Dockerfile to maximise cache hits? Why run as non-root? What does `depends_on: condition: service_healthy` actually guarantee — and what does it _not_? How does multi-stage differ for .NET (compiled) versus Python (interpreted)? What happens when a container exceeds `mem_limit`?
- Break-It Challenge: Reorder the `COPY` lines in a Dockerfile so dependency installation happens _after_ copying source code. Rebuild after a one-line source change and watch the cache bust. Then break a health check and watch `depends_on` behaviour.

---

### Phase 2 — PostgreSQL

**Week: Week 3 · Branch: Relational Data · Goal: real, well-designed persistence with migrations, indexes, and transactions you understand.**

Topics: schema design for the streaming domain (`users`, `channels`, `videos`, `video_assets`, `follows`, `playlists`, `playlist_items`) with proper foreign keys and constraints; versioned migrations — EF Core migrations for the .NET services, Alembic for the Python service, and _why_ database-per-service means that's fine; B-tree, partial, and composite indexes; `EXPLAIN ANALYZE` on every query; transaction isolation levels (Read Committed vs Repeatable Read vs Serializable); optimistic vs pessimistic locking; connection pooling and why N services × M threads can starve a database; MVCC.

- **Day 1 — Schema + first migration (`user-service`).** Build: design the schema on paper first, then EF Core entities and the first migration for `users` and `follows` (a many-to-many self-join). Read first: normalisation, foreign keys, the `follows` modelling problem. Done when: migration applies cleanly to a fresh DB. Explain it: "Why model `follows` as its own table instead of an array column?"
- **Day 2 — `catalog-service` schema.** Build: EF Core entities + migration for `channels`, `videos`, `video_assets` (one row per HLS rendition). Foreign keys, `NOT NULL` discipline, check constraints. Explain it: "Where did I use a check constraint, and what bad data does it stop?"
- **Day 3 — Alembic for `ingest-service`.** Build: set up Alembic, write a migration for an `upload_jobs` table (tracks an in-progress upload's state). Explain it: "Two services, two migration tools — when is that fine, and when is it a disaster?"
- **Day 4 — Indexes & `EXPLAIN ANALYZE`.** Build: a seed script with realistic data volume. Write five representative queries (channel's videos newest-first, a user's feed, search-by-title, etc.). Run `EXPLAIN ANALYZE` on each. Add indexes — a composite on `(channel_id, published_at DESC)`, a partial index `WHERE status = 'published'`. Re-run and compare. Read first: how to read a query plan. Done when: you can point at the line in the plan that proves the index is used. Explain it: "When is a partial index the right call over a plain B-tree?"
- **Day 5 — Transactions & Boss Check.** Build: a "publish video" operation that updates `videos` and `video_assets` atomically. Open two `psql` sessions and demonstrate the difference between Read Committed and Serializable on a concurrent update. Then take the Boss Check. Done when: Schema Smith badge earned.

**Deliverable:** all services persist to PostgreSQL via versioned migrations, with indexed queries you can justify from the query plan, and a demonstrated transaction under a chosen isolation level.

**Boss Check — Schema Smith**

- Questions: When would you use a partial index? What is a phantom read, and which isolation level prevents it? Explain MVCC in two sentences. Why does connection pooling exist — what fails without it? Optimistic vs pessimistic locking — give a StreamPulse example of each. What's the N+1 query problem and where would it bite in the followers feed?
- Break-It Challenge: Run two concurrent transactions that update the same video's view count — once at Read Committed, once at Serializable. Observe the lost update in one and the serialization failure in the other.

---

### Phase 3 — Nginx: gateway, load balancer, CDN

**Week: Week 4 · Branch: Edge & Networking · Goal: a single front door for all traffic, plus the CDN pattern that streaming lives or dies on.**

Topics: `nginx.conf` from scratch — `server` blocks, `location` blocks, `proxy_pass`; upstream groups (round-robin, `least_conn`, `ip_hash`); rate limiting with `limit_req_zone`, burst, and 429 responses; structured JSON access logs; `gzip`; upstream timeouts and retries; serving static segments with `Cache-Control: max-age` — the CDN pattern; HTTP range requests, which video seeking depends on.

- **Day 1 — Reverse proxy.** Build: an `nginx.conf` that routes `/api/users`, `/api/catalog`, `/api/ingest` to the three services. Read first: `server` vs `location` blocks, `proxy_pass`. Done when: every service is reachable only through Nginx. Explain it: "What does the gateway give me that direct service access doesn't?"
- **Day 2 — Load balancing.** Build: an upstream group for `catalog-service`; scale it to 2 replicas in Compose. Watch requests distribute. Try `round-robin` then `least_conn`. Done when: you can see both replicas serving traffic in the logs. Explain it: "When would `ip_hash` be the right choice — and what does it cost you?" (Hold that thought; it returns in Phase 7.)
- **Day 3 — Rate limiting.** Build: `limit_req_zone` keyed per IP, with a burst allowance; return 429s past the limit. Test it with a request loop. Explain it: "How does rate limiting at the edge protect a service three hops downstream?"
- **Day 4 — The CDN pattern.** Build: serve a folder of fake HLS segments and an `.m3u8` manifest directly from Nginx with `Cache-Control: max-age=...`; `gzip` the manifest but not the segments; support HTTP range requests. Read first: how HLS works at a high level, why range requests matter. Done when: a range request returns `206 Partial Content`. Explain it: "Why must video segments support range requests, and why would gzipping a segment be pointless?"
- **Day 5 — Logs, timeouts & Boss Check.** Build: structured JSON access logs; upstream timeouts and retries. Then take the Boss Check. Done when: Edge Warden badge earned.

**Deliverable:** an `api-gateway` that routes to all services, rate-limits per IP, load-balances `catalog-service` across two replicas, and serves HLS segments with CDN-style cache headers and range support.

**Boss Check — Edge Warden**

- Questions: What's the difference between a load balancer and an API gateway? What's a thundering herd? How does edge rate limiting protect a downstream service? When is `ip_hash` worth its downsides? Why do video segments need range request support?
- Break-It Challenge: Kill one `catalog-service` replica mid-traffic and watch how Nginx routes around it. Then hammer the rate limit and confirm the 429s — and confirm the _downstream service never sees the excess load_.

---

### Phase 4 — gRPC & service contracts

**Week: Week 5 · Branch: Service Contracts · Goal: typed, fast, contract-first communication across the .NET/Python boundary.**

This phase exists _because_ you went polyglot. REST at the edge is fine for browsers; between services you want a real contract.

Topics: Protobuf — messages, services, field numbers, the evolution rules (never reuse or renumber a field); gRPC — the four call types (unary, server streaming, client streaming, bidirectional); a .NET gRPC server with a Python gRPC client; gRPC vs REST tradeoffs; deadlines and timeouts; the gRPC error model and status codes; interceptors (which foreshadow correlation IDs in Phase 13); why you can't gRPC straight to a browser.

- **Day 1 — Write the contract.** Build: `streampulse.proto` — `Channel` and `Video` messages, a `CatalogService` with `GetVideo` (unary) and `ListChannelVideos` (server streaming). Put it in a shared `proto/` folder that both services compile from. Read first: Protobuf field numbers and the evolution rules. Done when: the proto compiles for both C# and Python. Explain it: "Why does Protobuf identify fields by number instead of by name?"
- **Day 2 — gRPC server in `catalog-service` (.NET).** Build: implement `CatalogService` with `Grpc.AspNetCore`. Done when: a gRPC client can call `GetVideo`. Explain it: "How does HTTP/2 multiplexing make gRPC faster than a REST call per request?"
- **Day 3 — gRPC client in `ingest-service` (Python).** Build: `ingest-service` calls `catalog-service` over gRPC to validate that a channel exists before accepting an upload. Done when: a real cross-language call works. Explain it: "What just happened to the type system across that language boundary?"
- **Day 4 — Deadlines, errors, streaming, interceptors.** Build: add a deadline to the client call; handle a gRPC status error properly; exercise the server-streaming `ListChannelVideos`; add a logging interceptor on both sides. Explain it: "What does a deadline do that a client-side timeout doesn't?"
- **Day 5 — Schema evolution & Boss Check.** Build: add a new field to `Video`, recompile only the server, prove the old client still works. Then deliberately reuse a field number and watch deserialization corrupt. Take the Boss Check. Done when: Contract Broker badge earned.

**Deliverable:** a Protobuf contract shared across languages; `catalog-service` exposes a gRPC API; `ingest-service` consumes it; both have logging interceptors.

**Boss Check — Contract Broker**

- Questions: gRPC vs REST — when each? Why do field numbers matter for compatibility? What are the four streaming types and when would you use server streaming? What does a deadline propagate that a timeout doesn't? Why can't a browser call gRPC directly? What's backward vs forward compatibility in a schema?
- Break-It Challenge: Reuse a retired field number for a new field of a different type. Watch an old client deserialize garbage. This is the lesson that makes you careful forever.

---

### Phase 5 — Redis: caching & coordination

**Weeks: 6–7 · Branch: Caching & Coordination · Goal: every major Redis pattern, including the ones streaming specifically needs.**

_From here, daily checkpoints are lighter. Ping me when you reach this phase and I'll expand the week into full Phase-1-style detail, tuned to where you struggled in Phases 1–4._

**Week 6 — caching fundamentals**

- Day 1 — Data structures and the time complexity of every operation: String, Hash, List, Set, Sorted Set. `SCAN` vs `KEYS` (and why `KEYS` is banned in production).
- Day 2 — TTL, `EXPIRE`, `PERSIST`; pipelining; `MULTI`/`EXEC` transactions.
- Day 3 — Cache-aside on `catalog-service` for video metadata. Measure the hit ratio.
- Day 4 — Write-through on video status; then _reproduce_ a cache stampede deliberately.
- Day 5 — Stampede prevention: a mutex lock on cache miss so only one caller rebuilds the entry.

**Week 7 — coordination & streaming patterns**

- Day 1 — A sliding-window rate limiter built from scratch with a Sorted Set (an app-layer complement to the Nginx edge limiter).
- Day 2 — JWT session store in Redis Hashes with TTL, wired into `user-service`.
- Day 3 — **Live viewer counts**: a Sorted Set per stream, plus a "top live channels" leaderboard via `ZREVRANGE`. The streaming-flavoured checkpoint.
- Day 4 — Redlock for distributed locks — implement it, then understand exactly where and why it's fragile. Use case: prevent a double-publish of the same video.
- Day 5 — Redis Streams vs Pub/Sub vs Kafka (conceptual + a small Pub/Sub demo that previews chat fan-out); Redis Cluster and consistent hashing (conceptual). Boss Check.

**Deliverable:** sessions in Redis; the catalog cached with stampede protection; an app-layer rate limiter; live viewer counts and a leaderboard; double-publish prevented via Redlock.

**Boss Check — Cache Conjurer**

- Questions: Cache-aside vs write-through vs write-behind? What is a cache stampede and how do you prevent it? Why is Redlock controversial? When would you use Redis Streams over Kafka? Why is `KEYS` forbidden? When is Redis the _wrong_ tool? How does a Sorted Set give you a leaderboard in O(log n)?
- Break-It Challenge: Fire concurrent requests at a cold cache key and watch the stampede hit the database; then enable the mutex and watch it collapse to a single rebuild. Then kill Redis entirely and catalogue exactly what degrades.

---

### Phase 6 — Kafka & event sourcing

**Weeks: 8–10 · Branch: Event Streaming · Goal: an async, event-driven core; the video lifecycle re-modelled as event sourcing.**

**Week 8 — fundamentals**

- Day 1 — Topics, partitions, offsets. Create `video-events`, `watch-events`, `channel-events`.
- Day 2 — Producers: `acks=0/1/all`; keys and how they decide partitioning. `catalog-service` produces `video.published`.
- Day 3 — Consumers and consumer groups: one group per service gives you pub/sub; one consumer per group gives you queue semantics.
- Day 4 — At-least-once delivery; build an _idempotent_ consumer using an idempotency key per event.
- Day 5 — Design the event-sourced video lifecycle on paper: `VideoUploaded` → `VideoTranscoded` → `VideoPublished` → `VideoUnlisted`.

**Week 9 — event sourcing**

- Days 1–2 — Implement the event-sourced video lifecycle: events are the source of truth, current state is derived by replaying them.
- Day 3 — Prove it: replay from offset 0 and reconstruct complete current state.
- Day 4 — `notification-service` as a decoupled consumer of `video.published` (notify followers — stub the delivery for now).
- Day 5 — `analytics-service` skeleton consuming `watch-events`.

**Week 10 — advanced**

- Day 1 — Replication factor, ISR (in-sync replicas), leader election.
- Day 2 — Log compaction vs retention — a compacted topic for "latest channel state."
- Day 3 — Exactly-once semantics with Kafka transactions.
- Day 4 — Schema Registry + Avro (or JSON Schema): why event schemas are contracts, exactly like your Protobuf contracts.
- Day 5 — Consumer lag: measure it, understand what it's telling you. Boss Check.

**Deliverable:** the full video lifecycle is event-driven; replaying from offset 0 reconstructs all state; `notification-service` and `analytics-service` are decoupled consumers.

**Boss Check — Stream Lord**

- Questions: At-least-once vs exactly-once — the tradeoff? How do you handle duplicate events? Why is event sourcing useful, and what does it cost? What is consumer lag and why does it matter? How do partition count and consumer group size relate? How would you reprocess history? When is Kafka overkill versus a simple queue?
- Break-It Challenge: Kill a broker mid-produce and watch the producer's behaviour at `acks=1` vs `acks=all`. Then inject a duplicate event and prove your idempotent consumer holds.

---

### Phase 7 — WebSockets & real-time

**Weeks: 11–12 · Branch: Real-Time · Goal: live chat, live notifications, and real-time viewer counts — and the hard part, scaling them.**

**Week 11 — building real-time features**

- Day 1 — The WebSocket protocol: the HTTP upgrade handshake, frames; WebSocket vs Server-Sent Events vs long-polling, and when each is correct.
- Day 2 — `chat-service` (.NET, SignalR): a basic live chat room per stream.
- Day 3 — Chat fan-out via Redis Pub/Sub as the backplane, so multiple `chat-service` instances share messages.
- Day 4 — `notification-service` push: when `video.published` is consumed, push a WebSocket notification to followers who are currently online.
- Day 5 — Live viewer count pushed over WebSockets, backed by the Redis Sorted Set from Phase 5.

**Week 12 — scaling real-time**

- Day 1 — Scaling WebSockets: sticky sessions (this is where `ip_hash` from Phase 3 pays off), the Redis backplane, connection limits.
- Day 2 — Presence: who's online, ping/pong heartbeats, handling abrupt disconnects.
- Day 3 — Backpressure and per-connection rate limiting (chat spam control).
- Day 4 — Auth over WebSockets: token in the handshake, not the query string.
- Day 5 — Load-test the chat with many concurrent connections (a preview of chaos testing). Boss Check.

**Deliverable:** live chat that fans out across instances via Redis; real-time follower notifications; live viewer counts pushed to clients; presence tracking; authenticated WebSocket connections.

**Boss Check — Realtime Ranger**

- Questions: WebSocket vs SSE vs long-polling — when each? Why do you need a backplane? Why sticky sessions, and what do they cost? How do you scale WebSockets to N nodes? How does presence/heartbeat detect a dead connection? How do you secure a WebSocket?
- Break-It Challenge: Kill one `chat-service` node that has active connections; watch clients reconnect and confirm the backplane keeps messages flowing. Then flood a connection with messages and watch backpressure engage.

---

### Phase 8 — Elasticsearch & CQRS

**Week: 13 · Branch: Search · Goal: full-text search and the CQRS pattern.**

- Day 1 — Index design: mappings, analyzers (standard vs edge n-gram for autocomplete), dynamic vs strict mapping.
- Day 2 — `search-service` (Python) consumes `channel-events` and `video.published` from Kafka and indexes into Elasticsearch — no dual writes from the catalog service.
- Day 3 — The search API: `multi_match`, `bool` queries, filters, pagination with `search_after`.
- Day 4 — Relevance scoring (`_score`), fuzzy matching, highlighting, the `search_as_you_type` field.
- Day 5 — CQRS in practice: PostgreSQL is the authoritative write model, Elasticsearch is the read model; reason about the eventual consistency window. Boss Check.

**Deliverable:** `search-service` answers video and channel queries from Elasticsearch; the index stays in sync purely through a Kafka consumer; `catalog-service` never touches Elasticsearch directly.

**Boss Check — Index Oracle**

- Questions: What is CQRS and why is it useful? What happens to your search index if Kafka goes down? How do you reason about eventual consistency between Postgres and Elasticsearch? What is an inverted index? Why not just use SQL `LIKE`?
- Break-It Challenge: Stop the `search-service` Kafka consumer, update a video's title in Postgres, observe the stale search result; restart the consumer and watch it catch up.

---

### Phase 9 — MinIO & HLS delivery

**Week: 14 · Branch: Object Storage · Goal: the S3 API, and real video delivery (lightweight-real: real HLS segments served, transcoding left as a stretch).**

- Day 1 — Run MinIO in Docker; create buckets; configure bucket policies (public-read for thumbnails, private for source assets).
- Day 2 — `ingest-service` uploads pre-made HLS segments and the `.m3u8` manifest to MinIO via `boto3` — using the presigned PUT URL pattern so the client uploads directly, not through the service.
- Day 3 — Presigned GET URLs for time-limited access to private VOD assets.
- Day 4 — Nginx proxies `/hls/*` and `/static/*` to MinIO with `Cache-Control: max-age` — the CDN pattern, now backed by real object storage, with range requests passing through.
- Day 5 — Multipart upload for large files; lifecycle rules to expire unfinished uploads. Boss Check.

**Optional stretch (revisit any time):** real `ffmpeg` transcoding of an upload into multiple HLS renditions. It's a genuine rabbit hole on an 8GB machine — treat it as a bonus phase, not a blocker.

**Deliverable:** video segments and thumbnails stored in MinIO, served through Nginx with cache headers and range support; services use presigned URLs so the app server is never in the upload/download hot path.

**Boss Check — Object Keeper**

- Questions: Why presigned URLs? What's the advantage of clients uploading directly to object storage instead of through your API? How does a CDN work at a high level? When is multipart upload necessary? Why object storage instead of a filesystem or a database BLOB column?
- Break-It Challenge: Serve the same segment with and without `Cache-Control` and compare repeat-load behaviour. Then let a presigned URL expire and watch the 403.

---

### Phase 10 — Cassandra & analytics

**Weeks: 15–16 · Branch: Wide-Column NoSQL · Goal: query-first NoSQL modelling, and the CAP theorem in your hands rather than on a slide.**

**Week 15 — modelling & ingestion**

- Day 1 — The query-first philosophy: list the analytics queries _first_ (views-per-video-per-day, watch-history-per-user, top-videos-per-day).
- Day 2 — Partition key vs clustering key: design a table _per query_. `watch_events` partitioned by `(video_id, day)`.
- Day 3 — `analytics-service` consumes `watch-events` from Kafka and writes to Cassandra — high write throughput is the point.
- Day 4 — A heartbeat telemetry table for watch-progress pings: extremely write-heavy, the firehose.
- Day 5 — Query the tables: views-by-user and views-by-day answered entirely from Cassandra, never touching Postgres.

**Week 16 — consistency & failure**

- Day 1 — Replication factor; consistency levels `ONE` vs `QUORUM`.
- Day 2 — CAP hands-on: take down a Cassandra node in Docker, query at `ONE` vs `QUORUM`, observe AP versus CP behaviour live.
- Day 3 — Tombstones and compaction: why deletes are expensive in an LSM-tree store.
- Day 4 — Hotspot partitions: deliberately pick a bad partition key (too high or too low cardinality) and watch what happens.
- Day 5 — Boss Check.

**Deliverable:** `analytics-service` writes all watch events to Cassandra; you can answer views-by-user and views-by-day from Cassandra without touching PostgreSQL.

**Boss Check — Wide-Column Warden**

- Questions: When would you choose Cassandra over PostgreSQL? What happens with a too-high-cardinality partition key? A too-low one? Why are deletes expensive in Cassandra? Explain CAP with a concrete StreamPulse example. With RF=3, what `R`/`W` consistency levels give you strong consistency, and why?
- Break-It Challenge: With RF=2, kill a node and query at `QUORUM` — watch it fail. Repeat with RF=3 and watch it survive. Then create a hotspot partition and observe the imbalance.

---

### Phase 11 — Kubernetes

**Weeks: 17–20 · Branch: Orchestration · Goal: the whole platform on a local cluster, with production-grade orchestration you understand line by line.**

Use `kind`, not Minikube (lighter on 8GB). Run infra subsets, never the whole stack at once.

**Week 17 — core objects**

- Day 1 — `kind` cluster up; Pod and Deployment basics; deploy `user-service`.
- Day 2 — Service types (ClusterIP / NodePort / LoadBalancer); in-cluster DNS (`svc.namespace.svc.cluster.local`).
- Day 3 — ConfigMap, Secret, Namespace.
- Day 4 — `kubectl` fluency: `describe`, `logs`, `exec`, `port-forward`, `rollout`.
- Day 5 — Deploy all stateless app services to the cluster.

**Week 18 — stateful workloads & access control**

- Day 1 — StatefulSet: deploy Redis as a StatefulSet (stable network identity).
- Day 2 — PersistentVolume, PersistentVolumeClaim, StorageClass.
- Day 3 — Deploy PostgreSQL as a StatefulSet with a PVC.
- Day 4 — Jobs and CronJobs — revisit the `pi-estimator` pattern; now understand it fully. A migration Job.
- Day 5 — RBAC: ServiceAccount, Role, RoleBinding. You built this for `pi-estimator` — now understand every line.

**Week 19 — production patterns**

- Day 1 — Resource requests vs limits — why a missing limit can starve a node (you've been practising this discipline since Phase 1).
- Day 2 — Liveness vs readiness probes — wire them; then demonstrate how a wrong liveness probe kills a healthy service under load.
- Day 3 — Ingress with the Nginx ingress controller — replace NodePort with a real Ingress resource.
- Day 4 — HorizontalPodAutoscaler — autoscale `catalog-service` on CPU; watch it scale under load.
- Day 5 — NetworkPolicy — restrict which services may talk to which.

**Week 20 — bringing it together**

- Day 1 — PodDisruptionBudget; rolling update strategy.
- Days 2–3 — Bring the heavy infra into the cluster _in subsets_ (Kafka, then Elasticsearch, then Cassandra — never together).
- Day 4 — Full-platform health pass.
- Day 5 — Boss Check.

**Deliverable:** all eight services plus infra (in subsets) running on a local `kind` cluster; `catalog-service` autoscales under load.

**Boss Check — Cluster Commander**

- Questions: Difference between liveness and readiness probes? What happens when a pod exceeds its memory limit? How does Kubernetes service discovery work? Why a StatefulSet instead of a Deployment? Requests vs limits — what does each control? What does HPA need in order to function?
- Break-It Challenge: Set a liveness probe that fails under load and watch the crash-loop. Then remove a memory limit from one pod and watch it pressure the whole node.

---

### Phase 12 — Helm

**Week: 21 · Branch: Packaging · Goal: package the platform so it deploys and upgrades in one command.**

- Day 1 — A chart per service: `deployment.yaml`, `service.yaml`, `ingress.yaml`, `hpa.yaml`, `configmap.yaml` templates.
- Day 2 — `values.yaml` per environment — `values-dev.yaml`, `values-prod.yaml` — overriding image tags, replicas, resource limits.
- Day 3 — Helm hooks: a `pre-install` hook that runs the migration Job before the app starts.
- Day 4 — The umbrella chart: `charts/streampulse` depends on every service chart.
- Day 5 — `helm diff`, `helm rollback`, semantic chart versioning. Boss Check.

**Deliverable:** `helm install streampulse ./charts/streampulse -f values-dev.yaml` deploys the entire stack in the right order; a rolling upgrade is a one-liner.

**Boss Check — Chart Captain**

- Questions: Helm versus raw manifests — what does Helm add? When does a Helm hook fail, and what happens to the release? How do you manage secrets in Helm? How does an umbrella chart control deploy ordering?
- Break-It Challenge: Make a `pre-install` hook fail and watch the install roll back. Then `helm rollback` a bad release and confirm recovery.

---

### Phase 13 — Observability, integration & CI/CD

**Weeks: 22–23 · Branch: Observability · Goal: make the system debuggable in production and shipping automatically.**

**Week 22 — observability**

- Day 1 — Prometheus scraping all services plus exporters (Redis, Kafka JMX, Postgres).
- Day 2 — Grafana dashboards: request rate per service, p99 latency, Kafka consumer lag per group, Redis cache hit ratio, active DB connections, live WebSocket connections.
- Day 3 — Structured JSON logging everywhere; a `correlation_id` propagated across `user-service` → `catalog-service` → Kafka → `analytics-service`, including through your gRPC interceptors from Phase 4.
- Day 4 — Distributed tracing: the correlation ID as a poor man's trace; optionally wire OpenTelemetry.
- Day 5 — Alerting rules: consumer lag too high, p99 too high.

**Week 23 — integration & CI/CD**

- Day 1 — GitHub Actions: lint + unit tests for both the .NET and Python services.
- Day 2 — CI: `docker build` and push images.
- Day 3 — CD: `helm upgrade` on merge to main.
- Day 4 — An end-to-end smoke test: upload a video → `video.published` emitted → search index updated → a watch event recorded → an analytics row in Cassandra.
- Day 5 — Full integration pass. Boss Check.

**Deliverable:** StreamPulse deploys via Helm, is fully observable in Grafana, and ships automatically via GitHub Actions on merge to main.

**Boss Check — Signal Seer**

- Questions: What should you alert on, and what should you not? How does a correlation ID propagate across an async Kafka boundary? Why p99 instead of average latency? What does rising consumer lag actually tell you? CI vs CD — where's the line?
- Break-It Challenge: Induce consumer lag (slow a consumer, or burst the producer) and watch the dashboard react and the alert fire.

---

### Phase 14 — System design mastery & mock interviews

**Week: 24 · Branch: System Design · Goal: convert everything you built into interview performance. The final boss.**

- Day 1 — Back-of-envelope estimation: StreamPulse at scale — 1M daily active users, concurrent streams, segment sizes, bandwidth, storage growth per day. Get fluent with the numbers.
- Day 2 — Whiteboard StreamPulse end to end from scratch in 45 minutes, out loud, recorded. Watch the recording. Note every place you hesitated.
- Day 3 — Design a _different_ system reusing your primitives — a distributed rate limiter, a URL shortener, or a notification system. Prove the knowledge transfers.
- Day 4 — Design "YouTube / Twitch." You have literally built a small one — now articulate the scaling story, the bottlenecks, the tradeoffs, fluently.
- Day 5 — A full mock interview: one system design round plus two DSA problems, timed. **Final Boss.** Earns the StreamPulse Architect badge.

**Deliverable:** you can whiteboard StreamPulse and one unseen system cold, with back-of-envelope numbers and defended tradeoffs.

**Boss Check — StreamPulse Architect (the final boss)**

- This one you take with me. Send me a request for the final mock and I'll run you through a real system design round plus DSA, grade it honestly, and tell you where you'd pass and where you'd wobble. That conversation is the badge.

---

## 9. Target repo structure

```
Practice/
├── pi-estimator/              <- keep as-is: your K8s + Helm reference
└── streampulse/
    ├── README.md
    ├── RUNBOOK.md             <- the 8GB slice table, how to run each slice
    ├── DECISIONS.md           <- every non-obvious choice + the tradeoff (Rule 5)
    ├── Makefile               <- make up-<slice>, make down, make ps
    ├── docker-compose.yml     <- local dev, sliced by profile
    ├── docker-compose.test.yml
    ├── proto/                 <- shared Protobuf contracts (Phase 4)
    │   └── streampulse.proto
    ├── services/
    │   ├── user-service/          (.NET)
    │   ├── catalog-service/       (.NET)
    │   ├── notification-service/  (.NET)
    │   ├── chat-service/          (.NET)
    │   ├── ingest-service/        (Python)
    │   ├── search-service/        (Python)
    │   ├── recommendation-service/(Python)
    │   └── analytics-service/     (Python)
    ├── infra/
    │   ├── nginx/nginx.conf
    │   ├── kafka/
    │   └── monitoring/
    │       ├── prometheus.yml
    │       └── grafana/
    ├── k8s/                   <- raw manifests (learning aid, pre-Helm)
    └── charts/                <- Helm
        ├── streampulse/       <- umbrella chart
        └── services/
```

---

## 10. The parallel DSA track — weekends

You're at a decent level — mediums are doable — so this track is about **consistency, pattern fluency, and timed reps**, not relearning from zero.

**Cadence:** Saturday and Sunday. Saturday is _learn/refresh the pattern_ + 2–3 problems worked carefully. Sunday is 3–4 problems, timed, then review. Target ~5–7 problems per weekend, ~140 across the 24 weeks.

**Language:** pick one and stay with it for every problem. Python is the fast choice for interview reps and it's already in your stack; C# is equally fine if that's your interview language. Consistency matters more than the choice.

**The pattern curriculum, mapped to the build weeks:**

| Weeks | Pattern                                                 | Why it's here now                               |
| ----- | ------------------------------------------------------- | ----------------------------------------------- |
| 1–2   | Arrays, two pointers, sliding window                    | The bedrock; warms up alongside Docker/Postgres |
| 3–4   | Hashing, prefix sums                                    | Pairs naturally with thinking about indexes     |
| 5–6   | Strings, stacks & queues                                | —                                               |
| 7–8   | Linked lists, fast/slow pointers                        | —                                               |
| 9–10  | Binary search (including "binary search on the answer") | —                                               |
| 11–12 | Trees, BSTs, traversals                                 | —                                               |
| 13–14 | Heaps / priority queues, top-K                          | Pairs with Elasticsearch relevance ranking      |
| 15–16 | Graphs: BFS, DFS, topological sort                      | —                                               |
| 17–18 | Graphs advanced: Dijkstra, Union-Find                   | Pairs with Kubernetes scheduling intuition      |
| 19–20 | Dynamic programming I: 1D, knapsack                     | —                                               |
| 21–22 | Dynamic programming II: 2D, intervals                   | —                                               |
| 23–24 | Backtracking, greedy, mixed timed sets                  | Interview-condition reps before the final mock  |

**DSA branch tiers:**

- **Apprentice** — finished patterns through week 8, can solve their mediums untimed.
- **Journeyman** — finished through week 16, solving mediums under ~25 minutes.
- **Master** — finished all patterns, clearing a mixed timed set at interview pace. Earns the **Pattern Master** badge.

For any weekend, you can send me the pattern and I'll hand you a curated problem set at the right difficulty, walk through the ones you miss, and quiz you on the underlying idea.

---

## 11. System design theory track — weekends

Alongside DSA, spend ~30 minutes each Sunday on one system design concept. By Phase 14 you'll have covered the whole canon, and most concepts will land _right when the build phase makes them concrete_.

| Weeks | Concept                                                                                                      | Lands alongside        |
| ----- | ------------------------------------------------------------------------------------------------------------ | ---------------------- |
| 1–2   | The interview framework: requirements → estimation → API → data model → high-level → deep dive → bottlenecks | —                      |
| 3–4   | Back-of-envelope estimation; latency numbers every engineer should know                                      | Postgres               |
| 5–6   | Caching strategies, eviction policies, cache invalidation                                                    | Redis                  |
| 7–8   | Load balancing strategies; the API gateway pattern                                                           | Redis / recap Nginx    |
| 9–10  | Message queues vs event streams; idempotency; exactly-once                                                   | Kafka                  |
| 11–12 | Consistency models; the CAP theorem; consistent hashing                                                      | Kafka / WebSockets     |
| 13–14 | Search systems and the inverted index; CQRS                                                                  | Elasticsearch          |
| 15–16 | CDNs; blob storage; the read-heavy vs write-heavy split                                                      | MinIO / Cassandra      |
| 17–18 | Database replication, sharding, partitioning; CDC                                                            | Cassandra / Kubernetes |
| 19–20 | Leader election and consensus (Raft/Paxos, intro depth); distributed transactions and sagas                  | Kubernetes             |
| 21–22 | Observability: SLI/SLO/SLA; circuit breakers; failure modes                                                  | Helm / Observability   |
| 23–24 | Putting it together: designing for scale; rate limiting algorithms; full mock prep                           | Phase 14               |

---

## 12. Interview cheat sheet — what an SDE2 is expected to know

- **Caching:** cache-aside vs write-through vs write-behind; stampede and its prevention; TTL strategy; what you must never cache.
- **Kafka / streaming:** at-least-once vs exactly-once; consumer lag; partition count vs consumer group sizing; when a stream, when a simple queue.
- **PostgreSQL:** reading a query plan; index types; the N+1 problem; connection pooling; when to denormalise; MVCC and isolation levels.
- **Redis:** picking the right data structure; Redlock's tradeoffs; when Redis is the wrong tool (durability requirements).
- **gRPC / contracts:** gRPC vs REST; schema evolution rules; deadlines; why browsers can't speak gRPC.
- **WebSockets / real-time:** WebSocket vs SSE vs polling; the backplane; sticky sessions; presence and heartbeats.
- **Elasticsearch:** the inverted index; why not `LIKE`; eventual consistency between write and read models; CQRS.
- **Cassandra:** why query-first; hotspot partitions; tombstone accumulation; CAP in practice; consistency-level math.
- **Object storage / CDN:** presigned URLs; direct-to-storage uploads; how a CDN works; range requests.
- **Kubernetes:** liveness vs readiness; requests vs limits; rolling update strategy; why a StatefulSet; service discovery.
- **System design fundamentals:** consistent hashing; CAP; back-of-envelope estimation; idempotency; distributed transactions (and why to avoid them); the saga pattern.

Every one of these has a checkpoint behind it where you _did the thing_. When an interviewer asks, you're not reciting — you're remembering.

---

## 13. Mock interview question bank

Use these from Phase 6 onward, and intensively in Phase 14. Ping me to run any of them as a live, timed mock.

**System design**

- Design StreamPulse (your own system — the warm-up).
- Design YouTube / a video streaming platform.
- Design Twitch / a live streaming platform with chat.
- Design a distributed rate limiter.
- Design a notification / fan-out service.
- Design a URL shortener at scale.
- Design a real-time analytics pipeline.
- Design a distributed cache.

**Deep-dive follow-ups (the SDE2 differentiator)**

- Your search index and primary DB disagree — walk me through why, and how you'd detect and bound it.
- A Kafka consumer group is lagging badly — diagnose it live.
- Your p99 latency tripled overnight, p50 is unchanged — where do you look?
- A single Cassandra partition is getting hammered — what happened and how do you fix it without downtime?
- You need to add a required field to an event schema — how, without breaking live consumers?

**Behavioural / experience (from this very project)**

- Tell me about a hard technical tradeoff you made. (Your `DECISIONS.md` is the source.)
- Tell me about a time you debugged a production-like failure. (Every Break-It Challenge is a story.)
- Why polyglot? Defend the .NET/Python split.

---

## 14. How to use your mentor

The roadmap runs solo, but it runs better with me. On demand, ask me to:

- **Expand a phase** — when you reach Phase 5+, ask me to break the week into full Phase-1-style daily detail, tuned to where you actually struggled.
- **Grade a Boss Check** — send your answers, I'll push on the weak spots before I confirm the badge.
- **Run a mock interview** — system design or DSA, timed, with honest feedback.
- **Curate a DSA set** — tell me the weekend's pattern, get a difficulty-matched problem set and walkthroughs of misses.
- **Unblock you** — stuck on a Kafka rebalance, a Cassandra timeout, a Helm hook? Bring me the error.
- **Review your `DECISIONS.md`** — I'll pressure-test your reasoning the way an interviewer would.
- **Re-plan** — if an interview date appears, or the pace is wrong, or a phase is dragging — we adjust. The plan serves you, not the reverse.

Log your work in `StreamPulse-Tracker.md`. Start tomorrow with Phase 0, Day 1. The first badge is two weeks out.

Let's build.
