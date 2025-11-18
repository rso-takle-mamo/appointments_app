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

**Indexes:**
- `IX_Users_Username` (Unique)
- `IX_Users_TenantId`

#### Tenants Table
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | Tenant identifier |
| `OwnerId` | UUID | Required, Unique | User who owns this tenant |
| `BusinessName` | VARCHAR(255) | Required | Business name |
| `BusinessEmail` | VARCHAR(255) | Nullable | Business email |
| `BusinessPhone` | VARCHAR(50) | Nullable | Business phone |
| `Address` | VARCHAR(500) | Nullable | Business address |
| `Description` | TEXT | Nullable | Business description |
| `CreatedAt` | TIMESTAMPTZ | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | Required | Last update timestamp |

**Indexes:**
- `IX_Tenants_OwnerId` (Unique)

#### UserSessions Table
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | Session identifier |
| `UserId` | UUID | Required, Indexed | Reference to user |
| `TokenJti` | TEXT | Required, Unique | JWT token identifier |
| `CreatedAt` | TIMESTAMPTZ | Required | Session creation time |
| `ExpiresAt` | TIMESTAMPTZ | Required | Session expiration time |

**Indexes:**
- `IX_UserSessions_UserId`
- `IX_UserSessions_TokenJti` (Unique)
- `IX_UserSessions_ExpiresAt`

### Database Relationships
1. **Users → Tenants:** One-to-many via `TenantId` (user can belong to one tenant)
2. **Tenants → Users:** One-to-one via `OwnerId` (each tenant has one owner)
3. **Users → UserSessions:** One-to-many (user can have multiple sessions)

### Foreign Key Constraints
- `FK_Users_Tenants_TenantId` → `Tenants(Id)` (ON DELETE SET NULL)
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

#### Register Provider
```http
POST /api/auth/register/provider
Content-Type: application/json
```
**Authentication:** None required
**Description:** Creates a new provider account with associated tenant and returns JWT token

**Request Body:**
```json
{
  "username": "janes_business",
  "password": "securePassword123",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@business.com",
  "businessName": "Jane's Professional Services",
  "businessEmail": "contact@business.com",
  "businessPhone": "+1-555-123-4567",
  "address": "123 Business St, City, State 12345",
  "description": "Professional consulting and advisory services"
}
```

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
  "businessName": "Jane's Professional Services",
  "businessEmail": "contact@business.com",
  "businessPhone": "+1-555-123-4567",
  "address": "123 Business St, City, State 12345",
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

**Request Body:**
```json
{
  "businessName": "Updated Business Name",
  "businessEmail": "updated@business.com",
  "businessPhone": "+1-555-987-6543",
  "address": "456 New Address, City, State 67890",
  "description": "Updated business description"
}
```

**Response:**
```json
{
  "id": "456e7890-e89b-12d3-a456-426614174001",
  "ownerId": "123e4567-e89b-12d3-a456-426614174000",
  "businessName": "Updated Business Name",
  "businessEmail": "updated@business.com",
  "businessPhone": "+1-555-987-6543",
  "address": "456 New Address, City, State 67890",
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
| `ASPNETCORE_ENVIRONMENT` | No | Environment (Development/Production) |

## Health Checks

- `GET /health` - Complete health check including database
- `GET /health/live` - Basic service liveness check
- `GET /health/ready` - Readiness check for dependencies