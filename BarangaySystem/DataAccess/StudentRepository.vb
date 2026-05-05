Imports MySql.Data.MySqlClient
Imports BarangaySystem.Models

Namespace BarangaySystem.DataAccess

    ' ── Students ─────────────────────────────────────────────────────────

    Public Class StudentRepository

        Public Function GetAll(Optional searchTerm As String = "",
                               Optional schoolFilter As Integer = 0,
                               Optional statusFilter As String = "") As List(Of StudentModel)
            Dim list As New List(Of StudentModel)
            Dim sql = "SELECT s.student_id, s.stud_code, s.resident_id,
                              s.last_name, s.first_name, s.middle_name,
                              s.birth_date, s.gender, s.address, s.purok,
                              s.school_id, sc.school_name,
                              s.grade_year, s.school_year, s.is_scholar,
                              s.status, s.created_by, s.created_at, s.updated_at
                       FROM students s
                       LEFT JOIN schools sc ON sc.school_id = s.school_id
                       WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If Not String.IsNullOrWhiteSpace(searchTerm) Then
                sql &= " AND (s.last_name LIKE @sr OR s.first_name LIKE @sr OR s.stud_code LIKE @sr)"
                params.Add(New MySqlParameter("@sr", $"%{searchTerm}%"))
            End If
            If schoolFilter > 0 Then
                sql &= " AND s.school_id = @sc"
                params.Add(New MySqlParameter("@sc", schoolFilter))
            End If
            If Not String.IsNullOrWhiteSpace(statusFilter) Then
                sql &= " AND s.status = @st"
                params.Add(New MySqlParameter("@st", statusFilter))
            End If
            sql &= " ORDER BY s.last_name, s.first_name"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read() : list.Add(MapStudent(rdr)) : End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As StudentModel
            Const sql = "SELECT s.student_id, s.stud_code, s.resident_id,
                                s.last_name, s.first_name, s.middle_name,
                                s.birth_date, s.gender, s.address, s.purok,
                                s.school_id, sc.school_name,
                                s.grade_year, s.school_year, s.is_scholar,
                                s.status, s.created_by, s.created_at, s.updated_at
                         FROM students s
                         LEFT JOIN schools sc ON sc.school_id = s.school_id
                         WHERE s.student_id = @id LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then Return MapStudent(rdr)
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        Public Function GetTotalCount() As Integer
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand("SELECT COUNT(*) FROM students", conn)
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
            End Using
        End Function

        Public Function GetScholarCount() As Integer
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand("SELECT COUNT(*) FROM students WHERE is_scholar=1 AND status='Enrolled'", conn)
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
            End Using
        End Function

        Public Function GetCountBySchool() As List(Of (SchoolName As String, Count As Integer))
            Dim result As New List(Of (String, Integer))
            Const sql = "SELECT sc.school_name, COUNT(s.student_id) AS cnt
                         FROM schools sc
                         LEFT JOIN students s ON s.school_id = sc.school_id AND s.status='Enrolled'
                         GROUP BY sc.school_id, sc.school_name ORDER BY sc.school_name"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
            Using rdr  = cmd.ExecuteReader()
                While rdr.Read()
                    result.Add((rdr.GetString("school_name"), rdr.GetInt32("cnt")))
                End While
            End Using
            End Using
            End Using
            Return result
        End Function

        Public Function Insert(m As StudentModel) As Boolean
            Const sql = "INSERT INTO students
                (stud_code, resident_id, last_name, first_name, middle_name,
                 birth_date, gender, address, purok, school_id,
                 grade_year, school_year, is_scholar, status, created_by)
                VALUES (@sc,@ri,@ln,@fn,@mn,@bd,@ge,@ad,@pu,@si,@gy,@sy,@is,@st,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(m As StudentModel) As Boolean
            Const sql = "UPDATE students SET
                last_name=@ln, first_name=@fn, middle_name=@mn,
                birth_date=@bd, gender=@ge, address=@ad, purok=@pu,
                school_id=@si, grade_year=@gy, school_year=@sy,
                is_scholar=@is, status=@st
                WHERE student_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                cmd.Parameters.AddWithValue("@id", m.StudentId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Const sql = "DELETE FROM students WHERE student_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function NextStudCode() As String
            Const sql = "SELECT stud_code FROM students ORDER BY student_id DESC LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                Dim last = TryCast(cmd.ExecuteScalar(), String)
                If last Is Nothing Then Return "S-001"
                Dim num = Integer.Parse(last.Replace("S-", "")) + 1
                Return $"S-{num:D3}"
            End Using
            End Using
        End Function

        Private Sub BindParams(cmd As MySqlCommand, m As StudentModel)
            cmd.Parameters.AddWithValue("@sc", m.StudCode)
            cmd.Parameters.AddWithValue("@ri", If(m.ResidentId.HasValue, m.ResidentId.Value, DBNull.Value))
            cmd.Parameters.AddWithValue("@ln", m.LastName)
            cmd.Parameters.AddWithValue("@fn", m.FirstName)
            cmd.Parameters.AddWithValue("@mn", If(String.IsNullOrWhiteSpace(m.MiddleName), DBNull.Value, m.MiddleName))
            cmd.Parameters.AddWithValue("@bd", If(m.BirthDate.HasValue, m.BirthDate.Value.ToString("yyyy-MM-dd"), DBNull.Value))
            cmd.Parameters.AddWithValue("@ge", If(String.IsNullOrWhiteSpace(m.Gender), DBNull.Value, m.Gender))
            cmd.Parameters.AddWithValue("@ad", m.Address)
            cmd.Parameters.AddWithValue("@pu", m.Purok)
            cmd.Parameters.AddWithValue("@si", If(m.SchoolId.HasValue, m.SchoolId.Value, DBNull.Value))
            cmd.Parameters.AddWithValue("@gy", m.GradeYear)
            cmd.Parameters.AddWithValue("@sy", m.SchoolYear)
            cmd.Parameters.AddWithValue("@is", m.IsScholar)
            cmd.Parameters.AddWithValue("@st", m.Status)
            cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
        End Sub

        Private Function MapStudent(rdr As MySqlDataReader) As StudentModel
            Return New StudentModel With {
                .StudentId   = rdr.GetInt32("student_id"),
                .StudCode    = rdr.GetString("stud_code"),
                .ResidentId  = If(rdr.IsDBNull(rdr.GetOrdinal("resident_id")), Nothing, rdr.GetInt32("resident_id")),
                .LastName    = rdr.GetString("last_name"),
                .FirstName   = rdr.GetString("first_name"),
                .MiddleName  = If(rdr.IsDBNull(rdr.GetOrdinal("middle_name")), "", rdr.GetString("middle_name")),
                .BirthDate   = If(rdr.IsDBNull(rdr.GetOrdinal("birth_date")), Nothing, rdr.GetDateTime("birth_date")),
                .Gender      = If(rdr.IsDBNull(rdr.GetOrdinal("gender")), "", rdr.GetString("gender")),
                .Address     = rdr.GetString("address"),
                .Purok       = rdr.GetString("purok"),
                .SchoolId    = If(rdr.IsDBNull(rdr.GetOrdinal("school_id")), Nothing, rdr.GetInt32("school_id")),
                .SchoolName  = If(rdr.IsDBNull(rdr.GetOrdinal("school_name")), "", rdr.GetString("school_name")),
                .GradeYear   = rdr.GetString("grade_year"),
                .SchoolYear  = rdr.GetString("school_year"),
                .IsScholar   = rdr.GetBoolean("is_scholar"),
                .Status      = rdr.GetString("status"),
                .CreatedBy   = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                .CreatedAt   = rdr.GetDateTime("created_at"),
                .UpdatedAt   = rdr.GetDateTime("updated_at")
            }
        End Function

    End Class

    ' ── Schools ──────────────────────────────────────────────────────────

    Public Class SchoolRepository

        Public Function GetAll() As List(Of SchoolModel)
            Dim list As New List(Of SchoolModel)
            Const sql = "SELECT school_id, school_name, school_type, address FROM schools ORDER BY school_name"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
            Using rdr  = cmd.ExecuteReader()
                While rdr.Read()
                    list.Add(New SchoolModel With {
                        .SchoolId   = rdr.GetInt32("school_id"),
                        .SchoolName = rdr.GetString("school_name"),
                        .SchoolType = rdr.GetString("school_type"),
                        .Address    = If(rdr.IsDBNull(rdr.GetOrdinal("address")), "", rdr.GetString("address"))
                    })
                End While
            End Using
            End Using
            End Using
            Return list
        End Function

    End Class

    ' ── Scholarships ─────────────────────────────────────────────────────

    Public Class ScholarshipRepository

        Public Function GetAll(Optional studentId As Integer = 0,
                               Optional statusFilter As String = "") As List(Of ScholarshipModel)
            Dim list As New List(Of ScholarshipModel)
            Dim sql = "SELECT sh.scholarship_id, sh.scholar_code, sh.student_id,
                              CONCAT(st.last_name,', ',st.first_name) AS student_name,
                              sc.school_name,
                              sh.grant_type, sh.amount, sh.school_year, sh.status,
                              sh.created_by, sh.created_at
                       FROM scholarships sh
                       JOIN students st ON st.student_id = sh.student_id
                       LEFT JOIN schools sc ON sc.school_id = st.school_id
                       WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If studentId > 0 Then
                sql &= " AND sh.student_id = @sid"
                params.Add(New MySqlParameter("@sid", studentId))
            End If
            If Not String.IsNullOrWhiteSpace(statusFilter) Then
                sql &= " AND sh.status = @st"
                params.Add(New MySqlParameter("@st", statusFilter))
            End If
            sql &= " ORDER BY sh.created_at DESC"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        list.Add(New ScholarshipModel With {
                            .ScholarshipId = rdr.GetInt32("scholarship_id"),
                            .ScholarCode   = rdr.GetString("scholar_code"),
                            .StudentId     = rdr.GetInt32("student_id"),
                            .StudentName   = rdr.GetString("student_name"),
                            .SchoolName    = If(rdr.IsDBNull(rdr.GetOrdinal("school_name")), "", rdr.GetString("school_name")),
                            .GrantType     = rdr.GetString("grant_type"),
                            .Amount        = rdr.GetDecimal("amount"),
                            .SchoolYear    = rdr.GetString("school_year"),
                            .Status        = rdr.GetString("status"),
                            .CreatedBy     = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                            .CreatedAt     = rdr.GetDateTime("created_at")
                        })
                    End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function Insert(m As ScholarshipModel) As Boolean
            Const sql = "INSERT INTO scholarships
                (scholar_code, student_id, grant_type, amount, school_year, status, created_by)
                VALUES (@sc,@si,@gt,@am,@sy,@st,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@sc", m.ScholarCode)
                cmd.Parameters.AddWithValue("@si", m.StudentId)
                cmd.Parameters.AddWithValue("@gt", m.GrantType)
                cmd.Parameters.AddWithValue("@am", m.Amount)
                cmd.Parameters.AddWithValue("@sy", m.SchoolYear)
                cmd.Parameters.AddWithValue("@st", m.Status)
                cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(m As ScholarshipModel) As Boolean
            Const sql = "UPDATE scholarships SET grant_type=@gt, amount=@am,
                         school_year=@sy, status=@st WHERE scholarship_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@gt", m.GrantType)
                cmd.Parameters.AddWithValue("@am", m.Amount)
                cmd.Parameters.AddWithValue("@sy", m.SchoolYear)
                cmd.Parameters.AddWithValue("@st", m.Status)
                cmd.Parameters.AddWithValue("@id", m.ScholarshipId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Const sql = "DELETE FROM scholarships WHERE scholarship_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function NextScholarCode() As String
            Const sql = "SELECT scholar_code FROM scholarships ORDER BY scholarship_id DESC LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                Dim last = TryCast(cmd.ExecuteScalar(), String)
                If last Is Nothing Then Return "SC-001"
                Dim num = Integer.Parse(last.Replace("SC-", "")) + 1
                Return $"SC-{num:D3}"
            End Using
            End Using
        End Function

    End Class

    ' ── Certificates ─────────────────────────────────────────────────────

    Public Class CertificateRepository

        Public Function GetByResident(residentId As Integer) As List(Of CertificateModel)
            Dim list As New List(Of CertificateModel)
            Const sql = "SELECT c.cert_id, c.cert_code, c.resident_id,
                                CONCAT(r.last_name,', ',r.first_name) AS resident_name,
                                c.cert_type, c.purpose, c.issued_by, c.issued_date,
                                c.or_number, c.amount, c.created_by, c.created_at
                         FROM certificates c
                         JOIN residents r ON r.resident_id = c.resident_id
                         WHERE c.resident_id = @rid ORDER BY c.issued_date DESC"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@rid", residentId)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        list.Add(MapCert(rdr))
                    End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetAll() As List(Of CertificateModel)
            Dim list As New List(Of CertificateModel)
            Const sql = "SELECT c.cert_id, c.cert_code, c.resident_id,
                                CONCAT(r.last_name,', ',r.first_name) AS resident_name,
                                c.cert_type, c.purpose, c.issued_by, c.issued_date,
                                c.or_number, c.amount, c.created_by, c.created_at
                         FROM certificates c
                         JOIN residents r ON r.resident_id = c.resident_id
                         ORDER BY c.issued_date DESC"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
            Using rdr  = cmd.ExecuteReader()
                While rdr.Read() : list.Add(MapCert(rdr)) : End While
            End Using
            End Using
            End Using
            Return list
        End Function

        Public Function Insert(m As CertificateModel) As Boolean
            Const sql = "INSERT INTO certificates
                (cert_code, resident_id, cert_type, purpose, issued_by,
                 issued_date, or_number, amount, created_by)
                VALUES (@cc,@ri,@ct,@pu,@ib,@id,@on,@am,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@cc", m.CertCode)
                cmd.Parameters.AddWithValue("@ri", m.ResidentId)
                cmd.Parameters.AddWithValue("@ct", m.CertType)
                cmd.Parameters.AddWithValue("@pu", If(String.IsNullOrWhiteSpace(m.Purpose), DBNull.Value, m.Purpose))
                cmd.Parameters.AddWithValue("@ib", If(String.IsNullOrWhiteSpace(m.IssuedBy), DBNull.Value, m.IssuedBy))
                cmd.Parameters.AddWithValue("@id", m.IssuedDate.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@on", If(String.IsNullOrWhiteSpace(m.OrNumber), DBNull.Value, m.OrNumber))
                cmd.Parameters.AddWithValue("@am", m.Amount)
                cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function NextCertCode() As String
            Const sql = "SELECT cert_code FROM certificates ORDER BY cert_id DESC LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                Dim last = TryCast(cmd.ExecuteScalar(), String)
                If last Is Nothing Then Return "C-001"
                Dim num = Integer.Parse(last.Replace("C-", "")) + 1
                Return $"C-{num:D3}"
            End Using
            End Using
        End Function

        Private Function MapCert(rdr As MySqlDataReader) As CertificateModel
            Return New CertificateModel With {
                .CertId       = rdr.GetInt32("cert_id"),
                .CertCode     = rdr.GetString("cert_code"),
                .ResidentId   = rdr.GetInt32("resident_id"),
                .ResidentName = rdr.GetString("resident_name"),
                .CertType     = rdr.GetString("cert_type"),
                .Purpose      = If(rdr.IsDBNull(rdr.GetOrdinal("purpose")), "", rdr.GetString("purpose")),
                .IssuedBy     = If(rdr.IsDBNull(rdr.GetOrdinal("issued_by")), "", rdr.GetString("issued_by")),
                .IssuedDate   = rdr.GetDateTime("issued_date"),
                .OrNumber     = If(rdr.IsDBNull(rdr.GetOrdinal("or_number")), "", rdr.GetString("or_number")),
                .Amount       = rdr.GetDecimal("amount"),
                .CreatedBy    = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                .CreatedAt    = rdr.GetDateTime("created_at")
            }
        End Function

    End Class

End Namespace
