# Microservices Technical Specification

## 1. User & Authentication Service

### Endpoints
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/logout` - Invalidate token
- `GET /api/users/{id}` - Get user profile
- `PUT /api/users/{id}` - Update user profile
- `GET /api/tenants/{id}` - Get tenant details
- `PUT /api/tenants/{id}` - Update tenant details
- `POST /api/tenants` - Create tenant (provider registration)

### Data Models
```csharp
User {
  Guid Id
  string Username (unique, indexed)
  string Password
  string FirstName
  string LastName
  UserRole Role (enum: Provider, Customer)
  Guid? TenantId (nullable, indexed) // NULL for customers, populated for providers
  DateTime CreatedAt
  DateTime UpdatedAt
}

Tenant {
  Guid Id
  Guid OwnerId (unique, indexed) // The Provider user who owns this tenant
  string BusinessName
  string? BusinessEmail (optional)
  string? BusinessPhone (optional)
  string Address
  string? Description (optional)
  DateTime CreatedAt
  DateTime UpdatedAt
}
```

### Role Model Explanation

**Two User Roles:**
1. **Customer** - Regular users who book appointments
   - `TenantId = NULL`
   - Can view providers and book services
   - Can only access their own bookings

2. **Provider** - Service providers who offer appointments
   - `TenantId = {some-guid}` (linked to their business)
   - Owns a Tenant (business entity)
   - Can manage services, availability, and view all bookings for their tenant

**Registration Flows:**
- **Customer Registration**: `POST /api/auth/register` → Creates User with Role=Customer, TenantId=NULL
- **Provider Registration**: `POST /api/auth/register-provider` → Creates User with Role=Provider AND creates Tenant, links them

**JWT Claims:**
```json
{
  "sub": "user-guid",
  "username": "johndoe",
  "role": "Provider", // or "Customer"
  "tenantId": "tenant-guid", // or null for customers
  "exp": 1234567890
}
```

**Authorization Pattern:**
```csharp
// Customers can only access their own data
if (role == Customer && userId != requestedUserId) 
  return Forbidden;

// Providers can access all data within their tenant
if (role == Provider && tenantId != requestedTenantId) 
  return Forbidden;
```

### Database Tables
- `Users` (PK: Id, Indexes: Email, TenantId)
- `Tenants` (PK: Id)
---

## 2. Service Catalog Service

### Endpoints
- `POST /api/services` - Create service
- `GET /api/services` - List services (filtered by tenantId)
- `GET /api/services/{id}` - Get service details
- `PUT /api/services/{id}` - Update service
- `DELETE /api/services/{id}` - Delete service
- `PATCH /api/services/{id}/toggle` - Activate/deactivate service
- `GET /api/categories` - List categories
- `POST /api/categories` - Create category

### Data Models
```csharp
Service {
  Guid Id
  Guid TenantId (indexed)
  string Name
  string Description
  decimal Price
  int DurationMinutes
  Guid? CategoryId (nullable, indexed)
  bool IsActive
  DateTime CreatedAt
  DateTime UpdatedAt
}

Category {
  Guid Id
  string Name
  string Description
  DateTime CreatedAt
}
```

### Database Tables
- `Services` (PK: Id, Indexes: TenantId, CategoryId, IsActive)
- `Categories` (PK: Id)

---

## 3. Availability Service

### Endpoints
- `POST /api/availability/working-hours` - Set working hours
- `GET /api/availability/working-hours/{tenantId}` - Get working hours
- `PUT /api/availability/working-hours/{id}` - Update working hours
- `POST /api/availability/time-blocks` - Block time (vacation, break)
- `GET /api/availability/time-blocks/{tenantId}` - Get blocked times
- `DELETE /api/availability/time-blocks/{id}` - Remove time block
- `GET /api/availability/free-slots` - Calculate free slots (query: tenantId, serviceId, date, duration)
- `POST /api/availability/google-calendar/connect` - OAuth2 connection
- `POST /api/availability/google-calendar/sync` - Manual sync trigger
- `GET /api/availability/buffer-time/{tenantId}` - Get buffer settings
- `PUT /api/availability/buffer-time/{tenantId}` - Update buffer time

### Data Models
```csharp
WorkingHours {
  Guid Id
  Guid TenantId (indexed)
  DayOfWeek Day (indexed)
  TimeOnly StartTime
  TimeOnly EndTime
  bool IsActive
  DateTime CreatedAt
  DateTime UpdatedAt
}

TimeBlock {
  Guid Id
  Guid TenantId (indexed)
  DateTime StartDateTime (indexed)
  DateTime EndDateTime (indexed)
  BlockType Type (enum: Vacation, Break, Custom)
  string Reason
  DateTime CreatedAt
}

GoogleCalendarIntegration {
  Guid Id
  Guid TenantId (unique, indexed)
  string GoogleCalendarId
  string RefreshToken (encrypted)
  string AccessToken (encrypted)
  DateTime TokenExpiresAt
  DateTime LastSyncAt
  DateTime CreatedAt
}

BufferTime {
  Guid Id
  Guid TenantId (unique)
  int BufferMinutes
  DateTime UpdatedAt
}
```

### Database Tables
- `WorkingHours` (PK: Id, Indexes: TenantId, Day)
- `TimeBlocks` (PK: Id, Indexes: TenantId, StartDateTime, EndDateTime)
- `GoogleCalendarIntegrations` (PK: Id, Unique: TenantId)
- `BufferTimes` (PK: Id, Unique: TenantId)

---

## 4. Booking Service

### Endpoints
- `POST /api/bookings` - Create booking
- `GET /api/bookings/{id}` - Get booking details
- `GET /api/bookings` - List bookings (query: tenantId, customerId, status, date range)
- `PUT /api/bookings/{id}/reschedule` - Reschedule booking
- `PUT /api/bookings/{id}/cancel` - Cancel booking
- `PUT /api/bookings/{id}/confirm` - Confirm booking
- `PUT /api/bookings/{id}/complete` - Mark as completed
- `GET /api/bookings/{id}/events` - Get event history

### Data Models
```csharp
// Event Sourcing
BookingEvent {
  Guid Id
  Guid BookingId (indexed)
  EventType Type (enum: Created, Confirmed, Rescheduled, Cancelled, Completed)
  string EventData (JSON)
  Guid ActorId (who made the change)
  DateTime CreatedAt
  int Version (for optimistic concurrency)
}

// Materialized View
Booking {
  Guid Id (PK, indexed)
  Guid TenantId (indexed)
  Guid CustomerId (indexed)
  Guid ServiceId (indexed)
  DateTime StartDateTime (indexed)
  DateTime EndDateTime
  BookingStatus Status (enum: Pending, Confirmed, Completed, Cancelled)
  string CustomerNotes
  string CancellationReason
  int CurrentVersion
  DateTime CreatedAt
  DateTime UpdatedAt
}
```

### Database Tables
- `BookingEvents` (PK: Id, Indexes: BookingId, CreatedAt)
- `Bookings` (Materialized view, PK: Id, Indexes: TenantId, CustomerId, ServiceId, StartDateTime, Status)

---

## 5. Notification Service

### Endpoints
- `POST /api/notifications/send` - Send notification (internal only)
- `GET /api/notifications/{userId}` - Get user notifications
- `GET /api/notifications/templates` - List email templates
- `PUT /api/notifications/templates/{id}` - Update template

### Data Models
```csharp
NotificationQueue {
  Guid Id
  NotificationType Type (enum: BookingConfirmation, Reminder, Cancellation, Reschedule)
  string RecipientEmail
  string Subject
  string Body
  Dictionary<string, string> TemplateData
  NotificationStatus Status (enum: Pending, Sent, Failed)
  DateTime ScheduledFor
  DateTime? SentAt
  int RetryCount
  string ErrorMessage
  DateTime CreatedAt
}

EmailTemplate {
  Guid Id
  NotificationType Type (unique)
  string Subject
  string BodyHtml
  string BodyText
  DateTime UpdatedAt
}

NotificationLog {
  Guid Id
  Guid NotificationQueueId (indexed)
  NotificationStatus Status
  string Message
  DateTime CreatedAt
}
```

### Database Tables
- `NotificationQueue` (PK: Id, Indexes: Status, ScheduledFor)
- `EmailTemplates` (PK: Id, Unique: Type)
- `NotificationLogs` (PK: Id, Index: NotificationQueueId)

---

## Cross-Cutting Concerns

### Message Broker Events (RabbitMQ)

#### User Service Publishes:
- `UserRegistered` → Notification Service
- `TenantCreated` → Notification Service

#### Booking Service Publishes:
- `BookingCreated` → Notification Service, Availability Service
- `BookingConfirmed` → Notification Service
- `BookingCancelled` → Notification Service, Availability Service
- `BookingRescheduled` → Notification Service, Availability Service
- `BookingCompleted` → Notification Service

#### Availability Service Publishes:
- `GoogleCalendarSynced` → Booking Service (for conflict detection)

### API Gateway / Ingress Controller
- Authentication middleware (JWT validation)
- Rate limiting
- Request routing
- CORS handling
- Tenant isolation (multi-tenancy filter)

### Common Patterns

#### Tenant Isolation
All services implement tenant-level data isolation:
```csharp
// Every query filtered by TenantId from JWT claims
WHERE TenantId = @currentUserTenantId
```

#### Pagination
Standard pagination for list endpoints:
```csharp
PagedResult<T> {
  List<T> Items
  int PageNumber
  int PageSize
  int TotalCount
  int TotalPages
}
```

#### Error Handling
Standard error response:
```csharp
ErrorResponse {
  string Code
  string Message
  Dictionary<string, string[]> Errors (validation)
  DateTime Timestamp
}
```

---

## Database Strategy

### Per-Service Databases
- **UserDb** - PostgreSQL for User & Auth Service
- **ServiceCatalogDb** - PostgreSQL for Service Catalog
- **AvailabilityDb** - PostgreSQL for Availability Service
- **BookingDb** - PostgreSQL for Booking Service (event store)
- **NotificationDb** - PostgreSQL for Notification Service

### Connection Strings
Each service has its own connection string, managed via Kubernetes secrets or Azure Key Vault.

### Migrations
Use Entity Framework Core migrations per service:
```bash
dotnet ef migrations add InitialCreate --project ServiceName
dotnet ef database update --project ServiceName
```

---

## Security Considerations

### Authentication Flow
1. User logs in → User Service returns JWT + Refresh Token
2. JWT contains: UserId, Email, Role, TenantId, Expiry (15 min)
3. Refresh Token stored in database (30 days, httpOnly cookie)
4. All services validate JWT via shared secret or public key

### Authorization
- Customers can only access their own bookings
- Providers can access all bookings for their tenant
- Tenant isolation enforced at database query level

### Data Protection
- Passwords: BCrypt hashing
- Google tokens: Encrypted at rest (AES-256)
- Sensitive data: HTTPS only, no logging of tokens

---

## Deployment Configuration

### Docker Compose (Development)
- 5 microservices
- 5 PostgreSQL instances
- 1 RabbitMQ instance
- 1 API Gateway (Nginx or Ocelot)

### Kubernetes (Production - AKS)
- Deployments per service (3 replicas minimum)
- Horizontal Pod Autoscaler
- Azure PostgreSQL Flexible Server (per service)
- Azure Service Bus (alternative to RabbitMQ)
- Azure Application Gateway (ingress)
- Azure Key Vault for secrets

### Environment Variables (per service)
```
DATABASE_CONNECTION_STRING
RABBITMQ_HOST
JWT_SECRET_KEY
JWT_ISSUER
JWT_AUDIENCE
GOOGLE_CLIENT_ID
GOOGLE_CLIENT_SECRET
SENDGRID_API_KEY (or Azure Communication Services)
ASPNETCORE_ENVIRONMENT
```

---

## Development Priorities

### Phase 1: Core Setup (Weeks 1-2)
1. User & Auth Service (registration, login, JWT)
2. Service Catalog Service (CRUD services)
3. Basic API Gateway setup

### Phase 2: Booking Flow (Weeks 3-4)
4. Availability Service (working hours, free slots calculation)
5. Booking Service (create, cancel bookings - basic state)
6. RabbitMQ integration

### Phase 3: Advanced Features (Weeks 5-6)
7. Event Sourcing in Booking Service
8. Notification Service (email sending)
9. Google Calendar integration

### Phase 4: Polish & Deploy (Week 7)
10. Frontend integration
11. Kubernetes deployment
12. Testing & bug fixes

---

## Testing Strategy

### Unit Tests
- Service layer logic (business rules)
- Repository patterns (mocked DbContext)
- Event handlers

### Integration Tests
- Database operations (TestContainers for PostgreSQL)
- RabbitMQ message publishing/consuming
- API endpoint tests (WebApplicationFactory)

### E2E Tests
- Full user journey (register → book → receive email)
- Postman/Newman collections for API testing

---

This specification provides a complete blueprint for implementation. Each service is independent, scalable, and follows microservices best practices.