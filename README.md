# FileSyncApp (Visual Basic 8 / WinForms)

Windows-Desktop-App zum Synchronisieren mehrerer Dateipfade:

- Mehrere **Quellpfade** verwalten (hinzufügen/entfernen).
- Für jede Quelle mehrere **Zielpfade** hinterlegen (hinzufügen/entfernen).
- Unterschiede (neu / geändert) als **TreeView** anzeigen.
- Integrierter einfacher **Diff-Editor** (2 Textansichten mit Highlighting).
- **Blockweise Synchronisation** für Textdateien:
  - Datei im TreeView auswählen
  - erkannte neue Blöcke (ähnlich Hunk-Auswahl) in Liste prüfen
  - ausgewählte Blöcke gezielt übernehmen
- Vollsynchronisierung weiterhin per Button für markierte oder alle Ziele.

## Öffnen in Visual Studio 2005

1. `FileSyncApp.sln` in Visual Studio 2005 öffnen.
2. Build-Konfiguration `Debug` oder `Release` auswählen.
3. Starten (`F5`).

> Ziel-Framework ist `.NET Framework 2.0`, passend zu Visual Basic 8.
