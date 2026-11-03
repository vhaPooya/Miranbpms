# Miran BPMS
### Open-Source Business Process Management & Office Automation Platform

<p align="center">
  <strong>A Modular, Extensible Enterprise Platform for Workflow Automation, Document Management, and Digital Office Operations.</strong>
</p>

<p align="center">
  <a href="https://github.com/vhaPooya/Miranbpms">GitHub Repository</a> ·
  <a href="https://github.com/vhaPooya/Miranbpms/issues">Report an Issue</a> ·
  <a href="https://github.com/vhaPooya/Miranbpms/blob/master/PROJECT_MAP.md">Architecture Documentation</a>
</p>

---

## Overview

**Miran BPMS** is an open-source Business Process Management (BPMS) and Office Automation platform designed to help organizations digitize, automate, and manage their administrative and business processes.

Built with **ASP.NET Core 9 and C#**, Miran BPMS brings together workflow automation, dynamic form building, electronic correspondence, document management, organizational structures, and enterprise access control in a unified platform.

The project aims to provide organizations with a flexible and extensible foundation for building digital workplaces, automating complex business processes, and reducing manual administrative operations.

Miran BPMS follows a modular architecture that allows individual business capabilities to evolve independently while sharing a common core.

### Key Objectives

- Digitize traditional administrative and organizational processes.
- Automate complex, multi-step business workflows.
- Provide a flexible visual form-building experience.
- Centralize electronic correspondence and document management.
- Improve organizational transparency and process traceability.
- Provide secure, role-based access to enterprise resources.
- Support integration with external enterprise systems.
- Establish a foundation for AI-assisted business process automation.

---

## Core Features

### 1. Workflow Management Engine

A configurable workflow engine designed to automate organizational processes and business operations.

- Visual workflow modeling concepts.
- Multiple workflow node types.
- Conditional branching and decision handling.
- Parallel execution and merge operations.
- Human tasks and automated service tasks.
- Workflow instances and lifecycle management.
- Workflow variables and execution context.
- Versioned workflow definitions.
- Process transitions and execution tracking.

Supported workflow node types include:

| Node Type | Description |
|---|---|
| Start | Starts a workflow |
| End | Completes a workflow |
| Task | Executes a workflow task |
| Decision | Conditional branching |
| Parallel | Parallel execution |
| Merge | Combines workflow paths |
| Wait | Waits for a condition or event |
| Service | Executes an automated operation |
| Script | Executes custom logic |
| Human | Requires user interaction |

### 2. Dynamic Form Builder

A flexible form-building system for creating business forms without manually developing individual pages.

- Drag-and-drop form design.
- Dynamic form fields and configurations.
- Flexible, Grid, and Table layouts.
- Field-level validation.
- Custom form actions and buttons.
- Dynamic form data tables.
- Form schema management.
- XML-based form configuration.
- Dynamic database table generation.
- Custom print templates and numbering rules.

The form-building module provides a foundation for creating forms that can be integrated with organizational workflows.

### 3. Electronic Correspondence & Secretariat

A centralized environment for managing organizational correspondence and official documents.

- Incoming correspondence management.
- Outgoing correspondence management.
- Internal correspondence.
- Document registration and numbering.
- Document referral and tracking.
- Document attachments.
- Digital signature support.
- Correspondence classification.
- Secretariat management.
- Document lifecycle management.

### 4. Document Management & Archiving

A document management foundation designed to organize and maintain enterprise information.

- Document registration and classification.
- Document tracking and history.
- Centralized document storage.
- Document attachments and metadata.
- Archive and cabinet management.
- Document search and retrieval.
- Document lifecycle operations.
- File handling and encrypted storage services.

### 5. Organizational Management & Access Control

A flexible authorization model designed for organizations with complex structures and responsibilities.

- User and role management.
- Organizational departments and positions.
- Group-based access control.
- Role-based permissions (RBAC).
- Permission groups.
- Position-based authorization.
- Workflow-level permissions.
- Field-level access control.
- Action-based authorization.
- User signatures and organizational context.

The permission system is designed to control not only access to application modules but also actions performed within forms, documents, and business processes.

### 6. Real-Time Notifications

Real-time communication and notification capabilities powered by ASP.NET Core SignalR.

- Real-time user notifications.
- Workflow-related notifications.
- Event-driven communication.
- Notification Hub integration.

### 7. Enterprise Integration

Miran BPMS is designed to integrate with existing organizational infrastructure and external business applications.

- REST API integration.
- API Gateway foundation.
- External Web Services.
- Database connectivity services.
- ERP integration foundation.
- CRM integration foundation.
- Event-driven communication between modules.
- Email and fax service integration.

### 8. Background Services

Dedicated background services for asynchronous administrative operations.

- Email processing using MailKit.
- Background email delivery.
- Fax service integration.
- Windows Service-based execution.

### 9. Security & Auditing

Security is an important part of the platform architecture because BPMS applications frequently handle confidential organizational information.

Current security-related capabilities include:

- Cookie-based authentication.
- Password hashing using BCrypt.
- Role-based authorization.
- Encryption services.
- Two-factor authentication foundation.
- Audit trail services.
- Secure file handling.
- Session-based user context.
- Organizational access control.

### 10. Caching & Performance

The platform includes infrastructure for improving application performance and managing frequently accessed information.

- In-memory caching.
- Redis caching integration.
- Database optimization services.
- Performance monitoring foundation.
- Asynchronous service operations.

---

## Architecture

Miran BPMS uses a **Modular Monolith architecture** to organize business capabilities into independent modules while maintaining a unified application deployment model.

This approach provides a balance between modularity, maintainability, and operational simplicity.

### Solution Structure

```text
MiranBPMS/
│
├── Automation.Core/
│   ├── Entities/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Events/
│   └── Shared/
│
├── Automation.Infrastructure/
│   ├── Data/
│   ├── Migrations/
│   ├── Services/
│   ├── Security/
│   ├── Caching/
│   └── Integration/
│
├── Automation.Module.FormBuilder/
│   ├── Services/
│   ├── Controllers/
│   └── Views/
│
├── Automation.Module.Workflow/
│   ├── Services/
│   ├── Controllers/
│   ├── Events/
│   └── Scripts/
│
├── Automation.Module.Secretariat/
│   ├── Services/
│   ├── Controllers/
│   └── Views/
│
├── Automation.Web/
│   ├── Controllers/
│   ├── Views/
│   ├── Hubs/
│   └── wwwroot/
│
├── Automation.Web.Host/
│   └── Program.cs
│
├── WinServices/
│   ├── EmailService/
│   └── FaxService/
│
├── Automation.Web.sln
├── PROJECT_MAP.md
└── REFACTORING.md
```

### Architectural Components

| Component | Responsibility |
|---|---|
| Automation.Core | Shared domain entities, interfaces, DTOs, and events |
| Automation.Infrastructure | Data access, infrastructure services, security, and integrations |
| Automation.Module.FormBuilder | Dynamic form-building capabilities |
| Automation.Module.Workflow | Workflow execution and process automation |
| Automation.Module.Secretariat | Electronic correspondence and secretariat operations |
| Automation.Web | MVC application and user interface |
| Automation.Web.Host | Application entry point and module initialization |
| WinServices | Background email and fax services |

### Database Architecture

The platform uses a hybrid data access strategy:

- **Entity Framework Core:** Structured application data and database schema management.
- **Dapper:** Efficient execution of SQL queries and stored procedures.
- **SQL Server:** Primary relational database.
- **Redis:** Distributed caching infrastructure.

The architecture separates identity and organizational data from dynamic form and workflow data at the logical application level.

### Module Communication

Modules are designed to communicate through shared contracts and application events.

MediatR provides an event-driven communication mechanism that helps reduce direct dependencies between business modules.

---

## Technology Stack

| Category | Technologies |
|---|---|
| Backend | C# / ASP.NET Core 9 |
| Application Architecture | Modular Monolith |
| Web Framework | ASP.NET Core MVC |
| ORM | Entity Framework Core 9 |
| Data Access | Dapper |
| Database | Microsoft SQL Server |
| Real-Time Communication | SignalR |
| Caching | Redis / In-Memory Cache |
| Authentication | ASP.NET Core Cookie Authentication |
| Password Security | BCrypt |
| Email | MailKit |
| Background Processing | .NET Worker Services / Windows Services |
| Frontend | Razor Views, Bootstrap, JavaScript, jQuery |
| Event Handling | MediatR |

---

## Getting Started

### Prerequisites

Before running the application, ensure that the following components are installed:

- .NET 9 SDK
- Microsoft SQL Server
- Visual Studio 2022 or a compatible .NET development environment
- Redis (when using Redis-based caching)

### 1. Clone the Repository

```bash
git clone https://github.com/vhaPooya/Miranbpms.git

cd Miranbpms
```

### 2. Restore Dependencies

```bash
dotnet restore Automation.Web.sln
```

### 3. Configure the Application

Configure your database connection and external services in the appropriate `appsettings.json` files.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MiranBPMS;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

Use your own database credentials and deployment-specific settings.

Do not commit production passwords, API keys, or other sensitive configuration values to the repository.

### 4. Configure the Database

Apply the database migrations and execute any required SQL scripts for the workflow engine according to the project documentation.

The workflow database scripts are located in:

```text
Automation.Module.Workflow/Scripts/
```

### 5. Build the Solution

```bash
dotnet build Automation.Web.sln
```

### 6. Run the Application

The intended application entry point is `Automation.Web.Host`.

```bash
dotnet run --project Automation.Web.Host
```

Additional configuration may be required depending on the selected deployment environment and the current state of the database.

---

## Project Documentation

Additional technical documentation is available in the repository:

| Document | Description |
|---|---|
| [PROJECT_MAP.md](PROJECT_MAP.md) | Project structure, components, services, and technology overview |
| [REFACTORING.md](REFACTORING.md) | Architecture evolution, modularization, and implementation notes |

---

## Roadmap

The long-term vision of Miran BPMS includes expanding its capabilities in several areas:

- [ ] Further modularization of business capabilities.
- [ ] Enhanced visual workflow designer.
- [ ] Advanced workflow monitoring and analytics.
- [ ] Expanded enterprise integration capabilities.
- [ ] Advanced document management and search.
- [ ] Improved API documentation and developer tooling.
- [ ] Automated testing and continuous integration.
- [ ] Enhanced security auditing and vulnerability management.
- [ ] AI-assisted document classification and information extraction.
- [ ] Natural-language interaction with business processes.
- [ ] Intelligent workflow recommendations.
- [ ] AI-powered administrative automation.

These items represent development goals and should not be interpreted as currently available features.

---

## Contributing

Contributions from developers and the open-source community are welcome.

If you are interested in improving Miran BPMS, you can contribute by:

1. Reporting bugs and suggesting improvements through GitHub Issues.
2. Proposing new features.
3. Improving documentation.
4. Reviewing existing code.
5. Submitting pull requests.

For significant changes, please open an issue first to discuss the proposed implementation.

---

## Security

Because Miran BPMS is designed to handle sensitive business documents and organizational workflows, security is a fundamental consideration.

If you discover a potential security vulnerability, please avoid publishing sensitive exploit details in a public issue. Contact the repository maintainer privately through an appropriate security reporting channel.

---

## License

A formal open-source license has not yet been specified in this repository.

Until a license is added, the source code should not be assumed to be freely available for redistribution, modification, or commercial use.

---

## Project Vision

**Miran BPMS aims to make enterprise process automation more accessible, flexible, and intelligent.**

The long-term goal is to build an extensible open-source platform that combines business process management, office automation, document intelligence, and AI-assisted workflows into a unified digital workplace.

By providing a modular foundation, Miran BPMS aims to help organizations gradually transform traditional administrative operations into connected, automated, and intelligent business processes.

---

<p align="center">
  <strong>Built with C# and ASP.NET Core.</strong>
  <br/>
  <sub>Developed by <a href="https://github.com/vhaPooya">vhaPooya</a></sub>
</p>