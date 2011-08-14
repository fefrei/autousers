'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Public Class CurrentState
    'Speichert Informationen, die das ganze Programm betreffen.

    Public Shared FullLicenseText As String = Nothing

    Public Shared CurrentUserGroup As String = Nothing
    Public Shared CurrentUserList As List(Of String) = Nothing
    Public Shared CurrentUserListFullName As String
    Public Shared CurrentlyPlannedUserChanges As UserChanges = Nothing
    Public Shared CurrentContext As System.DirectoryServices.AccountManagement.PrincipalContext = Nothing
    Public Shared JobPending As Boolean = False 'Gibt an, ob ein Auftrag schon komplett eingetragen wurden. Wenn dieser Wert gesetzt ist, kehren Assisten auf dem kürzesten Weg zur Zusammenfassung zurück.
    Public Shared LastLog As List(Of String) = Nothing
    Public Shared LastNewUsers As List(Of UserNameWithPassword) = Nothing
    Public Shared LastErrorList As List(Of String) = Nothing
    Public Shared ProcessIsRunning As Boolean = False 'Gibt an, ob zur Zeit ein Vorgang läuft.

End Class
