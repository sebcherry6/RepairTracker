# RepairTracker by WebCherry: build plan

A repair tracker for small landlords, tenants and contractors. Built to practise ASP.NET Core MVC and Web API, EF Core, SQL Server, authorisation, REST design and testing.

How we work: Sebastian writes the code for each milestone on its own branch and opens a pull request; Claude reviews it and helps when stuck. A milestone is done when its acceptance criteria hold and CI is green.

## Milestones

### M1: Skeleton (Size: M)
- Acceptance criteria:
  - Solution with `RepairTracker.Domain`, `RepairTracker.Web`, `RepairTracker.Tests`; Domain references nothing
  - Register, sign in, sign out; the roles Landlord, Tenant and Contractor exist; a new sign-up is a Landlord
  - CI builds and runs tests against SQL Server on every push
- Verification: `dotnet test` locally and a green CI run on the pull request
- Tests: anonymous user is redirected to sign-in; a new registration gets the Landlord role
- Blocked by: none

### M2: Properties (Size: S)
- Acceptance criteria: a landlord can add, edit and list properties; they only ever see their own
- Tests: landlord B gets 404 for landlord A's property
- Blocked by: M1

### M3: Invitations and tenancies (Size: M)
- Acceptance criteria: a landlord creates a single-use, expiring invite for a property; a tenant signs up from the link and gets a tenancy
- Tests: expired or used invite is rejected; the invite gives the Tenant role, never Landlord
- Blocked by: M2

### M4: Reporting issues (Size: M)
- Acceptance criteria: a tenant reports an issue on their property; the landlord sees it; access rules live in a resource-based authorisation handler
- Tests: a tenant cannot see or report on another property; a former tenant loses access when the tenancy ends
- Blocked by: M3

### M5: Issue workflow (Size: S)
- Acceptance criteria: statuses Reported, Assigned, In progress, Resolved, Closed; a domain class allows only valid transitions and records each one as an IssueEvent
- Tests: unit tests for every allowed and forbidden transition (no database needed)
- Blocked by: M4

### M6: Contractors (Size: M)
- Acceptance criteria: a landlord invites contractors, assigns them to issues; a contractor sees only issues assigned to them
- Tests: a contractor cannot see an unassigned issue or another landlord's issue
- Blocked by: M5

### M7: Comments and timeline (Size: S)
- Acceptance criteria: everyone involved in an issue can comment; the issue page shows one timeline of comments and status changes
- Blocked by: M6

### M8: Contractor API (Size: M)
- Acceptance criteria: token sign-in; list, view and update assigned jobs; paging, filtering, ProblemDetails errors; OpenAPI description
- Tests: API integration tests for 401, 403/404 and validation errors
- Blocked by: M6

### M9: Photo uploads (Size: S)
- Acceptance criteria: photos on issues, with file type and size checks, served only to people who can see the issue
- Blocked by: M4

### M10: Reports and deployment (Size: M)
- Acceptance criteria: average time to fix per contractor; open issues by age; deployed to Azure with Azure SQL
- Blocked by: M8, M9

## Decision log

| Decision | Chosen option | Why |
| --- | --- | --- |
| Who writes the code | Sebastian, with reviews | The project is for practice |
| Database | SQL Server (LocalDB locally, Azure SQL deployed) | What most UK .NET roles use |
| Project structure | Domain, Web, Tests | Shows separation of concerns without ceremony |
| UI split | MVC for landlords and tenants, REST API for contractors | Covers both MVC and Web API with a reason for each |
| Tenancy model | Each landlord has a private portfolio | Gives authorisation real work to do |
| Joining | Invitation links, no email in v1 | Avoids needing an email provider early |
| Sign-up role | Open sign-up creates a Landlord; Tenants and Contractors only join by invite | Nobody can self-assign a role |
| CI database | SQL Server service container in GitHub Actions; LocalDB locally | No Docker needed on the dev machine |

## Not yet specified

- M8: token approach (Identity bearer tokens as in FinanceTracker, or JWT) is decided at that milestone
- M9: where photos are stored in production (local volume or Azure Blob Storage) is decided at that milestone
- M10: fog; researched at its pickup

## Always / Never

- Always: every query scoped to what the signed-in user may see; tests for every access rule; CI green before merge
- Always: reuse FinanceTracker's security baseline (lockout, rate limits, headers, secure cookies)
- Never in v1: rent or payments, email/SMS sending, a mobile app, properties with more than one landlord

## Progress

| Milestone | Status |
| --- | --- |
| M1 Skeleton | Not started |
| M2 Properties | Not started |
| M3 Invitations and tenancies | Not started |
| M4 Reporting issues | Not started |
| M5 Issue workflow | Not started |
| M6 Contractors | Not started |
| M7 Comments and timeline | Not started |
| M8 Contractor API | Not started |
| M9 Photo uploads | Not started |
| M10 Reports and deployment | Not started |
