Imports BarangaySystem.DataAccess
Imports BarangaySystem.Models

Namespace BarangaySystem.BusinessLogic

    Public Class UserService

        Private ReadOnly _repo    As New UserRepository()
        Private ReadOnly _logRepo As New EventLogRepository()

        Public Function GetAll() As List(Of UserModel)
            Return _repo.GetAll()
        End Function

        Public Function GetById(id As Integer) As UserModel
            Return _repo.GetById(id)
        End Function

        Public Function SaveUser(model As UserModel, plainPassword As String,
                                 isNew As Boolean) As (Success As Boolean, Message As String)
            ' Validate
            If String.IsNullOrWhiteSpace(model.Username) Then Return (False, "Username is required.")
            If String.IsNullOrWhiteSpace(model.FullName) Then Return (False, "Full name is required.")
            If String.IsNullOrWhiteSpace(model.Email)    Then Return (False, "Email is required.")
            If isNew Then
                If String.IsNullOrWhiteSpace(plainPassword) Then Return (False, "Password is required.")
                If plainPassword.Length < 8 Then Return (False, "Password must be at least 8 characters.")
                If Not plainPassword.Any(Function(c) Char.IsUpper(c)) Then Return (False, "Password must contain at least one uppercase letter.")
                If Not plainPassword.Any(Function(c) Char.IsDigit(c)) Then Return (False, "Password must contain at least one digit.")
            End If

            Try
                If isNew Then
                    Dim ok = _repo.Insert(model, plainPassword)
                    If ok Then _logRepo.Log("INSERT", "Accounts", $"Added user '{model.Username}' with role {model.Role}")
                    Return (ok, If(ok, "User account created.", "Failed to create account."))
                Else
                    Dim ok = _repo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Accounts", $"Updated user '{model.Username}'")
                    Return (ok, If(ok, "User updated.", "Failed to update user."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function ChangePassword(userId As Integer, newPassword As String) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 8 Then
                Return (False, "Password must be at least 8 characters.")
            End If
            Try
                Dim ok = _repo.ChangePassword(userId, newPassword)
                If ok Then _logRepo.Log("UPDATE", "Accounts", $"Password changed for user ID {userId}")
                Return (ok, If(ok, "Password changed.", "Failed to change password."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function DeleteUser(userId As Integer) As (Success As Boolean, Message As String)
            ' Prevent deleting own account
            If Session.CurrentUser IsNot Nothing AndAlso Session.CurrentUser.UserId = userId Then
                Return (False, "You cannot delete your own account.")
            End If
            Try
                Dim user = _repo.GetById(userId)
                If user Is Nothing Then Return (False, "User not found.")
                Dim ok = _repo.Delete(userId)
                If ok Then _logRepo.Log("DELETE", "Accounts", $"Deleted user '{user.Username}'")
                Return (ok, If(ok, "User deleted.", "Failed to delete user."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

    End Class

End Namespace
