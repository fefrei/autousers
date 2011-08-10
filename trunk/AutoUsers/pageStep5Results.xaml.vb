'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageStep5Results

    Private Sub Page_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        'verhindern, dass der Benutzer von der Seite wegnavigiert
        While Me.NavigationService.CanGoBack
            NavigationService.RemoveBackEntry()
        End While

        'Fehler anzeigen
        If CurrentState.LastErrorList.Count > 0 Then
            listErrors.Items.Clear()
            For Each ErrorText In CurrentState.LastErrorList
                listErrors.Items.Add(New TextBlock With {.TextWrapping = TextWrapping.Wrap, .Text = ErrorText})
            Next
        End If

        'neue Benutzer anzeigen

        If CurrentState.LastNewUsers.Count > 0 Then
            listNewUsers.Items.Clear()
            For Each NewUser In CurrentState.LastNewUsers
                listNewUsers.Items.Add(New TextBlock With {.TextWrapping = TextWrapping.Wrap, .Text = NewUser.UserName & ": " & NewUser.Password})
            Next
        End If

        'Log anzeigen
        If CurrentState.LastLog.Count > 0 Then
            listLog.Items.Clear()
            For Each LogEntry In CurrentState.LastLog
                listLog.Items.Add(New TextBlock With {.TextWrapping = TextWrapping.Wrap, .Text = LogEntry})
            Next
        End If

    End Sub

    Private Sub btnGoToStart_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnGoToStart.Click
        Me.NavigationService.Navigate(New pageStart)
    End Sub

    Private Sub btnExportNewUsers_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnExportNewUsers.Click
        'Benutzerliste exportieren

        If CurrentState.LastNewUsers.Count > 1 Then
            ToolBox.exportUserPasswordList(CurrentState.LastNewUsers) 'diese Funktion fragt den Speicherort selbst ab
        Else
            MsgBox("Es wurden keine Benutzer angelegt.", MsgBoxStyle.Exclamation)
        End If
    End Sub
End Class
