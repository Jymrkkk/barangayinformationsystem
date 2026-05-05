Imports BarangaySystem.DataAccess
Imports BarangaySystem.Models

Namespace BarangaySystem.BusinessLogic

    Public Class ActivityService

        Private ReadOnly _repo    As New ActivityRepository()
        Private ReadOnly _logRepo As New EventLogRepository()

        Public Function GetActivities(Optional status As String = "",
                                      Optional search As String = "") As List(Of ActivityModel)
            Return _repo.GetAll(status, search)
        End Function

        Public Function GetById(id As Integer) As ActivityModel
            Return _repo.GetById(id)
        End Function

        Public Function GetUpcomingCount() As Integer
            Return _repo.GetAll("Upcoming").Count
        End Function

        Public Function Save(model As ActivityModel, isNew As Boolean) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(model.ActivityName) Then Return (False, "Activity name is required.")
            If String.IsNullOrWhiteSpace(model.Venue)        Then Return (False, "Venue is required.")
            If String.IsNullOrWhiteSpace(model.Organizer)    Then Return (False, "Organizer is required.")

            Try
                If isNew Then
                    model.ActCode   = _repo.NextActCode()
                    model.CreatedBy = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                    Dim ok = _repo.Insert(model)
                    If ok Then _logRepo.Log("INSERT", "Activities", $"Added activity {model.ActCode} — {model.ActivityName}")
                    Return (ok, If(ok, "Activity added.", "Failed to add activity."))
                Else
                    Dim ok = _repo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Activities", $"Updated activity {model.ActCode} — {model.ActivityName}")
                    Return (ok, If(ok, "Activity updated.", "Failed to update activity."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function Delete(id As Integer) As (Success As Boolean, Message As String)
            Try
                Dim act = _repo.GetById(id)
                If act Is Nothing Then Return (False, "Activity not found.")
                Dim ok = _repo.Delete(id)
                If ok Then _logRepo.Log("DELETE", "Activities", $"Deleted activity {act.ActCode} — {act.ActivityName}")
                Return (ok, If(ok, "Activity deleted.", "Failed to delete."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

    End Class

    ' ─────────────────────────────────────────────────────────────────────

    Public Class OrdinanceService

        Private ReadOnly _ordRepo As New OrdinanceRepository()
        Private ReadOnly _resRepo As New ResolutionRepository()
        Private ReadOnly _logRepo As New EventLogRepository()

        Public Function GetOrdinances(Optional status As String = "",
                                      Optional search As String = "") As List(Of OrdinanceModel)
            Return _ordRepo.GetAll(status, search)
        End Function

        Public Function GetResolutions(Optional status As String = "",
                                       Optional search As String = "") As List(Of ResolutionModel)
            Return _resRepo.GetAll(status, search)
        End Function

        Public Function GetTotalOrdinances() As Integer
            Return _ordRepo.GetTotalCount()
        End Function

        Public Function SaveOrdinance(model As OrdinanceModel, isNew As Boolean) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(model.BoNumber)     Then Return (False, "BO Number is required.")
            If String.IsNullOrWhiteSpace(model.IntroducedBy) Then Return (False, "Introduced By is required.")
            If String.IsNullOrWhiteSpace(model.Description)  Then Return (False, "Description is required.")

            Try
                If isNew Then
                    model.CreatedBy = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                    Dim ok = _ordRepo.Insert(model)
                    If ok Then _logRepo.Log("INSERT", "Ordinances", $"Added ordinance {model.BoNumber}")
                    Return (ok, If(ok, "Ordinance added.", "Failed to add ordinance."))
                Else
                    Dim ok = _ordRepo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Ordinances", $"Updated ordinance {model.BoNumber}")
                    Return (ok, If(ok, "Ordinance updated.", "Failed to update."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function DeleteOrdinance(id As Integer) As (Success As Boolean, Message As String)
            Try
                Dim ord = _ordRepo.GetById(id)
                If ord Is Nothing Then Return (False, "Ordinance not found.")
                Dim ok = _ordRepo.Delete(id)
                If ok Then _logRepo.Log("DELETE", "Ordinances", $"Deleted ordinance {ord.BoNumber}")
                Return (ok, If(ok, "Ordinance deleted.", "Failed to delete."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function SaveResolution(model As ResolutionModel, isNew As Boolean) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(model.ResNumber) Then Return (False, "Resolution number is required.")
            If String.IsNullOrWhiteSpace(model.Subject)   Then Return (False, "Subject is required.")

            Try
                If isNew Then
                    model.CreatedBy = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                    Dim ok = _resRepo.Insert(model)
                    If ok Then _logRepo.Log("INSERT", "Ordinances", $"Added resolution {model.ResNumber}")
                    Return (ok, If(ok, "Resolution added.", "Failed to add."))
                Else
                    Dim ok = _resRepo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Ordinances", $"Updated resolution {model.ResNumber}")
                    Return (ok, If(ok, "Resolution updated.", "Failed to update."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function DeleteResolution(id As Integer) As (Success As Boolean, Message As String)
            Try
                Dim res = _resRepo.GetById(id)
                If res Is Nothing Then Return (False, "Resolution not found.")
                Dim ok = _resRepo.Delete(id)
                If ok Then _logRepo.Log("DELETE", "Ordinances", $"Deleted resolution {res.ResNumber}")
                Return (ok, If(ok, "Resolution deleted.", "Failed to delete."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

    End Class

    ' ─────────────────────────────────────────────────────────────────────

    Public Class StudentService

        Private ReadOnly _stuRepo  As New StudentRepository()
        Private ReadOnly _schRepo  As New ScholarshipRepository()
        Private ReadOnly _logRepo  As New EventLogRepository()

        Public Function GetStudents(Optional search As String = "",
                                    Optional schoolId As Integer = 0,
                                    Optional status As String = "") As List(Of StudentModel)
            Return _stuRepo.GetAll(search, schoolId, status)
        End Function

        Public Function GetById(id As Integer) As StudentModel
            Return _stuRepo.GetById(id)
        End Function

        Public Function GetStats() As (Total As Integer, Scholars As Integer, BySchool As List(Of (String, Integer)))
            Return (_stuRepo.GetTotalCount(), _stuRepo.GetScholarCount(), _stuRepo.GetCountBySchool())
        End Function

        Public Function GetScholarships(Optional studentId As Integer = 0,
                                        Optional status As String = "") As List(Of ScholarshipModel)
            Return _schRepo.GetAll(studentId, status)
        End Function

        Public Function SaveStudent(model As StudentModel, isNew As Boolean) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(model.LastName)  Then Return (False, "Last name is required.")
            If String.IsNullOrWhiteSpace(model.FirstName) Then Return (False, "First name is required.")
            If String.IsNullOrWhiteSpace(model.Address)   Then Return (False, "Address is required.")

            Try
                If isNew Then
                    model.StudCode  = _stuRepo.NextStudCode()
                    model.CreatedBy = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                    Dim ok = _stuRepo.Insert(model)
                    If ok Then _logRepo.Log("INSERT", "Students", $"Added student {model.StudCode} — {model.FullName}")
                    Return (ok, If(ok, "Student added.", "Failed to add student."))
                Else
                    Dim ok = _stuRepo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Students", $"Updated student {model.StudCode} — {model.FullName}")
                    Return (ok, If(ok, "Student updated.", "Failed to update."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function DeleteStudent(id As Integer) As (Success As Boolean, Message As String)
            Try
                Dim stu = _stuRepo.GetById(id)
                If stu Is Nothing Then Return (False, "Student not found.")
                Dim ok = _stuRepo.Delete(id)
                If ok Then _logRepo.Log("DELETE", "Students", $"Deleted student {stu.StudCode} — {stu.FullName}")
                Return (ok, If(ok, "Student deleted.", "Failed to delete."))
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

        Public Function SaveScholarship(model As ScholarshipModel, isNew As Boolean) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(model.GrantType) Then Return (False, "Grant type is required.")
            If String.IsNullOrWhiteSpace(model.SchoolYear) Then Return (False, "School year is required.")

            Try
                If isNew Then
                    model.ScholarCode = _schRepo.NextScholarCode()
                    model.CreatedBy   = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                    Dim ok = _schRepo.Insert(model)
                    If ok Then _logRepo.Log("INSERT", "Students", $"Added scholarship {model.ScholarCode}")
                    Return (ok, If(ok, "Scholarship added.", "Failed to add."))
                Else
                    Dim ok = _schRepo.Update(model)
                    If ok Then _logRepo.Log("UPDATE", "Students", $"Updated scholarship {model.ScholarCode}")
                    Return (ok, If(ok, "Scholarship updated.", "Failed to update."))
                End If
            Catch ex As Exception
                Return (False, $"Database error: {ex.Message}")
            End Try
        End Function

    End Class

End Namespace
