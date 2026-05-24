# StreamPulse — Progress Tracker

_Your logbook. The roadmap (`StreamPulse-Roadmap.md`) tells you what to do; this file records that you did it. Rule 7: missing a build day is fine, missing the log entry is not._

---

## Current status

|                            |                                                                                                         |
| -------------------------- | ------------------------------------------------------------------------------------------------------- |
| Start date                 | 19/05/26                                                                                                |
| Current phase              | Phase 2 — PostgreSQL                                                                                    |
| Current week               | Week 3                                                                                                  |
| Badges earned              | 1 / 15                                                                                                  |
| Skill tree tiers unlocked  | 3 / 42 (13 tech branches × 3 tiers, + 3 DSA tiers)                                                      |
| Build-day streak (current) | 12                                                                                                      |
| Build-day streak (best)    | 12                                                                                                      |
| DSA problems solved        | 0                                                                                                       |
| Title                      | _Initiate_ — earn 5 badges for _Builder_, 10 for _Systems Engineer_, all 15 for _StreamPulse Architect_ |

Update this block whenever something changes. The streak is the number that predicts whether you finish — protect it.

---

## Mastery badges

| #   | Badge                 | Branch                   | Phase     | Earned? | Date    | Graded by mentor? |
| --- | --------------------- | ------------------------ | --------- | ------- | ------- | ----------------- |
| 1   | Container Captain     | Containers               | 1         | [x]     | 22/5/26 | [x]               |
| 2   | Schema Smith          | Relational Data          | 2         | [ ]     |         | [ ]               |
| 3   | Edge Warden           | Edge & Networking        | 3         | [ ]     |         | [ ]               |
| 4   | Contract Broker       | Service Contracts / gRPC | 4         | [ ]     |         | [ ]               |
| 5   | Cache Conjurer        | Caching & Coordination   | 5         | [ ]     |         | [ ]               |
| 6   | Stream Lord           | Event Streaming          | 6         | [ ]     |         | [ ]               |
| 7   | Realtime Ranger       | Real-Time / WebSockets   | 7         | [ ]     |         | [ ]               |
| 8   | Index Oracle          | Search                   | 8         | [ ]     |         | [ ]               |
| 9   | Object Keeper         | Object Storage           | 9         | [ ]     |         | [ ]               |
| 10  | Wide-Column Warden    | Wide-Column NoSQL        | 10        | [ ]     |         | [ ]               |
| 11  | Cluster Commander     | Orchestration            | 11        | [ ]     |         | [ ]               |
| 12  | Chart Captain         | Packaging / Helm         | 12        | [ ]     |         | [ ]               |
| 13  | Signal Seer           | Observability            | 13        | [ ]     |         | [ ]               |
| 14  | Pattern Master        | Algorithms               | DSA track | [ ]     |         | [ ]               |
| 15  | StreamPulse Architect | System Design            | 14        | [ ]     |         | [ ]               |

---

## Skill tree progress

Mark a tier `[x]` when its checkpoints are done. A branch's badge is earned at Tier III + Boss Check.

| Branch                     | Tier I — Foundations                             | Tier II — Practitioner | Tier III — Master |
| -------------------------- | ------------------------------------------------ | ---------------------- | ----------------- |
| Containers                 | [x]                                              | [x]                    | [x]               |
| Relational Data            | [ ]                                              | [ ]                    | [ ]               |
| Edge & Networking          | [ ]                                              | [ ]                    | [ ]               |
| Service Contracts / gRPC   | [ ]                                              | [ ]                    | [ ]               |
| Caching & Coordination     | [ ]                                              | [ ]                    | [ ]               |
| Event Streaming            | [ ]                                              | [ ]                    | [ ]               |
| Real-Time / WebSockets     | [ ]                                              | [ ]                    | [ ]               |
| Search                     | [ ]                                              | [ ]                    | [ ]               |
| Object Storage             | [ ]                                              | [ ]                    | [ ]               |
| Wide-Column NoSQL          | [ ]                                              | [ ]                    | [ ]               |
| Orchestration              | [ ]                                              | [ ]                    | [ ]               |
| Packaging / Helm           | [ ]                                              | [ ]                    | [ ]               |
| Observability              | [ ]                                              | [ ]                    | [ ]               |
| Algorithms                 | [ ] Apprentice                                   | [ ] Journeyman         | [ ] Master        |
| System Design _(capstone)_ | unlocks when all 13 tech branches hit Tier III → |                        | [ ] Architect     |

---

## Daily build log (weekdays)

One row per build day. "Explained?" means you said the checkpoint's Explain-it answer out loud.

| Date | Phase | Day  | Checkpoint built                  | Done? | Explained? | Streak | Notes / what was hard |
| ---- | ----- | ---- | --------------------------------- | ----- | ---------- | ------ | --------------------- |
|      | 0     | 1    | Restructure repo                  | [x]   | [x]        | 1      |                       |
|      | 0     | 2    | Dev env + survival kit            | [x]   | [x]        | 2      |                       |
|      | 1     | 3    | Scaffold user-service (.NET)      | [x]   | [x]        | 3      |                       |
|      | 1     | 4    | Multi-stage build, user-service   | [x]   | [x]        | 4      |                       |
|      | 1     | 5    | Scaffold catalog-service (.NET)   | [x]   | [x]        | 5      |                       |
|      | 1     | W2D1 | Scaffold ingest-service (Python)  | [x]   | [x]        | 6      |                       |
|      | 1     | W2D2 | docker-compose.yml                | [x]   | [x]        | 7      |                       |
|      | 1     | W2D3 | Add PostgreSQL                    | [x]   | [x]        | 8      |                       |
|      | 1     | W2D4 | Health checks + memory limits     | [x]   | [x]        | 9      |                       |
|      | 1     | W2D5 | trivy scan + Boss Check           | [x]   | [x]        | 10     |                       |
|      | 2     | W3D1 | user-service schema and migration | [x]   | [x]        | 11     |                       |
|      | 2     | W3D2 | catalog schema and migration      | [x]   | [x]        | 12     |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |
|      |       |      |                                   | [ ]   | [ ]        |        |                       |

_Copy the blank row as you go. From Phase 5 on, ask the mentor to expand the week into daily detail and add those rows here._

---

## Weekend DSA log

| Date | Week | Pattern                                | Problems solved | Timed? | Misses to review | Notes |
| ---- | ---- | -------------------------------------- | --------------- | ------ | ---------------- | ----- |
|      | 1    | Arrays / two pointers / sliding window |                 | [ ]    |                  |       |
|      | 1    | Arrays / two pointers / sliding window |                 | [ ]    |                  |       |
|      | 2    | Arrays / two pointers / sliding window |                 | [ ]    |                  |       |
|      | 2    | Arrays / two pointers / sliding window |                 | [ ]    |                  |       |
|      |      |                                        |                 | [ ]    |                  |       |

_Running total of problems solved: \_\_\_\_ / ~140 target._

---

## Boss check log

Record every Boss Check attempt — including the ones you don't pass first time. Failing a Boss Check is data, not shame.

| Phase | Badge                 | Date attempted | Passed? | Weak spots found | Re-attempt date |
| ----- | --------------------- | -------------- | ------- | ---------------- | --------------- |
| 1     | Container Captain     | 22/05/26       | [x]     |                  |                 |
| 2     | Schema Smith          |                | [ ]     |                  |                 |
| 3     | Edge Warden           |                | [ ]     |                  |                 |
| 4     | Contract Broker       |                | [ ]     |                  |                 |
| 5     | Cache Conjurer        |                | [ ]     |                  |                 |
| 6     | Stream Lord           |                | [ ]     |                  |                 |
| 7     | Realtime Ranger       |                | [ ]     |                  |                 |
| 8     | Index Oracle          |                | [ ]     |                  |                 |
| 9     | Object Keeper         |                | [ ]     |                  |                 |
| 10    | Wide-Column Warden    |                | [ ]     |                  |                 |
| 11    | Cluster Commander     |                | [ ]     |                  |                 |
| 12    | Chart Captain         |                | [ ]     |                  |                 |
| 13    | Signal Seer           |                | [ ]     |                  |                 |
| 14    | StreamPulse Architect |                | [ ]     |                  |                 |

---

## Weekly reflection log

Five minutes at the end of each week. This is where the learning consolidates.

| Week | Phase | What clicked this week | What's still shaky | Adjust the plan? |
| ---- | ----- | ---------------------- | ------------------ | ---------------- |
| 1    | 0–1   |                        |                    |                  |
| 2    | 1     |                        |                    |                  |
| 3    | 2     |                        |                    |                  |
| 4    | 3     |                        |                    |                  |
| 5    | 4     |                        |                    |                  |
| 6    | 5     |                        |                    |                  |
| 7    | 5     |                        |                    |                  |
| 8    | 6     |                        |                    |                  |
| 9    | 6     |                        |                    |                  |
| 10   | 6     |                        |                    |                  |
| 11   | 7     |                        |                    |                  |
| 12   | 7     |                        |                    |                  |
| 13   | 8     |                        |                    |                  |
| 14   | 9     |                        |                    |                  |
| 15   | 10    |                        |                    |                  |
| 16   | 10    |                        |                    |                  |
| 17   | 11    |                        |                    |                  |
| 18   | 11    |                        |                    |                  |
| 19   | 11    |                        |                    |                  |
| 20   | 11    |                        |                    |                  |
| 21   | 12    |                        |                    |                  |
| 22   | 13    |                        |                    |                  |
| 23   | 13    |                        |                    |                  |
| 24   | 14    |                        |                    |                  |

---

## Mentor check-in prompts

When you want to use your mentor (on-demand), these are the asks that work well — copy one into chat:

- "Expand Phase _ Week _ into full daily checkpoints, tuned to what I struggled with in Phase \_."
- "Grade my Boss Check for Phase \_ — here are my answers: ..."
- "Run me a timed mock interview: [system design prompt] from the question bank."
- "Curate a DSA set for [pattern] at my level, then walk me through the ones I miss."
- "I'm stuck — here's the error and what I've tried: ..."
- "Pressure-test my DECISIONS.md entry for [decision]."
- "An interview got scheduled for [date] — re-plan and mark a core path."
