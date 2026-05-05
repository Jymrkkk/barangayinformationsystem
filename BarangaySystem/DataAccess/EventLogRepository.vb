Imports MySql.Data.MySqlClient
Imports BarangaySystem.Models

Namespace BarangaySystem.DataAccess

    Public Class EventLogRepository

        ''' <summary>Writes an audit entry. Call this after every data-changing operation.</summary>
        Public Sub Log(eventType As String, moduleName As String,
                       description As String, Optional ipAddress As String = "127.0.0.1")
            Const sql = "INSERT INTO event_logs (user_id, username, event_type, module, description, ip_address)
                         VALUES (@uid, @un, @et, @mo, @de, @ip)"
            Try
                Using conn = DatabaseConfig.GetConnection()
                Using cmd  = New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@uid", If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, DBNull.Value))
                    cmd.Parameters.AddWithValue("@un",  If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.Username, "system"))
                    cmd.Parameters.AddWithValue("@et",  eventType)
                    cmd.Parameters.AddWithValue("@mo",  moduleName)
                    cmd.Parameters.AddWithValue("@de",  description)
                    cmd.Parameters.AddWithValue("@ip",  ipAddress)
                    cmd.ExecuteNonQuery()
                End Using
                End Using
            Catch
                ' Logging must never crash the application
            End Try
        End Sub

        Public Function GetRecent(Optional limit As Integer = 100,
                                  Optional eventTypeFilter As String = "",
                                  Optional moduleFilter As String = "") As List(Of EventLogModel)
            Dim list As New List(Of EventLogModel)
            Dim sql = "SELECT log_id, log_code, user_id, username, event_type, module,
                              description, ip_address, log_time
                       FROM event_logs WHERE 1=1"
            Dim params As New List(Of MySqlParameter)

            If Not String.IsNullOrWhiteSpace(eventTypeFilter) Then
                sql &= " AND event_type = @et"
                params.Add(New MySqlParameter("@et", eventTypeFilter))
            End If
            If Not String.IsNullOrWhiteSpace(moduleFilter) Then
                sql &= " AND module = @mo"
                params.Add(New MySqlParameter("@mo", moduleFilter))
            End If
            sql &= $" ORDER BY log_time DESC LIMIT {limit}"

            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                For Each p In params : cmd.Parameters.Add(p) : Next
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        list.Add(New EventLogModel With {
                            .LogId       = rdr.GetInt32("log_id"),
                            .LogCode     = If(rdr.IsDBNull(rdr.GetOrdinal("log_code")), "", rdr.GetString("log_code")),
                            .UserId      = If(rdr.IsDBNull(rdr.GetOrdinal("user_id")), Nothing, rdr.GetInt32("user_id")),
                            .Username    = rdr.GetString("username"),
                            .EventType   = rdr.GetString("event_type"),
                            .ModuleName  = rdr.GetString("module"),
                            .Description = rdr.GetString("description"),
                            .IpAddress   = If(rdr.IsDBNull(rdr.GetOrdinal("ip_address")), "", rdr.GetString("ip_address")),
                            .LogTime     = rdr.GetDateTime("log_time")
                        })
                    End While
                End Using
            End Using
            End Using
            Return list
        End Function

    End Class

End Namespace
