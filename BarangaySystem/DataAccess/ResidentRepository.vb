Imports MySql.Data.MySqlClient
Imports BarangaySystem.Models

Namespace BarangaySystem.DataAccess

    Public Class ResidentRepository

        Private Const SelectCols As String =
            "resident_id, res_code, last_name, first_name, middle_name, birth_date,
             gender, civil_status, address, purok, contact_no, email, occupation,
             is_voter, is_solo_parent, birth_certificate, is_active,
             created_by, created_at, updated_at"

        ' ── Read ─────────────────────────────────────────────────────────

        Public Function GetAll(Optional searchTerm As String = "",
                               Optional purokFilter As String = "",
                               Optional statusFilter As String = "") As List(Of ResidentModel)
            Dim list As New List(Of ResidentModel)
            Dim sql = $"SELECT {SelectCols} FROM residents WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If Not String.IsNullOrWhiteSpace(searchTerm) Then
                sql &= " AND (last_name LIKE @s OR first_name LIKE @s OR res_code LIKE @s OR contact_no LIKE @s)"
                params.Add(New MySqlParameter("@s", $"%{searchTerm}%"))
            End If
            If Not String.IsNullOrWhiteSpace(purokFilter) Then
                sql &= " AND purok = @p"
                params.Add(New MySqlParameter("@p", purokFilter))
            End If
            If statusFilter = "Active" Then
                sql &= " AND is_active = 1"
            ElseIf statusFilter = "Inactive" Then
                sql &= " AND is_active = 0"
            End If
            sql &= " ORDER BY last_name, first_name"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        list.Add(MapResident(rdr))
                    End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetById(residentId As Integer) As ResidentModel
            Dim sql = $"SELECT {SelectCols} FROM residents WHERE resident_id = @id LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", residentId)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then Return MapResident(rdr)
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        Public Function GetTotalCount() As Integer
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand("SELECT COUNT(*) FROM residents", conn)
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
            End Using
        End Function

        Public Function GetActiveCount() As Integer
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand("SELECT COUNT(*) FROM residents WHERE is_active=1", conn)
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
            End Using
        End Function

        Public Function GetCountByPurok() As Dictionary(Of String, Integer)
            Dim dict As New Dictionary(Of String, Integer)
            Const sql = "SELECT purok, COUNT(*) AS cnt FROM residents GROUP BY purok ORDER BY purok"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
            Using rdr  = cmd.ExecuteReader()
                While rdr.Read()
                    dict(rdr.GetString("purok")) = rdr.GetInt32("cnt")
                End While
            End Using
            End Using
            End Using
            Return dict
        End Function

        ' ── Write ────────────────────────────────────────────────────────

        Public Function Insert(model As ResidentModel) As Boolean
            Const sql = "INSERT INTO residents
                (res_code, last_name, first_name, middle_name, birth_date, gender,
                 civil_status, address, purok, contact_no, email, occupation,
                 is_voter, is_solo_parent, birth_certificate, is_active, created_by)
                VALUES
                (@rc,@ln,@fn,@mn,@bd,@ge,@cs,@ad,@pu,@cn,@em,@oc,@iv,@isp,@bc,@ia,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, model)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(model As ResidentModel) As Boolean
            Const sql = "UPDATE residents SET
                last_name=@ln, first_name=@fn, middle_name=@mn, birth_date=@bd,
                gender=@ge, civil_status=@cs, address=@ad, purok=@pu,
                contact_no=@cn, email=@em, occupation=@oc,
                is_voter=@iv, is_solo_parent=@isp, birth_certificate=@bc, is_active=@ia
                WHERE resident_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, model)
                cmd.Parameters.AddWithValue("@id", model.ResidentId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(residentId As Integer) As Boolean
            Const sql = "DELETE FROM residents WHERE resident_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", residentId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function NextResCode() As String
            Const sql = "SELECT MAX(CAST(SUBSTRING(res_code, 3) AS UNSIGNED)) FROM residents"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                Dim result = cmd.ExecuteScalar()
                Dim num    = If(result Is DBNull.Value OrElse result Is Nothing, 0,
                                Convert.ToInt32(result))
                Return $"R-{num + 1:D3}"
            End Using
            End Using
        End Function

        ' ── Helpers ──────────────────────────────────────────────────────

        Private Sub BindParams(cmd As MySqlCommand, m As ResidentModel)
            cmd.Parameters.AddWithValue("@rc", m.ResCode)
            cmd.Parameters.AddWithValue("@ln", m.LastName)
            cmd.Parameters.AddWithValue("@fn", m.FirstName)
            cmd.Parameters.AddWithValue("@mn", If(String.IsNullOrWhiteSpace(m.MiddleName), DBNull.Value, m.MiddleName))
            cmd.Parameters.AddWithValue("@bd", m.BirthDate.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ge", m.Gender)
            cmd.Parameters.AddWithValue("@cs", m.CivilStatus)
            cmd.Parameters.AddWithValue("@ad", m.Address)
            cmd.Parameters.AddWithValue("@pu", m.Purok)
            cmd.Parameters.AddWithValue("@cn", If(String.IsNullOrWhiteSpace(m.ContactNo), DBNull.Value, m.ContactNo))
            cmd.Parameters.AddWithValue("@em", If(String.IsNullOrWhiteSpace(m.Email), DBNull.Value, m.Email))
            cmd.Parameters.AddWithValue("@oc", If(String.IsNullOrWhiteSpace(m.Occupation), DBNull.Value, m.Occupation))
            cmd.Parameters.AddWithValue("@iv",  m.IsVoter)
            cmd.Parameters.AddWithValue("@isp", m.IsSoloParent)
            cmd.Parameters.AddWithValue("@bc",  If(m.BirthCertificate Is Nothing, DBNull.Value, CObj(m.BirthCertificate)))
            cmd.Parameters.AddWithValue("@ia",  m.IsActive)
            cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
        End Sub

        Private Function MapResident(rdr As MySqlDataReader) As ResidentModel
            Dim bcOrd = rdr.GetOrdinal("birth_certificate")
            Return New ResidentModel With {
                .ResidentId       = rdr.GetInt32("resident_id"),
                .ResCode          = rdr.GetString("res_code"),
                .LastName         = rdr.GetString("last_name"),
                .FirstName        = rdr.GetString("first_name"),
                .MiddleName       = If(rdr.IsDBNull(rdr.GetOrdinal("middle_name")), "", rdr.GetString("middle_name")),
                .BirthDate        = rdr.GetDateTime("birth_date"),
                .Gender           = rdr.GetString("gender"),
                .CivilStatus      = rdr.GetString("civil_status"),
                .Address          = rdr.GetString("address"),
                .Purok            = rdr.GetString("purok"),
                .ContactNo        = If(rdr.IsDBNull(rdr.GetOrdinal("contact_no")), "", rdr.GetString("contact_no")),
                .Email            = If(rdr.IsDBNull(rdr.GetOrdinal("email")), "", rdr.GetString("email")),
                .Occupation       = If(rdr.IsDBNull(rdr.GetOrdinal("occupation")), "", rdr.GetString("occupation")),
                .IsVoter          = rdr.GetBoolean("is_voter"),
                .IsSoloParent     = rdr.GetBoolean("is_solo_parent"),
                .BirthCertificate = If(rdr.IsDBNull(bcOrd), Nothing,
                                       CType(rdr.GetValue(bcOrd), Byte())),
                .IsActive         = rdr.GetBoolean("is_active"),
                .CreatedBy        = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                .CreatedAt        = rdr.GetDateTime("created_at"),
                .UpdatedAt        = rdr.GetDateTime("updated_at")
            }
        End Function

    End Class

End Namespace
