'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageStep4Execute
    Public PageIsVirgin As Boolean = True
    Public bgwApplyChanges As ComponentModel.BackgroundWorker
    Public ProcessIsRunning As Boolean = False 'Gibt an, ob zur Zeit ein Vorgang läuft.

    Class ApplyChangesResult
        Public Log As List(Of String)
        Public NewUsers As List(Of UserNameWithPassword)
        Public ErrorList As List(Of String)
    End Class

    Private Sub Page_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        If Not PageIsVirgin Then
            'aktuelle Version der Seite erzeugen
            Me.NavigationService.Navigate(New pageStep4Execute)
        End If
        PageIsVirgin = False

        CurrentState.JobPending = True

        'verhindern, dass der Benutzer von der Seite wegnavigiert
        While Me.NavigationService.CanGoBack
            NavigationService.RemoveBackEntry()
        End While

        'Einsetzen der wichtigen Informationen in den Zusammenfassungstext
        linkGroupName.Inlines.Add(New Run(CurrentState.CurrentUserGroup))
        linkListName.Inlines.Add(New Run(CurrentState.CurrentUserListFullName))
        linkNewUsers.Inlines.Add(New Run(CurrentState.CurrentlyPlannedUserChanges.UsersToAdd.Count.ToString))
        linkOldUsers.Inlines.Add(New Run(CurrentState.CurrentlyPlannedUserChanges.UsersToDelete.Count.ToString))

        'prüfen, ob der Job leer ist
        If CurrentState.CurrentlyPlannedUserChanges.UsersToAdd.Count = 0 And CurrentState.CurrentlyPlannedUserChanges.UsersToDelete.Count = 0 Then
            MsgBox("Es sind keine Veränderungen geplant. Der Vorgang wird abgebrochen.", MsgBoxStyle.Exclamation)
            CurrentState.JobPending = False
            CurrentState.LastErrorList = New List(Of String)
            CurrentState.LastNewUsers = New List(Of UserNameWithPassword)
            CurrentState.LastLog = New List(Of String)
            CurrentState.LastLog.Add("Es sind keine Veränderungen geplant. Der Vorgang wurde abgebrochen.")
            Me.NavigationService.Navigate(New pageStep5Results)
        End If
    End Sub

    Private Sub StartOrCancel(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnStartOrCancel.Click
        If ProcessIsRunning Then
            'Abruch
            If MsgBox("Wenn Sie den Vorgang abbrechen, haben Sie keinen Zugriff auf das Protokoll und die Liste angelegter Benutzer." & vbCrLf & "Möchten Sie den Vorgang abbrechen?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Vorgang abbrechen") = MsgBoxResult.Yes Then
                btnStartOrCancel.IsEnabled = False
                btnStartOrCancel.Content = "Der Vorgang wird abgebrochen..."
                bgwApplyChanges.CancelAsync() 'Abbruch beantragen
            End If
        Else
            'Start des Vorgangs

            'Sicherheitsabfragen
            If MsgBox("Sie sind im Begriff, AutoUsers automatisiert Änderungen an Ihrem System vornehmen zu lassen. Diese Änderungen lassen sich möglicherweise nur schwer oder gar nicht rückgängig machen." & vbCrLf & "Bitte prüfen Sie genau, ob alle Einstellungen korrekt sind und die korrekten Veränderungen ermittelt wurden, bevor Sie den Vorgang starten." & vbCrLf & "Wenn Sie den Vorgang gestartet haben, können Sie ihn zwar abbrechen, allerdings haben Sie in diesem Fall keinen Zugriff auf das Protokoll und die Liste erfolgreich angelegter Benutzer." & vbCrLf & "Wenn Sie fortfahren, handeln Sie auf eigene Gefahr." & vbCrLf & vbCrLf & "Sind Sie sich sicher, dass Sie den Vorgang starten möchten?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2, "Vorgang starten") = MsgBoxResult.No Then
                Exit Sub
            End If
            If CurrentState.CurrentlyPlannedUserChanges.UsersToDelete.Count > 0 AndAlso MsgBox("Wenn Sie fortfahren, werden Benutzer gelöscht oder deaktiviert!" & vbCrLf & "Je nachdem, welche Einstellungen und Skripte Sie verwenden, werden dabei möglicherweise auch die Daten der Benutzer unwiderruflich gelöscht." & vbCrLf & "Sind Sie sich sicher, dass Sie fortfahren möchten?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2, "Sicherheitswarnung") = MsgBoxResult.No Then
                Exit Sub
            End If
            If Application.VersionIsBeta AndAlso MsgBox("Achtung, Sie arbeiten mit einer nicht getesteten Version von AutoUsers." & vbCrLf & "Wenn Sie fortfahren, nimmt AutoUsers möglicherweise Änderungen an Ihrem System vor, die zu Datenverlust führen." & vbCrLf & "SETZEN SIE DIESEN VORGANG NICHT AUF EINEM PRODUKTIVSYSTEM FORT. DIES IST DIE LETZTE WARNUNG." & vbCrLf & "Sind Sie sich sicher, dass Sie fortfahren möchten?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2, "Beta-Version") = MsgBoxResult.No Then
                Exit Sub
            End If

            'UI deaktivieren
            gbPreviewChanges.IsEnabled = False

            'BackgroundWorker initialisieren
            bgwApplyChanges = New ComponentModel.BackgroundWorker
            With bgwApplyChanges
                .WorkerReportsProgress = True
                .WorkerSupportsCancellation = True
                AddHandler .DoWork, AddressOf DoApplyChanges
                AddHandler .RunWorkerCompleted, AddressOf ProcessFinished
                AddHandler .ProgressChanged, AddressOf ApplyProgressChangeAndNewLogEntries
            End With

            ProcessIsRunning = True
            bgwApplyChanges.RunWorkerAsync(CurrentState.CurrentlyPlannedUserChanges)
            btnStartOrCancel.Content = "Vorgang abbrechen"
            btnStartOrCancel.IsCancel = True
        End If
    End Sub

    Private Sub DoApplyChanges(ByVal sender As Object, ByVal e As ComponentModel.DoWorkEventArgs)
        'Diese Funktion wird im BackgroundWorker ausgeführt.

        Debug.WriteLine("Background worker started.")
        Dim myUserChanges As UserChanges = e.Argument 'Auftragsdaten speichern
        Dim TotalChanges As Integer = myUserChanges.UsersToAdd.Count + myUserChanges.UsersToDelete.Count 'Gesamte Anzahl der Veränderungen
        Dim CurrentChangeID As Integer = 0 'wird benutzt, um den Fortschritt anzugeben
        Dim LogBuffer As New List(Of String) 'speichert die Logeinträge, bevor Sie gesammelt übertragen werden
        Dim FullLog As New List(Of String) 'speichert alle Logeinträge
        Dim ErrorList As New List(Of String) 'speichert die Fehler, die auftreten
        Dim NewUsers As New List(Of UserNameWithPassword) 'speichert nur die Benutzer, die tatsächlich angelegt wurden

        'Wichtige Informationen loggen
        LogBuffer.Add("Protokoll vom " & Now.ToString)
        LogBuffer.Add("AutoUsers " & My.Application.Info.Version.ToString)
        If Application.VersionIsBeta Then LogBuffer.Add("Beta-Version!")
        LogBuffer.Add(myUserChanges.UsersToAdd.Count.ToString & " Benutzer werden angelegt.")
        LogBuffer.Add(myUserChanges.UsersToDelete.Count.ToString & " Benutzer werden gelöscht.")
        DoReportProgress(CurrentChangeID, TotalChanges, LogBuffer, FullLog)

        'neue Benutzer anlegen
        For Each UserName In myUserChanges.UsersToAdd
            'prüfen, ob ein Abbruch beantragt wurde
            If bgwApplyChanges.CancellationPending Then
                e.Cancel = True
                Exit For
            End If

            CurrentChangeID += 1
            LogBuffer.Add("Erzeuge Benutzer " & UserName & "...")
            Try
                'Passwort ermitteln
                Dim newPassword As String = Nothing
                If My.Settings.setDefaultPassword Then
                    newPassword = My.Settings.DefaultPassword
                ElseIf My.Settings.autoGeneratePasswords Then
                    newPassword = ToolBox.generatePassword(My.Settings.AutoPasswordLength)
                End If

                'Benutzer erzeugen
                NetAPI.CreateUser(UserName, newPassword, My.Settings.ExpireNewPasswords)

                'Benutzer in die Benutzergruppe einordnen
                If My.Settings.autoAddNewUsersToGroup Then
                    NetAPI.AddUserToGroup(UserName, CurrentState.CurrentUserGroup)
                End If

                'Informationen des neuen Benutzers speichern
                NewUsers.Add(New UserNameWithPassword(UserName, newPassword))
            Catch ex As Exception
                'Fehler beim Anlegen des Benutzers
                LogBuffer.Add("Fehler! Details im Fehlerprotokoll.")
                ErrorList.Add("Der Benutzer " & UserName & " konnte nicht angelegt werden. Fehlermeldung: " & ex.Message)
            End Try
            DoReportProgress(CurrentChangeID, TotalChanges, LogBuffer, FullLog)
        Next

        'alte Benutzer löschen
        For Each UserName In myUserChanges.UsersToDelete
            'prüfen, ob ein Abbruch beantragt wurde
            If bgwApplyChanges.CancellationPending Then
                e.Cancel = True
                Exit For
            End If

            CurrentChangeID += 1
            DoReportProgress(CurrentChangeID, TotalChanges, LogBuffer, FullLog)

            Try
                'Bnutzer löschen / deaktivieren
                If My.Settings.deleteUsers Then
                    LogBuffer.Add("Lösche Benutzer " & UserName & "...")
                    NetAPI.DeleteUser(UserName)
                ElseIf My.Settings.disableUsers Then
                    LogBuffer.Add("Deaktiviere Benutzer " & UserName & "...")
                    NetAPI.SetAccountDisabled(UserName)
                End If
            Catch ex As Exception
                'Fehler beim Löschen des Benutzers
                LogBuffer.Add("Fehler! Details im Fehlerprotokoll.")
                ErrorList.Add("Der Benutzer " & UserName & " konnte nicht gelöscht werden. Fehlermeldung: " & ex.Message)
            End Try

            'Dateien löschen
            If My.Settings.deleteUserFiles Then
                Try
                    ToolBox.deleteUserFiles(UserName)
                Catch ex As Exception
                    'Fehler beim Löschen der Dateien
                    LogBuffer.Add("Fehler! Details im Fehlerprotokoll.")
                    ErrorList.Add("Die Dateien des Benutzers " & UserName & " konnten nicht gelöscht werden. Fehlermeldung: " & ex.Message)
                End Try
            End If
        Next

        LogBuffer.Add("Der Vorgang wurde abgeschlossen.")
        DoReportProgress(CurrentChangeID, TotalChanges, LogBuffer, FullLog)

        'Ergebnisse speichern
        Dim myResult As New ApplyChangesResult
        With myResult
            .NewUsers = NewUsers
            .ErrorList = ErrorList
            .Log = FullLog
        End With
        e.Result = myResult
    End Sub

    Private Sub DoReportProgress(ByVal CurrentChangeID As Integer, ByVal TotalChanges As Integer, ByRef LogBuffer As List(Of String), ByRef FullLog As List(Of String))
        'sendet den Fortschritt und die Logeinträge

        Dim LogEntriesToSend As New List(Of String) 'eigene Variable als Kopie von LogBuffer, da es sonst Timing-Probleme gibt.
        LogEntriesToSend.AddRange(LogBuffer)
        bgwApplyChanges.ReportProgress((CurrentChangeID / TotalChanges) * 100, LogEntriesToSend) 'Fortschritt melden
        FullLog.AddRange(LogBuffer) 'speichert die Log-Einträge aus dem Buffer in die vollständige Liste
        LogBuffer.Clear()
    End Sub

    Private Sub ApplyProgressChangeAndNewLogEntries(ByVal sender As Object, ByVal e As ComponentModel.ProgressChangedEventArgs)
        'Nimmt den Fortschritt und eventuelle Logeinträge vom BackgroundWorker an

        prgOverallProgress.Value = e.ProgressPercentage 'Fortschritt setzen

        Dim NewListEntries As List(Of String) = e.UserState
        For Each ListEntry As String In NewListEntries
            'Log-Einträge in die Liste eintragen
            listLog.Items.Add(ListEntry)
        Next
        scrollLog.ScrollToBottom()
    End Sub

    Private Sub ProcessFinished(ByVal sender As Object, ByVal e As ComponentModel.RunWorkerCompletedEventArgs)
        'Fehler?
        If e.Error IsNot Nothing Then
            MsgBox("Der Vorgang führte zu einem unerwarteten Fehler. AutoUsers muss jetzt beendet werden. Möglicherweise wurde ein Teil der Änderungen übernommen." & vbCrLf & "Fehlermeldung: " & e.Error.Message, MsgBoxStyle.Critical, "Kritischer Fehler")
            End
        End If

        'Abbruch?
        If e.Cancelled Then
            MsgBox("Der Vorgang wurde abgebrochen. Ein Teil der Änderungen wurde übernommen." & vbCrLf & "AutoUsers muss jetzt beendet werden.", MsgBoxStyle.Exclamation, "Vorgang abgebrochen")
            End
        End If

        btnStartOrCancel.IsEnabled = False

        'Daten speichern
        CurrentState.JobPending = False
        Dim myResult As ApplyChangesResult = e.Result
        CurrentState.LastLog = myResult.Log
        CurrentState.LastNewUsers = myResult.NewUsers
        CurrentState.LastErrorList = myResult.ErrorList

        Me.NavigationService.Navigate(New pageStep5Results)
    End Sub
End Class
