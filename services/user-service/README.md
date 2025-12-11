# UserService

## Overview

The UserService manages user authentication, profiles, and tenant information for the appointments system. It handles customer and provider registration, JWT-based authentication, and user account management with support for multi-tenant architecture.

## Database

### Tables and Schema

#### Users Table
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | User identifier |
| `Username` | VARCHAR(255) | Required, Unique | User login name |
| `Password` | VARCHAR(255) | Required | Hashed password (PBKDF2) |
| `FirstName` | VARCHAR(255) | Required | First name |
| `LastName` | VARCHAR(255) | Required | Last name |
| `Email` | VARCHAR(255) | Required | Email address |
| `Role` | INTEGER | Required | 0=Provider, 1=Customer (stored as int, enum in code) |
| `TenantId` | UUID | Foreign Key, Nullable | Reference to tenant |
| `CreatedAt` | TIMESTAMPTZ | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | Required | Last update timestamp |


#### Tenants Table
| Column | Type          | Constraints | Description |
|--------|---------------|-------------|-------------|
| `Id` | UUID          | Primary Key | Tenant identifier |
| `OwnerId` | UUID          | Required, Unique | User who owns this tenant |
| `VatNumber` | VARCHAR(20)   | Required | VAT number (validated via external API) |
| `BusinessName` | VARCHAR(255)  | Required | Business name (from VAT validation) |
| `BusinessEmail` | VARCHAR(255)  | Nullable | Business email |
| `BusinessPhone` | VARCHAR(50)   | Nullable | Business phone |
| `Address` | VARCHAR(500)  | Nullable | Business address (from VAT validation) |
| `Description` | VARCHAR(1000) | Nullable | Business description |
| `CreatedAt` | TIMESTAMPTZ   | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ   | Required | Last update timestamp |


#### UserSessions Table
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | Session identifier |
| `UserId` | UUID | Required, Indexed | Reference to user |
| `TokenJti` | VARCHAR(255) | Required, Unique | JWT token identifier |
| `CreatedAt` | TIMESTAMPTZ | Required | Session creation time |
| `ExpiresAt` | TIMESTAMPTZ | Required | Session expiration time |


### Database Relationships
1. **Users → Tenants:** One-to-many via `TenantId` (user can belong to one tenant)
2. **Tenants → Users:** One-to-one via `OwnerId` (each tenant has one owner)
3. **Users → UserSessions:** One-to-many (user can have multiple sessions)

### Foreign Key Constraints
- `FK_Tenants_Users_OwnerId` → `Users(Id)` (ON DELETE CASCADE)
- `FK_UserSessions_Users_UserId` → `Users(Id)` (ON DELETE CASCADE)

## API Endpoints

### Authentication Endpoints (`/api/auth`)

#### Register Customer
```http
POST /api/auth/register/customer
Content-Type: application/json
```
**Authentication:** None required
**Description:** Creates a new customer user account and returns JWT token

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "securePassword123",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": "2024-01-01T12:00:00Z"
}
```

#### Check VAT Number
```http
GET /api/auth/check-vat?vatNumber=LU26375245
```
**Authentication:** None required
**Description:** Validates a VAT number and retrieves company information from external VAT validation service

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `vatNumber` | string | Yes | VAT number to validate (format: CC followed by numbers) |

**Response (Valid VAT):**
```json
{
  "isValid": true,
  "companyName": "AMAZON EUROPE CORE S.A R.L.",
  "address": "38, AVENUE JOHN F. KENNEDY\nL-1855  LUXEMBOURG",
  "countryCode": "LU",
  "vatNumber": "LU26375245"
}
```

**Response (Invalid VAT):**
```json
{
  "message": "Invalid VAT number"
}
```

#### Register Provider
```http
POST /api/auth/register/provider
Content-Type: application/json
```
**Authentication:** None required
**Description:** Creates a new provider account with associated tenant and returns JWT token. The VAT number is validated and company details are populated automatically.

**Request Body:**
```json
{
  "username": "janes_business",
  "password": "securePassword123",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@business.com",
  "vatNumber": "LU26375245",
  "businessEmail": "contact@business.com",
  "businessPhone": "+1-555-123-4567",
  "description": "Professional consulting and advisory services"
}
```

**Note:**
- `vatNumber` is required and will be validated
- `businessName` and `address` are automatically populated from VAT validation and cannot be set manually
- Optional fields: `businessEmail`, `businessPhone`, `description`

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": "2024-01-01T12:00:00Z"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json
```
**Authentication:** None required
**Description:** Authenticates user credentials and returns JWT token

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "securePassword123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": "2024-01-01T12:00:00Z"
}
```

#### Logout
```http
POST /api/auth/logout
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Invalidates the current user session

**Response:**
```json
{
  "message": "Logout successful"
}
```

### User Profile Endpoints (`/api/users`)

#### Get User Profile
```http
GET /api/users/me
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Retrieves the current user's profile information

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "username": "john_doe",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "role": "Customer",
  "tenantId": null,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z"
}
```

#### Update User Profile
```http
PATCH /api/users/me
Content-Type: application/json
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Updates the current user's profile information

**Request Body:**
```json
{
  "username": "john_doe_updated",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com"
}
```

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "username": "john_doe_updated",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "role": "Customer",
  "tenantId": null,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T11:00:00Z"
}
```

#### Delete User Account
```http
DELETE /api/users/me
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Permanently deletes the current user account and all associated data

**Response:** `204 No Content`

### Tenant Management Endpoints (`/api/tenants`)

#### Get Tenant
```http
GET /api/tenants/{id}
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Retrieves tenant information (users can only access their own tenant)

**Response:**
```json
{
  "id": "456e7890-e89b-12d3-a456-426614174001",
  "ownerId": "123e4567-e89b-12d3-a456-426614174000",
  "vatNumber": "LU26375245",
  "businessName": "AMAZON EUROPE CORE S.A R.L.",
  "businessEmail": "contact@business.com",
  "businessPhone": "+1-555-123-4567",
  "address": "38, AVENUE JOHN F. KENNEDY\nL-1855  LUXEMBOURG",
  "description": "Professional consulting and advisory services",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z"
}
```

#### Update Tenant
```http
PATCH /api/tenants/{id}
Content-Type: application/json
Authorization: Bearer <token>
```
**Authentication:** Required (JWT token)
**Description:** Updates tenant information (only tenant owners can update)

**Note:** `businessName` and `address` cannot be updated as they are sourced from VAT validation. Only optional fields can be updated.

**Request Body:**
```json
{
  "businessEmail": "updated@business.com",
  "businessPhone": "+1-555-987-6543",
  "description": "Updated business description"
}
```

**Response:**
```json
{
  "id": "456e7890-e89b-12d3-a456-426614174001",
  "ownerId": "123e4567-e89b-12d3-a456-426614174000",
  "vatNumber": "LU26375245",
  "businessName": "AMAZON EUROPE CORE S.A R.L.",
  "businessEmail": "updated@business.com",
  "businessPhone": "+1-555-987-6543",
  "address": "38, AVENUE JOHN F. KENNEDY\nL-1855  LUXEMBOURG",
  "description": "Updated business description",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T11:00:00Z"
}
```

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `DATABASE_CONNECTION_STRING` | Yes | PostgreSQL connection string |
| `JWT_SECRET_KEY` | Yes | JWT signing key (minimum 128 bits) |
| `VATCHECK_API_KEY` | Yes | API key for VAT validation service |
| `ASPNETCORE_ENVIRONMENT` | No | Environment (Development/Production) |

## Health Checks

- `GET /health` - Complete health check including database
- `GET /health/live` - Basic service liveness check
- `GET /health/ready` - Readiness check for dependencies