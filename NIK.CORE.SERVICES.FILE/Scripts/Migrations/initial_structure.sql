/* * FILE MANAGEMENT SYSTEM DATABASE STRUCTURE
 * Version: 1.1
 * Optimization: Post-creation Referential Integrity
 */

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ==========================================
-- STEP 1: CREATE TABLES (Independent First)
-- ==========================================

-- Physical Storage Registry
CREATE TABLE "PhysicalStorage" (
                                   "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                                   "fileHash" VARCHAR(64) UNIQUE NOT NULL,
                                   "fileSize" BIGINT NOT NULL CHECK ("fileSize" >= 0),
                                   "relativePath" TEXT NOT NULL,
                                   "mimeType" VARCHAR(100),
                                   "refCount" INT DEFAULT 0 CHECK ("refCount" >= 0),
                                   "createdAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Folder Hierarchy
CREATE TABLE "Folders" (
                           "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                           "name" VARCHAR(255) NOT NULL,
                           "parentId" UUID,
                           "createdAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                           "updatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- File Metadata
CREATE TABLE "FileEntry" (
                         "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                         "displayName" VARCHAR(255) NOT NULL,
                         "extension" VARCHAR(20),
                         "folderId" UUID NOT NULL,
                         "physicalId" UUID NOT NULL,
                         "isDeleted" BOOLEAN DEFAULT FALSE,
                         "metadata" JSONB DEFAULT '{}'::jsonb,
                         "createdAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                         "updatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Sharing Access
CREATE TABLE "FileShares" (
                              "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                              "fileId" UUID NOT NULL,
                              "shareToken" VARCHAR(100) UNIQUE NOT NULL,
                              "expiresAt" TIMESTAMP WITH TIME ZONE,
                              "passwordHash" TEXT,
                              "downloadCount" INT DEFAULT 0,
                              "createdAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);


-- ==========================================
-- STEP 2: ADD REFERENCES (Foreign Keys)
-- ==========================================

-- Self-reference for folder hierarchy
ALTER TABLE "Folders"
    ADD CONSTRAINT fk_folder_parent
        FOREIGN KEY ("parentId") REFERENCES "Folders"("id") ON DELETE CASCADE;

-- Link files to folders and physical content
ALTER TABLE "FileEntry"
    ADD CONSTRAINT fk_file_folder
        FOREIGN KEY ("folderId") REFERENCES "Folders"("id") ON DELETE CASCADE;

ALTER TABLE "FileEntry"
    ADD CONSTRAINT fk_file_physical
        FOREIGN KEY ("physicalId") REFERENCES "PhysicalStorage"("id") ON DELETE RESTRICT;

-- Link shares to files
ALTER TABLE "FileShares"
    ADD CONSTRAINT fk_share_file
        FOREIGN KEY ("fileId") REFERENCES "FileEntry"("id") ON DELETE CASCADE;

-- ==========================================
-- STEP 3: PERFORMANCE INDEXING
-- ==========================================

CREATE INDEX idx_files_folder_id ON "FileEntry"("folderId");
CREATE INDEX idx_files_physical_id ON "FileEntry"("physicalId");
CREATE INDEX idx_folders_parent_id ON "Folders"("parentId");
CREATE INDEX idx_physical_hash ON "PhysicalStorage"("fileHash");
CREATE INDEX idx_files_search ON "FileEntry" USING gin (to_tsvector('english', "displayName"));