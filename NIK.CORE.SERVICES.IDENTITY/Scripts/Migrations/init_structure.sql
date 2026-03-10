/* ==========================================================
 * IDENTITY SYSTEM DATABASE STRUCTURE
 * Version: 1.1
 * Optimization: Post-creation Referential Integrity
 * ========================================================== */

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ==========================================
-- STEP 1: CREATE TABLES (Independent First)
-- ==========================================

-- Users: stores user account credentials and profile only
CREATE TABLE "Users" (
                         "Id"                    UUID            NOT NULL DEFAULT uuid_generate_v4(),
                         "UserName"              VARCHAR(256)    NOT NULL,
                         "NormalizedUserName"    VARCHAR(256)    NOT NULL,
                         "Email"                 VARCHAR(256)    NULL,
                         "NormalizedEmail"       VARCHAR(256)    NULL,
                         "EmailConfirmed"        BOOLEAN         NOT NULL DEFAULT FALSE,
                         "PasswordHash"          TEXT            NULL,
                         "SecurityStamp"         TEXT            NULL,
                         "ConcurrencyStamp"      TEXT            NULL,
                         "PhoneNumber"           VARCHAR(20)     NULL,
                         "PhoneNumberConfirmed"  BOOLEAN         NOT NULL DEFAULT FALSE,
                         "TwoFactorEnabled"      BOOLEAN         NOT NULL DEFAULT FALSE,
                         "LockoutEnd"            TIMESTAMPTZ     NULL,
                         "LockoutEnabled"        BOOLEAN         NOT NULL DEFAULT TRUE,
                         "AccessFailedCount"     INT             NOT NULL DEFAULT 0,
    -- Profile
                         "FullName"              VARCHAR(256)    NULL,
                         "AvatarUrl"             TEXT            NULL,
                         "Status"                SMALLINT        NOT NULL DEFAULT 1,   -- 1=Active, 2=Inactive, 3=Suspended, 4=Banned
    -- Soft delete & audit
                         "IsActive"              BOOLEAN         NOT NULL DEFAULT TRUE,
                         "IsDeleted"             BOOLEAN         NOT NULL DEFAULT FALSE,
                         "CreatedAt"             TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                         "UpdatedAt"             TIMESTAMPTZ     NULL,
                         "DeletedAt"             TIMESTAMPTZ     NULL,
                         "CreatedBy"             UUID            NULL,
                         "UpdatedBy"             UUID            NULL
);

-- Roles: system roles (e.g. Admin, Manager, Staff)
CREATE TABLE "Roles" (
                         "Id"                UUID            NOT NULL DEFAULT uuid_generate_v4(),
                         "Name"              VARCHAR(256)    NOT NULL,
                         "NormalizedName"    VARCHAR(256)    NOT NULL,
                         "Description"       TEXT            NULL,
                         "ConcurrencyStamp"  TEXT            NULL,
                         "IsSystem"          BOOLEAN         NOT NULL DEFAULT FALSE,
                         "IsActive"          BOOLEAN         NOT NULL DEFAULT TRUE,
                         "IsDeleted"         BOOLEAN         NOT NULL DEFAULT FALSE,
                         "CreatedAt"         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                         "UpdatedAt"         TIMESTAMPTZ     NULL,
                         "CreatedBy"         UUID            NULL,
                         "UpdatedBy"         UUID            NULL
);

-- UserRoles: n-n relationship between Users and Roles
CREATE TABLE "UserRoles" (
                             "UserId"        UUID        NOT NULL,
                             "RoleId"        UUID        NOT NULL,
                             "AssignedAt"    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                             "AssignedBy"    UUID        NULL,
                             "ExpiredAt"     TIMESTAMPTZ NULL,
                             "IsActive"      BOOLEAN     NOT NULL DEFAULT TRUE
);

-- UserLogins: external SSO / OAuth provider logins
CREATE TABLE "UserLogins" (
                              "LoginProvider"         VARCHAR(128)    NOT NULL,
                              "ProviderKey"           VARCHAR(256)    NOT NULL,
                              "ProviderDisplayName"   VARCHAR(256)    NULL,
                              "UserId"                UUID            NOT NULL,
                              "CreatedAt"             TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- UserToken: tokens for email confirmation, password reset, 2FA, etc.
CREATE TABLE "UserToken" (
                             "Id"            UUID            NOT NULL DEFAULT uuid_generate_v4(),
                             "UserId"        UUID            NOT NULL,
                             "LoginProvider" VARCHAR(128)    NOT NULL,
                             "Name"          VARCHAR(128)    NOT NULL,   -- e.g. 'EmailConfirmation', 'PasswordReset', '2FA'
                             "Value"         TEXT            NULL,
                             "ExpiredAt"     TIMESTAMPTZ     NULL,
                             "IsUsed"        BOOLEAN         NOT NULL DEFAULT FALSE,
                             "UsedAt"        TIMESTAMPTZ     NULL,
                             "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- UserSessions: tracks active/expired sessions per user per device
CREATE TABLE "UserSessions" (
                                "Id"                UUID            NOT NULL DEFAULT uuid_generate_v4(),
                                "UserId"            UUID            NOT NULL,
                                "SessionToken"      TEXT            NOT NULL,               -- hashed token stored here
                                "RefreshToken"      TEXT            NULL,                   -- for JWT refresh flow
                                "DeviceId"          VARCHAR(256)    NULL,                   -- unique device fingerprint
                                "DeviceName"        VARCHAR(256)    NULL,                   -- e.g. 'Chrome on Windows'
                                "DeviceType"        SMALLINT        NOT NULL DEFAULT 1,     -- 1=Web, 2=Mobile, 3=Desktop, 4=API
                                "IpAddress"         VARCHAR(64)     NULL,
                                "UserAgent"         TEXT            NULL,
                                "Location"          VARCHAR(256)    NULL,                   -- geo-resolved location
                                "IsOnline"          BOOLEAN         NOT NULL DEFAULT TRUE,
                                "LastActivityAt"    TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                                "ExpiredAt"         TIMESTAMPTZ     NULL,
                                "RevokedAt"         TIMESTAMPTZ     NULL,
                                "RevokedReason"     VARCHAR(256)    NULL,                   -- e.g. 'Logout', 'ForceLogout', 'Expired'
                                "CreatedAt"         TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Modules: top-level system modules (e.g. HR, Finance, Inventory)
CREATE TABLE "Modules" (
                           "Id"            UUID            NOT NULL DEFAULT uuid_generate_v4(),
                           "Code"          VARCHAR(64)     NOT NULL,
                           "Name"          VARCHAR(256)    NOT NULL,
                           "Description"   TEXT            NULL,
                           "IconUrl"       TEXT            NULL,
                           "SortOrder"     INT             NOT NULL DEFAULT 0,
                           "IsActive"      BOOLEAN         NOT NULL DEFAULT TRUE,
                           "IsDeleted"     BOOLEAN         NOT NULL DEFAULT FALSE,
                           "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                           "UpdatedAt"     TIMESTAMPTZ     NULL,
                           "CreatedBy"     UUID            NULL,
                           "UpdatedBy"     UUID            NULL
);

-- Actions: atomic operations (e.g. Create, Read, Update, Delete, Export, Approve)
CREATE TABLE "Actions" (
                           "Id"            UUID            NOT NULL DEFAULT uuid_generate_v4(),
                           "Code"          VARCHAR(64)     NOT NULL,
                           "Name"          VARCHAR(256)    NOT NULL,
                           "Description"   TEXT            NULL,
                           "IsActive"      BOOLEAN         NOT NULL DEFAULT TRUE,
                           "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                           "UpdatedAt"     TIMESTAMPTZ     NULL,
                           "CreatedBy"     UUID            NULL,
                           "UpdatedBy"     UUID            NULL
);

-- Functions: business functions per module (e.g. CalculatePayroll, ApproveLeave)
CREATE TABLE "Functions" (
                             "Id"            UUID            NOT NULL DEFAULT uuid_generate_v4(),
                             "ModuleId"      UUID            NOT NULL,
                             "Code"          VARCHAR(128)    NOT NULL,
                             "Name"          VARCHAR(256)    NOT NULL,
                             "Description"   TEXT            NULL,
                             "Url"           TEXT            NULL,
                             "SortOrder"     INT             NOT NULL DEFAULT 0,
                             "IsActive"      BOOLEAN         NOT NULL DEFAULT TRUE,
                             "IsDeleted"     BOOLEAN         NOT NULL DEFAULT FALSE,
                             "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                             "UpdatedAt"     TIMESTAMPTZ     NULL,
                             "CreatedBy"     UUID            NULL,
                             "UpdatedBy"     UUID            NULL
);

-- FunctionActions: n-n between Functions and Actions
CREATE TABLE "FunctionActions" (
                                   "Id"            UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                   "FunctionId"    UUID        NOT NULL,
                                   "ActionId"      UUID        NOT NULL,
                                   "IsActive"      BOOLEAN     NOT NULL DEFAULT TRUE,
                                   "CreatedAt"     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                   "CreatedBy"     UUID        NULL
);

-- UserFunctionActions: explicit function-action permissions granted to a user
CREATE TABLE "UserFunctionActions" (
                                       "Id"                UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                       "UserId"            UUID        NOT NULL,
                                       "FunctionActionId"  UUID        NOT NULL,
                                       "IsGranted"         BOOLEAN     NOT NULL DEFAULT TRUE,  -- FALSE = explicitly denied
                                       "GrantedAt"         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                       "GrantedBy"         UUID        NULL,
                                       "ExpiredAt"         TIMESTAMPTZ NULL
);

-- RoleFunctionActions: function-action permissions granted to a role
CREATE TABLE "RoleFunctionActions" (
                                       "Id"                UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                       "RoleId"            UUID        NOT NULL,
                                       "FunctionActionId"  UUID        NOT NULL,
                                       "IsGranted"         BOOLEAN     NOT NULL DEFAULT TRUE,
                                       "GrantedAt"         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                       "GrantedBy"         UUID        NULL,
                                       "ExpiredAt"         TIMESTAMPTZ NULL
);

-- AuditLogs: sign-in / sign-out / security events history
CREATE TABLE "AuditLogs" (
                             "Id"            UUID            NOT NULL DEFAULT uuid_generate_v4(),
                             "UserId"        UUID            NULL,
                             "SessionId"     UUID            NULL,
                             "EventType"     VARCHAR(64)     NOT NULL,   -- e.g. 'Login', 'Logout', 'FailedLogin', 'PasswordChange'
                             "IpAddress"     VARCHAR(64)     NULL,
                             "UserAgent"     TEXT            NULL,
                             "DeviceInfo"    TEXT            NULL,
                             "Location"      VARCHAR(256)    NULL,
                             "IsSuccess"     BOOLEAN         NOT NULL DEFAULT TRUE,
                             "FailReason"    TEXT            NULL,
                             "Metadata"      JSONB           NULL,
                             "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Organizations: companies, branches, departments
CREATE TABLE "Organizations" (
                                 "Id"            UUID            NOT NULL DEFAULT uuid_generate_v4(),
                                 "ParentId"      UUID            NULL,
                                 "Code"          VARCHAR(64)     NOT NULL,
                                 "Name"          VARCHAR(256)    NOT NULL,
                                 "Description"   TEXT            NULL,
                                 "Type"          SMALLINT        NOT NULL DEFAULT 1,  -- 1=Company, 2=Branch, 3=Department
                                 "LogoUrl"       TEXT            NULL,
                                 "Address"       TEXT            NULL,
                                 "Phone"         VARCHAR(20)     NULL,
                                 "Email"         VARCHAR(256)    NULL,
                                 "TaxCode"       VARCHAR(64)     NULL,
                                 "IsActive"      BOOLEAN         NOT NULL DEFAULT TRUE,
                                 "IsDeleted"     BOOLEAN         NOT NULL DEFAULT FALSE,
                                 "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
                                 "UpdatedAt"     TIMESTAMPTZ     NULL,
                                 "DeletedAt"     TIMESTAMPTZ     NULL,
                                 "CreatedBy"     UUID            NULL,
                                 "UpdatedBy"     UUID            NULL
);

-- UserOrganizations: maps users to organizations they belong to
CREATE TABLE "UserOrganizations" (
                                     "Id"                UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                     "UserId"            UUID        NOT NULL,
                                     "OrganizationId"    UUID        NOT NULL,
                                     "IsDefault"         BOOLEAN     NOT NULL DEFAULT FALSE,
                                     "IsActive"          BOOLEAN     NOT NULL DEFAULT TRUE,
                                     "JoinedAt"          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                     "JoinedBy"          UUID        NULL,
                                     "LeftAt"            TIMESTAMPTZ NULL
);

-- OrganizationFunctions: enable / disable features per organization
CREATE TABLE "OrganizationFunctions" (
                                         "Id"                UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                         "OrganizationId"    UUID        NOT NULL,
                                         "FunctionId"        UUID        NOT NULL,
                                         "IsEnabled"         BOOLEAN     NOT NULL DEFAULT TRUE,
                                         "EnabledAt"         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                         "EnabledBy"         UUID        NULL,
                                         "DisabledAt"        TIMESTAMPTZ NULL,
                                         "DisabledBy"        UUID        NULL
);

-- UserOrganizationAccess: data-level access for users across organizations
CREATE TABLE "UserOrganizationAccess" (
                                          "Id"                UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                          "UserId"            UUID        NOT NULL,
                                          "OrganizationId"    UUID        NOT NULL,
                                          "AccessType"        SMALLINT    NOT NULL DEFAULT 1,  -- 1=Read, 2=Write, 3=Full
                                          "IsGranted"         BOOLEAN     NOT NULL DEFAULT TRUE,
                                          "GrantedAt"         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                          "GrantedBy"         UUID        NULL,
                                          "ExpiredAt"         TIMESTAMPTZ NULL
);

-- RoleOrganizationAccess: data-level access for roles across organizations
CREATE TABLE "RoleOrganizationAccess" (
                                          "Id"                UUID        NOT NULL DEFAULT uuid_generate_v4(),
                                          "RoleId"            UUID        NOT NULL,
                                          "OrganizationId"    UUID        NOT NULL,
                                          "AccessType"        SMALLINT    NOT NULL DEFAULT 1,  -- 1=Read, 2=Write, 3=Full
                                          "IsGranted"         BOOLEAN     NOT NULL DEFAULT TRUE,
                                          "GrantedAt"         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                          "GrantedBy"         UUID        NULL,
                                          "ExpiredAt"         TIMESTAMPTZ NULL
);


-- ==========================================
-- STEP 2: PRIMARY KEYS
-- ==========================================

ALTER TABLE "Users"                  ADD CONSTRAINT "PK_Users"                  PRIMARY KEY ("Id");
ALTER TABLE "Roles"                  ADD CONSTRAINT "PK_Roles"                  PRIMARY KEY ("Id");
ALTER TABLE "UserRoles"              ADD CONSTRAINT "PK_UserRoles"              PRIMARY KEY ("UserId", "RoleId");
ALTER TABLE "UserLogins"             ADD CONSTRAINT "PK_UserLogins"             PRIMARY KEY ("LoginProvider", "ProviderKey");
ALTER TABLE "UserToken"              ADD CONSTRAINT "PK_UserToken"              PRIMARY KEY ("Id");
ALTER TABLE "UserSessions"           ADD CONSTRAINT "PK_UserSessions"           PRIMARY KEY ("Id");
ALTER TABLE "Modules"                ADD CONSTRAINT "PK_Modules"                PRIMARY KEY ("Id");
ALTER TABLE "Actions"                ADD CONSTRAINT "PK_Actions"                PRIMARY KEY ("Id");
ALTER TABLE "Functions"              ADD CONSTRAINT "PK_Functions"              PRIMARY KEY ("Id");
ALTER TABLE "FunctionActions"        ADD CONSTRAINT "PK_FunctionActions"        PRIMARY KEY ("Id");
ALTER TABLE "UserFunctionActions"    ADD CONSTRAINT "PK_UserFunctionActions"    PRIMARY KEY ("Id");
ALTER TABLE "RoleFunctionActions"    ADD CONSTRAINT "PK_RoleFunctionActions"    PRIMARY KEY ("Id");
ALTER TABLE "AuditLogs"              ADD CONSTRAINT "PK_AuditLogs"              PRIMARY KEY ("Id");
ALTER TABLE "Organizations"          ADD CONSTRAINT "PK_Organizations"          PRIMARY KEY ("Id");
ALTER TABLE "UserOrganizations"      ADD CONSTRAINT "PK_UserOrganizations"      PRIMARY KEY ("Id");
ALTER TABLE "OrganizationFunctions"  ADD CONSTRAINT "PK_OrganizationFunctions"  PRIMARY KEY ("Id");
ALTER TABLE "UserOrganizationAccess" ADD CONSTRAINT "PK_UserOrganizationAccess" PRIMARY KEY ("Id");
ALTER TABLE "RoleOrganizationAccess" ADD CONSTRAINT "PK_RoleOrganizationAccess" PRIMARY KEY ("Id");


-- ==========================================
-- STEP 3: UNIQUE CONSTRAINTS
-- ==========================================

ALTER TABLE "Users"         ADD CONSTRAINT "UQ_Users_NormalizedUserName" UNIQUE ("NormalizedUserName");
ALTER TABLE "Users"         ADD CONSTRAINT "UQ_Users_NormalizedEmail"    UNIQUE ("NormalizedEmail");
ALTER TABLE "Roles"         ADD CONSTRAINT "UQ_Roles_NormalizedName"     UNIQUE ("NormalizedName");
ALTER TABLE "Modules"       ADD CONSTRAINT "UQ_Modules_Code"             UNIQUE ("Code");
ALTER TABLE "Actions"       ADD CONSTRAINT "UQ_Actions_Code"             UNIQUE ("Code");
ALTER TABLE "Functions"     ADD CONSTRAINT "UQ_Functions_Code"           UNIQUE ("Code");
ALTER TABLE "Organizations" ADD CONSTRAINT "UQ_Organizations_Code"       UNIQUE ("Code");

ALTER TABLE "UserSessions"
    ADD CONSTRAINT "UQ_UserSessions_SessionToken" UNIQUE ("SessionToken");

ALTER TABLE "FunctionActions"
    ADD CONSTRAINT "UQ_FunctionActions_FunctionId_ActionId"             UNIQUE ("FunctionId", "ActionId");

ALTER TABLE "UserFunctionActions"
    ADD CONSTRAINT "UQ_UserFunctionActions_UserId_FunctionActionId"     UNIQUE ("UserId", "FunctionActionId");

ALTER TABLE "RoleFunctionActions"
    ADD CONSTRAINT "UQ_RoleFunctionActions_RoleId_FunctionActionId"     UNIQUE ("RoleId", "FunctionActionId");

ALTER TABLE "UserOrganizations"
    ADD CONSTRAINT "UQ_UserOrganizations_UserId_OrganizationId"         UNIQUE ("UserId", "OrganizationId");

ALTER TABLE "OrganizationFunctions"
    ADD CONSTRAINT "UQ_OrganizationFunctions_OrgId_FunctionId"          UNIQUE ("OrganizationId", "FunctionId");

ALTER TABLE "UserOrganizationAccess"
    ADD CONSTRAINT "UQ_UserOrganizationAccess_UserId_OrgId"             UNIQUE ("UserId", "OrganizationId");

ALTER TABLE "RoleOrganizationAccess"
    ADD CONSTRAINT "UQ_RoleOrganizationAccess_RoleId_OrgId"             UNIQUE ("RoleId", "OrganizationId");


-- ==========================================
-- STEP 4: FOREIGN KEY CONSTRAINTS
-- ==========================================

-- UserRoles
ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;
ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_RoleId"
    FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id") ON DELETE CASCADE;

-- UserLogins
ALTER TABLE "UserLogins" ADD CONSTRAINT "FK_UserLogins_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;

-- UserToken
ALTER TABLE "UserToken" ADD CONSTRAINT "FK_UserToken_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;

-- UserSessions
ALTER TABLE "UserSessions" ADD CONSTRAINT "FK_UserSessions_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;

-- Functions -> Modules
ALTER TABLE "Functions" ADD CONSTRAINT "FK_Functions_ModuleId"
    FOREIGN KEY ("ModuleId") REFERENCES "Modules"("Id") ON DELETE RESTRICT;

-- FunctionActions
ALTER TABLE "FunctionActions" ADD CONSTRAINT "FK_FunctionActions_FunctionId"
    FOREIGN KEY ("FunctionId") REFERENCES "Functions"("Id") ON DELETE CASCADE;
ALTER TABLE "FunctionActions" ADD CONSTRAINT "FK_FunctionActions_ActionId"
    FOREIGN KEY ("ActionId") REFERENCES "Actions"("Id") ON DELETE CASCADE;

-- UserFunctionActions
ALTER TABLE "UserFunctionActions" ADD CONSTRAINT "FK_UserFunctionActions_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;
ALTER TABLE "UserFunctionActions" ADD CONSTRAINT "FK_UserFunctionActions_FunctionActionId"
    FOREIGN KEY ("FunctionActionId") REFERENCES "FunctionActions"("Id") ON DELETE CASCADE;

-- RoleFunctionActions
ALTER TABLE "RoleFunctionActions" ADD CONSTRAINT "FK_RoleFunctionActions_RoleId"
    FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id") ON DELETE CASCADE;
ALTER TABLE "RoleFunctionActions" ADD CONSTRAINT "FK_RoleFunctionActions_FunctionActionId"
    FOREIGN KEY ("FunctionActionId") REFERENCES "FunctionActions"("Id") ON DELETE CASCADE;

-- AuditLogs -> Users / Sessions (keep logs even if user/session deleted)
ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE SET NULL;
ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_SessionId"
    FOREIGN KEY ("SessionId") REFERENCES "UserSessions"("Id") ON DELETE SET NULL;

-- Organizations self-referencing parent
ALTER TABLE "Organizations" ADD CONSTRAINT "FK_Organizations_ParentId"
    FOREIGN KEY ("ParentId") REFERENCES "Organizations"("Id") ON DELETE RESTRICT;

-- UserOrganizations
ALTER TABLE "UserOrganizations" ADD CONSTRAINT "FK_UserOrganizations_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;
ALTER TABLE "UserOrganizations" ADD CONSTRAINT "FK_UserOrganizations_OrganizationId"
    FOREIGN KEY ("OrganizationId") REFERENCES "Organizations"("Id") ON DELETE CASCADE;

-- OrganizationFunctions
ALTER TABLE "OrganizationFunctions" ADD CONSTRAINT "FK_OrganizationFunctions_OrganizationId"
    FOREIGN KEY ("OrganizationId") REFERENCES "Organizations"("Id") ON DELETE CASCADE;
ALTER TABLE "OrganizationFunctions" ADD CONSTRAINT "FK_OrganizationFunctions_FunctionId"
    FOREIGN KEY ("FunctionId") REFERENCES "Functions"("Id") ON DELETE CASCADE;

-- UserOrganizationAccess
ALTER TABLE "UserOrganizationAccess" ADD CONSTRAINT "FK_UserOrganizationAccess_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;
ALTER TABLE "UserOrganizationAccess" ADD CONSTRAINT "FK_UserOrganizationAccess_OrganizationId"
    FOREIGN KEY ("OrganizationId") REFERENCES "Organizations"("Id") ON DELETE CASCADE;

-- RoleOrganizationAccess
ALTER TABLE "RoleOrganizationAccess" ADD CONSTRAINT "FK_RoleOrganizationAccess_RoleId"
    FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id") ON DELETE CASCADE;
ALTER TABLE "RoleOrganizationAccess" ADD CONSTRAINT "FK_RoleOrganizationAccess_OrganizationId"
    FOREIGN KEY ("OrganizationId") REFERENCES "Organizations"("Id") ON DELETE CASCADE;


-- ==========================================
-- STEP 5: CHECK CONSTRAINTS
-- ==========================================

ALTER TABLE "Users"
    ADD CONSTRAINT "CHK_Users_Status"
        CHECK ("Status" IN (1, 2, 3, 4)),              -- 1=Active 2=Inactive 3=Suspended 4=Banned
    ADD CONSTRAINT "CHK_Users_AccessFailedCount"
        CHECK ("AccessFailedCount" >= 0);

ALTER TABLE "UserSessions"
    ADD CONSTRAINT "CHK_UserSessions_DeviceType"
        CHECK ("DeviceType" IN (1, 2, 3, 4));          -- 1=Web 2=Mobile 3=Desktop 4=API

ALTER TABLE "Organizations"
    ADD CONSTRAINT "CHK_Organizations_Type"
        CHECK ("Type" IN (1, 2, 3));                   -- 1=Company 2=Branch 3=Department

ALTER TABLE "UserOrganizationAccess"
    ADD CONSTRAINT "CHK_UserOrgAccess_AccessType"
        CHECK ("AccessType" IN (1, 2, 3));             -- 1=Read 2=Write 3=Full

ALTER TABLE "RoleOrganizationAccess"
    ADD CONSTRAINT "CHK_RoleOrgAccess_AccessType"
        CHECK ("AccessType" IN (1, 2, 3));


-- ==========================================
-- STEP 6: INDEXES
-- ==========================================

-- Users
CREATE INDEX "IX_Users_NormalizedEmail"     ON "Users" ("NormalizedEmail");
CREATE INDEX "IX_Users_Status"              ON "Users" ("Status");
CREATE INDEX "IX_Users_IsDeleted"           ON "Users" ("IsDeleted");
CREATE INDEX "IX_Users_CreatedAt"           ON "Users" ("CreatedAt");

-- Roles
CREATE INDEX "IX_Roles_IsDeleted"           ON "Roles" ("IsDeleted");

-- UserRoles
CREATE INDEX "IX_UserRoles_UserId"          ON "UserRoles" ("UserId");
CREATE INDEX "IX_UserRoles_RoleId"          ON "UserRoles" ("RoleId");

-- UserLogins
CREATE INDEX "IX_UserLogins_UserId"         ON "UserLogins" ("UserId");

-- UserToken
CREATE INDEX "IX_UserToken_UserId"              ON "UserToken" ("UserId");
CREATE INDEX "IX_UserToken_LoginProvider_Name"  ON "UserToken" ("LoginProvider", "Name");
CREATE INDEX "IX_UserToken_IsUsed"              ON "UserToken" ("IsUsed");
CREATE INDEX "IX_UserToken_ExpiredAt"           ON "UserToken" ("ExpiredAt");

-- UserSessions
CREATE INDEX "IX_UserSessions_UserId"           ON "UserSessions" ("UserId");
CREATE INDEX "IX_UserSessions_IsOnline"         ON "UserSessions" ("IsOnline");
CREATE INDEX "IX_UserSessions_ExpiredAt"        ON "UserSessions" ("ExpiredAt");
CREATE INDEX "IX_UserSessions_LastActivityAt"   ON "UserSessions" ("LastActivityAt");
CREATE INDEX "IX_UserSessions_DeviceType"       ON "UserSessions" ("DeviceType");
CREATE INDEX "IX_UserSessions_IpAddress"        ON "UserSessions" ("IpAddress");

-- Modules
CREATE INDEX "IX_Modules_IsActive"          ON "Modules" ("IsActive");

-- Functions
CREATE INDEX "IX_Functions_ModuleId"        ON "Functions" ("ModuleId");
CREATE INDEX "IX_Functions_IsActive"        ON "Functions" ("IsActive");

-- FunctionActions
CREATE INDEX "IX_FunctionActions_FunctionId" ON "FunctionActions" ("FunctionId");
CREATE INDEX "IX_FunctionActions_ActionId"   ON "FunctionActions" ("ActionId");

-- UserFunctionActions
CREATE INDEX "IX_UserFunctionActions_UserId"            ON "UserFunctionActions" ("UserId");
CREATE INDEX "IX_UserFunctionActions_FunctionActionId"  ON "UserFunctionActions" ("FunctionActionId");

-- RoleFunctionActions
CREATE INDEX "IX_RoleFunctionActions_RoleId"            ON "RoleFunctionActions" ("RoleId");
CREATE INDEX "IX_RoleFunctionActions_FunctionActionId"  ON "RoleFunctionActions" ("FunctionActionId");

-- AuditLogs
CREATE INDEX "IX_AuditLogs_UserId"      ON "AuditLogs" ("UserId");
CREATE INDEX "IX_AuditLogs_SessionId"   ON "AuditLogs" ("SessionId");
CREATE INDEX "IX_AuditLogs_EventType"   ON "AuditLogs" ("EventType");
CREATE INDEX "IX_AuditLogs_CreatedAt"   ON "AuditLogs" ("CreatedAt");
CREATE INDEX "IX_AuditLogs_IpAddress"   ON "AuditLogs" ("IpAddress");

-- Organizations
CREATE INDEX "IX_Organizations_ParentId"    ON "Organizations" ("ParentId");
CREATE INDEX "IX_Organizations_Type"        ON "Organizations" ("Type");
CREATE INDEX "IX_Organizations_IsDeleted"   ON "Organizations" ("IsDeleted");

-- UserOrganizations
CREATE INDEX "IX_UserOrganizations_UserId"          ON "UserOrganizations" ("UserId");
CREATE INDEX "IX_UserOrganizations_OrganizationId"  ON "UserOrganizations" ("OrganizationId");

-- OrganizationFunctions
CREATE INDEX "IX_OrganizationFunctions_OrganizationId" ON "OrganizationFunctions" ("OrganizationId");
CREATE INDEX "IX_OrganizationFunctions_FunctionId"     ON "OrganizationFunctions" ("FunctionId");
CREATE INDEX "IX_OrganizationFunctions_IsEnabled"      ON "OrganizationFunctions" ("IsEnabled");

-- UserOrganizationAccess
CREATE INDEX "IX_UserOrganizationAccess_UserId"         ON "UserOrganizationAccess" ("UserId");
CREATE INDEX "IX_UserOrganizationAccess_OrganizationId" ON "UserOrganizationAccess" ("OrganizationId");

-- RoleOrganizationAccess
CREATE INDEX "IX_RoleOrganizationAccess_RoleId"         ON "RoleOrganizationAccess" ("RoleId");
CREATE INDEX "IX_RoleOrganizationAccess_OrganizationId" ON "RoleOrganizationAccess" ("OrganizationId");
