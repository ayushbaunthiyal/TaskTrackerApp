
# Prompts Used for AI-Assisted Development

This file documents the key prompts and requests used throughout the development of TaskTrackerApp with AI assistance. Prompts are organized by development phase and technical area, reflecting the actual workflow and decisions made during each stage.

---

## Phase 1: Backend Foundation & Database Setup

**Architecture & Solution Structure**
- "Generate Clean Architecture solution structure for .NET 8 Web API with Domain, Application, Infrastructure, and API layers."
- "Create entity classes for User, TaskItem, Attachment, and AuditLog with appropriate properties and relationships."
- "Implement TaskStatus and TaskPriority enums for task management."
- "Design repository and service interfaces for CRUD operations and audit logging."
- "Set up EF Core with PostgreSQL, enable JSONB for tags, and configure global query filters for soft delete."
- "Create Docker Compose file for PostgreSQL with health checks and persistent volume."
- "Seed sample data for users, tasks, and attachments."
- "Add Swagger/OpenAPI documentation and global exception handling middleware."
- "Implement health check endpoints for API and database connectivity."
- "Resolve namespace conflicts and package version issues."

**Prompts for Automation & Troubleshooting**
- "How do I resolve TaskStatus naming conflict with System.Threading.Tasks.TaskStatus?"
- "Enable dynamic JSON serialization for PostgreSQL JSONB columns in Npgsql 8.x."
- "Fix port conflict for PostgreSQL when 5432 is already in use."
- "Merge architecture documentation into a single comprehensive file."

---

## Phase 2: JWT Authentication & Authorization

**Authentication & Security**
- "Implement JWT-based authentication and authorization for .NET 9 Web API."
- "Create endpoints for user registration, login, token refresh, and token revocation."
- "Enforce owner-based permissions for task modification."
- "Integrate BCrypt password hashing for secure storage."
- "Configure JWT settings in appsettings.json and validate tokens with clock skew tolerance."
- "Add refresh token rotation and revocation logic."
- "Standardize error response formats across controllers."

**Testing & Quality Assurance**
- "Write unit tests for AuthController, TasksController, AuthService, TaskService, and TokenService."
- "Document manual test cases for authentication and authorization flows."
- "Fix token validation unit test mocking issues with IConfiguration indexer."

**Prompts for Security & Troubleshooting**
- "How do I prevent reuse of revoked refresh tokens?"
- "Implement case-insensitive search using EF.Functions.ILike for PostgreSQL."
- "Configure CORS for controlled access from React UI."

---

## Phase 3: React UI Implementation & Advanced Features

**UI/UX Design & Integration**
- "Build a modern, responsive React TypeScript UI for task management with JWT authentication."
- "Design card-based layout with color-coded status and priority indicators."
- "Implement real-time search, advanced filtering, sorting, and pagination."
- "Add file attachment support with drag & drop, upload progress, and download/delete functionality."
- "Create audit trail timeline component for task actions and attachments."
- "Integrate toast notifications, loading states, and confirmation dialogs for better UX."
- "Optimize UI for mobile, tablet, and desktop breakpoints."
- "Apply compact filter panel and increase search box width for usability."
- "Add gradient backgrounds, icon integration, and improved badge styling."

**API Integration & State Management**
- "Set up Axios with JWT interceptors and automatic token refresh."
- "Create API clients for authentication, tasks, attachments, and audit logs."
- "Implement protected routes and global authentication context."

**Testing & Troubleshooting**
- "Write manual and automated tests for all UI components and API flows."
- "Fix TypeScript build errors due to duplicate event handlers and unused imports."
- "Document troubleshooting steps for API connection, authentication, and file upload issues."

---

## Phase 4: Background Worker & Audit Logs

**Worker Service & Email Reminders**
- "Implement .NET 9 Worker Service to send email reminders for tasks due within 24 hours using Mailgun API."
- "Design idempotency mechanism to ensure one reminder per task."
- "Configure daily email quota and per-run limits for Mailgun sandbox."
- "Create responsive HTML email templates with urgency indicators and personalized greetings."
- "Log all reminder actions to audit trail."
- "Set up Docker deployment for worker service with environment variable configuration."

**Audit Logs Feature**
- "Create public API endpoint for viewing and filtering audit logs with advanced search, pagination, and color-coded badges."
- "Implement dynamic LINQ query building and UTC DateTime handling for PostgreSQL compatibility."
- "Build React component for audit logs with filter panel, search bar, and responsive table."
- "Integrate audit logs navigation into main UI and API client."

**DevOps & Security**
- "Exclude secret files from git using .gitignore and create template files for configuration."
- "Clean git history to remove exposed secrets and force push to remote."

---

## Phase 5: Monitoring, Metrics, Health Checks, Documentation & Docker Deployment

**Observability & Monitoring**
- "Integrate Prometheus metrics collection for API and Worker services, exposing /metrics endpoints."
- "Track business metrics: tasks created, updated, deleted, completed, active count, authentication success/failure, user registrations, attachments, operation durations."
- "Implement health check endpoints for API, Worker, PostgreSQL, and UI containers."
- "Configure Docker health checks and proper service startup ordering."
- "Set up structured logging with Serilog, correlation IDs, and contextual properties."
- "Create Grafana dashboard for metrics visualization."

**Testing & Documentation**
- "Write and run comprehensive unit and integration tests for controllers, services, middleware, and health checks."
- "Document architecture, deployment, and rationale in ARCHITECTURE.md, ARCHITECTURE_RATIONALE.md, and DOCKER_DEPLOYMENT.md."
- "Create .env.example and docker-compose.yml.example for environment configuration."

**DevOps & Deployment**
- "Optimize Docker builds with multi-stage builds, layer caching, and .dockerignore configuration."
- "Configure environment variables for secrets and service settings."
- "Document troubleshooting steps for metrics, health checks, logging, integration tests, and Docker build errors."

---

## General Prompts Used Throughout Development

- "Merge these two files into one comprehensive architecture document."
- "Create a markdown file with all UI snapshots from the provided docx."
- "Can we create a descriptive PDF from docx file?"
- "Add link to main documentation links like 'docs' folder architecture and readme file."
- "Filters in one line only; make the filter panel compact."
- "Apply same UI changes for Audit Logs also."
- "Can we increase the width of search box and filter controls?"
- "Increase size and content of middle info task boxes so that it looks big."
- "Reduce card size; pagination is going beyond, I have to scroll every time."
- "Lets run all four apps in docker together."
- "Can we clean and rebuild all because we have made changes?"
- "Let me know if I can test it."
- "Lets merge this feature branch to master."
- "Can we create an email by giving Git links to showcase what we have built?"
- "Enhance it more professionally and technically."

---

> This file is intended to help future contributors understand how AI-assisted prompts were used to accelerate development and improve project quality. Prompts are grouped by phase and technical area for clarity and traceability.

---

**End of Prompts.md**
