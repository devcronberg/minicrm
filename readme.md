# MiniCRM

Welcome to this MiniCRM. It can be used as REST backend for a simple CRM app.

## Available endpoints:

- **GET /customers** - Get all customers
- **GET /customers/{id}** - Get a customer by ID
- **GET /customers/findbyname/{name}** - Find customers by name
- **POST /customers** - Create a new customer
- **POST /api/customers** - Create a new customer
- **PUT /api/customers/{id}** - Update an existing customer
- **PATCH /api/customers/{id}** - Partially update an existing customer
- **DELETE /api/customers/{id}** - Delete a customer
- **GET /test?text={text}** - Returns the provided text (mirror)
- **GET /version** - Returns the version (major.minor.build)

## Customer class:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Country { get; set; } = string.Empty;
    public double Revenue { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
}
```

## Getting Started:

1. Clone the repository to your local machine.
2. Open the project in your preferred IDE (e.g., Visual Studio, Visual Studio Code).
3. Ensure you have .NET 6.0 SDK installed on your machine.
4. Restore the project dependencies by running `dotnet restore` in the project directory.
5. Build the project by running `dotnet build`.
6. Run the project by running `dotnet run`.

## Database Initialization:

When you run the project for the first time, it will check if a `customers.db` file exists. If it does, you will be prompted to delete it and start fresh. If you choose to delete it, a new database will be created and pre-seeded with data from the `customers.json` file.

## Testing the API:

There is a `test.http` file included in this project that you can use to test the API endpoints.

To use the `test.http` file in Visual Studio Code:
1. Install the REST Client extension from the Visual Studio Code marketplace.
2. Open the `test.http` file in Visual Studio Code.
3. Click on the "Send Request" links that appear above each request to execute them.

## Example Requests:

1. **Get all customers:**
   ```http
   GET http://localhost:5000/customers
   ```

2. **Get a customer by ID:**
   ```http
   GET http://localhost:5000/customers/1
   ```

3. **Find customers by name:**
   ```http
   GET http://localhost:5000/customers/findbyname/John
   ```

4. **Create a new customer:**
   ```http
   POST http://localhost:5000/customers
   Content-Type: application/json

   {
     "name": "Alice Johnson",
     "age": 28,
     "country": "USA",
     "revenue": 85000.00,
     "createdDate": "2024-08-01T00:00:00",
     "isActive": true,
     "tags": ["new", "tech"]
   }
   ```

5. **Update an existing customer:**
   ```http
   PUT http://localhost:5000/api/customers/1
   Content-Type: application/json

   {
     "id": 1,
     "name": "John Doe Updated",
     "age": 31,
     "country": "USA",
     "revenue": 155000.50,
     "createdDate": "2023-01-01T00:00:00",
     "isActive": true,
     "tags": ["vip", "regular"]
   }
   ```

6. **Partially update an existing customer:**
   ```http
   PATCH http://localhost:5000/api/customers/1
   Content-Type: application/json

   {
     "age": 32,
     "revenue": 160000.00
   }
   ```

7. **Delete a customer:**
   ```http
   DELETE http://localhost:5000/api/customers/1
   ```

8. **Test endpoint:**
   ```http
   GET http://localhost:5000/test?text=Hello
   ```

9. **Version endpoint:**
   ```http
   GET http://localhost:5000/version
   ```

We hope you find this MiniCRM useful for your projects. Happy coding!
