'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Imports System.Runtime.InteropServices

Public Class NetAPI
    'Diese Klasse enthält Funktionen, die auf die Network Management Functions zurückgreifen.

#Region "Constants"
    Private Const USER_PRIV_USER As Integer = 1
#End Region

#Region "Structures"
    Private Structure LocalGroup_Info_0
        <MarshalAs(UnmanagedType.LPWStr)> Dim GroupName As String
    End Structure
    'typedef struct _LOCALGROUP_INFO_0 {
    '  LPWSTR lgrpi0_name;
    '} LOCALGROUP_INFO_0, *PLOCALGROUP_INFO_0, *LPLOCALGROUP_INFO_0;


    Private Structure LOCALGROUP_MEMBERS_INFO_3
        <MarshalAs(UnmanagedType.LPWStr)> Dim DomainAndName As String
    End Structure
    'typedef struct _LOCALGROUP_MEMBERS_INFO_3 {
    '  LPWSTR lgrmi3_domainandname;
    '} LOCALGROUP_MEMBERS_INFO_3;



    Private Structure USER_INFO_1
        <MarshalAs(UnmanagedType.LPWStr)> Dim UserName As String
        <MarshalAs(UnmanagedType.LPWStr)> Dim Password As String
        Dim PasswordAge As Integer
        Dim Privileges As Integer
        <MarshalAs(UnmanagedType.LPWStr)> Dim HomeDirectory As String
        <MarshalAs(UnmanagedType.LPWStr)> Dim Comment As String
        Dim Flags As Integer
        <MarshalAs(UnmanagedType.LPWStr)> Dim ScriptPath As String
    End Structure
    'typedef struct _USER_INFO_1 {
    '  LPWSTR usri1_name;
    '  LPWSTR usri1_password;
    '  DWORD  usri1_password_age;
    '  DWORD  usri1_priv;
    '  LPWSTR usri1_home_dir;
    '  LPWSTR usri1_comment;
    '  DWORD  usri1_flags;
    '  LPWSTR usri1_script_path;
    '} USER_INFO_1, *PUSER_INFO_1, *LPUSER_INFO_1;


#End Region

#Region "GetLocalGroupNames"
    Private Declare Function NetLocalGroupEnum Lib "netapi32.dll" (<MarshalAs(UnmanagedType.LPWStr)> ByVal servername As String, ByVal level As Integer, ByRef bufptr As IntPtr, ByVal prefmaxlen As Integer, ByRef entriesread As Integer, ByRef totalentries As Integer, ByRef resumhandle As Integer) As Integer
    '__in     LPCWSTR servername,
    '__in     DWORD level,
    '__out    LPBYTE *bufptr,
    '__in     DWORD prefmaxlen,
    '__out    LPDWORD entriesread,
    '__out    LPDWORD totalentries,
    '__inout  PDWORD_PTR resumehandle


    Public Shared Function GetLocalGroupNames() As List(Of String)
        'Gibt eine Liste der Namen der lokalen Gruppen aus.
        Dim ResultPtr As IntPtr
        Dim EntriesRead As Integer
        Dim TotalEntries As Integer
        Dim ResumeHandle As Integer
        Dim ErrorCode As Integer = NetLocalGroupEnum(Nothing, 0, ResultPtr, &HFFFFFFFF, EntriesRead, TotalEntries, ResumeHandle)
        If Not ErrorCode = 0 Then Throw New Exception("API-Kommuniktationsfehler bei NetLocalGroupEnum: " & ErrorCode.ToString)

        Dim Result As New List(Of String)
        For n As Integer = 0 To TotalEntries - 1
            Dim ThisGroupInfo As LocalGroup_Info_0 = Marshal.PtrToStructure(CType(ResultPtr.ToInt32 + Marshal.SizeOf(GetType(LocalGroup_Info_0)) * n, IntPtr), GetType(LocalGroup_Info_0)) 'komplexes Konstrukt, da ein Array zurückgegeben wurde
            Result.Add(ThisGroupInfo.GroupName)
        Next

        Return Result
    End Function
#End Region

#Region "GetUsersInGroup"
    Private Declare Function NetLocalGroupGetMembers Lib "netapi32.dll" (<MarshalAs(UnmanagedType.LPWStr)> ByVal servername As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal localgroupname As String, ByVal level As Integer, ByRef bufptr As IntPtr, ByVal prefmaxlen As Integer, ByRef entriesread As Integer, ByRef totalentries As Integer, ByRef resumhandle As Integer) As Integer
    '__in     LPCWSTR servername,
    '__in     LPCWSTR localgroupname,
    '__in     DWORD level,
    '__out    LPBYTE *bufptr,
    '__in     DWORD prefmaxlen,
    '__out    LPDWORD entriesread,
    '__out    LPDWORD totalentries,
    '__inout  PDWORD_PTR resumehandle


    Public Shared Function GetUsersInGroup(ByVal GroupName As String) As List(Of String)
        'Gibt eine Liste der Namen der Benutzer in einer lokalen Gruppe aus.
        Dim ResultPtr As IntPtr
        Dim EntriesRead As Integer
        Dim TotalEntries As Integer
        Dim ResumeHandle As Integer
        Dim ErrorCode As Integer = NetLocalGroupGetMembers(Nothing, GroupName, 3, ResultPtr, &HFFFFFFFF, EntriesRead, TotalEntries, ResumeHandle)
        If Not ErrorCode = 0 Then Throw New Exception("API-Kommuniktationsfehler bei NetLocalGroupGetMembers: " & ErrorCode.ToString)

        Dim Result As New List(Of String)
        For n As Integer = 0 To TotalEntries - 1
            Dim ThisGroupInfo As LOCALGROUP_MEMBERS_INFO_3 = Marshal.PtrToStructure(CType(ResultPtr.ToInt32 + Marshal.SizeOf(GetType(LOCALGROUP_MEMBERS_INFO_3)) * n, IntPtr), GetType(LOCALGROUP_MEMBERS_INFO_3))
            Result.Add(ThisGroupInfo.DomainAndName.Substring(ThisGroupInfo.DomainAndName.IndexOf("\") + 1))
        Next

        Return Result
    End Function
#End Region

#Region "CreateUser"
    Private Declare Function NetUserAdd Lib "netapi32.dll" (<MarshalAs(UnmanagedType.LPWStr)> ByVal servername As String, ByVal level As Integer, ByRef buf As USER_INFO_1, ByRef parm_err As Integer) As Integer
    '__in   LMSTR servername,
    '__in   DWORD level,
    '__in   LPBYTE buf,
    '__out  LPDWORD parm_err


    Public Shared Sub CreateUser(ByVal UserName As String, ByVal Password As String, ByVal ExpirePassword As Boolean)
        'Creates a new user.

        Dim NewUserInfo As New USER_INFO_1
        With NewUserInfo
            .UserName = UserName
            .Password = Password
            .Privileges = USER_PRIV_USER
        End With

        Dim ErrorCode As Integer = NetUserAdd(Nothing, 1, NewUserInfo, Nothing)
        If Not ErrorCode = 0 Then Throw New Exception("API-Kommuniktationsfehler bei NetUserAdd: " & ErrorCode.ToString)

        Try
            If ExpirePassword Then
                Dim objUser = GetObject("WinNT://" & My.Computer.Name & "/" & UserName & ",user")
                objUser.PasswordExpired = 1
                objUser.SetInfo()
            End If
        Catch ex As Exception
            Throw New Exception("Das Kennwort des Benutzers konnte nicht als abgelaufen markiert werden. Der Benutzer kann sich wahrscheinlich anmelden, ohne das Kennwort zu ändern.", ex)
        End Try
    End Sub
#End Region

#Region "DeleteUser"
    Private Declare Function NetUserDel Lib "netapi32.dll" (<MarshalAs(UnmanagedType.LPWStr)> ByVal servername As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal username As String) As Integer
    '__in  LPCWSTR servername,
    '__in  LPCWSTR username


    Public Shared Sub DeleteUser(ByVal UserName As String)
        'Deletes a user account.

        Dim ErrorCode As Integer = NetUserDel(Nothing, UserName)
        If Not ErrorCode = 0 Then Throw New Exception("API-Kommuniktationsfehler bei NetUserDel: " & ErrorCode.ToString)
    End Sub
#End Region

#Region "AddUserToGroup"
    Private Declare Function NetLocalGroupAddMembers Lib "netapi32.dll" (<MarshalAs(UnmanagedType.LPWStr)> ByVal servername As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal groupname As String, ByVal level As Integer, ByRef buf As LOCALGROUP_MEMBERS_INFO_3, ByVal totalentries As Integer) As Integer
    '__in  LPCWSTR servername,
    '__in  LPCWSTR groupname,
    '__in  DWORD level,
    '__in  LPBYTE buf,
    '__in  DWORD totalentries

    Public Shared Sub AddUserToGroup(ByVal UserName As String, ByVal GroupName As String)
        Dim UserStruct As LOCALGROUP_MEMBERS_INFO_3
        UserStruct.DomainAndName = UserName

        Dim ErrorCode As Integer = NetLocalGroupAddMembers(Nothing, GroupName, 3, UserStruct, 1)
        If Not ErrorCode = 0 Then Throw New Exception("API-Kommuniktationsfehler bei NetLocalGroupAddMembers: " & ErrorCode.ToString)
    End Sub
#End Region

#Region "SetAccountDisabled"
    Public Shared Sub SetAccountDisabled(ByVal UserName As String, Optional ByVal IsDisabled As Boolean = True)
        Try
            Dim objUser = GetObject("WinNT://" & My.Computer.Name & "/" & UserName & ",user")
            objUser.AccountDisabled = IsDisabled
            objUser.SetInfo()
        Catch ex As Exception
            Throw New Exception("Der Benutzer kann nicht deaktiviert werden. Der Benutzer kann sich wahrscheinlich immer noch anmelden.", ex)
        End Try
    End Sub
#End Region


#Region "Unused"
    Private Declare Function NetUserGetInfo Lib "netapi32.dll" (<MarshalAs(UnmanagedType.LPWStr)> ByVal servername As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal username As String, ByVal level As Integer, ByRef bufptr As IntPtr) As Integer
    '__in   LPCWSTR servername,
    '__in   LPCWSTR username,
    '__in   DWORD level,
    '__out  LPBYTE *bufptr

    Private Shared Function GetUserInfo(ByVal UserName As String) As USER_INFO_1
        Dim ResultPtr As IntPtr
        Dim ErrorCode As Integer = NetUserGetInfo(Nothing, UserName, 1, ResultPtr)
        If Not ErrorCode = 0 Then Throw New Exception("API-Kommuniktationsfehler bei NetUserGetInfo: " & ErrorCode.ToString)
        Return Marshal.PtrToStructure(ResultPtr, GetType(USER_INFO_1))
    End Function
#End Region
End Class
