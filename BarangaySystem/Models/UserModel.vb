Namespace BarangaySystem.Models

    Public Class UserModel
        Public Property UserId       As Integer
        Public Property Username     As String = String.Empty
        Public Property FullName     As String = String.Empty
        Public Property Email        As String = String.Empty
        Public Property PasswordHash As String = String.Empty
        Public Property Role         As String = "Viewer"   ' Admin | Encoder | Viewer
        Public Property IsActive     As Boolean = True
        Public Property CreatedAt    As DateTime
        Public Property UpdatedAt    As DateTime

        ' Convenience helpers
        Public ReadOnly Property IsAdmin   As Boolean = (Role = "Admin")
        Public ReadOnly Property IsEncoder As Boolean = (Role = "Encoder" OrElse Role = "Admin")
    End Class

    ''' <summary>Holds the currently logged-in user for the session.</summary>
    Public Module Session
        Public CurrentUser As UserModel = Nothing

        Public ReadOnly Property IsLoggedIn As Boolean
            Get
                Return CurrentUser IsNot Nothing
            End Get
        End Property

        Public Sub Clear()
            CurrentUser = Nothing
        End Sub
    End Module

End Namespace
