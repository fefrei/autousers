'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.

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

        My.Settings.Save()

        Me.NavigationService.Navigate(New pageStart)
    End Sub
End Class
