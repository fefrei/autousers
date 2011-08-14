'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Imports System.Runtime.InteropServices
Imports System.DirectoryServices.AccountManagement

Public Class NetAPI
    'Diese Klasse enthält Funktionen, die die folgende API benutzen: http://msdn.microsoft.com/en-us/magazine/cc135979.aspx


#Region "GetGroupNames"
    Public Shared Function GetGroupNames() As List(Of String)
        'Gibt die vorhandenen Gruppen aus.

        Dim GroupNames = New List(Of String)

        Dim group = New GroupPrincipal(CurrentState.CurrentContext)

        Dim pS = New PrincipalSearcher(group)
        Dim results = pS.FindAll

        For Each item In results
            GroupNames.Add(item.ToString)
        Next

        Return GroupNames
    End Function
#End Region

#Region "GetUsersInGroup"

    Public Shared Function GetUsersInGroup(ByVal GroupName As String) As List(Of String)
        'Gibt eine Liste der Namen der Benutzer in einer Gruppe aus.
        Dim group = GroupPrincipal.FindByIdentity(CurrentState.CurrentContext, GroupName)
        Dim members = group.GetMembers

        Dim Result = New List(Of String)

        For Each item In members
            If TypeOf item Is UserPrincipal Then
                Result.Add(item.ToString)
            End If
        Next

        Return Result
    End Function
#End Region

#Region "CreateUser"
    Public Shared Sub CreateUser(ByVal UserName As String, ByVal Password As String, ByVal ExpirePassword As Boolean)
        'Creates a new user.

        Dim user As New UserPrincipal(CurrentState.CurrentContext, UserName, Password, True)

        If (ExpirePassword) Then
            user.ExpirePasswordNow()
        End If

        user.Save()
    End Sub
#End Region

#Region "DeleteUser"
    Public Shared Sub DeleteUser(ByVal UserName As String)
        'Deletes a user account.

        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)
        user.Delete()
    End Sub
#End Region

#Region "AddUserToGroup"
    Public Shared Sub AddUserToGroup(ByVal UserName As String, ByVal GroupName As String)
        Dim group = GroupPrincipal.FindByIdentity(CurrentState.CurrentContext, GroupName)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        group.Members.Add(user)
        group.Save()
    End Sub
#End Region

#Region "SetAccountDisabled"
    Public Shared Sub SetAccountDisabled(ByVal UserName As String, Optional ByVal IsDisabled As Boolean = True)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        user.Enabled = Not IsDisabled

        user.Save()
    End Sub
#End Region

#Region "SetHomeDir"
    Public Shared Sub SetHomeDir(ByVal UserName As String, ByVal NewHomeDir As String)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        user.HomeDirectory = NewHomeDir

        user.Save()
    End Sub
#End Region

#Region "DoesUserExist"
    Public Shared Function DoesUserExist(ByVal UserName As String) As Integer
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        Return user IsNot Nothing
    End Function
#End Region

#Region "UserOptions"
    Public Shared Function GetUserOptions(ByVal UserName As String) As UserOptions
        Dim ReturnValue = New UserOptions()

        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        ReturnValue.UserIsDisabled = Not user.Enabled
        ReturnValue.PasswordExpired = Nothing 'Reading not supported

        Return ReturnValue
    End Function

    Public Shared Sub SetUserOptions(ByVal UserName As String, ByVal newUserOptions As UserOptions)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If newUserOptions.Password IsNot Nothing Then
            user.SetPassword(newUserOptions.Password)
        End If

        If newUserOptions.PasswordExpired IsNot Nothing Then
            If newUserOptions.PasswordExpired Then
                user.ExpirePasswordNow()
            Else
                user.RefreshExpiredPassword()
            End If
        End If

        user.Enabled = Not newUserOptions.UserIsDisabled

        user.Save()
    End Sub
#End Region
End Class
