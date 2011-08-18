Class pageSetContext

    Private Sub btnConfirmMode_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnConfirmMode.Click
        If radioSAM.IsChecked Then
            CurrentState.CurrentContext = New System.DirectoryServices.AccountManagement.PrincipalContext(DirectoryServices.AccountManagement.ContextType.Machine)
        End If
        If radioAD.IsChecked Then
            Try
                CurrentState.CurrentContext = New System.DirectoryServices.AccountManagement.PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Domain, txtName.Text, txtContainer.Text)
                NetAPI.GetGroupNames() 'Test context
                MsgBox("Die Verbindung zum Active Directory wurde aufgebaut.", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Fehler beim Setzen des Kontextes auf Active Directory." + vbCrLf + ex.Message, MsgBoxStyle.Exclamation)
                Exit Sub
            End Try
        End If

        Me.NavigationService.Navigate(New pageStart)
    End Sub
End Class
