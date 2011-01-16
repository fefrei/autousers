'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageSettings

    Public WarnOnReductionOfPwdChars As Boolean = False

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
            Dim SecurePasswordLength As Long = ToolBox.getSecurePasswordLength
            If SecurePasswordLength > NewLength AndAlso MsgBox("Die gewählte Kennwortlänge ist zu gering, um ein sicheres Kennwort zu generieren. Beim gewählten Zeichenvorrat ist es sinnvoll, Kennwörter mit mindestens " & SecurePasswordLength.ToString & " Zeichen Länge zu verwenden. Möchten Sie die empfohlen Kennwortlänge verwenden?" & vbCrLf & "Wählen Sie ""Nein"", um die von Ihnen eingegebene Kennwortlänge zu verwenden.", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Unsichere Kennwortlänge") = MsgBoxResult.Yes Then
                NewLength = SecurePasswordLength
            End If
            txtAutoPasswordLength.Text = NewLength.ToString
        End If

    End Sub

    Private Sub SetNewDeleteUserFilesBatch(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim OpenFileDialog As New Microsoft.Win32.OpenFileDialog() With {.Filter = "Batch-Dateien|*.bat"}
        Dim UserCompletedForm As Boolean = OpenFileDialog.ShowDialog
        If UserCompletedForm = False Or OpenFileDialog.FileName = "" Then Exit Sub

        My.Settings.deleteUserFilesBatch = OpenFileDialog.FileName
    End Sub

    Private Sub txtPasswordChars_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs) Handles txtPasswordChars.TextChanged
        If WarnOnReductionOfPwdChars Then
            Dim SecurePasswordLength As Long = ToolBox.getSecurePasswordLength(txtPasswordChars.Text)
            If SecurePasswordLength > My.Settings.AutoPasswordLength Then
                If MsgBox("Beim gewählten Zeichenvorrat ist es sinnvoll, Kennwörter mit mindestens " & SecurePasswordLength.ToString & " Zeichen Länge zu verwenden. Möchten Sie die empfohlen Kennwortlänge verwenden?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Unsichere Kennwortlänge") = MsgBoxResult.Yes Then
                    My.Settings.AutoPasswordLength = SecurePasswordLength
                Else
                    WarnOnReductionOfPwdChars = False
                End If
            End If
        End If
    End Sub

    Private Sub Page_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        'Kennwortsicherheit prüfen
        Dim SecurePasswordLength As Long = ToolBox.getSecurePasswordLength()
        If SecurePasswordLength <= My.Settings.AutoPasswordLength Then
            'Kennwort jetzt sicher --> Benutzer warnen, wenn er das ändert
            WarnOnReductionOfPwdChars = True
        End If
    End Sub
End Class
