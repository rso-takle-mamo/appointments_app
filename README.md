# Appointment reservation system

## Project requirements

- [x] **API documentation**
  - Each service has its own swagger
  - Each service has its own markdown documentation


- [x] **Helm charts**
  - Kubernetes resources are deployed using helm
  - We use values-minikube and values-production to distinguish between the two environments


- [x] **Cloud deployment**
  - Application is deployed to Azure
  - Accessible at: http://4.165.176.7.nip.io/


- [x] **External API**
  - Vatcheckapi: https://vatcheckapi.com/
  - Checking VAT number on provider registration, so only registered companies are allowed to register.


- [x] **Healthchecks**
  - Each service has healthchecks
  - They check the database connection, kafka and also grpc connectivity in some services


- [x] **gRPC**
  - Checking if the selected timeframe for booking is actually free
  - Booking service sends a gRPC request to Availability service. Availability then return an answer via gRPC as well.


- [x] **Message Queues**
  - Kakfa is used for inter-service communication
  - User service and booking service trigger events, notification service consumes them.
  - For instance, when a user or a booking is created notification service sends emails to users


- [x] **Event Streaming**
  - Kafka is used in an event streaming fashion to sysncronize the database
  - Certain tables are replicated in many service, this ensures that the data is the same everywhere


- [x] **Centralized logs: Fluent bit -> Loki -> Grafana**
  - Logs pour from services
  - We display them in Grafana
  - We set up alerts in Grafana when many exceptions are logged in short time

- [x] **Metrics with Prometheus + Grafana**
  - Every service exposes a prmetheus /metrics endpoint
  - We can view metrics such as CPU usage on a dashboard in Grafana


- [x] **Frontend**
  - Frontend is deployed on vercel
  - Production: https://appointments-booking-rso.vercel.app/
  - Preview (dev): https://appointments-booking-rso-dev.vercel.app/


- [x] **Ingress controller**
  - Single API url, automatically switches services besed on request path
  - TLS is enabled
   

## Table of Contents

- [PRODUCTION - Azure](#production---azure)
  - [1. Set up Azure CLI and authenticate to AKS cluster](#1-set-up-azure-cli-and-authenticate-to-aks-cluster)
  - [2. Build images](#2-build-images)
  - [3. Kafka](#3-kafka)
  - [4. Deployment of services](#4-deployment-of-services)
    - [Deploying services](#deploying-services)
    - [Verifying deployment](#verifying-deployment)
  - [5. Database](#5-database)
  - [6. Accessing the services](#6-accessing-the-services)
- [DEVELOPMENT - Minikube](#development---minikube)
  - [1. Start and Configure Minikube](#1-start-and-configure-minikube)
  - [2. Build and Load Docker Images](#2-build-and-load-docker-images)
  - [3. Deploy Kafka (Event Streaming)](#3-deploy-kafka-event-streaming)
- [Deployment](#deployment)
  - [4. Deploy the Application Stack](#4-deploy-the-application-stack)
  - [5. Verify Deployment](#5-verify-deployment)
- [Accessing the Services](#accessing-the-services)
  - [6. Access Your Applications](#6-access-your-applications)
    - [Option A: Via Ingress (Recommended)](#option-a-via-ingress-recommended)
    - [Option B: Direct Port Forwarding](#option-b-direct-port-forwarding)
    - [Access the db locally](#access-the-db-locally)
- [Monitoring with Prometheus & Grafana](#monitoring-with-prometheus--grafana)
  - [7. Deploy Monitoring Stack](#7-deploy-monitoring-stack)
  - [8. Access Monitoring Tools](#8-access-monitoring-tools)
- [Development Workflow](#development-workflow)
  - [Updating Service Code](#updating-service-code)
  - [Updating Database Migrations](#updating-database-migrations)
  - [Common kubectl Commands](#common-kubectl-commands)
  - [Cleaning Up](#cleaning-up)

---

# PRODUCTION - Azure

### 1. Set up Azure CLI and authenticate to AKS cluster
```bash
# Download azure CLI
winget install --exact --id Microsoft.AzureCLI

# Login with your azure account
az login

# Download cluster info
az aks get-credentials --resource-group rso_group --name appointments-cluster

# Switch local context
kubectl config use-context appointments-cluster
```
### 2. Build images

In the produciton environment, images are built and published to Azure Container Registry accessible at appointmentsapp.azurecr.io.

```bash
# Use scripts from root dir to build images directly in ACR
# To build service images, run:
./infrastructure/scripts/build_images_ACR.sh

# To build migraiton images, run:
./infrastructure/scripts/build_migrators_ACR.sh
```
The scripts allow selecting which images we want to build. Tags are composed of three numbers, for instance 0.1.0, where the first number denotes a major release,
the second a minor one and the third a patch. The scripts allow the user to select which number to increment depending on the type and size of the upgrade. If no images are present in ACR
the tag will start from 0.0.0 but will have one of the numbers incremented depending on what is chosen. Default tag is 0.1.0.

### 3. Kafka

In production kafka is implemented using Event Hubs Namespace and Event Hubs which are kafka compatible. No extra deployment is needed.


### 4. Deployment of services

Services are deployed vith Helm. We need to use the correct file for the production environment: values-production.yaml.

#### Deploying services

```bash
# Deploy with migrations enabled
helm upgrade --install appointments-app ./infrastructure/deployments/appointments-app \
  --namespace appointments-app \
  --create-namespace \
  --values ./infrastructure/deployments/appointments-app/values-production.yaml

# Alternative: If already installed, use upgrade instead
helm upgrade appointments-app ./infrastructure/deployments/appointments-app \
  --namespace appointments-app \
  --values ./infrastructure/deployments/appointments-app/values-production.yaml
```

#### Verifying deployment

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

### 5. Database

In production we use Azure Database for PostgreSQL flexible service service to host the database.

### 6. Accessing the services

The application is ready to recieve requests. We are using the Azure Application Routing,
which relies on the already existing Ingress resource.

The API enpoints are accessible at: http://4.165.176.7.nip.io/





# DEVELOPMENT - Minikube

### 1. Start and Configure Minikube

```bash
# Set kubernetes context to minikube
kubectl config use-context minikube

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
docker build -t notification-service:latest -f services/notification-service/src/NotificationService.Api/Dockerfile services/notification-service/src --no-cache

# Build migrator images from root
docker build -t user-database-migrator:latest -f services/user-service/src/UserService.DatabaseMigrator/Dockerfile services/user-service/src --no-cache
docker build -t service-catalog-database-migrator:latest -f services/service-catalog-service/src/ServiceCatalogService.DatabaseMigrator/Dockerfile services/service-catalog-service/src --no-cache
docker build -t availability-database-migrator:latest -f services/availability-service/src/AvailabilityService.DatabaseMigrator/Dockerfile services/availability-service/src --no-cache
docker build -t booking-database-migrator:latest -f services/booking-service/src/BookingService.DatabaseMigrator/Dockerfile services/booking-service/src --no-cache
docker build -t notification-database-migrator:latest -f services/notification-service/src/NotificationService.DatabaseMigrator/Dockerfile services/notification-service/src --no-cache

# Verify built images
docker images | grep -E "(user|availability|booking|service-catalog|notification)"
```

### 3. Deploy Kafka (Event Streaming)

```bash
# Deploy Kafka
helm upgrade --install kafka ./infrastructure/deployments/kafka \
  --namespace kafka \
  --create-namespace \
  --values ./infrastructure/deployments/kafka/values-minikube.yaml

# Or use the deployment script
cd infrastructure/scripts
./deploy-kafka.sh

# Verify Kafka is running
kubectl get pods -n kafka

# Wait for Kafka to be ready
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=kafka -n kafka --timeout=300s
```

## Deployment

### 4. Deploy the Application Stack

```bash
# Deploy with migrations enabled
helm upgrade --install appointments-app ./infrastructure/deployments/appointments-app \
  --namespace appointments-app \
  --create-namespace \
  --values ./infrastructure/deployments/appointments-app/values-minikube.yaml

# Alternative: If already installed, use upgrade instead
helm upgrade appointments-app ./infrastructure/deployments/appointments-app \
  --namespace appointments-app \
  --values ./infrastructure/deployments/appointments-app/values-minikube.yaml
```

### 5. Verify Deployment

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

### 6. Access Your Applications

#### Option A: Via Ingress (Recommended)

```bash
# Port forward ingress controller (keep shell open)
kubectl port-forward -n ingress-nginx svc/ingress-nginx-controller 8080:80

# Access services through ingress
# User Service: http://localhost:8080/api/users
# Service Catalog: http://localhost:8080/api/services
# Availability: http://localhost:8080/api/availability
# Booking: http://localhost:8080/api/bookings
# Notification: has not external API endpoints
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

#### Access the db locally
```bash
# Port forward the database - (keep shell open)
kubectl port-forward -n appointments-app deployments/appointments-app-postgresql 5432:5432

# Connection string for user service:
# Host=localhost;Port=5432;Database=userdb;Username=userdb_user;Password=${userdb_password}
```

## Monitoring with Prometheus & Grafana

### 7. Deploy Monitoring Stack

```bash
# Add dependencies (if they dont yet exit)
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update
helm dependency update

# Install Prometheus and Grafana
helm install monitoring ./infrastructure/deployments/monitoring \
  --namespace monitoring \
  --create-namespace \
  --values ./infrastructure/deployments/monitoring/values-minukube.yaml

# Or upgrade if already installed
helm upgrade monitoring infrastructure/deployments/monitoring \
  --namespace monitoring \
  --values infrastructure/deployments/monitoring/values-minikube.yaml
```

### 8. Access Monitoring Tools

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
# Delete Kafka
helm uninstall kafka -n kafka
kubectl delete namespace kafka --wait

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