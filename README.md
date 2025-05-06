Product Sorter API
A simple ASP.NET Core application that sorts product data and provides API access.
What it does

Reads product data from a text file
Sorts products by ID, name, and price
Provides API endpoints to retrieve sorted products

Setup

Make sure you have .NET 9.0 installed

Run with dotnet run

API Usage
Get products with:
GET /api/products/getproducts

Parameters:

pageNumber (default: 1)
pageSize (default: 10)
sortBy ("id", "name", or "price")

Example:
GET /api/products/getproducts?pageNumber=1&pageSize=10&sortBy=price
