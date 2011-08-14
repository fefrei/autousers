'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageStep3CompareLists
    Public AnalyzeChangesBackgroundWorker As ComponentModel.BackgroundWorker
    Public PageIsVirgin As Boolean = True 'Gibt an, ob die Seite noch nie eine Analyse durchgeführt hat

    Class AnalyzeUserChangesArgument
        Public GroupName As String
        Public NewUserList As List(Of String)

        Sub New(ByVal newGroupName As String, ByVal newNewUserList As List(Of String))
            GroupName = newGroupName
            NewUserList = newNewUserList
        End Sub
    End Class


    Private Sub pageStep3ChooseGroup_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        If Not PageIsVirgin Then
            If MsgBox("Die Informationen auf dieser Seite sind eventuell nicht mehr aktuell. Möchten Sie sie aktualisieren?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton1 + MsgBoxStyle.Exclamation) = MsgBoxResult.Yes Then
                Me.NavigationService.Navigate(New pageStep3CompareLists)
            End If
            Exit Sub
        End If
        PageIsVirgin = False

        AnalyzeChangesBackgroundWorker = New ComponentModel.BackgroundWorker
        AddHandler AnalyzeChangesBackgroundWorker.DoWork, AddressOf DoAnalyzeChanges
        AddHandler AnalyzeChangesBackgroundWorker.RunWorkerCompleted, AddressOf ListChanges
        AnalyzeChangesBackgroundWorker.RunWorkerAsync(New AnalyzeUserChangesArgument(CurrentState.CurrentUserGroup, CurrentState.CurrentUserList))
        DirectCast(Me.Parent, NavigationWindow).IsEnabled = False
    End Sub

    Private Sub DoAnalyzeChanges(ByVal sender As Object, ByVal e As ComponentModel.DoWorkEventArgs)
        Dim Arguments As AnalyzeUserChangesArgument = e.Argument
        Dim GroupName As String = Arguments.GroupName
        Dim NewUserList As List(Of String) = Arguments.NewUserList

        Dim CurrentUsers As New List(Of String)

        Dim Result As New UserChanges

        If My.Settings.syncModeSync Then
            Debug.WriteLine("Querying current users...")
            CurrentUsers = NetAPI.GetUsersInGroup(CurrentState.CurrentUserGroup)

            If My.Settings.syncModeSyncAdd Then
                Debug.WriteLine("Searching new users...")
                Result.UsersToAdd = ListMembersOfANotInB(NewUserList, CurrentUsers)
                Debug.WriteLine(Result.UsersToAdd.Count.ToString & " new users found.")
            End If
            If My.Settings.syncModeSyncDelete Then
                Debug.WriteLine("Searching obsolete users...")
                Result.UsersToDelete = ListMembersOfANotInB(CurrentUsers, NewUserList)
                Debug.WriteLine(Result.UsersToDelete.Count.ToString & " obsolete users found.")
            End If
        End If
        If My.Settings.syncModeAddAll Then
            Result.UsersToAdd = NewUserList
        End If
        If My.Settings.syncModeDeleteAll Then
            Result.UsersToDelete = NewUserList
        End If

        Debug.WriteLine("Analysis completed.")
        e.Result = Result
    End Sub

    Private Function ListMembersOfANotInB(ByVal A As List(Of String), ByVal B As List(Of String)) As List(Of String)
        'listet alle Strings auf, die in A vorkommen, aber nicht in B sind

        Dim Result As New List(Of String)

        For Each Item As String In A
            If Not B.Contains(Item) Then
                Result.Add(Item)
            End If
        Next

        Return Result

    End Function

    Private Sub ListChanges(ByVal sender As Object, ByVal e As ComponentModel.RunWorkerCompletedEventArgs)
        Dim ComputedChanges As UserChanges = e.Result

        Debug.WriteLine("Constructing list of users to add...")
        AddCheckBoxesFromList(listUsersToCreate, ComputedChanges.UsersToAdd)
        Debug.WriteLine("Constructing list of users to delete...")
        AddCheckBoxesFromList(listUsersToDelete, ComputedChanges.UsersToDelete)

        Debug.WriteLine("Counting items...")
        ReCountItems()

        prgFileAnalyzing.Visibility = Windows.Visibility.Collapsed
        btnConfirmChanges.IsEnabled = True

        gridUserChanges.Visibility = Windows.Visibility.Visible

        Debug.WriteLine("Finished.")
        DirectCast(Me.Parent, NavigationWindow).IsEnabled = True
        btnConfirmChanges.Focus()
    End Sub

    Private Sub AddCheckBoxesFromList(ByVal targetList As ListBox, ByVal sourceList As List(Of String))
        targetList.BeginInit()
        For Each Item As String In sourceList
            'Erstellen der Objekte
            Dim myCheckBox As New CheckBox

            With myCheckBox
                .Content = Item
                .IsChecked = True
                .Margin = New Thickness(2)
            End With

            'Ereignishandler registrieren
            AddHandler myCheckBox.Checked, AddressOf ReCountItems
            AddHandler myCheckBox.Unchecked, AddressOf ReCountItems

            'Einbauen ins UI
            targetList.Items.Add(myCheckBox)

            'UI-Reaktivität erhalten
            Me.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, New Action(AddressOf DoNothing))
        Next
        targetList.EndInit()
    End Sub

    Private Function CountCheckedCheckboxesInListBox(ByVal targetList As ListBox) As Integer
        Dim Count As Integer = 0

        For Each Item As CheckBox In targetList.Items
            If Item.IsChecked Then
                Count += 1
            End If

            'UI-Reaktivität erhalten
            Me.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, New Action(AddressOf DoNothing))
        Next

        Return Count
    End Function

    Private Sub ReCountItems()
        txtUserCreation.Text = CountCheckedCheckboxesInListBox(listUsersToCreate).ToString & " neue Benutzer"
        txtUserDeletion.Text = CountCheckedCheckboxesInListBox(listUsersToDelete).ToString & " gelöschte Benutzer"
    End Sub

    Private Sub DoNothing()
    End Sub

    Private Sub btnConfirmChanges_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnConfirmChanges.Click
        Debug.WriteLine("Building final user changes list...")
        DirectCast(Me.Parent, NavigationWindow).IsEnabled = False
        gridUserChanges.Visibility = Windows.Visibility.Collapsed
        prgFileAnalyzing.Visibility = Windows.Visibility.Visible

        CurrentState.CurrentlyPlannedUserChanges = New UserChanges(ListToListBox(listUsersToCreate), ListToListBox(listUsersToDelete))
        Debug.WriteLine("Finished.")

        listUsersToCreate.Items.Clear()
        listUsersToDelete.Items.Clear()
        DirectCast(Me.Parent, NavigationWindow).IsEnabled = True
        Me.NavigationService.Navigate(New pageStep4Execute)
    End Sub

    Private Function ListToListBox(ByVal sourceList As ListBox) As List(Of String)
        Dim Result As New List(Of String)

        For Each PotentialUserChange As CheckBox In sourceList.Items
            If PotentialUserChange.IsChecked Then
                Result.Add(PotentialUserChange.Content)
            End If

            'UI-Reaktivität erhalten
            Me.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, New Action(AddressOf DoNothing))
        Next

        Return Result
    End Function

    Private Sub btnBuildList_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnBuildList.Click
        My.Settings.Save()
        Me.NavigationService.Navigate(New pageStep3CompareLists)
    End Sub
End Class
