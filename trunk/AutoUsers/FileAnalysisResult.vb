'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Public Class FileAnalysisResult
    Public FilePath As String
    Public UserNames As List(Of String)
    Public ErrorsEncountered As Boolean
    Public ErrorList As List(Of String)
    Public ResultString As String 'Text-Repräsentation des Ergebnisses

    Public Sub New()
        'nimmt keine Werte entgegen, stellt nur eine leere Instanz zur Verfügung
        FilePath = Nothing
        UserNames = New List(Of String)
        ErrorsEncountered = False
        ErrorList = New List(Of String)()
        ResultString = Nothing
    End Sub
End Class
