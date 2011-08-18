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

#Region "ProfileDir"
    Public Shared Function GetProfileDir(ByVal UserName As String) As String
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If Not TypeOf user.GetUnderlyingObject Is DirectoryServices.DirectoryEntry Then
            Throw New Exception("Fehler bei GetProfileDir: Der Objekttyp ist nicht DirectoryEntry.")
        End If

        Return CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("profilePath").Value
    End Function

    Public Shared Sub SetProfileDir(ByVal UserName As String, ByVal NewProfileDir As String)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If NewProfileDir Is Nothing Then NewProfileDir = ""

        If Not TypeOf user.GetUnderlyingObject Is DirectoryServices.DirectoryEntry Then
            Throw New Exception("Fehler bei SetProfileDir: Der Objekttyp ist nicht DirectoryEntry.")
        End If

        CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("profilePath").Value = NewProfileDir

        user.Save()
    End Sub
#End Region

#Region "LogonScript"
    Public Shared Function GetLogonScript(ByVal UserName As String) As String
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If Not TypeOf user.GetUnderlyingObject Is DirectoryServices.DirectoryEntry Then
            Throw New Exception("Fehler bei GetLogonScript: Der Objekttyp ist nicht DirectoryEntry.")
        End If

        Return CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("scriptPath").Value
    End Function

    Public Shared Sub SetLogonScript(ByVal UserName As String, ByVal NewLogonScript As String)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If NewLogonScript Is Nothing Then NewLogonScript = ""

        If Not TypeOf user.GetUnderlyingObject Is DirectoryServices.DirectoryEntry Then
            Throw New Exception("Fehler bei SetLogonScript: Der Objekttyp ist nicht DirectoryEntry.")
        End If

        CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("scriptPath").Value = NewLogonScript

        user.Save()
    End Sub
#End Region

#Region "HomeDir"
    Public Shared Function GetHomeDir(ByVal UserName As String) As String
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If Not TypeOf user.GetUnderlyingObject Is DirectoryServices.DirectoryEntry Then
            Throw New Exception("Fehler bei GetHomeDir: Der Objekttyp ist nicht DirectoryEntry.")
        End If

        Return CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("homeDrive").Value & "|" & CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("homeDirectory").Value
    End Function

    Public Shared Sub SetHomeDir(ByVal UserName As String, ByVal NewHomeDir As String)
        Dim user = UserPrincipal.FindByIdentity(CurrentState.CurrentContext, UserName)

        If Not TypeOf user.GetUnderlyingObject Is DirectoryServices.DirectoryEntry Then
            Throw New Exception("Fehler bei SetHomeDir: Der Objekttyp ist nicht DirectoryEntry.")
        End If

        If NewHomeDir.Contains("|") Then
            Dim separators() As Char = {"|"}
            Dim parameters() As String = NewHomeDir.Split(separators, 2)

            CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("homeDirectory").Value = parameters(1)
            CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("homeDrive").Value = parameters(0)
        Else
            CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("homeDirectory").Value = NewHomeDir
            CType(user.GetUnderlyingObject, DirectoryServices.DirectoryEntry).Properties("homeDrive").Value = ""
        End If

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
        ReturnValue.HomeDir = GetHomeDir(UserName)
        ReturnValue.ProfileDir = GetLogonScript(UserName)
        ReturnValue.LogonScript = GetLogonScript(UserName)

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

        SetHomeDir(UserName, newUserOptions.HomeDir)
        SetProfileDir(UserName, newUserOptions.ProfileDir)
        SetLogonScript(UserName, newUserOptions.LogonScript)

        user.Save()
    End Sub
#End Region
End Class
