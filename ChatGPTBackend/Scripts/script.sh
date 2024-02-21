#!/bin/bash

# Array of example packages to re-add
packages=(    
    "Swashbuckle.AspNetCore", "Microsoft.AspNetCore.OpenApi", "Microsoft.AspNetCore", 
    "Microsoft.AspNetCore.Hosting", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.SqlServer", 
    "Microsoft.EntityFrameworkCore.Relational", "Microsoft.AspNetCore.Authentication.JwtBearer", 
    "Microsoft.AspNetCore.Identity.EntityFrameworkCore", "Microsoft.Extensions.Configuration", 
    "Microsoft.Extensions.DependencyInjection", "Microsoft.Extensions.Hosting", "Microsoft.IdentityModel.Tokens",
    "System.IdentityModel.Tokens.Jwt"
)

# Go to base folder
cd ../

# Remove all added dotnet packages
dotnet remove package --all

# Clean up bin/
dotnet clean

# Loop through the array and re-add each package
for package in "${packages[@]}"; do
    dotnet add package "$package"
done
