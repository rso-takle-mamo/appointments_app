kubectl port-forward -n appointments-app service/service-catalog-service 8081:8000 -n appointments-app
# Swagger UI -> http://localhost:8081/swagger/
 
kubectl port-forward -n appointments-app service/user-service 8082:8000 -n appointments-app
# Swagger UI -> http://localhost:8082/swagger/