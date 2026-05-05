Namespace BarangaySystem.Models

    Public Class ActivityModel
        Public Property ActivityId   As Integer
        Public Property ActCode      As String = String.Empty
        Public Property ActivityName As String = String.Empty
        Public Property Description  As String = String.Empty
        Public Property ActivityDate As DateTime
        Public Property Venue        As String = String.Empty
        Public Property Organizer    As String = String.Empty
        Public Property Participants As Integer
        Public Property Status       As String = "Upcoming"  ' Upcoming|Ongoing|Completed|Cancelled
        Public Property CreatedBy    As Integer
        Public Property CreatedAt    As DateTime
        Public Property UpdatedAt    As DateTime
    End Class

    Public Class OrdinanceModel
        Public Property OrdinanceId  As Integer
        Public Property BoNumber     As String = String.Empty
        Public Property IntroducedBy As String = String.Empty
        Public Property Description  As String = String.Empty
        Public Property FullText     As String = String.Empty
        Public Property DateEnacted  As DateTime
        Public Property ApprovedBy   As String = String.Empty
        Public Property Status       As String = "Active"    ' Active|Inactive|Repealed
        Public Property CreatedBy    As Integer
        Public Property CreatedAt    As DateTime
        Public Property UpdatedAt    As DateTime
    End Class

    Public Class ResolutionModel
        Public Property ResolutionId As Integer
        Public Property ResNumber    As String = String.Empty
        Public Property Subject      As String = String.Empty
        Public Property Sponsor      As String = String.Empty
        Public Property DatePassed   As DateTime
        Public Property Status       As String = "Pending"   ' Approved|Pending|Rejected
        Public Property CreatedBy    As Integer
        Public Property CreatedAt    As DateTime
        Public Property UpdatedAt    As DateTime
    End Class

    Public Class StudentModel
        Public Property StudentId   As Integer
        Public Property StudCode    As String = String.Empty
        Public Property ResidentId  As Integer?
        Public Property LastName    As String = String.Empty
        Public Property FirstName   As String = String.Empty
        Public Property MiddleName  As String = String.Empty
        Public Property BirthDate   As DateTime?
        Public Property Gender      As String = String.Empty
        Public Property Address     As String = String.Empty
        Public Property Purok       As String = String.Empty
        Public Property SchoolId    As Integer?
        Public Property SchoolName  As String = String.Empty
        Public Property GradeYear   As String = String.Empty
        Public Property SchoolYear  As String = String.Empty
        Public Property IsScholar   As Boolean
        Public Property Status      As String = "Enrolled"   ' Enrolled|Dropped|Graduated
        Public Property CreatedBy   As Integer
        Public Property CreatedAt   As DateTime
        Public Property UpdatedAt   As DateTime

        Public ReadOnly Property FullName As String
            Get
                Return $"{LastName}, {FirstName}"
            End Get
        End Property
    End Class

    Public Class EventLogModel
        Public Property LogId       As Integer
        Public Property LogCode     As String = String.Empty
        Public Property UserId      As Integer?
        Public Property Username    As String = String.Empty
        Public Property EventType   As String = String.Empty
        Public Property ModuleName  As String = String.Empty   ' renamed: Module is a VB keyword
        Public Property Description As String = String.Empty
        Public Property IpAddress   As String = String.Empty
        Public Property LogTime     As DateTime
    End Class

End Namespace
