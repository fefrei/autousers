'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Public Class ToolBox
    'enthält Funktionen, die im Programm eventuell an verschiedenen Stellen benötigt werden

    Public Shared Function IsUserNameValid(ByVal UserName As String) As Boolean
        'Prüft, ob ein gegebener Benutzername prinzipiell auf Windows-System benutzt werden kann.
        'Alle Fehlerkriterien entnommen von http://msdn.microsoft.com/en-us/library/aa370281%28v=VS.85%29.aspx

        'Liste der verbotenen Zeichen bauen
        Dim InvalidUserNameCharacters As String = """/\[]:;|=,+*?<>"
        For n As Byte = 1 To 31
            InvalidUserNameCharacters &= Chr(n)
        Next

        If UserName.Length > 20 Then
            Return False
        End If

        If UserName.Substring(UserName.Length - 1, 1) = "." Then
            Return False
        End If

        For n As Byte = 1 To InvalidUserNameCharacters.Length
            If UserName.Contains(InvalidUserNameCharacters.Substring(n - 1, 1)) Then
                Return False
            End If
        Next

        Return True
    End Function

    Public Shared Function generatePassword(ByVal PasswordLength As Integer)
        'Generiert selbstständig ein Kennwort aus einem vorgegenbenen Zeichenvorrat.

        Try
            Dim PasswordCharBlocks() As String = My.Settings.AutoPasswordChars.Split("|")
            Dim PasswordChars As String = My.Settings.AutoPasswordChars.Replace("|", Nothing)

            If PasswordLength < PasswordCharBlocks.GetLength(0) Then Throw New Exception("Kennwortlänge geringer als die Anzahl der Zeichengruppen.")

            Dim newPassword As String = Nothing

            'Zuerst einmal aus jedem Block ein Zeichen, damit die Kennwortrichtlinien erfüllt werden
            For n As Integer = 0 To PasswordCharBlocks.GetLength(0) - 1
                newPassword &= PasswordCharBlocks(n).Substring(Int(Rnd() * PasswordCharBlocks(n).Length), 1) 'Zufallszeichen aus diesem Block
            Next

            'Jetzt auffüllen mit Zufallszeichen
            While newPassword.Length < PasswordLength
                newPassword &= PasswordChars.Substring(Int(Rnd() * PasswordChars.Length), 1)
            End While

            'mischen
            newPassword = randomizeCharOrder(newPassword)

            Return newPassword
        Catch ex As Exception
            Throw New Exception("Ein Kennwort konnte nicht generiert werden. Möglicherweise haben Sie eine ungültige Zeichenfolge in den Einstellungen eingegeben. Fehler: " & ex.Message, ex)
        End Try
    End Function

    Public Shared Function getPassword() As String
        Dim newPassword As String = Nothing
        If My.Settings.setDefaultPassword Then
            newPassword = My.Settings.DefaultPassword
        ElseIf My.Settings.autoGeneratePasswords Then
            newPassword = ToolBox.generatePassword(My.Settings.AutoPasswordLength)
        End If
        Return newPassword
    End Function

    Public Shared Function randomizeCharOrder(ByVal InString As String) As String
        'mischt die Zeichen in einem String

        Dim OutString As String = ""

        While InString.Length > 0
            Dim CharID As Integer = Int(Rnd() * InString.Length)
            OutString &= InString.Substring(CharID, 1)
            InString = InString.Remove(CharID, 1)
        End While

        Return OutString
    End Function

    Public Shared Sub deleteUserFiles(ByVal UserName As String)
        'Führt ein Batch-Skript aus, das sich um das Löschen von Benutzerdaten kümmert
        If Not My.Computer.FileSystem.FileExists(My.Settings.deleteUserFilesBatch) Then Throw New Exception("Die in den Einstellungen angegebene Batch-Datei existiert nicht.")

        Try
            Dim myBatchStartInfo = New ProcessStartInfo("cmd.exe", "/Q /S /D /C """"" & My.Settings.deleteUserFilesBatch & """ " & UserName & """")
            Dim myBatchProcess As Process = Process.Start(myBatchStartInfo)
            myBatchProcess.WaitForExit()
        Catch ex As Exception
            Throw New Exception("Fehler beim Ausführen des Skriptes.", ex)
        End Try
    End Sub

    Public Shared Sub createUserDirs(ByVal UserName As String)
        'Führt ein Batch-Skript aus, das sich um das Anlegen von Verzeichnissen
        If Not My.Computer.FileSystem.FileExists(My.Settings.createUserDirsBatch) Then Throw New Exception("Die in den Einstellungen angegebene Batch-Datei existiert nicht.")

        Try
            Dim myBatchStartInfo = New ProcessStartInfo("cmd.exe", "/Q /S /D /C """"" & My.Settings.createUserDirsBatch & """ " & UserName & """")
            Dim myBatchProcess As Process = Process.Start(myBatchStartInfo)
            myBatchProcess.WaitForExit()
        Catch ex As Exception
            Throw New Exception("Fehler beim Ausführen des Skriptes.", ex)
        End Try
    End Sub

    Public Shared Function getHash(ByVal InputString As String) As String
        'Ermittelt einen Hash-Wert einer Datei. SHA512-basiert, aber nicht standardkonform implementiert. Nur zur programminternen Nutzung.
        Dim CryptoProvider As New Security.Cryptography.SHA512CryptoServiceProvider
        Dim BytesArray() As Byte = System.Text.Encoding.UTF8.GetBytes(InputString)
        BytesArray = CryptoProvider.ComputeHash(BytesArray)
        Dim strResult As String = ""
        For Each CurrentByte As Byte In BytesArray
            strResult &= CurrentByte.ToString("x2")
        Next
        Return strResult
    End Function

    Public Shared Sub exportStringList(ByVal myList As List(Of String))
        'Dateipfad und Dateityp abfragen
        Dim mySaveFileDialog As New Microsoft.Win32.SaveFileDialog() With {.Filter = "Textdatei|*.txt"}
        Dim UserCompletedForm As Boolean = mySaveFileDialog.ShowDialog
        If UserCompletedForm = False Then Exit Sub
        Dim FilePath As String = mySaveFileDialog.FileName

        Dim myFileWriter As New System.IO.StreamWriter(FilePath, False)
        For Each myItem In myList
            myFileWriter.WriteLine(myItem)
        Next

        myFileWriter.Close()

        MsgBox("Exportieren erfolgreich abgeschlossen.", MsgBoxStyle.Information)
    End Sub

    Public Shared Sub exportUserPasswordList(ByVal myList As List(Of UserNameWithPassword))
        'Exportiert eine Liste aus Benutzernamen und Passwörtern. Diese Funktion ist interaktiv (!) und hat eine eigene Fehlerbehandlung.
        Try
            If MsgBox("Wenn Sie fortfahren, wird eine Datei, die Benutzernamen und Kennwörter im Klartext enthält, exportiert. Sind Sie sich sicher?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Sicherheitswarnung") = MsgBoxResult.No Then
                Exit Sub
            End If

            'Dateipfad und Dateityp abfragen
            Dim mySaveFileDialog As New Microsoft.Win32.SaveFileDialog() With {.Filter = "Kommagetrennte Dateien|*.csv"}
            Dim UserCompletedForm As Boolean = mySaveFileDialog.ShowDialog
            If UserCompletedForm = False Then Exit Sub
            Dim FilePath As String = mySaveFileDialog.FileName

            Dim ExportWithHeadlines As Boolean = False
            If MsgBox("Soll die exportierte CSV-Datei Überschriften enthalten?", MsgBoxStyle.Question + vbYesNo, "CSV-Export") = MsgBoxResult.Yes Then ExportWithHeadlines = True

            Dim myFileWriter As New System.IO.StreamWriter(FilePath, False)
            If ExportWithHeadlines Then
                myFileWriter.WriteLine("Benutzername;Kennwort")
            End If
            For Each myItem In myList
                myFileWriter.WriteLine(makeStringCSVOK(myItem.UserName) & ";" & makeStringCSVOK(myItem.Password))
            Next

            myFileWriter.Close()

            MsgBox("Exportieren erfolgreich abgeschlossen.", MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox("Fehler beim Exportieren." & vbCrLf & "Fehlermeldung: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Shared Function makeStringCSVOK(ByVal myString As String) As String
        'sorgt dafür, dass ein String problemlos in eine CSV-Datei geschrieben werden kann
        Dim ProblematicCharacters As String = """;"
        Dim EscapingNeeded As Boolean = False
        For n As Byte = 1 To ProblematicCharacters.Length
            If myString.Contains(ProblematicCharacters.Substring(n - 1, 1)) Then
                EscapingNeeded = True
                Exit For
            End If
        Next

        If Not EscapingNeeded Then Return myString 'keine Probleme gefunden

        'Escaping
        myString.Replace("""", """""") 'Anführungszeichen verdoppeln

        Return """" & myString & """" 'gibt setzt Anführungszeichen am Anfang und Ende
    End Function

    Public Shared Function getCurrentVersionNumber() As Version
        'Ermittelt die Versionsnummer der aktuellen Version aus dem Web. Kann mit Exception enden. Kann lange dauern.

        Dim myWebClient As New System.Net.WebClient()
        Dim VersionString As String = myWebClient.DownloadString(My.Settings.updateURL & "version.txt")

        Dim CurrentVersion = New Version(VersionString)

        Return CurrentVersion
    End Function

    Public Shared Function getSecurePasswordLength(Optional ByVal PasswordChars As String = Nothing) As Long
        If PasswordChars Is Nothing Then
            PasswordChars = My.Settings.AutoPasswordChars
        End If

        Try
            Return Math.Ceiling(Math.Log(800000000000000, PasswordChars.Replace("|", Nothing).Length)) 'Länge des Kennworts, bei der es bei einem Brute-Force-Angriff mit 2,5 Mio Kennwörtern pro Sekunde 10 Jahre stand hält
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Shared Function RemoveAccentMarks(ByVal s As String) As String

        Dim normalizedString As String = s.Normalize(Text.NormalizationForm.FormD)

        Dim stringBuilder As New Text.StringBuilder()

        Dim c As Char

        For i = 0 To normalizedString.Length - 1

            c = normalizedString(i)

            If System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) <> System.Globalization.UnicodeCategory.NonSpacingMark Then

                stringBuilder.Append(c)

            End If

        Next

        Return stringBuilder.ToString
    End Function
End Class
