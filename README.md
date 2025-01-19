# YourFoodBackend

YourFoodBackend is a server-side application written in C# that powers the backend for the YourFood platform. This project provides APIs and backend functionalities to manage food-related services, such as recipes, orders, users, and more.

## Features
- User authentication and authorization
- Management of food recipes and categories
- Order processing and tracking
- Admin panel for managing platform data
- Integration with third-party services (e.g., payment systems)

## Requirements
To run this project, you need to have the following installed:
- .NET 7.0 SDK or higher
- Microsoft SQL Server (or another supported database)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/0nufriy/YourFoodBackend.git
   ```

2. Navigate to the project directory:
   ```bash
   cd YourFoodBackend
   ```

3. Install dependencies:
   ```bash
   dotnet restore
   ```

4. Set up the database:
   - Configure the connection string in the `appsettings.json` file under the `ConnectionStrings` section.
   - Apply migrations to the database:
     ```bash
     dotnet ef database update
     ```

5. Run the application:
   ```bash
   dotnet run
   ```

## Usage
Once the server is running, you can access the API at `http://localhost:7178` (or the specified port). API documentation (e.g., Swagger) may be available at `http://localhost:7178/swagger`.
