  docker-compose -f docker-compose.dev.yaml --env-file .env.dev up postgres --build -d

  docker-compose -f docker-compose.dev.yaml --env-file .env.dev run --rm user-service-migrator

  docker-compose -f docker-compose.dev.yaml --env-file .env.dev run --rm service-catalog-migrator
  
  docker-compose -f docker-compose.dev.yaml --env-file .env.dev build availability-service-migrator
  
  docker-compose -f docker-compose.dev.yaml --env-file .env.dev run --rm availability-service-migrator
  
  docker-compose -f docker-compose.dev.yaml --env-file .env.dev build booking-service-migrator
  
  docker-compose -f docker-compose.dev.yaml --env-file .env.dev run --rm booking-service-migrator
  
  docker-compose -f docker-compose.dev.yaml --env-file .env.dev build notification-service-migrator
    
  docker-compose -f docker-compose.dev.yaml --env-file .env.dev run --rm notification-service-migrator

  docker-compose -f docker-compose.dev.yaml --env-file .env.dev up --build