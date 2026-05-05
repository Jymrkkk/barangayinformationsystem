Imports MySql.Data.MySqlClient
Imports BarangaySystem.Models
Imports BCrypt.Net

Namespace BarangaySystem.DataAccess

    Public Class UserRepository

        ' ── Authentication ───────────────────────────────────────────────

        Public Function Authenticate(username As String, password As String) As UserModel
            Const sql = "SELECT user_id, username, full_name, email, password_hash, role, is_active,
                                created_at, updated_at
                         FROM users WHERE username = @u AND is_active = 1 LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@u", username)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim hash = rdr.GetString("password_hash")
                        Try
                            If BCrypt.Net.BCrypt.Verify(password, hash) Then
                                Return MapUser(rdr)
                            End If
                        Catch
                            ' Invalid hash format — treat as wrong password
                        End Try
                    End If
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        ' ── CRUD ─────────────────────────────────────────────────────────

        Public Function GetAll() As List(Of UserModel)
            Dim list As New List(Of UserModel)
            Const sql = "SELECT user_id, username, full_name, email, password_hash, role, is_active,
                                created_at, updated_at FROM users ORDER BY user_id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
            Using rdr  = cmd.ExecuteReader()
                While rdr.Read()
                    list.Add(MapUser(rdr))
                End While
            End Using
            End Using
            End Using
            Return list
        End Function

        Public Function GetById(userId As Integer) As UserModel
            Const sql = "SELECT user_id, username, full_name, email, password_hash, role, is_active,
                                created_at, updated_at FROM users WHERE user_id = @id LIMIT 1"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", userId)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then Return MapUser(rdr)
                End Using
            End Using
            End Using
            Return Nothing
        End Function

        Public Function Insert(model As UserModel, plainPassword As String) As Boolean
            Const sql = "INSERT INTO users (username, full_name, email, password_hash, role, is_active)
                         VALUES (@u, @fn, @em, @ph, @ro, @ia)"
            Dim hash = BCrypt.Net.BCrypt.HashPassword(plainPassword)
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@u",  model.Username)
                cmd.Parameters.AddWithValue("@fn", model.FullName)
                cmd.Parameters.AddWithValue("@em", model.Email)
                cmd.Parameters.AddWithValue("@ph", hash)
                cmd.Parameters.AddWithValue("@ro", model.Role)
                cmd.Parameters.AddWithValue("@ia", model.IsActive)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Update(model As UserModel) As Boolean
            Const sql = "UPDATE users SET full_name=@fn, email=@em, role=@ro, is_active=@ia
                         WHERE user_id=@id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@fn", model.FullName)
                cmd.Parameters.AddWithValue("@em", model.Email)
                cmd.Parameters.AddWithValue("@ro", model.Role)
                cmd.Parameters.AddWithValue("@ia", model.IsActive)
                cmd.Parameters.AddWithValue("@id", model.UserId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function ChangePassword(userId As Integer, newPlainPassword As String) As Boolean
            Const sql = "UPDATE users SET password_hash=@ph WHERE user_id=@id"
            Dim hash = BCrypt.Net.BCrypt.HashPassword(newPlainPassword)
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@ph", hash)
                cmd.Parameters.AddWithValue("@id", userId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        Public Function Delete(userId As Integer) As Boolean
            Const sql = "DELETE FROM users WHERE user_id = @id"
            Using conn = DatabaseConfig.GetConnection()
            Using cmd  = New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", userId)
                Return cmd.ExecuteNonQuery() > 0
            End Using
            End Using
        End Function

        ' ── Mapper ───────────────────────────────────────────────────────

        Private Function MapUser(rdr As MySqlDataReader) As UserModel
            Return New UserModel With {
                .UserId       = rdr.GetInt32("user_id"),
                .Username     = rdr.GetString("username"),
                .FullName     = rdr.GetString("full_name"),
                .Email        = rdr.GetString("email"),
                .PasswordHash = rdr.GetString("password_hash"),
                .Role         = rdr.GetString("role"),
                .IsActive     = rdr.GetBoolean("is_active"),
                .CreatedAt    = If(rdr.IsDBNull(rdr.GetOrdinal("created_at")), DateTime.MinValue, rdr.GetDateTime("created_at")),
                .UpdatedAt    = If(rdr.IsDBNull(rdr.GetOrdinal("updated_at")), DateTime.MinValue, rdr.GetDateTime("updated_at"))
            }
        End Function

    End Class

End Namespace
