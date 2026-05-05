-- ============================================================
--  Migration: Add birth_certificate column to residents
--  Run once against barangay_db
-- ============================================================

USE barangay_db;

ALTER TABLE residents
    ADD COLUMN birth_certificate LONGBLOB NULL
        COMMENT 'Scanned birth certificate image (JPEG/PNG stored as binary)'
    AFTER is_solo_parent;
