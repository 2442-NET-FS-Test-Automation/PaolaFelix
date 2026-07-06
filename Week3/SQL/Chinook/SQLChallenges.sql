-- Parking Lot*******
-- *                *
-- *                *
--- *****************



-- Comment can be done single line with --
-- Comment can be done multi line with /* */

/*
DQL - Data Query Language
Keywords:

SELECT - retrieve data, select the columns from the resulting set
FROM - the table(s) to retrieve data from
WHERE - a conditional filter of the data
GROUP BY - group the data based on one or more columns
HAVING - a conditional filter of the grouped data
ORDER BY - sort the data
*/
USE [Chinook_AutoIncrement];

-- BASIC CHALLENGES
-- List all customers (full name, customer id, and country) who are not in the USA
SELECT FirstName + ' ' + LastName AS FullName, CustomerId, Country
FROM dbo.Customer
WHERE Country != 'USA';

-- List all customers from Brazil
SELECT *
FROM dbo.Customer
WHERE Country = 'Brazil';

-- List all sales agents
SELECT *
FROM dbo.Employee
WHERE Title LIKE '%Sales%';

-- SELECT * FROM employee WHERE title LIKE '%Agent%;
SELECT *
FROM dbo.Employee
WHERE Title LIKE '%Agent%';

-- Retrieve a list of all countries in billing addresses on invoices
SELECT DISTINCT BillingCountry
FROM dbo.Invoice;

-- Retrieve how many invoices there were in 2021, and what was the sales total for that year?
SELECT COUNT(InvoiceId) AS Invoices2021, SUM(Total) AS Total2021
FROM dbo.Invoice
WHERE InvoiceDate >= '2021-01-01' AND InvoiceDate <= '2021-12-31';

-- (challenge: find the invoice count sales total for every year using one query)
SELECT SUM(Total) AS SalesTotal, YEAR(InvoiceDate) AS [YEAR]
FROM dbo.Invoice
GROUP BY YEAR(InvoiceDate);

-- how many line items were there for invoice #37
SELECT COUNT(InvoiceLineId) AS LineItems, InvoiceId
FROM dbo.InvoiceLine
WHERE InvoiceId = 37
GROUP BY InvoiceId;

-- how many invoices per country? BillingCountry  # of invoices 
SELECT COUNT(InvoiceId) AS Invoices, BillingCountry AS Country
FROM dbo.Invoice
GROUP BY BillingCountry;

-- Retrieve the total sales per country, ordered by the highest total sales first.
SELECT SUM(Total) AS TotalSales, BillingCountry AS Country
FROM dbo.Invoice
GROUP BY BillingCountry
ORDER BY TotalSales DESC;

-- JOINS CHALLENGES
-- Every Album by Artist
-- (inner keyword is optional for inner join)
SELECT a.Title AS Album, b.Name AS Artist 
FROM dbo.Album AS a
JOIN dbo.Artist b ON a.ArtistId = b.ArtistId;

-- All songs of the rock genre
SELECT t.Name AS Song
FROM dbo.Track AS t
JOIN dbo.Genre g ON t.GenreId = g.GenreId;

-- Show all invoices of customers from brazil (mailing address not billing)
SELECT i.InvoiceId, c.country
FROM dbo.Invoice AS i
JOIN dbo.Customer c ON i.CustomerId = c.CustomerId;

-- Show all invoices together with the name of the sales agent for each one
SELECT i.InvoiceId, e.FirstName + ' ' + e.LastName AS SalesAgent
FROM dbo.Invoice AS i
JOIN dbo.Customer AS c ON i.CustomerId = c.CustomerId
JOIN dbo.Employee AS e ON c.SupportRepId = e.EmployeeId; 

-- Which sales agent made the most sales in 2024?
SELECT SUM(i.Total) AS TotalSales2024, e.FirstName + ' ' + e.LastName AS SalesAgent
FROM dbo.Invoice AS i
JOIN dbo.Customer AS c ON i.CustomerId = c.CustomerId
JOIN dbo.Employee AS e ON c.SupportRepId = e.EmployeeId
WHERE YEAR(i.InvoiceDate) = 2024
GROUP BY e.FirstName, e.LastName
ORDER BY TotalSales2024 DESC; 

-- How many customers are assigned to each sales agent?
SELECT COUNT(c.CustomerId) AS Customer, e.FirstName + ' ' + e.LastName AS SalesAgent
FROM dbo.Customer AS c
JOIN dbo.Employee AS e ON c.SupportRepId = e.EmployeeId
GROUP BY e.FirstName, e.LastName; 

-- Which track was purchased the most in 2021?
SELECT SUM(il.Quantity) AS Sales, t.Name AS Track
FROM dbo.InvoiceLine AS il
JOIN dbo.Track AS t ON il.TrackId = t.TrackId
JOIN dbo.Invoice AS i ON il.InvoiceId = i.InvoiceId
WHERE YEAR(i.InvoiceDate) = 2021
GROUP BY t.Name
ORDER BY Sales DESC;

-- Show the top three best selling artists.
SELECT TOP 3 a.Name, SUM(il.Quantity) AS Sales
FROM dbo.Artist AS a
JOIN dbo.Album AS al ON a.ArtistId = al.ArtistId
JOIN dbo.Track AS t ON al.AlbumId = t.AlbumId
JOIN dbo.InvoiceLine AS il ON t.TrackId = il.TrackId
GROUP BY a.Name
ORDER BY Sales DESC;

-- Which customers have the same initials as at least one other customer?
SELECT FirstName,
       LastName
FROM dbo.Customer
WHERE SUBSTRING(FirstName,1,1) + SUBSTRING(LastName,1,1) IN
(
    SELECT SUBSTRING(FirstName,1,1) + SUBSTRING(LastName,1,1)
    FROM dbo.Customer
    GROUP BY SUBSTRING(FirstName,1,1) + SUBSTRING(LastName,1,1)
    HAVING COUNT(*) > 1
);

-- Which countries have the most invoices?
SELECT SUM(i.InvoiceId) AS Invoices, c.Country
FROM dbo.Invoice AS i
JOIN dbo.Customer AS c ON i.CustomerId = c.CustomerId
GROUP BY c.Country
ORDER BY Invoices DESC;

-- Which city has the customer with the highest sales total?
SELECT TOP 1 SUM(i.Total) AS Sales, c.City
FROM dbo.Invoice AS i
JOIN dbo.Customer AS c ON i.CustomerId = c.CustomerId
GROUP BY c.City, c.CustomerId
ORDER BY Sales DESC;

-- Who is the highest spending customer?
SELECT TOP 1 SUM(i.Total) AS Sales, c.FirstName + ' ' + c.LastName AS Name, c.City
FROM dbo.Invoice AS i
JOIN dbo.Customer AS c ON i.CustomerId = c.CustomerId
GROUP BY c.City, c.FirstName, c.LastName
ORDER BY Sales DESC;

-- Return the email and full name of of all customers who listen to Rock.
SELECT c.Email, c.FirstName + ' ' + c.LastName AS Name
FROM dbo.Customer AS c
JOIN dbo.Invoice AS i ON i.CustomerId = c.CustomerId
JOIN dbo.InvoiceLine AS il ON il.InvoiceId = i.InvoiceId
JOIN dbo.Track AS t ON t.TrackId = il.TrackId
JOIN dbo.Genre AS g ON g.GenreId = T.GenreId
WHERE g.Name = 'Rock'
GROUP BY c.Email, c.FirstName + ' ' + c.LastName;

-- Which artist has written the most Rock songs?
SELECT a.Name, COUNT(t.TrackId) AS Songs
FROM dbo.Artist AS a
JOIN dbo.Album AS al ON a.ArtistId = al.ArtistId
JOIN dbo.Track AS t ON al.AlbumId = t.AlbumId
JOIN dbo.Genre AS g ON t.GenreId = g.GenreId
WHERE g.Name = 'Rock'
GROUP BY a.Name
ORDER BY Songs DESC;

-- Which artist has generated the most revenue?
SELECT a.Name, SUM(i.Total) Revenue
FROM dbo.Artist AS a
JOIN dbo.Album AS al ON a.ArtistId = al.ArtistId
JOIN dbo.Track AS t ON al.AlbumId = t.AlbumId
JOIN dbo.InvoiceLine AS il ON t.TrackId = il.TrackId
JOIN dbo.Invoice AS i ON il.InvoiceId = i.InvoiceId
GROUP BY a.Name
ORDER BY Revenue DESC;

-- ADVANCED CHALLENGES
-- solve these with a mixture of joins, subqueries, CTE, and set operators.
-- solve at least one of them in two different ways, and see if the execution
-- plan for them is the same, or different.

-- 1. which artists did not make any albums at all?
SELECT a.Name
FROM dbo.Artist AS a
LEFT JOIN dbo.Album AS al ON a.ArtistId = al.ArtistId
WHERE al.AlbumId IS NULL;

-- 2. which artists did not record any tracks of the Latin genre?
SELECT DISTINCT a.Name
FROM dbo.Artist a
LEFT JOIN dbo.Album al ON a.ArtistId = al.ArtistId
LEFT JOIN dbo.Track t ON al.AlbumId = t.AlbumId
LEFT JOIN dbo.Genre g ON t.GenreId = g.GenreId AND g.Name = 'Latin'
WHERE g.GenreId IS NULL;

-- 3. which video track has the longest length? (use media type table)
SELECT t.Name, t.Milliseconds
FROM dbo.MediaType as m
LEFT JOIN dbo.Track as t ON m.MediaTypeId = t.MediaTypeId
WHERE m.MediaTypeId = 3
ORDER BY t.Milliseconds DESC;

/*
INSERT INTO [dbo].[MediaType] ([Name]) VALUES
    (N'MPEG audio file'),
    (N'Protected AAC audio file'),
    (N'Protected MPEG-4 video file'),
    (N'Purchased AAC audio file'),
    (N'AAC audio file');
*/

-- 4. boss employee (the one who reports to nobody)
SELECT e.FirstName + ' ' + e.LastName AS Name
FROM dbo.Employee AS e
WHERE e.ReportsTo IS NULL;

-- 5. how many audio tracks were bought by German customers, and what was
--    the total price paid for them?



-- 6. list the names and countries of the customers supported by an employee
--    who was hired younger than 35.




-- DML exercises

-- 1. insert two new records into the employee table.

-- 2. insert two new records into the tracks table.

-- 3. update customer Aaron Mitchell's name to Robert Walter

-- 4. delete one of the employees you inserted.

-- 5. delete customer Robert Walter.