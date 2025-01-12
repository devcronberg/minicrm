# MiniCRM

This project serves as a REST backend for a simple CRM application, demonstrating basic CRUD operations in a .NET 9.0 Web API project. It is also utilized in training and assignments for courses instructed by [Michell Cronberg](https://mcronberg.github.io/bogenomcsharp/diverse/ommichell.html).

## Available endpoints

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

## Customer class

The API works with a `Customer` class that has the following properties:

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

## Getting Started

1. Clone the repository to your local machine. You can also download the repository as a ZIP file and extract it to a local directory.
2. Ensure you have .NET 9.0 SDK installed on your machine.
3. Run the project by running `dotnet run`.
4. Open your web browser and navigate to `http://localhost:5000` (Swagger is integrated into this project to provide interactive API documentation).

## Database Initialization:

When you run the project for the first time, it will check if a `customers.db` file exists. If it does, you will be prompted to delete it and start fresh. If you choose to delete it, a new database will be created and pre-seeded with data from the `customers.json` file.

## Testing the API:

There is a `test.http` file included in this project that you can use to test the API endpoints.

To use the `test.http` file in Visual Studio Code:
1. Install the REST Client extension from the Visual Studio Code marketplace.
2. Open the `test.http` file in Visual Studio Code.
3. Click on the "Send Request" links that appear above each request to execute them.

## Swagger Documentation:

Swagger is integrated into this project to provide interactive API documentation. You can access the Swagger UI at the root URL of the application.

To view the Swagger UI:
1. Run the project by running `dotnet run`.
2. Open your web browser and navigate to `http://localhost:5000`.
3. You will see the Swagger UI, which lists all available endpoints and allows you to interact with them.

## Generating a C# Client using NSwag:

You can generate a C# client for this API using NSwag. Follow these steps:

1. Install the NSwag CLI tool:
   ```sh
   dotnet tool install -g NSwag.ConsoleCore
   ```

2. Generate the C# client code:
   ```sh
   nswag openapi2csclient /input:http://localhost:5000/swagger/v1/swagger.json /output:MiniCrmClient.cs
   ```

### Dependencies:

NSwag uses Newtonsoft.Json for JSON serialization and deserialization. Ensure you have the following dependencies installed in your project:

- `Newtonsoft.Json` (version 13.0.1 or later)

You can add this dependency to your project by running:
```sh
dotnet add package Newtonsoft.Json --version 13.0.1
```

This will generate a `MiniCrmClient.cs` file containing the C# client code for the API.

## Generating Clients in Other Languages:

You can also generate clients for this API in other languages such as Python, JavaScript, and TypeScript. Here are some tools you can use:

- **Python**: Use tools like `swagger-codegen` or `openapi-generator` to generate a Python client.
- **JavaScript**: Use tools like `swagger-js-codegen` or `openapi-generator` to generate a JavaScript client.
- **TypeScript**: Use tools like `swagger-typescript-api` or `openapi-generator` to generate a TypeScript client.

Refer to the documentation of these tools for detailed instructions on how to generate clients in these languages.

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

