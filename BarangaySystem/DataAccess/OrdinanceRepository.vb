Imports MySql.Data.MySqlClient
Imports BarangaySystem.Models

Namespace BarangaySystem.DataAccess

    ' ── Ordinances ───────────────────────────────────────────────────────

    Public Class OrdinanceRepository

        Public Function GetAll(Optional statusFilter As String = "",
                               Optional searchTerm As String = "") As List(Of OrdinanceModel)
            Dim list As New List(Of OrdinanceModel)
            Dim sql = "SELECT ordinance_id, bo_number, introduced_by, description,
                              full_text, date_enacted, approved_by, status,
                              created_by, created_at, updated_at
                       FROM ordinances WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If Not String.IsNullOrWhiteSpace(statusFilter) Then
                sql &= " AND status = @st"
                params.Add(New MySqlParameter("@st", statusFilter))
            End If
            If Not String.IsNullOrWhiteSpace(searchTerm) Then
                sql &= " AND (bo_number LIKE @s OR description LIKE @s OR introduced_by LIKE @s)"
                params.Add(New MySqlParameter("@s", $"%{searchTerm}%"))
            End If
            sql &= " ORDER BY date_enacted DESC"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read() : list.Add(MapOrdinance(rdr)) : End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As OrdinanceModel
            Const sql = "SELECT ordinance_id, bo_number, introduced_by, description,
                                full_text, date_enacted, approved_by, status,
                                created_by, created_at, updated_at
                         FROM ordinances WHERE ordinance_id = @id LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then Return MapOrdinance(rdr)
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        Public Function GetTotalCount() As Integer
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand("SELECT COUNT(*) FROM ordinances", conn)
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
            End Using
        End Function

        Public Function Insert(m As OrdinanceModel) As Boolean
            Const sql = "INSERT INTO ordinances
                (bo_number, introduced_by, description, full_text,
                 date_enacted, approved_by, status, created_by)
                VALUES (@bn,@ib,@de,@ft,@da,@ab,@st,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(m As OrdinanceModel) As Boolean
            Const sql = "UPDATE ordinances SET
                bo_number=@bn, introduced_by=@ib, description=@de, full_text=@ft,
                date_enacted=@da, approved_by=@ab, status=@st
                WHERE ordinance_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                cmd.Parameters.AddWithValue("@id", m.OrdinanceId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Const sql = "DELETE FROM ordinances WHERE ordinance_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Private Sub BindParams(cmd As MySqlCommand, m As OrdinanceModel)
            cmd.Parameters.AddWithValue("@bn", m.BoNumber)
            cmd.Parameters.AddWithValue("@ib", m.IntroducedBy)
            cmd.Parameters.AddWithValue("@de", m.Description)
            cmd.Parameters.AddWithValue("@ft", If(String.IsNullOrWhiteSpace(m.FullText), DBNull.Value, m.FullText))
            cmd.Parameters.AddWithValue("@da", m.DateEnacted.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ab", If(String.IsNullOrWhiteSpace(m.ApprovedBy), DBNull.Value, m.ApprovedBy))
            cmd.Parameters.AddWithValue("@st", m.Status)
            cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
        End Sub

        Private Function MapOrdinance(rdr As MySqlDataReader) As OrdinanceModel
            Return New OrdinanceModel With {
                .OrdinanceId  = rdr.GetInt32("ordinance_id"),
                .BoNumber     = rdr.GetString("bo_number"),
                .IntroducedBy = rdr.GetString("introduced_by"),
                .Description  = rdr.GetString("description"),
                .FullText     = If(rdr.IsDBNull(rdr.GetOrdinal("full_text")), "", rdr.GetString("full_text")),
                .DateEnacted  = rdr.GetDateTime("date_enacted"),
                .ApprovedBy   = If(rdr.IsDBNull(rdr.GetOrdinal("approved_by")), "", rdr.GetString("approved_by")),
                .Status       = rdr.GetString("status"),
                .CreatedBy    = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                .CreatedAt    = rdr.GetDateTime("created_at"),
                .UpdatedAt    = rdr.GetDateTime("updated_at")
            }
        End Function

    End Class

    ' ── Resolutions ──────────────────────────────────────────────────────

    Public Class ResolutionRepository

        Public Function GetAll(Optional statusFilter As String = "",
                               Optional searchTerm As String = "") As List(Of ResolutionModel)
            Dim list As New List(Of ResolutionModel)
            Dim sql = "SELECT resolution_id, res_number, subject, sponsor,
                              date_passed, status, created_by, created_at, updated_at
                       FROM resolutions WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If Not String.IsNullOrWhiteSpace(statusFilter) Then
                sql &= " AND status = @st"
                params.Add(New MySqlParameter("@st", statusFilter))
            End If
            If Not String.IsNullOrWhiteSpace(searchTerm) Then
                sql &= " AND (res_number LIKE @s OR subject LIKE @s OR sponsor LIKE @s)"
                params.Add(New MySqlParameter("@s", $"%{searchTerm}%"))
            End If
            sql &= " ORDER BY date_passed DESC"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read() : list.Add(MapResolution(rdr)) : End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As ResolutionModel
            Const sql = "SELECT resolution_id, res_number, subject, sponsor,
                                date_passed, status, created_by, created_at, updated_at
                         FROM resolutions WHERE resolution_id = @id LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then Return MapResolution(rdr)
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        Public Function Insert(m As ResolutionModel) As Boolean
            Const sql = "INSERT INTO resolutions (res_number, subject, sponsor, date_passed, status, created_by)
                         VALUES (@rn,@su,@sp,@dp,@st,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(m As ResolutionModel) As Boolean
            Const sql = "UPDATE resolutions SET res_number=@rn, subject=@su, sponsor=@sp,
                         date_passed=@dp, status=@st WHERE resolution_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                cmd.Parameters.AddWithValue("@id", m.ResolutionId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Const sql = "DELETE FROM resolutions WHERE resolution_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Private Sub BindParams(cmd As MySqlCommand, m As ResolutionModel)
            cmd.Parameters.AddWithValue("@rn", m.ResNumber)
            cmd.Parameters.AddWithValue("@su", m.Subject)
            cmd.Parameters.AddWithValue("@sp", If(String.IsNullOrWhiteSpace(m.Sponsor), DBNull.Value, m.Sponsor))
            cmd.Parameters.AddWithValue("@dp", m.DatePassed.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@st", m.Status)
            cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
        End Sub

        Private Function MapResolution(rdr As MySqlDataReader) As ResolutionModel
            Return New ResolutionModel With {
                .ResolutionId = rdr.GetInt32("resolution_id"),
                .ResNumber    = rdr.GetString("res_number"),
                .Subject      = rdr.GetString("subject"),
                .Sponsor      = If(rdr.IsDBNull(rdr.GetOrdinal("sponsor")), "", rdr.GetString("sponsor")),
                .DatePassed   = rdr.GetDateTime("date_passed"),
                .Status       = rdr.GetString("status"),
                .CreatedBy    = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                .CreatedAt    = rdr.GetDateTime("created_at"),
                .UpdatedAt    = rdr.GetDateTime("updated_at")
            }
        End Function

    End Class

End Namespace
