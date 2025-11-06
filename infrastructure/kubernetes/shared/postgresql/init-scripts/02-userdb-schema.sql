-- User Service Database Schema
-- This script will be executed in the userdb database

-- Switch to userdb database context
\c userdb;

-- Verify we're in the correct database
SELECT current_database() as current_db;

-- Set up permissions for userdb_user before creating tables
GRANT ALL ON SCHEMA public TO userdb_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO userdb_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO userdb_user;

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create UserRole enum type
DO $$ BEGIN
    CREATE TYPE UserRole AS ENUM ('Provider', 'Customer');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Username VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    FirstName VARCHAR(255) NOT NULL,
    LastName VARCHAR(255) NOT NULL,
    Role UserRole NOT NULL,
    TenantId UUID NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create Tenants table
CREATE TABLE IF NOT EXISTS Tenants (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    OwnerId UUID UNIQUE NOT NULL,
    BusinessName VARCHAR(255) NOT NULL,
    BusinessEmail VARCHAR(255) NULL,
    BusinessPhone VARCHAR(50) NULL,
    Address TEXT NOT NULL,
    Description TEXT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes if they don't exist
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_tenantid ON Users(TenantId);
CREATE INDEX IF NOT EXISTS idx_tenants_ownerid ON Tenants(OwnerId);

-- Create foreign key constraint if it doesn't exist
DO $$
BEGIN
    ALTER TABLE Users ADD CONSTRAINT fk_users_tenant
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE SET NULL;
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create function to update UpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updating UpdatedAt if they don't exist
DROP TRIGGER IF EXISTS update_users_updated_at ON Users;
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON Users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_tenants_updated_at ON Tenants;
CREATE TRIGGER update_tenants_updated_at
    BEFORE UPDATE ON Tenants
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Grant specific permissions on created tables to userdb_user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO userdb_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO userdb_user;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO userdb_user;

-- Set table ownership to userdb_user for proper access
ALTER TABLE Users OWNER TO userdb_user;
ALTER TABLE Tenants OWNER TO userdb_user;

-- Note: No sample data inserted - tables will be empty on creation
-- Applications should handle initial data seeding as needed