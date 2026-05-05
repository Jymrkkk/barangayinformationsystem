Namespace BarangaySystem.Models

    Public Class ScholarshipModel
        Public Property ScholarshipId As Integer
        Public Property ScholarCode   As String = String.Empty
        Public Property StudentId     As Integer
        Public Property StudentName   As String = String.Empty   ' joined from students
        Public Property SchoolName    As String = String.Empty   ' joined from schools
        Public Property GrantType     As String = String.Empty
        Public Property Amount        As Decimal
        Public Property SchoolYear    As String = String.Empty
        Public Property Status        As String = "Active"       ' Active|Inactive|Completed
        Public Property CreatedBy     As Integer
        Public Property CreatedAt     As DateTime
    End Class

    Public Class CertificateModel
        Public Property CertId       As Integer
        Public Property CertCode     As String = String.Empty
        Public Property ResidentId   As Integer
        Public Property ResidentName As String = String.Empty    ' joined
        Public Property CertType     As String = String.Empty
        Public Property Purpose      As String = String.Empty
        Public Property IssuedBy     As String = String.Empty
        Public Property IssuedDate   As DateTime
        Public Property OrNumber     As String = String.Empty
        Public Property Amount       As Decimal
        Public Property CreatedBy    As Integer
        Public Property CreatedAt    As DateTime
    End Class

    Public Class SchoolModel
        Public Property SchoolId   As Integer
        Public Property SchoolName As String = String.Empty
        Public Property SchoolType As String = String.Empty
        Public Property Address    As String = String.Empty
    End Class

End Namespace
