# FlowerShop API

## 1. Project Overview

FlowerShop API is a backend RESTful API for a flower shop management system, built with ASP.NET Core 8 Web API and PostgreSQL.

The project provides backend services for managing flower shop operations, including customer information, delivery addresses, products, and orders. It focuses on implementing business logic, handling data persistence, validating requests, and maintaining data consistency throughout the order lifecycle.

The application is designed with a separate API and automated test project, making it easier to maintain, test, and extend. Docker and Docker Compose are used to simplify application and database setup across development environments.

### Key Objectives

- Develop RESTful API endpoints for flower shop operations.
- Implement business logic for customer and order management.
- Manage relational data and persist application data using PostgreSQL.
- Validate order requests and handle product inventory updates.
- Write automated tests to verify application behavior and business rules.
- Containerize the application and database environment using Docker.

## 2. Tech Stack

| Technology | Purpose |
|---|---|
| C# | Primary programming language |
| ASP.NET Core 8 Web API | Building RESTful API endpoints and handling HTTP requests |
| Entity Framework Core | Object-relational mapping (ORM) and database access |
| PostgreSQL | Relational database for application data |
| AutoMapper | Mapping between application objects and data transfer objects (DTOs) |
| Docker | Containerizing the application environment |
| Docker Compose | Configuring and running the application and database services |
| xUnit | Automated testing framework |
| Moq | Creating mock dependencies for isolated tests |
| FluentAssertions | Writing readable and expressive test assertions |
| EF Core InMemory | Providing an in-memory database for selected tests |

### Testing

The project includes an automated test suite covering customer account operations, customer administration, order processing, inventory updates, order status transitions, and order querying.

**Latest test run:**

- Total tests: 41
- Passed: 41
- Failed: 0
- Skipped: 0
- Pass rate: 100%
- Execution time: approximately 5.5 seconds

These results represent the reported test run. The test suite includes EF Core InMemory tests; passing tests do not, by themselves, establish that all database behavior has been verified against a live PostgreSQL instance.

## 3. Automated Testing

The project uses xUnit, Moq, FluentAssertions, and EF Core InMemory to test account controller behavior, customer administration, and order service business logic.

### Test Results

| Metric | Result |
|---|---|
| Total tests | 41 |
| Passed | 41 |
| Failed | 0 |
| Skipped | 0 |
| Pass rate | 100% |
| Execution time | ~5.5 seconds |

### Tested Scenarios

#### AccountControllerTests

- Retrieve a customer profile successfully or handle a profile that cannot be found.
- Update a customer profile successfully or handle an update failure.
- Create a delivery address.
- Update an address when authorized.
- Reject address updates or deletion when the user does not own the address.

#### AdminCustomerControllerTests

- Retrieve customers with pagination and search.
- Retrieve a customer by ID, including the not-found case.
- Delete a customer, including the not-found case.
- Handle an empty search query and an extreme page number.
- Update a customer successfully or handle update failure.
- Handle service or database errors.

#### OrderServiceTests

- Handle order creation with an empty order, a missing address or payment method, discontinued products, and insufficient stock.
- Verify successful order creation, inventory deduction, and order item snapshots.
- Validate order cancellation based on ownership and order status.
- Verify inventory restoration when an order is cancelled.
- Validate order status values and permitted status transitions.
- Verify the amount-paid field when completing an order and refund-related flags.
- Handle a simulated exception during order creation.
- Handle missing orders.
- Verify order filtering by date, delivery method, and payment method.
- Search orders using shipping information.
- Retrieve order status counts and paginated results.
- Verify soft-delete filtering and refund-related behavior.

### Run Tests

```bash
dotnet test FlowerShop.sln --logger "console;verbosity=normal"
```

**Testing note:** Some order service tests use EF Core InMemory. These tests do not independently verify PostgreSQL-specific behavior or actual transaction rollback against PostgreSQL.

## 4. Database Design

The database schema is documented using an Entity-Relationship Diagram (ERD), illustrating the main entities and their relationships within the FlowerShop API.

The diagram provides an overview of how application data is organized and how entities relate to one another.

![FlowerShop Database ERD](https://res.cloudinary.com/dxz14apyl/image/upload/v1792393243/flowershopdb-diagram_plgluq.png)

## 5. API Endpoints & Access Control

The API provides RESTful endpoints organized by functional module. Access is separated into three roles: Customer, Admin, and SuperAdmin.

### 5.1 Authentication

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/Auth/register` | Public* |
| POST | `/api/Auth/login` | Public* |
| POST | `/api/Auth/external-login` | Public* |

### 5.2 Account Management

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/account/profile` | Customer — own profile |
| PUT | `/api/account/profile` | Customer — own profile |
| POST | `/api/account/addresses` | Customer — own addresses |
| PUT | `/api/account/addresses/{id}` | Customer — own addresses |
| DELETE | `/api/account/addresses/{id}` | Customer — own addresses |

### 5.3 Product Management

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/products` | Customer, Admin, SuperAdmin |
| GET | `/api/products/{identifier}` | Customer, Admin, SuperAdmin |
| POST | `/api/admin/products` | Admin, SuperAdmin |
| PUT | `/api/admin/products/{id}` | Admin, SuperAdmin |
| DELETE | `/api/admin/products/{id}` | Admin, SuperAdmin |

### 5.4 Product Categories

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/product-categories` | Customer, Admin, SuperAdmin |
| GET | `/api/product-categories/{id}` | Customer, Admin, SuperAdmin |
| POST | `/api/admin/product-categories` | Admin, SuperAdmin |
| PUT | `/api/admin/product-categories/{id}` | Admin, SuperAdmin |
| DELETE | `/api/admin/product-categories/{id}` | Admin, SuperAdmin |

### 5.5 Product Tags

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/product-tags` | Customer, Admin, SuperAdmin |
| GET | `/api/product-tags/{id}` | Customer, Admin, SuperAdmin |
| POST | `/api/admin/product-tags` | Admin, SuperAdmin |
| PUT | `/api/admin/product-tags/{id}` | Admin, SuperAdmin |
| DELETE | `/api/admin/product-tags/{id}` | Admin, SuperAdmin |

### 5.6 Order Management

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/orders` | Customer — create own order |
| GET | `/api/orders/history` | Customer — own order history |
| GET | `/api/orders/{id}` | Customer — own order; Admin/SuperAdmin subject to authorization |
| PUT | `/api/orders/{id}/cancel` | Customer — own order, subject to business rules |
| GET | `/api/admin/orders` | Admin, SuperAdmin |
| GET | `/api/admin/orders/{id}` | Admin, SuperAdmin |
| PUT | `/api/admin/orders/{id}/status` | Admin, SuperAdmin |

### 5.7 Customer Administration

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/admin/customers` | Admin, SuperAdmin |
| GET | `/api/admin/customers/{id}` | Admin, SuperAdmin |
| PUT | `/api/admin/customers/{id}` | Admin, SuperAdmin |
| DELETE | `/api/admin/customers/{id}` | Admin, SuperAdmin |

### 5.8 Role & Permission Management

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/admin/permissions` | SuperAdmin* |
| GET | `/api/admin/permissions/{permissionId}` | SuperAdmin* |
| GET | `/api/admin/roles` | SuperAdmin* |
| GET | `/api/admin/roles/{roleId}` | SuperAdmin* |
| POST | `/api/admin/roles` | SuperAdmin* |
| PUT | `/api/admin/roles/{roleId}` | SuperAdmin* |
| DELETE | `/api/admin/roles/{roleId}` | SuperAdmin* |
| POST | `/api/admin/roles/{roleId}/permissions` | SuperAdmin* |

### 5.9 Media Management

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/Media/upload-raw` | Admin, SuperAdmin* |
| DELETE | `/api/Media/delete-raw` | Admin, SuperAdmin* |
| POST | `/api/Media/delete-multi` | Admin, SuperAdmin* |

### 5.10 Health & Diagnostics

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/health` | Public, if configured |
| GET | `/api/debug/claims` | Development/authorized access only |

\* Access-control note: The role assignments above are intended as a proposed access policy and must be verified against the actual authorization configuration. Public means an endpoint intended to be accessible without authentication; confirm that the endpoint explicitly permits anonymous access.

### Authentication

For protected endpoints, include a JWT access token in the HTTP request header:

```http
Authorization: Bearer <your_access_token>
```

### Access Control Principles

- Customers can access only their own profiles, addresses, and orders.
- Admins manage store operations within their assigned permissions.
- SuperAdmins manage administrative roles and permissions.
- Sensitive administrative endpoints must enforce authorization on the server.
- Resource ownership and order business rules must be validated by the API, not trusted from client input.

## 6. Setup & Run with Docker

### Prerequisites

Make sure the following tools are installed:

- Git
- Docker Desktop
- .NET 8 SDK (optional, for running the API outside Docker)

### Step 1: Clone the Repository

```bash
git clone https://github.com/nhatloi1204/FlowerShopApi.git
cd FlowerShopApi
```

### Step 2: Configure Environment Variables

Create a local `.env` file based on `.env.example`:

```bash
cp .env.example .env
```

On Windows PowerShell, use:

```powershell
Copy-Item .env.example .env
```

Update `.env` with your local configuration. The following is an example of the variables required by the Docker Compose setup and application configuration:

```dotenv
# PostgreSQL
DB_PORT=5432
DB_NAME=flowershop
DB_USER=flowershopdb
DB_PASSWORD=replace_with_a_local_password

# JWT
JWT__SECRET=replace_with_a_random_secret_at_least_32_characters
JWT__ISSUER=FlowerShop.API
JWT__AUDIENCE=FlowerShop.Client
JWT__EXPIRATIONMINUTES=60

# SuperAdmin Seeder
SUPERADMIN__USERNAME=SuperAdmin
SUPERADMIN__EMAIL=superadmin@example.com
SUPERADMIN__PASSWORD=replace_with_a_strong_local_password

# Application
FRONTEND_URL=http://localhost:3000

# Cloudinary (configure if required by the features being tested)
CLOUDINARYSETTINGS__CLOUDNAME=your_cloud_name
CLOUDINARYSETTINGS__APIKEY=your_api_key
CLOUDINARYSETTINGS__APISECRET=your_api_secret

# Google OAuth (configure if required)
GOOGLE_CLIENTID=your_google_client_id
```

Use your own local credentials and valid third-party credentials where required. Do not commit `.env` or real secrets to the repository.

**Configuration note:** ASP.NET Core maps environment variables containing double underscores (`__`) to nested configuration keys. For example, `JWT__SECRET` maps to `Jwt:Secret`. Make sure the variable names in `.env` match the configuration expected by the application.

### Step 3: Build and Start the Containers

Run the following command from the repository root:

```bash
docker compose up --build -d
```

This starts the PostgreSQL database and builds and starts the FlowerShop API container.

Check the container status:

```bash
docker compose ps
```

View the application logs:

```bash
docker compose logs -f flowershop-api
```

To inspect database logs:

```bash
docker compose logs -f postgres
```

The API runs on port 5000 on the host machine.

### Step 4: Open Swagger UI

Once the API has started successfully, open:

[http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)

On application startup, the API runs database migrations and executes the configured seeders, including the SuperAdmin seeder.

If the application fails to start, inspect the API logs and verify the database connection, environment variables, and migration configuration.

### Step 5: Stop the Application

Stop and remove the running containers:

```bash
docker compose down
```

The PostgreSQL data volume is retained, so the database data remains available when the containers are started again.

To remove the database volume and reset local database data, use:

```bash
docker compose down -v
```

**Warning:** The second command deletes the persisted PostgreSQL data. Use it only when you intentionally want to reset your local database.

## 7. API Testing with Swagger

Swagger UI provides an interactive interface for sending HTTP requests to the API.

### 7.1 Authenticate as SuperAdmin

The `AdminSeeder` creates the initial SuperAdmin account when the configured email does not already exist.

Use the credentials configured in your local `.env` file.

1. Open Swagger UI.
2. Find the `Auth` section.
3. Expand the login endpoint, `POST /api/Auth/login`.
4. Click **Try it out**.
5. Enter the request body according to the schema shown in Swagger.
6. Click **Execute**.
7. Inspect the response and copy the JWT access token returned by the API.

The exact request fields and response structure should be taken from the endpoint schema and response returned by your running version.

### 7.2 Authorize Swagger Requests

1. Click the **Authorize** button near the top of Swagger UI.
2. Paste the access token into the Bearer authentication field.
3. Confirm the authorization.
4. Execute protected endpoints to test authenticated requests.

The current Swagger configuration uses HTTP Bearer authentication. If the UI asks for the token only, paste the raw token without adding the `Bearer ` prefix; Swagger adds the authentication scheme to the request.

### 7.3 Test Customer Endpoints

Register a customer through the registration endpoint, then log in with that account to obtain a JWT.

Use the customer token to test endpoints such as:

- `GET /api/account/profile`
- `PUT /api/account/profile`
- `POST /api/account/addresses`
- `PUT /api/account/addresses/{id}`
- `DELETE /api/account/addresses/{id}`
- `GET /api/products`
- `GET /api/product-categories`
- `GET /api/product-tags`
- `POST /api/orders`
- `GET /api/orders/history`

Use the request schemas displayed in Swagger and provide valid IDs and required fields.

For ownership checks, verify that a customer cannot modify another customer's address or access another customer's order.

### 7.4 Test Admin Endpoints

Authenticate with an account that has the appropriate Admin role and permissions.

Test administrative operations available in Swagger, including:

- Product creation, updates, and deletion.
- Product category and tag management.
- Customer search, retrieval, updates, and deletion.
- Administrative order listing and retrieval.
- Order status updates.

The SuperAdmin seeder creates a SuperAdmin account; it does not automatically create a separate Admin test account. Create or assign an Admin account using the application's supported role-management flow before testing Admin-specific access.

### 7.5 Test SuperAdmin Endpoints

Authenticate with the seeded SuperAdmin account and test the role and permission management endpoints:

- `GET /api/admin/roles`
- `POST /api/admin/roles`
- `GET /api/admin/roles/{roleId}`
- `PUT /api/admin/roles/{roleId}`
- `DELETE /api/admin/roles/{roleId}`
- `GET /api/admin/permissions`
- `GET /api/admin/permissions/{permissionId}`
- `POST /api/admin/roles/{roleId}/permissions`

Use the schemas and required fields displayed in Swagger for each request.

### 7.6 Verify Authorization Behavior

For each protected endpoint, test the following cases:

| Scenario | Expected behavior |
|---|---|
| Request without a token | `401 Unauthorized` |
| Request with an invalid or expired token | `401 Unauthorized` |
| Authenticated user without the required permission | `403 Forbidden`, if the endpoint enforces authorization |
| Customer accessing another customer's resource | Access denied according to the endpoint's ownership checks |
| User with the required role or permission | Request succeeds when the input and business rules are valid |

The actual response can vary depending on the endpoint's authentication, authorization, ownership checks, and business validation.

### 7.7 Testing Notes

- Use the request and response schemas shown by the running Swagger instance.
- Use separate accounts to verify Customer, Admin, and SuperAdmin access.
- When role assignments change, log in again if the application embeds role or permission claims in the JWT.
- Do not expose access tokens, passwords, or secret configuration values in screenshots or public documentation.
- Verify actual role and permission requirements in the controller attributes and authorization configuration before documenting them as enforced behavior.
