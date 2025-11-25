-- Create Dummy table (idempotent)
CREATE TABLE IF NOT EXISTS Dummy (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);
