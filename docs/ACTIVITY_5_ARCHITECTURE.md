\# Activity 5 Architecture



\## Component flow



```mermaid

flowchart LR

&#x20;   View\[Avalonia views] --> VM\[Desktop view models]

&#x20;   VM --> Client\[BorrowingApiClient - HttpClient]

&#x20;   Client --> API\[ASP.NET Core API controllers and contracts]

&#x20;   API --> App\[Application services]

&#x20;   App --> Repo\[Repository interfaces]

&#x20;   Repo --> Infra\[EF Core and Npgsql infrastructure]

&#x20;   Infra --> DB\[(PostgreSQL)]
