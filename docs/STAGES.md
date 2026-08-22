# Development stages

HomeChef is built in clearly separated stages. Each stage ships, builds, and is
tested before the next begins. Future-stage features are never implemented
ahead of their stage unless required as architectural foundations.

| Stage | Scope | Status |
| ----- | ----- | ------ |
| 0 | Architecture & repository setup — scaffold, DB wiring, health endpoint | Done |
| 1 | Users and authentication — Identity, roles, JWT cookie auth | Done |
| 2 | Chef profiles | Done |
| 3 | Food / menu system | Done |
| 4 | Search and locations — keyword search, city/area directory, proximity | Done |
| 5 | Reviews and ratings — 1–5 stars, text feedback, summaries, ownership rules | Done |
| 6 | Favorites — save favorite chefs and dishes, quick toggles, user favorites page | Done |
| 7 | Contact chef — customers message chefs, chef inbox with unread counts and read receipts | Done |
| 8 | Image storage and optimization — chef photos and dish images, WebP re-encode, thumbnails | Done |
| 9 | Admin and moderation — admin seeding, account suspension, review/dish/kitchen moderation | Done |
| 10 | Reporting and abuse prevention — content reports, admin report queue, blocklist, message/report rate limits | Done |
| 11 | Notifications — in-app notifications for new messages and reviews, unread counts, mark read | Done |
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