Imports MySql.Data.MySqlClient
Imports BarangaySystem.Models

Namespace BarangaySystem.DataAccess

    Public Class ActivityRepository

        Public Function GetAll(Optional statusFilter As String = "",
                               Optional searchTerm As String = "") As List(Of ActivityModel)
            Dim list As New List(Of ActivityModel)
            Dim sql = "SELECT activity_id, act_code, activity_name, description,
                              activity_date, venue, organizer, participants, status,
                              created_by, created_at, updated_at
                       FROM activities WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If Not String.IsNullOrWhiteSpace(statusFilter) Then
                sql &= " AND status = @st"
                params.Add(New MySqlParameter("@st", statusFilter))
            End If
            If Not String.IsNullOrWhiteSpace(searchTerm) Then
                sql &= " AND (activity_name LIKE @s OR venue LIKE @s OR organizer LIKE @s)"
                params.Add(New MySqlParameter("@s", $"%{searchTerm}%"))
            End If
            sql &= " ORDER BY activity_date DESC"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        list.Add(MapActivity(rdr))
                    End While
                End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As ActivityModel
            Const sql = "SELECT activity_id, act_code, activity_name, description,
                                activity_date, venue, organizer, participants, status,
                                created_by, created_at, updated_at
                         FROM activities WHERE activity_id = @id LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then Return MapActivity(rdr)
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        Public Function Insert(m As ActivityModel) As Boolean
            Const sql = "INSERT INTO activities
                (act_code, activity_name, description, activity_date, venue,
                 organizer, participants, status, created_by)
                VALUES (@ac,@an,@de,@ad,@ve,@or,@pa,@st,@cb)"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(m As ActivityModel) As Boolean
            Const sql = "UPDATE activities SET
                activity_name=@an, description=@de, activity_date=@ad,
                venue=@ve, organizer=@or, participants=@pa, status=@st
                WHERE activity_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                BindParams(cmd, m)
                cmd.Parameters.AddWithValue("@id", m.ActivityId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Const sql = "DELETE FROM activities WHERE activity_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", id)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function NextActCode() As String
            Const sql = "SELECT act_code FROM activities ORDER BY activity_id DESC LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                Dim last = TryCast(cmd.ExecuteScalar(), String)
                If last Is Nothing Then Return "A-001"
                Dim num = Integer.Parse(last.Replace("A-", "")) + 1
                Return $"A-{num:D3}"
            End Using
            End Using
        End Function

        Private Sub BindParams(cmd As MySqlCommand, m As ActivityModel)
            cmd.Parameters.AddWithValue("@ac", m.ActCode)
            cmd.Parameters.AddWithValue("@an", m.ActivityName)
            cmd.Parameters.AddWithValue("@de", If(String.IsNullOrWhiteSpace(m.Description), DBNull.Value, m.Description))
            cmd.Parameters.AddWithValue("@ad", m.ActivityDate.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ve", m.Venue)
            cmd.Parameters.AddWithValue("@or", m.Organizer)
            cmd.Parameters.AddWithValue("@pa", m.Participants)
            cmd.Parameters.AddWithValue("@st", m.Status)
            cmd.Parameters.AddWithValue("@cb", m.CreatedBy)
        End Sub

        Private Function MapActivity(rdr As MySqlDataReader) As ActivityModel
            Return New ActivityModel With {
                .ActivityId   = rdr.GetInt32("activity_id"),
                .ActCode      = rdr.GetString("act_code"),
                .ActivityName = rdr.GetString("activity_name"),
                .Description  = If(rdr.IsDBNull(rdr.GetOrdinal("description")), "", rdr.GetString("description")),
                .ActivityDate = rdr.GetDateTime("activity_date"),
                .Venue        = If(rdr.IsDBNull(rdr.GetOrdinal("venue")), "", rdr.GetString("venue")),
                .Organizer    = If(rdr.IsDBNull(rdr.GetOrdinal("organizer")), "", rdr.GetString("organizer")),
                .Participants = rdr.GetInt32("participants"),
                .Status       = rdr.GetString("status"),
                .CreatedBy    = If(rdr.IsDBNull(rdr.GetOrdinal("created_by")), 0, rdr.GetInt32("created_by")),
                .CreatedAt    = rdr.GetDateTime("created_at"),
                .UpdatedAt    = rdr.GetDateTime("updated_at")
            }
        End Function

    End Class

End Namespace
