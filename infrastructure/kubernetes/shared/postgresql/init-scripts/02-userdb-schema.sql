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
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    firstname VARCHAR(255) NOT NULL,
    lastname VARCHAR(255) NOT NULL,
    role userrole NOT NULL,
    tenantid UUID NULL,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updatedat TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create Tenants table
CREATE TABLE IF NOT EXISTS tenants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ownerid UUID UNIQUE NOT NULL,
    businessname VARCHAR(255) NOT NULL,
    businessemail VARCHAR(255) NULL,
    businessphone VARCHAR(50) NULL,
    address TEXT NOT NULL,
    description TEXT NULL,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updatedat TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes if they don't exist
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_tenantid ON users(tenantid);
CREATE INDEX IF NOT EXISTS idx_tenants_ownerid ON tenants(ownerid);

-- Create foreign key constraint if it doesn't exist
DO $$
BEGIN
    ALTER TABLE users ADD CONSTRAINT fk_users_tenant
        FOREIGN KEY (tenantid) REFERENCES tenants(id) ON DELETE SET NULL;
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
DROP TRIGGER IF EXISTS update_users_updated_at ON users;
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_tenants_updated_at ON tenants;
CREATE TRIGGER update_tenants_updated_at
    BEFORE UPDATE ON tenants
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Grant specific permissions on created tables to userdb_user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO userdb_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO userdb_user;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO userdb_user;

-- Set table ownership to userdb_user for proper access
ALTER TABLE users OWNER TO userdb_user;
ALTER TABLE tenants OWNER TO userdb_user;