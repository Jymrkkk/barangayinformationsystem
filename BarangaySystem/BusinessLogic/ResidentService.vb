Imports BarangaySystem.DataAccess
Imports BarangaySystem.Models
Imports BarangaySystem.Helpers

Namespace BarangaySystem.BusinessLogic

    Public Class ResidentService

        Private ReadOnly _repo    As New ResidentRepository()
        Private ReadOnly _logRepo As New EventLogRepository()

        Public Function GetResidents(Optional search As String = "",
                                     Optional purok As String = "",
                                     Optional status As String = "") As List(Of ResidentModel)
            Return _repo.GetAll(search, purok, status)
        End Function

        Public Function GetById(id As Integer) As ResidentModel
            Return _repo.GetById(id)
        End Function

        Public Function GetStats() As (Total As Integer, Active As Integer, ByPurok As Dictionary(Of String, Integer))
            Return (_repo.GetTotalCount(), _repo.GetActiveCount(), _repo.GetCountByPurok())
        End Function

        Public Function SaveResident(model As ResidentModel, isNew As Boolean) As (Success As Boolean, Message As String)
            ' Validate
            Dim vErr = ValidationHelper.ValidateResident(model)
            If vErr IsNot Nothing Then Return (False, vErr)

            Try
                If isNew Then
                    model.ResCode    = _repo.NextResCode()
                    model.CreatedBy  = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                    Dim ok = _repo.Insert(model)
                    If ok Then _logRepo.Log("INSERT", "Residents", $"Added resident {model.ResCode} — {model.FullName}")
                    Return (ok, If(ok, "Resident added successfully.", "Failed to add resident."))
                Else
                    Dim ok = _repo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Residents", $"Updated resident {model.ResCode} — {model.FullName}")
                    Return (ok, If(ok, "Resident updated successfully.", "Failed to update resident."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function DeleteResident(id As Integer) As (Success As Boolean, Message As String)
            Try
                Dim resident = _repo.GetById(id)
                If resident Is Nothing Then Return (False, "Resident not found.")
                Dim ok = _repo.Delete(id)
                If ok Then _logRepo.Log("DELETE", "Residents", $"Deleted resident {resident.ResCode} — {resident.FullName}")
                Return (ok, If(ok, "Resident deleted.", "Failed to delete resident."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

    End Class

End Namespace
