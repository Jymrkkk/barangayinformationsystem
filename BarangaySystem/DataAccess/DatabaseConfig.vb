Imports MySql.Data.MySqlClient
Imports System.Data

Namespace BarangaySystem.DataAccess

    ''' <summary>
    ''' Manages the MySQL connection string and provides a factory
    ''' method for creating open connections.
    ''' </summary>
    Public Module DatabaseConfig

        ' ── Change these values to match your MySQL server ──────────────
        Private Const Server   As String = "localhost"
        Private Const Port     As String = "3306"
        Private Const Database As String = "barangay_db"
        Private Const UserId   As String = "root"
        Private Const Password As String = ""
        ' ─────────────────────────────────────────────────────────────────

        Public ReadOnly Property ConnectionString As String
            Get
                Return $"Server={Server};Port={Port};Database={Database};" &
                       $"Uid={UserId};Pwd={Password};CharSet=utf8mb4;SslMode=None;"
            End Get
        End Property

        ''' <summary>Returns an open MySqlConnection. Caller must dispose it.</summary>
        Public Function GetConnection() As MySqlConnection
            Dim conn As New MySqlConnection(ConnectionString)
            conn.Open()
            Return conn
        End Function

        ''' <summary>Quick connectivity test used at startup.</summary>
        Public Function TestConnection() As Boolean
            Try
                Using conn = GetConnection()
                    Return conn.State = ConnectionState.Open
                End Using
            Catch
                Return False
            End Try
        End Function

    End Module

End Namespace
