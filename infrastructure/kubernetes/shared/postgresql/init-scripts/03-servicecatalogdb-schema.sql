-- Service Catalog Service Database Schema
-- This script will be executed in the servicecatalogdb database

-- Switch to servicecatalogdb database context
\c servicecatalogdb;

-- Verify we're in the correct database
SELECT current_database() as current_db;

-- Set up permissions for servicecatalogdb_user before creating tables
GRANT ALL ON SCHEMA public TO servicecatalogdb_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO servicecatalogdb_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO servicecatalogdb_user;

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Categories table with PascalCase columns
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create Services table with PascalCase columns
CREATE TABLE IF NOT EXISTS "Services" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" UUID NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT NULL,
    "Price" DECIMAL(10, 2) NOT NULL,
    "DurationMinutes" INTEGER NOT NULL,
    "CategoryId" UUID NULL,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes if they don't exist (using quoted column names)
CREATE INDEX IF NOT EXISTS idx_categories_name ON "Categories"("Name");
CREATE INDEX IF NOT EXISTS idx_services_name ON "Services"("Name");
CREATE INDEX IF NOT EXISTS idx_services_tenantid ON "Services"("TenantId");
CREATE INDEX IF NOT EXISTS idx_services_categoryid ON "Services"("CategoryId");
CREATE INDEX IF NOT EXISTS idx_services_isactive ON "Services"("IsActive");

-- Create foreign key constraint if it doesn't exist
DO $$
BEGIN
    ALTER TABLE "Services" ADD CONSTRAINT fk_services_category
        FOREIGN KEY ("CategoryId") REFERENCES "Categories"("Id") ON DELETE SET NULL;
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create function to update UpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updating UpdatedAt if they don't exist
DROP TRIGGER IF EXISTS update_categories_updated_at ON "Categories";
CREATE TRIGGER update_categories_updated_at
    BEFORE UPDATE ON "Categories"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_services_updated_at ON "Services";
CREATE TRIGGER update_services_updated_at
    BEFORE UPDATE ON "Services"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Grant specific permissions on created tables to servicecatalogdb_user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO servicecatalogdb_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO servicecatalogdb_user;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO servicecatalogdb_user;

-- Set table ownership to servicecatalogdb_user for proper access
ALTER TABLE "Categories" OWNER TO servicecatalogdb_user;
ALTER TABLE "Services" OWNER TO servicecatalogdb_user;