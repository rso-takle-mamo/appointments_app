# Appointment reservation system

- [x] **Zunanji API**
  - Vatcheckapi: https://vatcheckapi.com/
  - Checking VAT number on provider registration, so only registered companies are allowed to register.


- [x] **gRPC**
  - Checking if the selected timeframe for booking is actually free
  - Booking service sends a gRPC request to Availability service. Availability then return an answer via gRPC as well.


- [x] **Metrics with Prometheus + Grafana**


## Minikube Environment

### 1. Start and Configure Minikube

**Done through infrastructure/helm, infrastructure/kubernetes is not used here**

```bash
# Start Minikube with sufficient resources
minikube start --cpus=4 --memory=8192

# Enable required addons
minikube addons enable ingress
minikube addons enable metrics-server
```

### 2. Build and Load Docker Images

```bash
# Set Docker environment to use Minikube's Docker daemon
eval $(minikube -p minikube docker-env --shell bash)

# Build your service images from root
# --no-cache ensures we get a fresh image on every build
docker build -t user-service:latest -f services/user-service/src/UserService.Api/Dockerfile services/user-service/src --no-cache
docker build -t service-catalog-service:latest -f services/service-catalog-service/src/ServiceCatalogService.Api/Dockerfile services/service-catalog-service/src --no-cache
docker build -t availability-service:latest -f services/availability-service/src/AvailabilityService.Api/Dockerfile services/availability-service/src --no-cache
docker build -t booking-service:latest -f services/booking-service/src/BookingService.Api/Dockerfile services/booking-service/src --no-cache

# Build migrator images from root
docker build -t user-database-migrator:latest -f services/user-service/src/UserService.DatabaseMigrator/Dockerfile services/user-service/src --no-cache
docker build -t service-catalog-database-migrator:latest -f services/service-catalog-service/src/ServiceCatalogService.DatabaseMigrator/Dockerfile services/service-catalog-service/src --no-cache
docker build -t availability-database-migrator:latest -f services/availability-service/src/AvailabilityService.DatabaseMigrator/Dockerfile services/availability-service/src --no-cache
docker build -t booking-database-migrator:latest -f services/booking-service/src/BookingService.DatabaseMigrator/Dockerfile services/booking-service/src --no-cache

# Verify built images
docker images | grep -E "(user|availability|booking|service-catalog)"
```

## Deployment

### 3. Deploy the Application Stack

```bash
# Deploy with migrations enabled
helm upgrade --install appointments-app ./infrastructure/helm \
  --namespace appointments-app \
  --create-namespace \
  --values ./infrastructure/helm/values-minikube.yaml

# Alternative: If already installed, use upgrade instead
helm upgrade appointments-app ./infrastructure/helm \
  --namespace appointments-app \
  --values ./infrastructure/helm/values-minikube.yaml
```

### 4. Verify Deployment

```bash
# Check all pods
kubectl get pods -n appointments-app

# Check migrations completed successfully (might take some time)
kubectl get jobs -n appointments-app

# Check all resources in the namespace
kubectl get all -n appointments-app

# Check services
kubectl get services -n appointments-app

# Get ingress status
kubectl get ingress -n appointments-app
```

## Accessing the Services

### 5. Access Your Applications

#### Option A: Via Ingress (Recommended)

```bash
# Port forward ingress controller (keep shell open)
kubectl port-forward -n ingress-nginx svc/ingress-nginx-controller 8080:80

# Access services through ingress
# User Service: http://localhost:8080/api/users
# Service Catalog: http://localhost:8080/api/services
# Availability: http://localhost:8080/api/availability
# Booking: http://localhost:8080/api/bookings
```

#### Option B: Direct Port Forwarding

```bash
# Port forward each service individually (useful for Swagger) - (keep shell open)
kubectl port-forward -n appointments-app svc/appointments-app-user 8001:8000
kubectl port-forward -n appointments-app svc/appointments-app-servicecatalog 8002:8000
kubectl port-forward -n appointments-app svc/appointments-app-availability 8003:8000
kubectl port-forward -n appointments-app svc/appointments-app-booking 8004:8000

# Example endpoints
# Health checks:
curl http://localhost:8001/health
curl http://localhost:8002/health

# Swagger UI:
# http://localhost:8001/swagger
# http://localhost:8002/swagger
```

## Monitoring with Prometheus & Grafana

### 6. Deploy Monitoring Stack

```bash
# Add dependencies (if they dont yet exit)
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update
helm dependency update

# Install Prometheus and Grafana
helm install monitoring infrastructure/deployments/monitoring \
  --namespace monitoring \
  --create-namespace \
  --values infrastructure/deploments/monitoring/values-minikube.yaml

# Or upgrade if already installed
helm upgrade monitoring infrastructure/helm/monitoring \
  --namespace monitoring \
  --values infrastructure/helm/monitoring/values-minikube.yaml
```

### 7. Access Monitoring Tools

```bash
# Port forward for Prometheus
kubectl port-forward -n monitoring svc/monitoring-prometheus-server 9090:80
# Open: http://localhost:9090

# Check Prometheus targets
# Open: http://localhost:9090/targets
# Verify all services are "UP"

# Port forward for Grafana
kubectl port-forward -n monitoring svc/monitoring-grafana 3000:3000
# Open: http://localhost:3000
# Login: admin / admin123
# 2 dahsboards form infrastructure/monitoring/dashboards shouls be loaded
# Create the alert rule by importing the json at infrastructure/monitoring/alerts

# Port forward for Loki
kubectl port-forward svc/monitoring-loki 3100:3100 -n monitoring
# Test labels:
# curl http://localhost:3100/loki/api/v1/labels


```

## Development Workflow

### Updating Service Code

```bash
# 1. Rebuild the changed service
docker build -t user-service:latest -f services/user-service/src/UserService.Api/Dockerfile services/user-service/src

# 2. Restart the deployment
kubectl rollout restart deployment/appointments-app-user -n appointments-app

# 3. Check rollout status
kubectl rollout status deployment/appointments-app-user -n appointments-app

# 4. Check logs
kubectl logs -n appointments-app deployment/appointments-app-user -f
```

### Updating Database Migrations

```bash
# 1. Rebuild migrator image
docker build -t user-database-migrator:latest -f services/user-service/src/UserService.DatabaseMigrator/Dockerfile services/user-service/src --no-cache
...

# 2. Delete existing migration jobs
kubectl delete job -n appointments-app migration-user-job
kubectl delete job -n appointments-app migration-availability-job
kubectl delete job -n appointments-app migration-service-catalog-job
kubectl delete job -n appointments-app migration-booking-job

# 3. Re-run migrations
helm upgrade appointments-app ./infrastructure/helm \
  --namespace appointments-app \
  --values ./infrastructure/helm/values-minikube.yaml

# 4. Verify migrations completed
kubectl get jobs -n appointments-app
```

### Common kubectl Commands

```bash
# View logs
kubectl logs -n appointments-app <pod-name>                    # View logs
kubectl logs -n appointments-app deployment/<deployment-name>  # View all logs for deployment
kubectl logs -n appointments-app -f <pod-name>                 # Follow logs in real-time

# Describe resources
kubectl describe pod -n appointments-app <pod-name>            # Get detailed pod info
kubectl describe deployment -n appointments-app <deployment-name>

# Get resources
kubectl get pods -n appointments-app                           # List all pods
kubectl get deployments -n appointments-app                    # List deployments
kubectl get services -n appointments-app                      # List services
kubectl get all -n appointments-app                            # List everything

# Check events
kubectl get events -n appointments-app --sort-by='.lastTimestamp'

# Port forwarding
kubectl port-forward -n appointments-app svc/<service-name> <local-port>:<service-port>
```


### Cleaning Up

```bash
# Delete application
helm uninstall appointments-app -n appointments-app
kubectl delete namespace appointments-app --wait

# Delete monitoring
helm uninstall monitoring -n monitoring
kubectl delete namespace monitoring --wait

# Stop Minikube (Optional)
minikube stop

# Delete Minikube cluster (complete cleanup, deletes all data)
minikube delete

# Remove Docker images
eval $(minikube -p minikube docker-env --shell bash)
docker rmi user-service:latest service-catalog-service:latest availability-service:latest booking-service:latest
docker rmi user-database-migrator:latest service-catalog-database-migrator:latest availability-database-migrator:latest booking-database-migrator:latest
```