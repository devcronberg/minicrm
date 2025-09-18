# MiniCRM REST API Backend

MiniCRM is a .NET 9.0 Web API project demonstrating basic CRUD operations for a simple CRM application. This project is used for C#/TS/JS/HtmlX/Python training and assignments.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Environment Setup and Dependencies
- Install .NET 9.0 SDK to ~/.dotnet/ directory:
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.101 --install-dir ~/.dotnet
  export PATH="$HOME/.dotnet:$PATH"
  ```
- Verify installation: `~/.dotnet/dotnet --version` should show 9.0.101

### Build and Run Commands
- Restore packages: `~/.dotnet/dotnet restore` -- takes 2-3 seconds
- Build project: `~/.dotnet/dotnet build` -- takes 2-6 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- Run application: `~/.dotnet/dotnet run` -- starts in 5-10 seconds with database initialization

### Application Startup
- Application listens on http://localhost:5000
- Database initialization happens automatically on first run
- Pre-seeds 30 customers from customers.json file
- Swagger UI available at http://localhost:5000/swagger

### Database Management
- Uses SQLite database (customers.db)
- To skip database initialization: `~/.dotnet/dotnet run -- /nodbinit`
- Database is automatically created and seeded on first run
- If customers.db exists, you will be prompted to delete and recreate

## Testing and Validation

### API Endpoints Validation
Always test these key endpoints after making changes:

#### System Endpoints
```bash
# Version endpoint
curl -s http://localhost:5000/version
# Should return: 1.1.0.0

# Test endpoint
curl -s http://localhost:5000/test?text=Hello
# Should return: Hello
```

#### Customer Endpoints (Unauthenticated)
```bash
# Get all customers (should return 30 customers)
curl -s http://localhost:5000/customers | jq '. | length'

# Get customer by ID
curl -s http://localhost:5000/customers/1

# Find customers by name
curl -s http://localhost:5000/customers/findbyname/Lars
```

#### Authentication Flow Testing
**CRITICAL**: Always test the complete authentication flow:
```bash
# Get authentication token
TOKEN=$(curl -s -X POST http://localhost:5000/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId": "testclient", "clientSecret": "testsecret"}' | \
  jq -r '.access_token')

# Test authenticated endpoints
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5000/auth/customers | jq '. | length'
# Should return: 30
```

#### Test Credentials
- Client ID: `testclient`
- Client Secret: `testsecret`

### Manual Testing with HTTP Files
- Use Test/test.http for basic API testing
- Use Test/test-auth.http for authentication flow testing
- Use Test/test-api.html and Test/test-api.js for browser-based CORS testing
- Use UI/index.html for complete web interface testing
- Install REST Client extension in VS Code to execute requests

### Swagger UI Testing
- Access Swagger UI at http://localhost:5000/swagger
- Use interactive documentation to test all endpoints
- Verify authentication endpoints work with test credentials

### Web UI Testing
- Open UI/index.html in a web browser
- Test all CRUD operations through the web interface
- Verify modal forms work correctly
- Check responsive design on different screen sizes

## Development Guidelines

### Key Project Structure
```
Controllers/
├── SystemController.cs      # Version, test endpoints
├── CustomerController.cs    # CRUD operations (unauthenticated)
├── CustomerAuthController.cs # CRUD operations (authenticated)
└── AuthController.cs        # JWT authentication

Models/
├── Customer.cs             # Main data model
├── AppSettings.cs          # Configuration model
└── TokenResponse.cs        # Authentication response

Services/
└── jwtservice.cs          # JWT token management

Data/
└── CustomerDbContext.cs   # Entity Framework context
```

### Configuration
- Main config: appsettings.json
- Error simulation: Set `errorFactor` (0.0-1.0) and `delay` (milliseconds)
- JWT settings: SecretKey, Issuer, Audience, TokenExpirationMinutes
- Test client credentials configured in appsettings.json

### Important Files
- customers.json: Seed data for database initialization
- MiniCrm.csproj: Project dependencies and .NET 9.0 target framework
- Program.cs: Application startup and middleware configuration

## Validation Scenarios

### Complete End-to-End Testing
**ALWAYS** perform these scenarios after making changes:

1. **Application Startup Scenario**:
   ```bash
   # Clean start (delete customers.db if it exists)
   rm -f customers.db
   ~/.dotnet/dotnet run
   # Verify: Database created, 30 customers loaded, app listening on port 5000
   ```

2. **Basic API Functionality**:
   ```bash
   # Test system endpoints
   curl -s http://localhost:5000/version
   curl -s http://localhost:5000/test?text=TestMessage
   
   # Test customer data
   curl -s http://localhost:5000/customers | jq '. | length'
   ```

3. **Authentication Flow**:
   ```bash
   # Complete auth test
   TOKEN=$(curl -s -X POST http://localhost:5000/auth/token \
     -H "Content-Type: application/json" \
     -d '{"clientId": "testclient", "clientSecret": "testsecret"}' | \
     jq -r '.access_token')
   
   curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5000/auth/customers
   ```

4. **Swagger Documentation**:
   - Navigate to http://localhost:5000/swagger/index.html
   - Verify all endpoints are documented
   - Test authentication in Swagger UI

### Performance Expectations
- **NEVER CANCEL**: Build takes 2-6 seconds. Set timeout to 30+ seconds.
- Restore takes 2-3 seconds
- Application startup takes 5-10 seconds with DB initialization
- API responses should be immediate (unless delay is configured)

## Common Issues and Solutions

### .NET SDK Issues
- Ensure .NET 9.0 SDK is installed: `~/.dotnet/dotnet --version`
- If build fails with package compatibility: Verify target framework is net9.0
- Set PATH: `export PATH="$HOME/.dotnet:$PATH"`

### Database Issues
- If database errors occur: Delete customers.db and restart application
- To skip DB init: Use `-- /nodbinit` parameter

### Authentication Issues
- Use exact credentials: clientId="testclient", clientSecret="testsecret"
- Token expires after 60 minutes (configurable)
- Check JWT settings in appsettings.json

## Project Context
- **WARNING**: This repo is used in training and code should not be used in production
- Created by Michell Cronberg for educational purposes
- Demonstrates REST API patterns, JWT authentication, Entity Framework, and Swagger integration