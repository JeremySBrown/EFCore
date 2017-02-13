# EF Core
## Setup
We are going to pick up from the last session and work with the ChoreRepository.cs file and add database support through EF Core.

The ChoreRepository did an excellent job of outlining the models and behavior needed for the ChoreApp, and now is the time to start moving the project to production.  One of the primary strength of MVC is how it lends itself to separating concerns. We could have one project and dump everything into it, but that's no fun. The Start folder contains a refactored version of the SimpleAspNetCore application to help get things started to work with EF Core. The primary changes other than moving the classes to individual files is the domain models are moved into its own project. Below is an outline of the changes that were made. 

1. Solution renamed to ChoreApp because it will be containing more than just the SimpleAspNetCore project.
2. A .NET Core Library project named "ChoreApp" was added. 
3. The models classes from the ChoreRepository file were moved to this project 
    and refactored to work with EF Core by adding parameter less contractors, 
    and changing private property setters to public.
4. The exception classes were also moved to this project.
5. An interface was extracted from the ChoreRepostory class and added to the Contracts folder.

The SimpleAspNetCore project have the following changes
1. Refrence to ChoreApp library project was added.
2. ChoreRepository class was renamed to ChoreRepositoryStub and inherits from IChoreRepository.
3. Startup changes
   1. DI changed to use IChoreRepository
5. Controller changes
   1. Change ChoreRepository to use IChoreRepository


## Adding EF Core project
1. Add a new project by selecting File -> New -> Project
2. Select .NET Core on the left of the New Project dialog box
3. Select Console Application (.NET Core)
4. Name the project ChoreApp.DataStore

The reason we are using a Console Application rather than a Class Library project has to do with the tooling for Migrations. 
The .NET Core CLI must be able run a .NET Core app that targets a framework, and class Library projects are built without
a framework. Therefore dotnet-ef cannot run against a library project. So as of now if you want to separate you EF Core code 
a console app is the only option.

5. To add EF Core open the project.json file and the following to "dependencies" 
```
    "Microsoft.EntityFrameworkCore.SqlServer": "1.0.1",
    "Microsoft.EntityFrameworkCore.Tools": "1.0.0-preview2-final",
```
6. Add the following "tools" section after "dependencies"
```
  "tools": {
    "Microsoft.EntityFrameworkCore.Tools": {
      "version": "1.0.0-preview2-final",
      "type": "build"
    } 
  },
```
By adding Microsoft.EntityFrameworkCore.SqlServer this will provide support for EF Core and SQL Server Data Provider.
If you wanted target a different provider list SQLite you would use "Microsoft.EntityFrameworkCore.SQLite". 
The other settings are strictly for tooling and how Migrations are executed.

7. Since we want our data access to participte with dependency injection we need to add the 
    following to dependences.
```
    "Microsoft.Extensions.Configuration": "1.0.1",
    "Microsoft.Extensions.Configuration.Abstractions": "1.0.1",
    "Microsoft.Extensions.Configuration.FileExtensions": "1.0.1",
    "Microsoft.Extensions.Configuration.Json": "1.0.1"
```
8. Before we can build our data access code we need to add a reference to the ChoreApp library to dependencies.
```
    "ChoreApp": { "target": "project" },
```
9. Add a new class to called "ChoreAppDbContext" and have it inherit from ChoreAppDbContext.

```
    public class ChoreAppDbContext : DbContext
    {
       
    }
```

10. Add the following for the contructor.
```
    private readonly IConfigurationRoot _configurationRoot;

    public ChoreAppDbContext(DbContextOptions<ChoreAppDbContext> options, IConfigurationRoot configurationRoot)
    {
        _configurationRoot = configurationRoot;
    }
```
