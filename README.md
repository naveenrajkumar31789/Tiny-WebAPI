[ { "Action": "ShowModulesWindow", "Message": "Open Visual Studio -> Debug > Windows > Modules while your process is running. Locate the module for your project (look for a name like TinyUrlApi). Is the module listed? If it is listed, check the "Symbol Status" column. If symbols are not loaded, right-click that module and choose "Symbol Load Information..." and copy the status text here. If the module is not listed, confirm which process you are debugging and that the debugger is configured for Managed (CoreCLR) for .NET 8, then report back what you see." } ]

•	POST /api/add — create a new short URL (returns 201 Created with short code)
•	DELETE /api/delete/{code} — delete mapping by short code
•	DELETE /api/delete-all — delete all mappings
•	PUT /api/update/{code} — update original URL for a given short code
•	GET /api/public — list all mappings (public)
•	GET /{code} — redirect to the original URL (increments hit count)
