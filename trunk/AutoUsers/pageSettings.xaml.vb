'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageSettings

    Private Sub btnSaveSettings_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnSaveSettings.Click
        My.Settings.Save()
        If CurrentState.JobPending Then
            Me.NavigationService.Navigate(New pageStep4Execute)
        Else
            Me.NavigationService.Navigate(New pageStart)
        End If
    End Sub

    Private Sub SetNewDefaultPassword(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim NewPassword As String = InputBox("Bitte geben Sie das Kennwort ein, das für alle neuen Benutzer verwendet werden soll:", "Standardkennwort festlegen", txtDefaultPassword.Text)
        If NewPassword.Length > 0 Then
            txtDefaultPassword.Text = NewPassword
        End If
    End Sub

    Private Sub SetNewAutoPasswordLength(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim NewLength As Integer = Val(InputBox("Bitte geben Sie die Länge ein, die neue Kennwörter haben sollen: ", "Kennwortlänge festlegen", txtAutoPasswordLength.Text)).ToString
        If NewLength > 0 Then
            txtAutoPasswordLength.Text = NewLength.ToString
        End If
    End Sub

    Private Sub SetNewDeleteUserFilesBatch(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim OpenFileDialog As New Microsoft.Win32.OpenFileDialog() With {.Filter = "Batch-Dateien|*.bat"}
        Dim UserCompletedForm As Boolean = OpenFileDialog.ShowDialog
        If UserCompletedForm = False Or OpenFileDialog.FileName = "" Then Exit Sub

        My.Settings.deleteUserFilesBatch = OpenFileDialog.FileName
    End Sub
End Class
