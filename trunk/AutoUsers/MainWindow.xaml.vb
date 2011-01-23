'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class MainWindow

    Public UpdateCheckBackgroundWorker As ComponentModel.BackgroundWorker

    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        If My.Settings.lastUpdateCheck.AddDays(My.Settings.performUpdateCheckEvery) < Now Then
            'Im Hintergund nach Updates suchen
            UpdateCheckBackgroundWorker = New ComponentModel.BackgroundWorker
            AddHandler UpdateCheckBackgroundWorker.DoWork, AddressOf DoUpdateCheck
            UpdateCheckBackgroundWorker.RunWorkerAsync()
        Else
            Debug.WriteLine("Update check omitted.")
        End If
    End Sub

    Private Sub DoUpdateCheck(ByVal sender As Object, ByVal e As ComponentModel.DoWorkEventArgs)
        Debug.WriteLine("Update check running...")

        Try
            Dim CurrentVersion As Version = ToolBox.getCurrentVersionNumber

            If CurrentVersion > My.Application.Info.Version Then
                MsgBox("AutoUsers ist nicht mehr aktuell. Die aktuelle Version können Sie unter <autousers.googlecode.com> herunterladen.", MsgBoxStyle.Information)
            Else
                Debug.WriteLine("AutoUsers is up to date.")
                My.Settings.lastUpdateCheck = Now
                My.Settings.Save()
            End If
        Catch ex As Exception
            Debug.WriteLine("Update check failed.")
        End Try
    End Sub
End Class
