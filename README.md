# vet-clinic-web-app
Web application that manages the interactions between clients and veterinarians. Clients can make appointments with certain doctors for their pet, which can be accepted or refused by the admin. If an appointment is accepted, it leads to an in-person consultation for the pet. Medical records are stored and bills created.

# dotnet commands I ran:
dotnet new mvc -n VetClinic -au Individual
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet ef migrations add InitialCreate -o Data/Migrations
docker run --name vet-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=admin -p 5432:5432 -d postgres
dotnet ef database update
dotnet ef migrations add AddBusinessEntities
dotnet ef migrations add SyncAppointments

# Modifying the way registering is done (not just email, password)
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet aspnet-codegenerator identity -dc VetClinic.Data.ApplicationDbContext --files "Account.Register"

# Using user-secrets (windows feature) to prevent uploading hard-coded admin credentials
dotnet user-secrets init