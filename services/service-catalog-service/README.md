# ServiceCatalogService

## Overview

The ServiceCatalogService manages service and category information for the appointments system. It provides endpoints for creating, reading, updating, and deleting services and categories with support for multi-tenant architecture and role-based access control.

## Database

### Tables and Schema

#### Services Table
| Column | Type          | Constraints                                           | Description |
|--------|---------------|-------------------------------------------------------|-------------|
| `Id` | UUID          | Primary Key                                           | Service identifier |
| `TenantId` | UUID          | Required, Foreign Key                                 | Reference to tenant |
| `Name` | VARCHAR(255)  | Required                                              | Service name |
| `Description` | VARCHAR(1000) | Nullable                                              | Service description |
| `Price` | NUMERIC(10,2) | Required, Price >= 0                                  | Service price |
| `DurationMinutes` | INTEGER       | Required, DurationMinutes > 0, DurationMinutes <= 480 | Service duration in minutes |
| `CategoryId` | UUID          | Foreign Key, Nullable                                 | Reference to category |
| `IsActive` | BOOLEAN       | Required                                              | Whether service is active |
| `CreatedAt` | TIMESTAMPTZ   | Required                                              | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ   | Required                                              | Last update timestamp |


#### Categories Table
| Column | Type         | Constraints | Description |
|--------|--------------|-------------|-------------|
| `Id` | UUID         | Primary Key | Category identifier |
| `TenantId` | UUID         | Required, Foreign Key | Reference to tenant |
| `Name` | VARCHAR(100) | Required | Category name |
| `Description` | VARCHAR(500) | Nullable | Category description |
| `CreatedAt` | TIMESTAMPTZ  | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ  | Required | Last update timestamp |


#### Tenants Table
**Note:** This table replicates data from the UserService for service catalog queries. TODO: Add Kafka/RabbitMQ to synchronize it.

| Column | Type         | Constraints | Description |
|--------|--------------|-------------|-------------|
| `Id` | UUID         | Primary Key | Tenant identifier |
| `BusinessName` | VARCHAR(200) | Required | Business name |
| `Address` | VARCHAR(500) | Required | Business address |
| `CreatedAt` | TIMESTAMPTZ  | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ  | Required | Last update timestamp |


### Database Relationships
1. **Services → Categories:** Many-to-one via `CategoryId` ( Multiple services can belong to one category, but a service doesn't need a category)
2. **Categories → Services:** One-to-many (category can have multiple services)
3. **Services/Categories → Tenants:** Many-to-one via `TenantId` (service/category belongs to one tenant)

### Foreign Key Constraints
- `FK_Services_Categories_CategoryId` → `Categories(Id)` (ON DELETE SET NULL)
- `FK_Services_Tenants_TenantId` → `Tenants(Id)` (ON DELETE CASCADE)
- `FK_Categories_Tenants_TenantId` → `Tenants(Id)` (ON DELETE CASCADE)

## API Endpoints

### Services Controller (`/api/services`)

#### List Services
```http
GET /api/services
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Lists services with filtering and pagination

#### Query Parameters:
**Customer only params:**
- `tenantId` (guid, required for customers, forbidden for providers): Filter by tenant ID
- `address` (string, optional): Filter by tenant address (max 500 chars)
- `businessName` (string, optional): Filter by tenant business name (max 200 chars)

**Filters:**
- `serviceName` (string, optional): Filter by service name (max 100 chars)
- `categoryId` (guid, optional): Filter by category ID
- `categoryName` (string, optional): Filter by category name (max 100 chars)
- `minPrice` (decimal, optional): Minimum price filter (0-9,999,999,999.99)
- `maxPrice` (decimal, optional): Maximum price filter (0-9,999,999,999.99)
- `maxDuration` (int, optional): Maximum duration filter in minutes (1-480)
- `isActive` (bool, optional): Filter by active status

**Pagination and sorting:**
- `offset` (int, default: 0): Number of items to skip
- `limit` (int, default: 100): Maximum number of items to return (1-100)
- `orderBy` (enum, optional): Sort field (Name, Price, Duration)
- `orderDirection` (enum, optional): Sort direction (Ascending, Descending)

**Response:**
```json
{
  "offset": 0,
  "limit": 100,
  "totalCount": 1,
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "tenantId": "456e7890-e89b-12d3-a456-426614174001",
      "name": "Professional Consultation",
      "description": "Expert consultation services",
      "price": 150.00,
      "durationMinutes": 60,
      "categoryId": "789e1234-e89b-12d3-a456-426614174002",
      "categoryName": "Consulting",
      "isActive": true,
      "businessName": "Professional Services Inc",
      "address": "123 Business St, City, State 12345",
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z",
    }
  ]
}
```

#### Get Service
```http
GET /api/services/{id}
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Retrieves a specific service by ID

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "tenantId": "456e7890-e89b-12d3-a456-426614174001",
  "name": "Professional Consultation",
  "description": "Expert consultation services",
  "price": 150.00,
  "durationMinutes": 60,
  "categoryId": "789e1234-e89b-12d3-a456-426614174002",
  "categoryName": "Consulting",
  "isActive": true,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "businessName": "Professional Services Inc",
  "address": "123 Business St, City, State 12345"
}
```

#### Create Service (Providers Only)
```http
POST /api/services
Content-Type: application/json
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Creates a new service (only for the provider's tenant)

**Request Body:**
```json
{
  "name": "Professional Consultation",
  "description": "Expert consultation services",
  "price": 150.00,
  "durationMinutes": 60,
  "categoryId": "789e1234-e89b-12d3-a456-426614174002",
  "isActive": true
}
```

**Validation Rules:**
- `name`: Required, max 255 chars, alphanumeric with spaces, hyphens, and periods
- `description`: Optional, max 1000 chars
- `price`: Required, between 0 and 9,999,999,999.99
- `durationMinutes`: Between 1 and 480 minutes

**Response:** `201 Created` with the created service object in the response body

#### Update Service (Providers Only)
```http
PATCH /api/services/{id}
Content-Type: application/json
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Updates an existing service (only if it belongs to the provider's tenant)

**Request Body:** (All fields optional)
```json
{
  "name": "Updated Consultation Service",
  "price": 175.00,
  "durationMinutes": 90,
  "isActive": false
}
```

#### Delete Service (Providers Only)
```http
DELETE /api/services/{id}
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Deletes a service (only if it belongs to the provider's tenant)
**Response:** `204 No Content`

### Categories Controller (`/api/categories`)

#### Get Service Category
```http
GET /api/services/{serviceId}/category
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Retrieves the category for a specific service

**Response:**
```json
{
  "id": "789e1234-e89b-12d3-a456-426614174002",
  "tenantId": "456e7890-e89b-12d3-a456-426614174001",
  "name": "Consulting",
  "description": "Professional consulting services",
  "createdAt": "2024-01-01T09:00:00Z",
  "updatedAt": "2024-01-01T09:00:00Z"
}
```

#### List Categories
```http
GET /api/categories
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Lists categories

**Query Parameters:**
- `tenantId` (guid, required for customers): Tenant ID filter

- `offset` (int, default: 0): Number of items to skip
- `limit` (int, default: 100): Maximum number of items to return (1-100)

**Note:**
- **Customers:** Must provide `tenantId` parameter
- **Providers:** Auto-filtered by their tenant, cannot specify tenantId

**Response:**
```json
{
  "offset": 0,
  "limit": 100,
  "totalCount": 1,
  "data": [
    {
      "id": "789e1234-e89b-12d3-a456-426614174002",
      "tenantId": "456e7890-e89b-12d3-a456-426614174001",
      "name": "Consulting",
      "description": "Professional consulting services",
      "createdAt": "2024-01-01T09:00:00Z",
      "updatedAt": "2024-01-01T09:00:00Z"
    }
  ]
}
```

#### Get Category by ID (Providers Only)
```http
GET /api/categories/{categoryId}
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Retrieves a specific category by ID (providers only)

**Response:**
```json
{
  "id": "789e1234-e89b-12d3-a456-426614174002",
  "tenantId": "456e7890-e89b-12d3-a456-426614174001",
  "name": "Consulting",
  "description": "Professional consulting services",
  "createdAt": "2024-01-01T09:00:00Z",
  "updatedAt": "2024-01-01T09:00:00Z"
}
```

#### Create Category (Providers Only)
```http
POST /api/categories
Content-Type: application/json
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Creates a new category for the provider's tenant

**Request Body:**
```json
{
  "name": "Consulting",
  "description": "Professional consulting services"
}
```

**Validation Rules:**
- `name`: Required, 2-100 characters
- `description`: Optional, max 500 characters

**Response:** `201 Created` with the created category object

#### Update Category (Providers Only)
```http
PATCH /api/categories/{categoryId}
Content-Type: application/json
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Updates a category (only if it belongs to the provider's tenant)

**Request Body:** (All fields optional)
```json
{
  "name": "Updated Category Name",
  "description": "Updated category description"
}
```

**Validation Rules:**
- `name`: Optional, max 100 chars
- `description`: Optional, max 500 characters

#### Delete Category (Providers Only)
```http
DELETE /api/categories/{categoryId}
Authorization: Bearer <token>
```
**Authentication:** Required (Provider role)
**Description:** Deletes a category (only if it belongs to the provider's tenant)
**Response:** `204 No Content`

## Role-Based Access Control

### Customers:
- ✅ Read services from all tenants
- ✅ Use Address/BusinessName filtering
- ✅ Use all service filters
- ❌ Cannot create/update/delete services or categories

### Providers:
- ✅ Access only own tenant data
- ✅ Create/update/delete services and categories
- ✅ Use service filters (except tenant-based ones)
- ❌ Cannot use Address/BusinessName filters
- ❌ Cannot access other tenants' data

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `DATABASE_CONNECTION_STRING` | Yes | PostgreSQL connection string |
| `JWT_SECRET_KEY` | Yes | JWT signing key (minimum 128 bits) |
| `ASPNETCORE_ENVIRONMENT` | No | Environment (Development/Production) |

## Health Checks

- `GET /health` - Complete health check including database
- `GET /health/live` - Basic service liveness check
- `GET /health/ready` - Readiness check for dependencies
