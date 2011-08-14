'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageToolbox
    Dim bgwUserOptionsWorker As ComponentModel.BackgroundWorker

    Structure UserOptionsArgument
        Dim WriteMode As Boolean
        Dim UserName As String
        Dim UserOptions As UserOptions

        Public Sub New(ByVal newWriteMode As Boolean, ByVal newUserName As String, Optional ByVal newUserOptions As UserOptions = Nothing)
            WriteMode = newWriteMode
            UserName = newUserName
            UserOptions = newUserOptions
        End Sub
    End Structure

    Private Sub CreateDummyEntry(ByVal myComboBox As ComboBox, ByVal Message As String)
        myComboBox.Items.Clear()
        myComboBox.Items.Add(New ComboBoxItem() With {.Content = Message, .IsEnabled = False})
    End Sub

    Private Function ComboBoxHasValidSelection(ByVal myComboBox As ComboBox)
        'prüft, ob eine Combobox ausgefüllt ist (oder leer / mit einem Dummy-Eintrag gefüllt ist)

        If myComboBox.IsEditable And myComboBox.Text.Length > 0 Then Return True

        If myComboBox.SelectedItem Is Nothing Then Return False
        If DirectCast(myComboBox.SelectedItem, ComboBoxItem).IsEnabled = False Then Return False
        Return True
    End Function

    Private Sub Page_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        'Gruppenliste laden
        Dim GroupList As List(Of String) = NetAPI.GetGroupNames

        cbGroupSelector.Items.Clear()

        For Each GroupName As String In GroupList
            cbGroupSelector.Items.Add(New ComboBoxItem() With {.Content = GroupName})
        Next

        bgwUserOptionsWorker = New ComponentModel.BackgroundWorker
        With bgwUserOptionsWorker
            AddHandler .DoWork, AddressOf DoBackgroundWork
            AddHandler .RunWorkerCompleted, AddressOf HandleBackgroundWorkerFinish
        End With

        cbGroupSelector.Focus()
    End Sub

    Private Sub cbGroupSelector_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles cbGroupSelector.SelectionChanged
        If cbUserSelector Is Nothing Then Exit Sub 'verhindert einen Crash beim Start

        If ComboBoxHasValidSelection(cbGroupSelector) Then
            Try
                cbUserSelector.Items.Clear()
                Dim GroupName As String = DirectCast(cbGroupSelector.SelectedItem, ComboBoxItem).Content
                Dim UserList As List(Of String) = NetAPI.GetUsersInGroup(GroupName)

                For Each UserName In UserList
                    cbUserSelector.Items.Add(New ComboBoxItem With {.Content = UserName})
                Next
            Catch ex As Exception
                CreateDummyEntry(cbUserSelector, "Fehler beim Auslesen der Benutzerliste.")
            End Try
        Else
            CreateDummyEntry(cbUserSelector, "Bitte wählen Sie eine Benutzergruppe aus.")
        End If

        ReEvaluateLoadButtonUnlock()
    End Sub

    Private Sub ReEvaluateLoadButtonUnlock()
        btnLoadUnloadUser.IsEnabled = ComboBoxHasValidSelection(cbGroupSelector) And ComboBoxHasValidSelection(cbUserSelector)
    End Sub

    Private Sub cbUserSelector_TextChanged()
        ReEvaluateLoadButtonUnlock()
    End Sub

    Private Sub SetUIState(ByVal UserLoaded As Boolean, Optional ByVal WorkInProgress As Boolean = False)
        If UserLoaded Then
            gridUserOptions.IsEnabled = True
            cbGroupSelector.IsEnabled = False
            cbUserSelector.IsEnabled = False
            btnLoadUnloadUser.Content = "Bearbeitung abbrechen"
            btnLoadUnloadUser.IsCancel = True
            btnLoadUnloadUser.IsDefault = False
            btnSaveUserOptions.IsDefault = True
        Else
            gridUserOptions.IsEnabled = False
            cbGroupSelector.IsEnabled = True
            cbUserSelector.IsEnabled = True
            btnLoadUnloadUser.Content = "Benutzer bearbeiten / anlegen"
            btnLoadUnloadUser.IsCancel = False
            btnLoadUnloadUser.IsDefault = True
            btnSaveUserOptions.IsDefault = False
        End If
        If WorkInProgress Then
            DirectCast(Me.Parent, NavigationWindow).IsEnabled = False
            prgWorking.Visibility = Windows.Visibility.Visible
            gridUserOptions.Visibility = Windows.Visibility.Collapsed
        Else
            DirectCast(Me.Parent, NavigationWindow).IsEnabled = True
            prgWorking.Visibility = Windows.Visibility.Collapsed
            gridUserOptions.Visibility = Windows.Visibility.Visible
        End If
    End Sub

    Private Sub btnLoadUnloadUser_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnLoadUnloadUser.Click
        If gridUserOptions.IsEnabled Then
            SetUIState(False)
        Else
            Dim UserName As String = cbUserSelector.Text
            Dim GroupName As String = DirectCast(cbGroupSelector.SelectedItem, ComboBoxItem).Content

            If NetAPI.DoesUserExist(UserName) Then
                SetUIState(False, True)
                bgwUserOptionsWorker.RunWorkerAsync(New UserOptionsArgument(False, UserName))
            Else
                If MsgBox("Dieses Benutzerkonto existiert nicht. Sie können es aber jetzt anlegen." & vbCrLf & "Möchten Sie das jetzt tun?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2, "Benutzer existiert nicht") = MsgBoxResult.Yes Then
                    'Benutzerkonto anlegen
                    Try
                        'Passwort ermitteln
                        Dim newPassword As String = ToolBox.getPassword

                        'Benutzer erzeugen
                        NetAPI.CreateUser(UserName, newPassword, My.Settings.ExpireNewPasswords)

                        'Home-Dir setzen
                        If My.Settings.setHomeDir Then
                            NetAPI.SetHomeDir(UserName, My.Settings.HomeDir.Replace("$USER", UserName))
                        End If

                        'Benutzer in die Benutzergruppe einordnen
                        If My.Settings.autoAddNewUsersToGroup Then
                            NetAPI.AddUserToGroup(UserName, GroupName)
                        End If

                        MsgBox("Der Benutzer wurde angelegt." & vbCrLf & "Benutzername: " & UserName & vbCrLf & "Kennwort: " & newPassword, MsgBoxStyle.Information)
                    Catch ex As Exception
                        MsgBox("Fehler beim Anlegen des Benutzers." & vbCrLf & ex.Message)
                        Exit Sub
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub DoBackgroundWork(ByVal sender As Object, ByVal e As ComponentModel.DoWorkEventArgs)
        Dim myArgument As UserOptionsArgument = e.Argument
        If myArgument.WriteMode Then
            NetAPI.SetUserOptions(myArgument.UserName, myArgument.UserOptions)
            e.Result = myArgument
        Else
            e.Result = New UserOptionsArgument(False, myArgument.UserName, NetAPI.GetUserOptions(myArgument.UserName))
        End If
    End Sub

    Private Sub HandleBackgroundWorkerFinish(ByVal sender As Object, ByVal e As ComponentModel.RunWorkerCompletedEventArgs)
        'Fehler?
        If e.Error IsNot Nothing Then
            MsgBox("Der Vorgang führte zu einem unerwarteten Fehler." & vbCrLf & "Fehlermeldung: " & e.Error.Message, MsgBoxStyle.Critical)
        End If

        Dim Result As UserOptionsArgument = e.Result
        If Result.WriteMode Then
            'schreiben abgeschlossen
            MsgBox("Der Benutzer wurde geändert.", MsgBoxStyle.Information)
            SetUIState(False)
        Else
            lblPassword.Content = Result.UserOptions.Password
            chkUserDisabled.IsChecked = Result.UserOptions.UserIsDisabled
            chkPasswordExpired.IsChecked = Result.UserOptions.PasswordExpired
            SetUIState(True)
        End If
    End Sub

    Private Sub btnSaveUserOptions_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnSaveUserOptions.Click
        SetUIState(True, True)
        bgwUserOptionsWorker.RunWorkerAsync(New UserOptionsArgument(True, cbUserSelector.Text, New UserOptions(chkUserDisabled.IsChecked, chkPasswordExpired.IsChecked, lblPassword.Content)))
    End Sub

    Private Sub btnResetPassword_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnResetPassword.Click
        lblPassword.Content = ToolBox.getPassword
        chkPasswordExpired.IsChecked = True
    End Sub

    Private Sub btnSetPassword_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnSetPassword.Click
        lblPassword.Content = InputBox("Geben Sie ein Kennwort für den Benutzer ein:", "Kennwort ändern")
    End Sub

    Private Sub btnDeleteUser_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnDeleteUser.Click
        If MsgBox("Möchten Sie das Benutzerkonto wirklich löschen?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2) = MsgBoxResult.Yes Then
            Try
                NetAPI.DeleteUser(cbUserSelector.Text)
                MsgBox("Das Benutzerkonto wurde gelöscht.", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Beim Löschen des Benutzerkontos ist ein Fehler aufgetreten." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            End Try
            SetUIState(False)
        End If
    End Sub
End Class
