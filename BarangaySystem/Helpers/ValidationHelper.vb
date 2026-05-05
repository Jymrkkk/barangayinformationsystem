Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Drawing
Imports BarangaySystem.Models

Namespace BarangaySystem.Helpers

    Public Module ValidationHelper

        ''' <summary>Returns Nothing if valid, or an error message string.</summary>
        Public Function ValidateResident(m As ResidentModel) As String
            If String.IsNullOrWhiteSpace(m.LastName)  Then Return "Last name is required."
            If String.IsNullOrWhiteSpace(m.FirstName) Then Return "First name is required."
            If Not IsValidName(m.LastName)             Then Return "Last name contains invalid characters."
            If Not IsValidName(m.FirstName)            Then Return "First name contains invalid characters."
            If m.BirthDate = DateTime.MinValue         Then Return "Birth date is required."
            If m.BirthDate >= DateTime.Today           Then Return "Birth date must be in the past."
            Dim age = CalculateAge(m.BirthDate)
            If age > 120                               Then Return "Birth date is too far in the past."
            If String.IsNullOrWhiteSpace(m.Gender)     Then Return "Gender is required."
            If String.IsNullOrWhiteSpace(m.Address)    Then Return "Address is required."
            If String.IsNullOrWhiteSpace(m.Purok)      Then Return "Purok is required."
            If Not String.IsNullOrWhiteSpace(m.ContactNo) AndAlso
               Not IsValidPHPhone(m.ContactNo)         Then Return "Contact number must be in format 09XXXXXXXXX."
            If Not String.IsNullOrWhiteSpace(m.Email) AndAlso
               Not IsValidEmail(m.Email)               Then Return "Email address format is invalid."
            Return Nothing
        End Function

        Public Function ValidatePassword(password As String,
                                         confirmPassword As String) As String
            If String.IsNullOrWhiteSpace(password)     Then Return "Password is required."
            If password.Length < 8                     Then Return "Password must be at least 8 characters."
            If Not password.Any(Function(c) Char.IsUpper(c)) Then Return "Password must contain at least one uppercase letter."
            If Not password.Any(Function(c) Char.IsDigit(c)) Then Return "Password must contain at least one digit."
            If password <> confirmPassword             Then Return "Passwords do not match."
            Return Nothing
        End Function

        Public Function ValidateBoNumber(boNumber As String) As String
            If String.IsNullOrWhiteSpace(boNumber) Then Return "BO Number is required."
            If Not Regex.IsMatch(boNumber, "^BO-\d{4}-\d{3,}$") Then
                Return "BO Number must follow format BO-YYYY-NNN (e.g. BO-2025-001)."
            End If
            Return Nothing
        End Function

        ' ── Field-level helpers ──────────────────────────────────────────

        Public Function IsValidName(name As String) As Boolean
            Return Regex.IsMatch(name.Trim(), "^[A-Za-zÀ-ÿ\s\-\.\']+$")
        End Function

        Public Function IsValidPHPhone(phone As String) As Boolean
            Return Regex.IsMatch(phone.Trim(), "^09\d{9}$")
        End Function

        Public Function IsValidEmail(email As String) As Boolean
            Return Regex.IsMatch(email.Trim(),
                "^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)
        End Function

        Public Function CalculateAge(birthDate As DateTime) As Integer
            Dim today = DateTime.Today
            Dim age   = today.Year - birthDate.Year
            If birthDate.Date > today.AddYears(-age) Then age -= 1
            Return age
        End Function

        ' ── UI helpers — mark invalid fields red ─────────────────────────

        Public Sub MarkInvalid(ctrl As Control, Optional message As String = "")
            ctrl.BackColor = ColorTranslator.FromHtml("#fadbd8")
            If Not String.IsNullOrWhiteSpace(message) Then
                Dim tt As New ToolTip()
                tt.SetToolTip(ctrl, message)
            End If
        End Sub

        Public Sub MarkValid(ctrl As Control)
            ctrl.BackColor = Color.White
        End Sub

        Public Sub ClearValidation(ParamArray controls As Control())
            For Each c In controls : MarkValid(c) : Next
        End Sub

    End Module

End Namespace
