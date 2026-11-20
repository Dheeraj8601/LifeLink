# LifeLink — Blood Donor & Emergency Request Platform

LifeLink is a platform designed to connect people who need blood with eligible donors nearby. It helps requesters create blood requests, helps donors manage their availability, and supports location-based donor discovery.

## Business Requirements

### 1. User Registration and Authentication
- Users can register and log in securely.
- Duplicate email registrations must be prevented.
- Authenticated operations must require a valid session.
- Users can log out.

### 2. Donor Profile Management
- Donors have profiles containing relevant information such as blood group and location.
- Donors can view their profiles and manage donation availability.
- Donor verification status is represented in the system.
- Location information supports nearby donor discovery.

### 3. Blood Request Management
- Authenticated users can create blood requests.
- Requests capture blood group, units required, urgency, and location-related details.
- Urgency levels: Normal, Urgent, and Critical.
- Requests have statuses to track their lifecycle.
- Request owners can view and manage their requests.

### 4. Donor Matching
- Find donors matching the requested blood group.
- Matching donors must be available and verified, with valid location data.
- Search is limited by a configurable radius and ordered by proximity.
- Nearby donor results support pagination.
- Matching is a discovery aid; medical compatibility and eligibility must be confirmed by qualified medical professionals.

### 5. Request Responses and Fulfillment
- Donors can respond to blood requests.
- Responses support Pending, Accepted, Rejected, and Withdrawn states.
- Request fulfillment is tracked.
- Invalid state transitions and inconsistent updates must be prevented.

### 6. Security and Data Integrity
- Protect user-specific operations with authentication and authorization.
- Validate incoming data.
- Never store passwords in plain text.
- Enforce ownership for user-specific operations.
- Handle consistency-sensitive database operations safely.
- Keep secrets out of source control.

## Current Implementation

### Backend
- ASP.NET Core 10 Web API
- Clean Architecture (API, Application, Domain, Infrastructure)
- SQL Server and Entity Framework Core (Database First/scaffolding workflow)
- User registration and login
- JWT authentication using Secure, HttpOnly cookies
- Donor profile retrieval and availability updates
- City lookup API
- Blood request/response domain, services, and repositories
- SQL Server `geography` location storage and spatial matching
- Nearby donor search by blood group, availability, verification, and radius

### Frontend
- React + Vite, JavaScript, Bootstrap
- React Router and Axios
- Authentication context
- Registration and login
- Guest/protected routes
- Dashboard and donor profile summary
- City selection and browser geolocation
- Donor availability toggle integrated with the backend

## Planned Features
- Blood request creation and management UI
- Nearby donor results UI
- Donor response UI
- Request tracking and fulfillment UI
- Real-time notifications with SignalR
- Asynchronous workflows with RabbitMQ
- AI-assisted features using locally hosted models (exploration)
- Additional automated testing and deployment

## Technology Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| Architecture | Clean Architecture |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Frontend | React, Vite, JavaScript |
| UI | Bootstrap |
| HTTP | Axios |
| Authentication | JWT, Secure HttpOnly cookies |
| Real-time (planned) | SignalR |
| Messaging (planned) | RabbitMQ |
| AI (planned) | Ollama / local models |

## Solution Structure
- **API** — endpoints, authentication, DTOs, and application configuration
- **Application** — use cases, services, interfaces, and DTOs
- **Domain** — entities, enums, and business rules
- **Infrastructure** — persistence, repositories, and integrations
- **LifeLink.Client** — React frontend

## Running Locally

### Prerequisites
- .NET 10 SDK
- SQL Server
- Node.js and npm
- Git

### Backend
1. Configure the database connection string and JWT settings in local configuration.
2. Run the API from Visual Studio or the .NET CLI.
3. Use Swagger if enabled to explore the endpoints.

### Frontend
```bash
cd LifeLink.Client
npm install
npm run dev
```

Update the frontend API base URL if your local backend URL differs.

## Configuration and Secrets
Do not commit real database credentials, JWT signing keys, API keys, SMTP credentials, or other secrets. Use .NET User Secrets, environment variables, or local untracked configuration. The included `appsettings.json` contains placeholders only.

## Project Status
LifeLink is actively developed as a learning and portfolio project. Features are added incrementally, with emphasis on business requirements, architecture, frontend/backend integration, and testing.

## Disclaimer
LifeLink facilitates donor discovery and coordination. It is not a substitute for medical advice, blood-bank procedures, laboratory testing, or professional confirmation of donor-recipient compatibility.
