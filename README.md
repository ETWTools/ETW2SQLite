# ETW2SQLite
======================

ETW2SQLite is a tool that converts ETW Log Files (.ETL) to a SQLite database. A table is created per unique event type and each events is added as rows of this table. For each "flat" property in the event, a column is created matching the closest SQLite type, and for "structs" and "arrays" the information is encoded as JSON (using the Newtonsoft.Json library) and stored with the remaining properties.

## Command-line usage

``ETW2SQLite C:\MyFile.etl C:\MyFile.Kernel.etl --output=C:\MyFile.sqlite``

## Download

``ETW2SQLite.zip`` can be downloaded from https://github.com/ETWTools/ETW2SQLite/releases and consists of ETW2SQLite.exe and sqlite3.dll from the SQLite project.

**NOTE**: Currently only built as a 32-bit executable (which is faster than 64-bit anyway, and usable on 32-bit and 64-bit Windows).

## Why SQLite?

Converting ETW Log Files (.ETL) to a SQLite database helps is event information analysis using a familiar declarative query language, SQL.

This allows the user to do complex queries on their ETW logs just as if it were coming from any other datasource.

The choice of SQLite was made due to the ease-of-use and single file deployment nature of SQLite databases.

## SQLite Event Viewer

SQLite has a great unofficial database viewer: [DB Browser for SQLite](http://sqlitebrowser.org), here is a screenshot of a real ETW log file converted to SQLite:

![screenshot][screenshot]

And here is the DB schema view (visually describing how ETW2SQLite has converted the event schemas to SQLite schemas)

![screenshot2][screenshot2]

[screenshot]: https://raw.githubusercontent.com/ETWTools/ETW2SQLite/master/browser1.png
[screenshot2]: https://raw.githubusercontent.com/ETWTools/ETW2SQLite/master/browser2.png

## Does it understand Kernel, .NET EventSource, XPERF, etc. events?

ETW2SQLite is built upon [ETWDeserializer](http://github.com/ETWTools/ETWDeserializer), a library that understands Windows MOF Classes events, Windows Vista Manifest events and EventSource .NET events. It also understands events that XPERF (WPR) adds as part of its merging process (to give PDB information) for profiler tools like the Windows Performance Recorder.