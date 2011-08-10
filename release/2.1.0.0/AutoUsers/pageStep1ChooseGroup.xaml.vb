'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageStep1ChooseGroup

    Public QueryGroupsBackgroundWorker As ComponentModel.BackgroundWorker

    Private Sub pageStep1ChooseGroup_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        'Im Hintergund die Liste der Gruppen abfragen
        QueryGroupsBackgroundWorker = New ComponentModel.BackgroundWorker
        AddHandler QueryGroupsBackgroundWorker.DoWork, AddressOf DoQueryGroups
        AddHandler QueryGroupsBackgroundWorker.RunWorkerCompleted, AddressOf DisplayGroupList
        QueryGroupsBackgroundWorker.RunWorkerAsync()
    End Sub

    Private Sub DoQueryGroups(ByVal sender As Object, ByVal e As ComponentModel.DoWorkEventArgs)
        e.Result = NetAPI.GetGroupNames() 'von der NetAPI die Gruppenliste abfragen
    End Sub

    Private Sub DisplayGroupList(ByVal sender As Object, ByVal e As ComponentModel.RunWorkerCompletedEventArgs)
        cbGroupSelector.Items.Clear()

        For Each GroupName As String In e.Result
            cbGroupSelector.Items.Add(GroupName)
        Next

        btnConfirmGroup.IsEnabled = True
        cbGroupSelector.Focus()

        If CurrentState.CurrentUserGroup = Nothing Then
            cbGroupSelector.SelectedIndex = 0 'einfach das erste Element wählen
        Else
            'Es ist schon eine Gruppe gewählt! Falls möglich: Im Formular anzeigen.
            For Each ListEntry As String In cbGroupSelector.Items
                If ListEntry = CurrentState.CurrentUserGroup Then
                    'Eintrag gefunden!
                    cbGroupSelector.SelectedItem = ListEntry
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub btnConfirmGroup_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnConfirmGroup.Click
        CurrentState.CurrentUserGroup = cbGroupSelector.SelectedItem.ToString
        If CurrentState.JobPending Then
            Me.NavigationService.Navigate(New pageStep3CompareLists) 'Schritt 2 überspringen, da schon erledigt
        Else
            Me.NavigationService.Navigate(New pageStep2ChooseFile)
        End If
    End Sub
End Class
