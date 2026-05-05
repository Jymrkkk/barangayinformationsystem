-- ============================================================
--  Barangay Centralized Information System
--  MySQL Database Schema
--  Version: 1.0
-- ============================================================

CREATE DATABASE IF NOT EXISTS barangay_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE barangay_db;

-- ============================================================
-- TABLE: users  (Account Management)
-- ============================================================
CREATE TABLE IF NOT EXISTS users (
    user_id     INT AUTO_INCREMENT PRIMARY KEY,
    username    VARCHAR(50)  NOT NULL UNIQUE,
    full_name   VARCHAR(100) NOT NULL,
    email       VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,          -- BCrypt hash
    role        ENUM('Admin','Encoder','Viewer') NOT NULL DEFAULT 'Viewer',
    is_active   TINYINT(1) NOT NULL DEFAULT 1,
    created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ============================================================
-- TABLE: residents
-- ============================================================
CREATE TABLE IF NOT EXISTS residents (
    resident_id   INT AUTO_INCREMENT PRIMARY KEY,
    res_code      VARCHAR(10)  NOT NULL UNIQUE,   -- e.g. R-001
    last_name     VARCHAR(60)  NOT NULL,
    first_name    VARCHAR(60)  NOT NULL,
    middle_name   VARCHAR(60),
    birth_date    DATE         NOT NULL,
    gender        ENUM('Male','Female','Other') NOT NULL,
    civil_status  ENUM('Single','Married','Widowed','Separated') NOT NULL DEFAULT 'Single',
    address       VARCHAR(200) NOT NULL,
    purok         VARCHAR(50)  NOT NULL,
    contact_no    VARCHAR(20),
    email         VARCHAR(100),
    occupation    VARCHAR(100),
    is_voter      TINYINT(1) NOT NULL DEFAULT 0,
    is_solo_parent TINYINT(1) NOT NULL DEFAULT 0,
    is_active     TINYINT(1) NOT NULL DEFAULT 1,
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES users(user_id) ON DELETE SET NULL
);

-- ============================================================
-- TABLE: certificates  (linked to residents)
-- ============================================================
CREATE TABLE IF NOT EXISTS certificates (
    cert_id       INT AUTO_INCREMENT PRIMARY KEY,
    cert_code     VARCHAR(10) NOT NULL UNIQUE,
    resident_id   INT NOT NULL,
    cert_type     ENUM('Barangay Clearance','Certificate of Residency','Indigency Certificate','Business Clearance','Good Moral') NOT NULL,
    purpose       VARCHAR(200),
    issued_by     VARCHAR(100),
    issued_date   DATE NOT NULL,
    or_number     VARCHAR(50),
    amount        DECIMAL(10,2) DEFAULT 0.00,
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (resident_id) REFERENCES residents(resident_id) ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES users(user_id) ON DELETE SET NULL
);

-- ============================================================
-- TABLE: activities
-- ============================================================
CREATE TABLE IF NOT EXISTS activities (
    activity_id   INT AUTO_INCREMENT PRIMARY KEY,
    act_code      VARCHAR(10) NOT NULL UNIQUE,
    activity_name VARCHAR(150) NOT NULL,
    description   TEXT,
    activity_date DATE NOT NULL,
    venue         VARCHAR(150),
    organizer     VARCHAR(100),
    participants  INT DEFAULT 0,
    status        ENUM('Upcoming','Ongoing','Completed','Cancelled') NOT NULL DEFAULT 'Upcoming',
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES users(user_id) ON DELETE SET NULL
);

-- ============================================================
-- TABLE: ordinances
-- ============================================================
CREATE TABLE IF NOT EXISTS ordinances (
    ordinance_id  INT AUTO_INCREMENT PRIMARY KEY,
    bo_number     VARCHAR(30) NOT NULL UNIQUE,
    introduced_by VARCHAR(100) NOT NULL,
    description   VARCHAR(300) NOT NULL,
    full_text     TEXT,
    date_enacted  DATE NOT NULL,
    approved_by   VARCHAR(100),
    status        ENUM('Active','Inactive','Repealed') NOT NULL DEFAULT 'Active',
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES users(user_id) ON DELETE SET NULL
);

-- ============================================================
-- TABLE: resolutions
-- ============================================================
CREATE TABLE IF NOT EXISTS resolutions (
    resolution_id INT AUTO_INCREMENT PRIMARY KEY,
    res_number    VARCHAR(30) NOT NULL UNIQUE,
    subject       VARCHAR(300) NOT NULL,
    sponsor       VARCHAR(100),
    date_passed   DATE NOT NULL,
    status        ENUM('Approved','Pending','Rejected') NOT NULL DEFAULT 'Pending',
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES users(user_id) ON DELETE SET NULL
);

-- ============================================================
-- TABLE: schools
-- ============================================================
CREATE TABLE IF NOT EXISTS schools (
    school_id   INT AUTO_INCREMENT PRIMARY KEY,
    school_name VARCHAR(150) NOT NULL UNIQUE,
    school_type ENUM('Elementary','High School','College','Vocational') NOT NULL,
    address     VARCHAR(200)
);

-- ============================================================
-- TABLE: students
-- ============================================================
CREATE TABLE IF NOT EXISTS students (
    student_id    INT AUTO_INCREMENT PRIMARY KEY,
    stud_code     VARCHAR(10) NOT NULL UNIQUE,
    resident_id   INT,                            -- optional link to residents
    last_name     VARCHAR(60) NOT NULL,
    first_name    VARCHAR(60) NOT NULL,
    middle_name   VARCHAR(60),
    birth_date    DATE,
    gender        ENUM('Male','Female','Other'),
    address       VARCHAR(200),
    purok         VARCHAR(50),
    school_id     INT,
    grade_year    VARCHAR(50),
    school_year   VARCHAR(20),
    is_scholar    TINYINT(1) NOT NULL DEFAULT 0,
    status        ENUM('Enrolled','Dropped','Graduated') NOT NULL DEFAULT 'Enrolled',
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (resident_id) REFERENCES residents(resident_id) ON DELETE SET NULL,
    FOREIGN KEY (school_id)   REFERENCES schools(school_id)    ON DELETE SET NULL,
    FOREIGN KEY (created_by)  REFERENCES users(user_id)        ON DELETE SET NULL
);

-- ============================================================
-- TABLE: scholarships
-- ============================================================
CREATE TABLE IF NOT EXISTS scholarships (
    scholarship_id INT AUTO_INCREMENT PRIMARY KEY,
    scholar_code   VARCHAR(10) NOT NULL UNIQUE,
    student_id     INT NOT NULL,
    grant_type     VARCHAR(100) NOT NULL,
    amount         DECIMAL(10,2) DEFAULT 0.00,
    school_year    VARCHAR(20),
    status         ENUM('Active','Inactive','Completed') NOT NULL DEFAULT 'Active',
    created_by     INT,
    created_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id)  REFERENCES students(student_id) ON DELETE CASCADE,
    FOREIGN KEY (created_by)  REFERENCES users(user_id)       ON DELETE SET NULL
);

-- ============================================================
-- TABLE: event_logs  (Audit Trail)
-- ============================================================
CREATE TABLE IF NOT EXISTS event_logs (
    log_id      INT AUTO_INCREMENT PRIMARY KEY,
    log_code    VARCHAR(10),
    user_id     INT,
    username    VARCHAR(50),
    event_type  ENUM('LOGIN','LOGOUT','INSERT','UPDATE','DELETE','EXPORT','PRINT') NOT NULL,
    module      VARCHAR(50),
    description VARCHAR(300),
    ip_address  VARCHAR(45),
    log_time    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL
);

-- ============================================================
-- SEED DATA
-- ============================================================

-- Default users  (passwords shown in comments)
-- admin    → Admin@1234
-- encoder1 → Encoder@1234
-- encoder2 → Encoder@1234
-- viewer1  → Viewer@1234
INSERT INTO users (username, full_name, email, password_hash, role, is_active)
VALUES
('admin',    'Brgy. Administrator', 'admin@brgy.gov.ph',    '$2a$11$XH..l1F5rOagJZBDVQfq6eWGKxqjFVYr6fhFG6CDTKP63GjknQktq', 'Admin',   1),
('encoder1', 'Maria Encoder',       'encoder1@brgy.gov.ph', '$2a$11$uV0WM92bOLVeqBoURQZJAOPLfaNYpL0LFo9uIDw5NUc.15bg3tIE.', 'Encoder', 1),
('encoder2', 'Pedro Encoder',       'encoder2@brgy.gov.ph', '$2a$11$uV0WM92bOLVeqBoURQZJAOPLfaNYpL0LFo9uIDw5NUc.15bg3tIE.', 'Encoder', 1),
('viewer1',  'Ana Viewer',          'viewer1@brgy.gov.ph',  '$2a$11$tCCwti8.XkStRTWClrZ.XeZQaeeR.il/56ZQAnbApCx1n2p2tR/by',  'Viewer',  0);

-- Schools
INSERT INTO schools (school_name, school_type, address) VALUES
('Brgy. Elementary School',  'Elementary',  'Purok 1, Barangay'),
('Brgy. National High School','High School', 'Purok 2, Barangay'),
('Pamantasan ng Lungsod',     'College',     'City Proper');

-- Sample residents
INSERT INTO residents (res_code, last_name, first_name, middle_name, birth_date, gender, civil_status, address, purok, contact_no, is_voter, is_active, created_by)
VALUES
('R-001','Dela Cruz','Juan',   'Santos',  '1991-03-15','Male',  'Married', '123 Mabini St.', 'Purok 1','09171234567',1,1,1),
('R-002','Santos',   'Maria',  'Reyes',   '1997-07-22','Female','Single',  '45 Rizal Ave.',  'Purok 2','09181234567',1,1,1),
('R-003','Reyes',    'Pedro',  'Cruz',    '1973-11-05','Male',  'Married', '78 Bonifacio Rd.','Purok 3','09191234567',1,0,1),
('R-004','Gomez',    'Ana',    'Lim',     '2006-01-30','Female','Single',  '10 Luna St.',    'Purok 1','09221234567',0,1,1),
('R-005','Bautista', 'Carlos', 'Tan',     '1984-09-12','Male',  'Married', '22 Mabini St.',  'Purok 4','09231234567',1,1,1);

-- Sample activities
INSERT INTO activities (act_code, activity_name, activity_date, venue, organizer, participants, status, created_by)
VALUES
('A-001','Brgy. Cleanup Drive',  '2025-06-15','Plaza',           'Brgy. Captain',120,'Upcoming',  1),
('A-002','Health Seminar',       '2025-06-20','Brgy. Hall',      'RHU Nurse',     80,'Upcoming',  1),
('A-003','Livelihood Training',  '2025-06-05','Multi-Purpose Hall','DSWD',         45,'Completed', 1),
('A-004','Sports Fest',          '2025-07-04','Covered Court',   'SK Chair',     200,'Upcoming',  1),
('A-005','Nutrition Month Program','2025-05-28','Brgy. Hall',    'BNS',           60,'Completed', 1);

-- Sample ordinances
INSERT INTO ordinances (bo_number, introduced_by, description, date_enacted, approved_by, status, created_by)
VALUES
('BO-2025-001','Kgd. Reyes',   'Anti-Littering Ordinance',    '2025-01-15','Kgd. Santos', 'Active',1),
('BO-2025-002','Kgd. Cruz',    'Curfew for Minors',           '2025-02-01','Kgd. Gomez',  'Active',1),
('BO-2025-003','Kgd. Bautista','Noise Pollution Control',     '2025-03-10','Kgd. Santos', 'Active',1),
('BO-2025-004','Kgd. Tan',     'Community Garden Program',    '2025-04-05','Kgd. Reyes',  'Inactive',1),
('BO-2025-005','Kgd. Diaz',    'Waste Segregation Mandate',   '2025-04-20','Kgd. Gomez',  'Active',1),
('BO-2025-006','Kgd. Reyes',   'Barangay Road Safety Rules',  '2025-05-10','Kgd. Cruz',   'Active',1),
('BO-2025-007','Kgd. Santos',  'Backyard Composting Program', '2025-06-09','Kgd. Bautista','Active',1);

-- Sample students
INSERT INTO students (stud_code, last_name, first_name, birth_date, gender, address, purok, school_id, grade_year, school_year, is_scholar, status, created_by)
VALUES
('S-001','Dela Rosa','Mark', '2009-04-10','Male',  'Purok 2','Purok 2',2,'Grade 11','2024-2025',1,'Enrolled',1),
('S-002','Ramos',    'Lisa', '2011-08-22','Female','Purok 1','Purok 1',2,'Grade 9', '2024-2025',0,'Enrolled',1),
('S-003','Vega',     'Carlo','2006-02-14','Male',  'Purok 3','Purok 3',3,'2nd Year','2024-2025',1,'Enrolled',1),
('S-004','Castillo', 'Nina', '2013-11-30','Female','Purok 4','Purok 4',1,'Grade 6', '2024-2025',0,'Enrolled',1),
('S-005','Mendoza',  'Jose', '2008-06-05','Male',  'Purok 2','Purok 2',2,'Grade 12','2024-2025',1,'Enrolled',1);

-- Sample event logs
INSERT INTO event_logs (log_code, user_id, username, event_type, module, description, ip_address)
VALUES
('L-1201',1,'admin',   'INSERT','Residents','Added resident ID R-821',   '192.168.1.10'),
('L-1200',1,'admin',   'INSERT','Ordinances','Added BO #2025-007',        '192.168.1.10'),
('L-1199',3,'encoder2','UPDATE','Students',  'Updated student S-203',     '192.168.1.13'),
('L-1198',1,'admin',   'LOGIN', 'System',    'User login successful',     '192.168.1.10'),
('L-1197',2,'encoder1','DELETE','Activities','Deleted activity A-008',    '192.168.1.12');
