// Aura
// Documentation
// --------------------------------------------------------------------------

Aura uses 2 folders to organize databases, configurations, scripts, etc:
"system" and "user". The developers only make changes in system,
leaving the user folder to you.

When loading data, the user folder is treated with a higher priority,
making it possible for you to extend and modify things without touching
the system files. This ensures that there will never be any conflicts
when updating or something.

While it's technically possible for you to just make your changes in the
system folder, it's strongly suggested that you only use user
as it makes updating, backups, and similar operations much simpler.

For more information check the readme in the user folder.
