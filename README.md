# SSProjectSolution

Enterprise-grade Inventory and Delivery Challan Management System.

## Project Overview
SSProjectSolution is a management platform designed for handling Inward and Outward inventory entries, generating Delivery Challans (DC), and managing company/user records. The system utilizes a .NET backend with Dapper for efficient data access and is structured for scalability and high performance.

## Technology Stack
- **Backend:** .NET (Web API)
- **Data Access:** Dapper ORM
- **Database:** SQL Server
- **Frontend:** (Angular / Modern Web Framework)
- **DevOps:** GitHub Actions

## Branching Strategy
We follow a structured branching model to ensure stability and quality:
- `master`: Production-ready code only. Restricted access.
- `test`: QA validation and integration testing branch.
- `dev`: Active development. All feature work merges here.

### Branch Protection
- **master**: Pull requests required. Mandatory approvals (1-2). Status checks must pass.
- **test**: Pull requests required. Build validation mandatory.

## Naming Conventions
- **Feature**: `feature/<feature-name>`
- **Bugfix**: `bugfix/<issue-description>`
- **Hotfix**: `hotfix/<critical-issue>`
- **Release**: `release/vX.Y.Z`

## Commit Standards (Conventional Commits)
- `feat: ...` (New feature)
- `fix: ...` (Bug fix)
- `docs: ...` (Documentation changes)
- `refactor: ...` (Code changes that neither fix a bug nor add a feature)
- `chore: ...` (Updates to build process, dependencies, etc.)

## Docker Setup

The application is containerized using Docker and orchestrated with Docker Compose. This setup includes the .NET 8 Web API and a SQL Server 2022 instance.

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Running with Docker Compose
1. Build and start the containers:
   ```bash
   docker-compose up --build -d
   ```
2. The API will be available at `http://localhost:8080/swagger/index.html`.
3. PDF outputs will be saved to the `./dc_outputs` directory in the project root.

### Database Initialization
After the database container is running, you must execute the setup scripts against the container:
- SQL Server Host: `localhost,1433`
- Username: `sa`
- Password: `YourSecurePassword123!` (as defined in `docker-compose.yml`)

## Setup & Running (Local Development)
1. Clone the repository:
   ```bash
   git clone https://github.com/selvarajcs002-collab/SSProjectSolution.git
   ```
2. Setup the database:
   - Execute `DatabaseSetup.sql` and `Inward_Setup.sql` in your SQL Server instance.
   - Update `appsettings.json` with your connection string.
3. Run the backend:
   ```bash
   dotnet run
   ```

## Repository Owners
- **Principal Owner:** Selvaraj
