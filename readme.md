# MiniCRM

This project serves as a REST backend for a simple CRM application, demonstrating basic CRUD operations in a .NET 9.0 Web API project. It is also utilized in C#/TS/JS/HtmlX/Python training and assignments for courses instructed by [Michell Cronberg](https://mcronberg.github.io/bogenomcsharp/diverse/ommichell.html).

> WARNING: This repo is used in training and code should not be used in production.

## Available endpoints

### System

- **GET /test?text={text}** - Returns the provided text (mirror)
- **GET /version** - Returns the version (major.minor.build)

### Customers (not authenticated)

- **GET /customers** - Get all customers
- **GET /customers/{id}** - Get a customer by ID
- **GET /customers/findbyname/{name}** - Find customers by name
- **POST /customers** - Create a new customer
- **POST /customers** - Create a new customer
- **PUT /customers/{id}** - Update an existing customer
- **PATCH /customers/{id}** - Partially update an existing customer
- **DELETE /customers/{id}** - Delete a customer

### Authentication Endpoints

- **POST /auth/token** - Generate a new token
- **POST /auth/refresh** - Refresh an expired token
- **POST /auth/revoke** - Revoke a token
- **POST /auth/validate** - Validate a token

### Customers (authenticated)

- **GET auth/customers** - Get all customers
- **GET auth/customers/{id}** - Get a customer by ID
- **GET auth/customers/findbyname/{name}** - Find customers by name
- **POST auth/customers** - Create a new customer
- **POST auth/customers** - Create a new customer
- **PUT auth/customers/{id}** - Update an existing customer
- **PATCH auth/customers/{id}** - Partially update an existing customer
- **DELETE auth/customers/{id}** - Delete a customer

### Examples

See thw two http-files in `/test` for examples.

## Test Clients

For testing purposes, the following client credentials are configured in the `appsettings.json` file:

```json
"Clients": [
  {
    "ClientId": "testClient",
    "ClientSecret": "testSecret"
  }
]
```

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

## Database Initialization

When you run the project for the first time, it will check if a `customers.db` file exists. If it does, you will be prompted to delete it and start fresh. If you choose to delete it, a new database will be created and pre-seeded with data from the `customers.json` file.

If needed you can start the app with

 ```
 dotnet run -- /nodbinit
 ``` 

## Error factor and delay

To simulate real-world scenarios, the API includes an error factor and delay mechanism. This can be useful for testing how your application handles network latency and random errors.

### Configuring Error Factor and Delay

You can configure the error factor and delay by setting the following variables in appsettings.json:

- `errorFactor`: A value between 0 and 1 that determines the probability of an error occurring. For example, setting `errorFactor` to `0.1` means there is a 10% chance of an error occurring on each request.
- `delay`: The amount of delay (in milliseconds) to introduce in each request. For example, setting `delay` to `500` will introduce a 500ms delay in each request.

## Testing the API

There is a `test.http` file included in this project that you can use to test the API endpoints.

To use the `test.http` file in Visual Studio Code:
1. Install the REST Client extension from the Visual Studio Code marketplace.
2. Open the `test.http` file in Visual Studio Code.
3. Click on the "Send Request" links that appear above each request to execute them.

## Swagger Documentation

Swagger is integrated into this project to provide interactive API documentation. You can access the Swagger UI at the root URL of the application.

To view the Swagger UI:
1. Run the project by running `dotnet run`.
2. Open your web browser and navigate to `http://localhost:5000`.
3. You will see the Swagger UI, which lists all available endpoints and allows you to interact with them.

## Generating a C# Client using NSwag

You can talk directly to the API using pure http requests (like `HttpClient` i C#), but you can also generate a C# client to interact with the API programmatically. This can be useful if you are building a .NET application that needs to communicate with the API.

You can generate a C# client for this API using NSwag. Follow these steps:

1. Install the NSwag CLI tool:
   ```sh
   dotnet tool install -g NSwag.ConsoleCore
   ```

2. Generate an console app, add NewtonSoft.Json and then generate the C# client code:
   ```sh
   dotnet new console -n MiniCrmClientTest
   cd MiniCrmClientTest
   dotnet add package Newtonsoft.Json 
   nswag openapi2csclient /input:swagger.json /output:MiniCrmClient.cs /namespace:MiniCrmClient
   ```
   This will generate a `MiniCrmClient.cs` file containing the C# client code for the API.

3. Change program.cs to use the generated client code:
   ```csharp
    using MiniCrm.Client;
    HttpClient httpClient = new HttpClient();
    Client client = new Client("http://localhost:5000", httpClient);

    Console.WriteLine((await client.VersionAsync()));
    Console.WriteLine(await client.TestAsync("**"));
    var r = (await client.CustomersAllAsync()).ToList();
    r.ForEach(i => System.Console.WriteLine(i.Name));

    // Auth

    var tokenResponse = await client.TokenAsync(new ClientCredentials
    {
        ClientId = "testclient",
        ClientSecret = "testsecret"
    });
    string token = tokenResponse.Access_token;
    httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var t = await client.FindByNameAuthAsync("lars");
    t.ToList().ForEach(i => System.Console.WriteLine(i.Name));
   ```
### Dependencies:

NSwag uses Newtonsoft.Json for JSON serialization and deserialization. Ensure you have the following dependencies installed in your project:

- `Newtonsoft.Json` (version 13.0.1 or later)

You can add this dependency to your project by running:
```sh
dotnet add package Newtonsoft.Json --version 13.0.1
```
## Generating Clients in Other Languages:

You can also generate clients for this API in other languages such as Python, JavaScript, and TypeScript. Here are some tools you can use:

- **Python**: Use tools like `swagger-codegen` or `openapi-generator` to generate a Python client.
- **JavaScript**: Use tools like `swagger-js-codegen` or `openapi-generator` to generate a JavaScript client.
- **TypeScript**: Use tools like `swagger-typescript-api` or `openapi-generator` to generate a TypeScript client.

Refer to the documentation of these tools for detailed instructions on how to generate clients in these languages.
