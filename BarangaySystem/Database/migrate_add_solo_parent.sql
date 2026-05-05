-- ============================================================
--  Migration: Add is_solo_parent column to residents table
--  Run this ONLY if you already ran barangay_schema.sql
--  and the column doesn't exist yet.
-- ============================================================

USE barangay_db;

ALTER TABLE residents
    ADD COLUMN IF NOT EXISTS is_solo_parent TINYINT(1) NOT NULL DEFAULT 0
    AFTER is_voter;
