Imports BarangaySystem.DataAccess
Imports BarangaySystem.Models

Namespace BarangaySystem.BusinessLogic

    ''' <summary>
    ''' Handles login, logout, and account lockout.
    ''' Lockout: 5 failed attempts → locked for 15 minutes (in-memory).
    ''' </summary>
    Public Class AuthService

        Private Shared ReadOnly _userRepo As New UserRepository()
        Private Shared ReadOnly _logRepo  As New EventLogRepository()

        ' username → (failCount, lockUntil)
        Private Shared ReadOnly _lockout As New Dictionary(Of String, (Count As Integer, Until As DateTime))

        Public Shared Function Login(username As String, password As String) As (Success As Boolean, Message As String, User As UserModel)
            If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
                Return (False, "Username and password are required.", Nothing)
            End If

            ' Check lockout
            If _lockout.ContainsKey(username.ToLower()) Then
                Dim entry = _lockout(username.ToLower())
                If entry.Count >= 5 AndAlso DateTime.Now < entry.Until Then
                    Dim remaining = CInt(Math.Ceiling((entry.Until - DateTime.Now).TotalMinutes))
                    Return (False, $"Account locked. Try again in {remaining} minute(s).", Nothing)
                ElseIf DateTime.Now >= entry.Until Then
                    _lockout.Remove(username.ToLower())
                End If
            End If

            Try
                Dim user = _userRepo.Authenticate(username, password)
                If user Is Nothing Then
                    ' Increment fail counter
                    If Not _lockout.ContainsKey(username.ToLower()) Then
                        _lockout(username.ToLower()) = (1, DateTime.Now.AddMinutes(15))
                    Else
                        Dim entry2 = _lockout(username.ToLower())
                        _lockout(username.ToLower()) = (entry2.Count + 1, DateTime.Now.AddMinutes(15))
                    End If
                    _logRepo.Log("LOGIN", "System", $"Failed login attempt for '{username}'")
                    Return (False, "Invalid username or password.", Nothing)
                End If

                ' Success — clear lockout, set session
                If _lockout.ContainsKey(username.ToLower()) Then _lockout.Remove(username.ToLower())
                Session.CurrentUser = user
                _logRepo.Log("LOGIN", "System", $"User '{username}' logged in successfully")
                Return (True, "Login successful.", user)

            Catch ex As Exception
                Return (False, $"Login error: {ex.Message}", Nothing)
            End Try
        End Function

        Public Shared Sub Logout()
            If Session.IsLoggedIn Then
                _logRepo.Log("LOGOUT", "System", $"User '{Session.CurrentUser.Username}' logged out")
                Session.Clear()
            End If
        End Sub

    End Class

End Namespace
