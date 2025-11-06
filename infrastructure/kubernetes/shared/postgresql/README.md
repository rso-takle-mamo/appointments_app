# Shared PostgreSQL Deployment

This directory contains Kubernetes manifests for deploying a shared PostgreSQL instance that serves multiple microservices with separate databases.

## Architecture Overview

```
Kubernetes Cluster:
├── shared-postgresql pod
│   ├── userdb (User Service database)
│   ├── appointmentdb (Appointment Service database)
│   └── notificationdb (Notification Service database)
├── user-service pod → connects to userdb
├── appointment-service pod → connects to appointmentdb
└── notification-service pod → connects to notificationdb
```

## Security Model

- **Database-level isolation**: Each service has its own database
- **User-level permissions**: Each service has a dedicated database user
- **Service-specific secrets**: Each service gets its own connection string

## Files Overview

### PostgreSQL Infrastructure
- `postgresql-pvc.yaml` - Persistent storage (5Gi)
- `postgresql-secret.yaml` - Admin credentials (password in secret)
- `postgresql-configmap.yaml` - PostgreSQL configuration
- `postgresql-init-configmap.yaml` - Database and user initialization scripts
- `postgresql-deployment.yaml` - PostgreSQL pod deployment
- `postgresql-service.yaml` - Internal cluster service

### Service Secrets (`../secrets/`)
- `user-service-secret.yaml` - User Service connection string

## Connection Details

### Admin Access
- **Host:** `shared-postgresql-service`
- **Port:** `5432`
- **Database:** `postgres`
- **Username:** `postgres`
- **Password:** From `shared-postgresql-secret` (adminpassword)

### Service Authentication

Each service authenticates using its own dedicated database user and connection string secret:

#### User Service Authentication Flow:

1. **Connection String Secret:** `user-service-db-secret`
   ```
   Host=shared-postgresql-service;Port=5432;Database=userdb;Username=userdb_user;Password=userdb_password
   ```

2. **How User Service Uses It:**
   ```yaml
   # In user-service deployment:
   env:
   - name: DATABASE_CONNECTION_STRING
     valueFrom:
       secretKeyRef:
         name: user-service-db-secret
         key: connection-string
   ```

3. **Application Code (.NET Example):**
   ```csharp
   var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
   services.AddDbContext<UserDbContext>(options =>
       options.UseNpgsql(connectionString));
   ```

#### Security Model:
- **Database Isolation:** Each service has its own database (userdb, appointmentdb, etc.)
- **User Isolation:** Each service user can only access its own database
- **Secret Management:** Connection strings stored as Kubernetes secrets
- **No Cross-Access:** userdb_user cannot access other databases

## Databases and Users

### Databases Created
- `userdb` - User Service database with Users and Tenants tables

### Users Created
- `userdb_user` - Access to userdb only
- `postgres` - Admin access (from secret)

## Quick Start

### Deploy Shared PostgreSQL
```bash
./scripts/deploy-shared-postgresql.sh
```

### Test the Deployment
```bash
./scripts/test-shared-postgresql.sh
```

### Manual Connection Examples
```bash
# Connect as admin
kubectl exec -it deployment/shared-postgresql -- psql -U postgres -d postgres

# Connect to User Service database
kubectl exec -it deployment/shared-postgresql -- psql -U userdb_user -d userdb

# List all databases
kubectl exec deployment/shared-postgresql -- psql -U postgres -d postgres -c "\l"

# List all users
kubectl exec deployment/shared-postgresql -- psql -U postgres -d postgres -c "\du"
```

## Database Schemas

### User Service Schema (userdb)
- **Users table** - User accounts with roles and tenant associations
- **Tenants table** - Business information for provider users
- **Indexes** - Optimized queries for username and tenant lookups
- **Triggers** - Automatic UpdatedAt timestamp management

## Adding a New Service

1. **Create a new database** in `01-create-databases.sql`
2. **Create a new database user** with limited permissions
3. **Add schema initialization script** (e.g., `05-newservice-schema.sql`)
4. **Create service-specific secret** in `../secrets/`
5. **Update ConfigMap** with the new schema script

## Resource Allocation

- **Memory:** 512Mi request, 1Gi limit
- **CPU:** 500m request, 1000m limit
- **Storage:** 5Gi persistent volume
- **Connections:** 200 max connections

## Monitoring and Health Checks

- **Liveness Probe:** Every 10 seconds (starts after 30s)
- **Readiness Probe:** Every 5 seconds (starts after 5s)
- **Logging:** All SQL statements logged

## Backup Strategy

```bash
# Backup all databases
kubectl exec deployment/shared-postgresql -- pg_dump -U postgres > full-backup.sql

# Backup specific database
kubectl exec deployment/shared-postgresql -- pg_dump -U postgres userdb > userdb-backup.sql

# Restore database
kubectl exec -i deployment/shared-postgresql -- psql -U postgres userdb < userdb-backup.sql
```

## Cleanup

```bash
kubectl delete -f infrastructure/kubernetes/shared/postgresql/
kubectl delete -f infrastructure/kubernetes/shared/secrets/
```

## Troubleshooting

### Connection Issues
```bash
# Check pod status
kubectl get pods -l app=shared-postgresql

# Check service
kubectl get service shared-postgresql-service

# Check logs
kubectl logs deployment/shared-postgresql

# Test connectivity
kubectl exec deployment/shared-postgresql -- pg_isready -U postgres
```

### Permission Issues
```bash
# Check user permissions
kubectl exec deployment/shared-postgresql -- psql -U postgres -d postgres -c "\du"

# Test service user access
kubectl exec deployment/shared-postgresql -- psql -U userdb_user -d userdb -c "SELECT current_user, current_database();"
```

This shared approach provides efficient resource usage while maintaining proper database isolation for microservices.