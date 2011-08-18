Public Class UserOptions
    Public UserIsDisabled As Boolean
    Public PasswordExpired As Nullable(Of Boolean)
    Public Password As String
    Public HomeDir As String
    Public ProfileDir As String
    Public LogonScript As String

    Public Sub New(Optional ByVal newUserIsDisabled As Boolean = False,
                   Optional ByVal newPasswordExpired As Nullable(Of Boolean) = Nothing,
                   Optional ByVal newPassword As String = Nothing,
                   Optional ByVal newHomeDir As String = Nothing,
                   Optional ByVal newProfileDir As String = Nothing,
                   Optional ByVal newLogonScript As String = Nothing)
        UserIsDisabled = newUserIsDisabled
        PasswordExpired = newPasswordExpired
        Password = newPassword
        HomeDir = newHomeDir
        ProfileDir = newProfileDir
        LogonScript = newLogonScript
    End Sub
End Class
