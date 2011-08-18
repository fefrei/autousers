'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageStep2ChooseFile

    Public AnalyzeFileBackgroundWorker As ComponentModel.BackgroundWorker
    Public CurrentUserList As List(Of String) = Nothing
    Public CurrentUserListFullName As String = Nothing

    Class FileAnalysisResult
        Public FilePath As String
        Public usedDriver As String
        Public UserNames As List(Of String)
        Public ErrorsEncountered As Boolean
        Public ErrorList As List(Of String)
        Public ResultString As String 'Text-Repräsentation des Ergebnisses

        Sub New()
            UserNames = New List(Of String)
            ErrorList = New List(Of String)
        End Sub
    End Class

    Structure FileAnalysisArgument
        Public FilePath As String
        Public selectedDriver As Byte

        Sub New(ByVal newFilePathas As String, ByVal newSelectedDriver As Byte)
            FilePath = newFilePathas
            selectedDriver = newSelectedDriver
        End Sub
    End Structure

    Private Sub btnOpenFile_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnOpenFile.Click
        Dim myOpenFileDialog As New Microsoft.Win32.OpenFileDialog() With {.Filter = "Kommagetrennte Dateien|*.csv|Textdateien|*.txt|Alle Dateien|*"}
        Dim UserCompletedForm As Boolean = myOpenFileDialog.ShowDialog
        If UserCompletedForm = False Then Exit Sub
        Dim FilePath As String = myOpenFileDialog.FileName
        Debug.WriteLine("Analyzing " & FilePath & "...")

        DirectCast(Me.Parent, NavigationWindow).IsEnabled = False
        txtResults.Visibility = Windows.Visibility.Collapsed
        prgFileAnalyzing.Visibility = Windows.Visibility.Visible

        AnalyzeFileBackgroundWorker = New ComponentModel.BackgroundWorker
        With AnalyzeFileBackgroundWorker
            AddHandler .DoWork, AddressOf DoAnalyzeFile
            AddHandler .RunWorkerCompleted, AddressOf ProcessAnalysisResults
        End With

        AnalyzeFileBackgroundWorker.RunWorkerAsync(New FileAnalysisArgument(FilePath, comboFileDriver.SelectedIndex))
    End Sub

    Private Sub DoAnalyzeFile(ByVal sender As Object, ByVal e As ComponentModel.DoWorkEventArgs)
        Dim SeparatingCharacters As String

        Dim ReturnValue As New FileAnalysisResult

        Debug.WriteLine("Reading file...")
        ReturnValue.FilePath = CType(e.Argument, FileAnalysisArgument).FilePath
        ReturnValue.usedDriver = CType(e.Argument, FileAnalysisArgument).selectedDriver
        Dim FileStream As New IO.StreamReader(ReturnValue.FilePath, System.Text.Encoding.Default)
        Dim FileContent() As Char = FileStream.ReadToEnd.ToCharArray
        FileStream.Close()

        Select Case ReturnValue.usedDriver
            Case 0
                SeparatingCharacters = ";," & vbCr & vbLf
            Case 1
                SeparatingCharacters = vbCr & vbLf 'nicht bei Zeilenwechseln splitten
            Case Else
                Throw New NotImplementedException
        End Select

        Dim Buffer As String = "" 'Hier kommen die Zeichen einzeln rein, bis der Name komplett ist

        Debug.WriteLine("Parsing file content...")
        For n As Integer = 0 To FileContent.GetLength(0) - 1
            If SeparatingCharacters.Contains(FileContent(n)) Then
                'Trennzeichen gefunden
                If Buffer.Length > 0 Then 'Nur wen im Buffer wirklich schon Inhalt ist
                    ReturnValue.UserNames.Add(Buffer)
                    Buffer = ""
                End If
            Else
                'Inhaltszeichen gefunden
                Buffer &= FileContent(n) 'Im Buffer speichern
            End If
        Next

        If Buffer.Length > 0 Then
            'nach dem letzen Namen kam kein Trennzeichen --> muss manuell noch eingetragen werden
            ReturnValue.UserNames.Add(Buffer)
            Debug.WriteLine("NOTE: No separating character after the last name found. It was manually separated. Last Name: " & Buffer)
        End If

        For n As Integer = ReturnValue.UserNames.Count - 1 To 0 Step -1
            Dim UserName = ReturnValue.UserNames.Item(n)

            If ReturnValue.usedDriver = 1 Then
                UserName = UserName.Replace("Ä", "AE").Replace("Ö", "OE").Replace("Ü", "ÜE")
                UserName = UserName.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
                UserName = UserName.Replace("ß", "ss")
                UserName = ToolBox.RemoveAccentMarks(UserName)

                'verbotene Zeichen am Anfang entfernen
                Do Until UserName.Length = 0 OrElse Char.IsLetterOrDigit(UserName.ToCharArray()(0))
                    UserName.Remove(0, 1)
                Loop

                Dim FirstSeparationIndex As Integer = 0 'Index, der speichert, wo das erste Trennzeichen entfernt wurde (um den Beginn des Nachnamens zu ermitteln)

                'verbotene Zeichen komplett entfernen
                For m As Integer = UserName.Length - 1 To 0 Step -1
                    If Not Char.IsLetterOrDigit(UserName.ToCharArray()(m)) Then
                        UserName = UserName.Remove(m, 1)
                    End If
                Next

                'Kürzen des Namens
                If UserName.Length > 20 Then
                    UserName = UserName.Substring(0, 20)
                End If
            End If

            If Not ToolBox.IsUserNameValid(UserName) Then
                'Fehler speichern
                ReturnValue.ErrorsEncountered = True
                Debug.WriteLine("ERROR: " & UserName & " is invalid.")
                ReturnValue.ErrorList.Add("WARNUNG: Der Benutzername " & UserName & " ist ungültig und wurde ignoriert.")
                ReturnValue.UserNames.RemoveAt(n) 'Eintrag löschem
                Continue For 'Keine weitere Verarbeitung
            End If

            ReturnValue.UserNames.Item(n) = UserName
        Next

        'Ergebnis als String aufbereiten
        'Dieser Vorgang ist fast ausschließlich für die Bearbeitungsdauer ausschlaggebend
        Debug.WriteLine("Building result string...")
        ReturnValue.ResultString = ReturnValue.UserNames.Count & " Benutzer wurden gefunden." & vbCrLf

        For Each UserName As String In ReturnValue.UserNames
            ReturnValue.ResultString &= UserName & ", "
        Next

        Debug.WriteLine("Finished.")
        e.Result = ReturnValue
    End Sub

    Private Sub ProcessAnalysisResults(ByVal sender As Object, ByVal e As ComponentModel.RunWorkerCompletedEventArgs)
        If e.Error IsNot Nothing Then
            MsgBox("Die Datei konnte nicht analysiert werden." & vbCrLf & "Fehlermeldung: " & e.Error.Message, MsgBoxStyle.Critical, "Dateianalyse fehlgeschlagen")
        Else
            Dim Result As FileAnalysisResult = e.Result

            If Result.ErrorsEncountered Then
                If MsgBox("Die Datei wurde analysiert, dabei sind jedoch " & Result.ErrorList.Count.ToString & " Fehler aufgetreten. Möchten Sie eine Liste der aufgetretenen Fehler sehen?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2 + MsgBoxStyle.Exclamation, "Analyse mit Fehlern abgeschlossen") = MsgBoxResult.Yes Then
                    'Fehlerliste aufbereiten
                    Dim ErrorList As String = Nothing
                    For Each ErrorDescription As String In Result.ErrorList
                        ErrorList &= ErrorDescription & vbCrLf
                    Next
                    MsgBox("Folgende Fehler sind aufgetreten:" & vbCrLf & ErrorList & "Insgesamt sind " & Result.ErrorList.Count.ToString & " Fehler aufgetreten.", MsgBoxStyle.Information, "Fehlerliste")
                End If
            End If

            CurrentUserList = Result.UserNames
            CurrentUserListFullName = Result.FilePath

            txtResults.Text = Result.ResultString

            btnConfirmList.IsEnabled = True
        End If


        DirectCast(Me.Parent, NavigationWindow).IsEnabled = True
        txtResults.Visibility = Windows.Visibility.Visible
        prgFileAnalyzing.Visibility = Windows.Visibility.Collapsed
        btnConfirmList.Focus()
    End Sub

    Private Sub btnConfirmList_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnConfirmList.Click
        If CurrentUserList IsNot Nothing Then 'Abfrage nötig, denn die Liste könnte schon von einer anderen Instanz gespeicher worden sein (und soll ja nicht mit Nothing überschrieben werden)
            CurrentState.CurrentUserList = CurrentUserList
            CurrentState.CurrentUserListFullName = CurrentUserListFullName
        End If
        Me.NavigationService.Navigate(New pageStep3CompareLists)
    End Sub

    Private Sub Page_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        If CurrentState.CurrentUserList IsNot Nothing Then
            txtResults.Text = "Es wurde bereits eine Benutzerliste geladen. Klicken Sie auf ""Datei öffnen"", wenn Sie eine andere Liste laden möchten."
            btnConfirmList.IsEnabled = True
        End If
    End Sub
End Class
