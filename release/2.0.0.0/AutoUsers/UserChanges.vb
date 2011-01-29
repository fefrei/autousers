'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Public Class UserChanges
    'speichert eine Liste von Benutzern, die hinzugefügt werden sollen, und eine Liste von Benutzern, die gelöscht werden sollen

    Public UsersToAdd As List(Of String)
    Public UsersToDelete As List(Of String)

    Public Sub New()
        UsersToAdd = New List(Of String)
        UsersToDelete = New List(Of String)
    End Sub

    Public Sub New(ByVal newUsersToAdd As List(Of String), ByVal newUsersToDelete As List(Of String))
        UsersToAdd = newUsersToAdd
        UsersToDelete = newUsersToDelete
    End Sub
End Class
