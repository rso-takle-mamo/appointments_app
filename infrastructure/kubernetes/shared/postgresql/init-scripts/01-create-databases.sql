-- Create databases for each service
-- These databases will be created only if they don't exist

-- User Service Database
CREATE DATABASE userdb;


-- Create service-specific users with limited permissions
-- Each user can only access their own database

-- User Service user
CREATE USER userdb_user WITH PASSWORD 'userdb_password';


-- Grant database-level permissions
GRANT ALL PRIVILEGES ON DATABASE userdb TO userdb_user;