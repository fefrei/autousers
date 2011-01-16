'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class Application

    ' Ereignisse auf Anwendungsebene wie Startup, Exit und DispatcherUnhandledException
    ' können in dieser Datei verarbeitet werden.

    Public Const VersionIsBeta As Boolean = True

    Private Sub Application_Startup(ByVal sender As System.Object, ByVal e As System.Windows.StartupEventArgs) Handles MyBase.Startup
        Randomize()
        Debug.WriteLine("AutoUsers " & My.Application.Info.Version.ToString & " starting...")

        'Prüfung, ob eine Beta-Version ausgeführt wird
        If VersionIsBeta Then
            If e.Args.Length > 0 AndAlso e.Args(0) = "/NoBetaWarning" Then
                Debug.WriteLine("BETA Version starting without interactive warning.")
            Else
                'Benutzer warnen, dass er eine Betaversion ausführt
                If MsgBox("Diese Version von AutoUsers ist nicht vollständig getestet. Wenn Sie fortfahren, kann es zu unerwarteten Resultaten und Datenverlust kommen." & vbCrLf & "Möchten Sie fortfahren?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2, "Betaversion von AutoUsers") = MsgBoxResult.No Then
                    End
                End If
            End If
        End If

        'Prüfung der Betriebssystemversion
        If Environment.OSVersion.Version.CompareTo(New Version(5, 1)) < 0 Then
            MsgBox("Sie führen eine Version von Windows aus, die nicht unterstützt wird." & vbCrLf & "Sie benötigen mindestens Windows NT 5.1.", MsgBoxStyle.Critical, "Betriebssystem nicht unterstützt")
            End
        End If

        'Prüfung auf Administratorrechte
        My.User.InitializeWithWindowsUser()
        If Not My.User.IsInRole(ApplicationServices.BuiltInRole.Administrator) Then
            MsgBox("Sie verfügen zur Zeit nicht über Administratorrechte." & vbCrLf & "Starten Sie AutoUsers mit Administratorrechten, um fortzufahren.", MsgBoxStyle.Exclamation, "Fehlende Rechte")
            End
        End If

        'Einstellungen aus vorherigen Versionen laden
        If My.Settings.upgradeSettings Then 'wenn mit Standardeinstellungen gearbeitet wird
            My.Settings.Upgrade()
            My.Settings.upgradeSettings = False 'speichert, dass die Einstellungen nun aktuell sind
            My.Settings.Save()
            MsgBox("Willkommen bei AutoUsers " & My.Application.Info.Version.ToString & "!" & vbCrLf & "Einstellungen aus vorherigen Versionen wurden übernommen.", MsgBoxStyle.Information)
        End If

        'Copyright-Hinweis
        If Not My.Settings.CopyrightNoticeShown Then 'nur beim ersten Start
            MsgBox("AutoUsers ist freie Software. Bitte lesen Sie sich dazu folgende Lizenzinformationen durch:" & vbCrLf & vbCrLf & "Copyright (c) 2010, 2011 Felix Freiberger" & vbCrLf & "felix@familie-freiberger.net" & vbCrLf & vbCrLf & "AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version." & vbCrLf & "AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details." & vbCrLf & "You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.", MsgBoxStyle.Information, "Lizenzinformationen")
            My.Settings.CopyrightNoticeShown = True
            My.Settings.Save()
        End If

        'Lizenztext laden und prüfen
        Try
            Dim licenseFileStreamReader As New System.IO.StreamReader("COPYING.txt")
            CurrentState.FullLicenseText = licenseFileStreamReader.ReadToEnd
            licenseFileStreamReader.Close()

            If Not ToolBox.getHash(CurrentState.FullLicenseText) = "4937848b94f5b50ea16c51f9e98fdcd3953aca63d63ca3bb05d8a62c107e382b71c496838d130ae504a52032398630b957acaea6c48032081a6366d27cba5ea9" Then
                Throw New Exception("COPYING.txt hat den Integritätstest nicht bestanden!")
            End If
        Catch ex As Exception
            CurrentState.FullLicenseText = "Der Lizenztext konnte nicht aus der COPYING.txt geladen werden." & vbCrLf & "Bitte rufen Sie ihn unter http://www.gnu.org/licenses/gpl.txt ab."
            MsgBox("Die Datei mit dem Lizenztext (COPYING.txt) fehlt, ist beschädigt / verändert worden oder kann nicht gelesen werden." & vbCrLf & "Die Lizenz kann unter http://www.gnu.org/licenses/gpl.txt abgerufen werden.", MsgBoxStyle.Exclamation, "Lizenzinformationen")
        End Try
    End Sub
End Class
