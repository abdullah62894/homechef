# Development stages

HomeChef is built in clearly separated stages. Each stage ships, builds, and is
tested before the next begins. Future-stage features are never implemented
ahead of their stage unless required as architectural foundations.

| Stage | Scope | Status |
| ----- | ----- | ------ |
| 0 | Architecture & repository setup — scaffold, DB wiring, health endpoint | Done |
| 1 | Users and authentication — Identity, roles, JWT cookie auth | Done |
| 2 | Chef profiles | Planned |
| 3 | Food / menu system | Planned |
| 4 | Search and locations (PostGIS) | Planned |
| 5 | Reviews and ratings | Planned |
| 6 | Favorites | Planned |
| 7 | Contact chef | Planned |
| 8 | Image storage and optimization | Planned |
| 9 | Admin and moderation | Planned |
| 10 | Reporting and abuse prevention | Planned |
| 11 | Notifications | Planned |
| 12 | Redis and performance optimization | Planned |
| 13 | SEO and discovery | Planned |
| 14 | Production deployment | Planned |
| 15 | Observability | Planned |
| 16 | Scaling | Planned |

## Stage execution rules

When a stage starts:

1. Inspect the current repository and existing architecture.
2. Do not unnecessarily rewrite working code.
3. Identify dependencies between the stage and existing code.
4. Implement the smallest clean solution.
5. Build and test the affected projects.
6. Explain what changed and how to run/test the stage.