Namespace BarangaySystem.Models

    Public Class ResidentModel
        Public Property ResidentId  As Integer
        Public Property ResCode     As String = String.Empty
        Public Property LastName    As String = String.Empty
        Public Property FirstName   As String = String.Empty
        Public Property MiddleName  As String = String.Empty
        Public Property BirthDate   As DateTime
        Public Property Gender      As String = String.Empty
        Public Property CivilStatus As String = "Single"
        Public Property Address     As String = String.Empty
        Public Property Purok       As String = String.Empty
        Public Property ContactNo   As String = String.Empty
        Public Property Email       As String = String.Empty
        Public Property Occupation  As String = String.Empty
        Public Property IsVoter     As Boolean
        Public Property IsSoloParent As Boolean
        Public Property IsActive    As Boolean = True
        Public Property CreatedBy   As Integer
        Public Property CreatedAt   As DateTime
        Public Property UpdatedAt   As DateTime

        Public ReadOnly Property FullName As String
            Get
                Dim mid = If(String.IsNullOrWhiteSpace(MiddleName), "", $" {MiddleName.Substring(0, 1)}.")
                Return $"{LastName}, {FirstName}{mid}"
            End Get
        End Property

        Public ReadOnly Property Age As Integer
            Get
                Dim today = DateTime.Today
                Dim a = today.Year - BirthDate.Year
                If BirthDate.Date > today.AddYears(-a) Then a -= 1
                Return a
            End Get
        End Property
    End Class

End Namespace
