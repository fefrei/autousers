Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Globalization
Imports System.Resources
Imports System.Windows

' Allgemeine Informationen über eine Assembly werden über die folgenden 
' Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
' die mit einer Assembly verknüpft sind.

' Die Werte der Assemblyattribute überprüfen

<Assembly: AssemblyTitle("AutoUsers")> 
<Assembly: AssemblyDescription("Programm zum automatischen Verwalten von Windows-Benutzerkonten")> 
<Assembly: AssemblyCompany("Felix Freiberger")> 
<Assembly: AssemblyProduct("AutoUsers")> 
<Assembly: AssemblyCopyright("Copyright (c) 2010, 2011 Felix Freiberger")> 
<Assembly: AssemblyTrademark("")> 
<Assembly: ComVisible(false)>

'Um mit dem Erstellen lokalisierbarer Anwendungen zu beginnen, legen Sie 
'<UICulture>ImCodeVerwendeteKultur</UICulture> in der VBPROJ-Datei
'in einer <PropertyGroup> fest. Wenn Sie in den Quelldateien beispielsweise Deutsch 
'(Deutschland) verwenden, legen Sie <UICulture> auf "de-DE" fest. Heben Sie dann die Auskommentierung
'des nachstehenden NeutralResourceLanguage-Attributs auf. Aktualisieren Sie "en-US" in der nachstehenden Zeile,
'sodass es mit der UICulture-Einstellung in der Projektdatei übereinstimmt.

'<Assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)> 


'Das ThemeInfo-Attribut beschreibt, wo Sie designspezifische und generische Ressourcenwörterbücher finden.
'1. Parameter: Speicherort der designspezifischen Ressourcenwörterbücher
'(wird verwendet, wenn eine Ressource auf der Seite 
' oder in den Anwendungsressourcen-Wörterbüchern nicht gefunden werden kann.)

'2. Parameter: Speicherort des generischen Ressourcenwörterbuchs
'(wird verwendet, wenn eine Ressource auf der Seite, 
'in der Anwendung sowie in den designspezifischen Ressourcenwörterbüchern nicht gefunden werden kann)
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>



'Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
<Assembly: Guid("015b5db8-b1df-444c-8b9c-f1df3455c7b5")> 

' Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
'
'      Hauptversion
'      Nebenversion 
'      Buildnummer
'      Revision
'
' Sie können alle Werte angeben oder die standardmäßigen Build- und Revisionsnummern 
' übernehmen, indem Sie "*" eingeben:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("2.2.0.0")> 
<Assembly: AssemblyFileVersion("2.2.0.0")> 
