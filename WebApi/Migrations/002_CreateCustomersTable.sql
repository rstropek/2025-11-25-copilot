-- Create Customers table if it doesn't exist
CREATE TABLE IF NOT EXISTS Customers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    Country TEXT NOT NULL,
    Revenue REAL NOT NULL
);

-- Insert seed data only if table is empty
INSERT INTO Customers (Name, Email, Country, Revenue)
SELECT 'Acme Corporation', 'contact@acme.com', 'USA', 125000.50
WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'contact@acme.com');

INSERT INTO Customers (Name, Email, Country, Revenue)
SELECT 'Global Tech Ltd', 'info@globaltech.co.uk', 'UK', 89500.75
WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'info@globaltech.co.uk');

INSERT INTO Customers (Name, Email, Country, Revenue)
SELECT 'Deutsche Industries', 'sales@deutsche.de', 'Germany', 245000.00
WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'sales@deutsche.de');

INSERT INTO Customers (Name, Email, Country, Revenue)
SELECT 'French Solutions', 'contact@frenchsol.fr', 'France', 67800.25
WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'contact@frenchsol.fr');

INSERT INTO Customers (Name, Email, Country, Revenue)
SELECT 'Tokyo Enterprises', 'hello@tokyo-ent.jp', 'Japan', 310000.00
WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'hello@tokyo-ent.jp');
