'Copyright (c) 2010, 2011 Felix Freiberger
'felix@familie-freiberger.net

'This file is part of AutoUsers.

'AutoUsers is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'AutoUsers is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with AutoUsers.  If not, see <http://www.gnu.org/licenses/>.


Class pageStart

    Private Sub Page_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        txtVersion.Text = "Version " & My.Application.Info.Version.ToString
        If Application.VersionIsBeta Then
            txtVersion.Text &= " BETA"
            txtVersion.Foreground = Brushes.Red
        End If
    End Sub

    Private Sub OpenLicenseURL(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        NavigationService.Navigate(New pageLicense)
    End Sub

    Private Sub GoToSettings(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        NavigationService.Navigate(New pageSettings)
    End Sub

    Private Sub GoToStep1(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        NavigationService.Navigate(New pageStep1ChooseGroup)
    End Sub
End Class
