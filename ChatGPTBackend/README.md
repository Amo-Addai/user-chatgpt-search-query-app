# user-gpt-search-query-app - back-end - .net entity-framework & chatgpt

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes. 
See deployment for notes on how to deploy the project on a live system.

## Getting Started

Technologies Required:

- Databases - Sqlite / SQL Server / MySQL - Setup any of the databases, and get their connection strings
- C# .NET SDK - download it from the official [.NET website](https://dotnet.microsoft.com/download)

<br />

# Installation Steps:

After cloning the mono-repo, and going into the base directory.

Install the .NET SDK download

Ensure it's installed correctly by checking its version

```
dotnet --version
```

Go into the back-end directory

```
cd ChatGPTBackend/
```

In some cases, you may need to clean up dev-certificates

```
dotnet clean
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Setup Sqlite Database file in the Data folder (Sqlite by default; you can also setup MSSql and MySql, and get their connection strings)

```
cd Data/
touch sqlite.db
cd ..
```

Enable & Apply Migrations

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Add path to sqlite.db file to appsettings.json

```
  "ConnectionStrings": {
    "Sqlite": "Data Source=Data/sqlite.db",
  }
```

Create .env file (in the base directory), and add your OpenAI Api Key

```
OPENAI_API_KEY=put_openai_api_key_here
```

Install all .NET packages

```
dotnet restore
```

Run the server with the https launch-profile

```
dotnet run --launch-profile https
```

