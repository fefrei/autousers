Public Class UserOptions
    Public UserIsDisabled As Boolean
    Public PasswordExpired As Nullable(Of Boolean)
    Public Password As String

    Public Sub New(Optional ByVal newUserIsDisabled As Boolean = False, Optional ByVal newPasswordExpired As Nullable(Of Boolean) = False, Optional ByVal newPassword As String = Nothing)
        UserIsDisabled = newUserIsDisabled
        PasswordExpired = newPasswordExpired
        Password = newPassword
    End Sub
End Class
