Follow these steps:

```sh
# Step 1: Clone the repository using the project's Git URL.
git clone https://github.com/hiai-demo-qms/qms_backend.git

# Step 2: Navigate to the project directory.
cd qms_backend

# Step 3: Configure Connection String.
# Open appsettings.json
# Edit the DefaultConnection string to match your SQL Server, for example:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=QmsDb;User Id=sa;Password=yourStrong(!)Password;Trusted_Connection=False;MultipleActiveResultSets=true"
}

# Step 4: Initialize Database and Run Migration.
dotnet tool install --global dotnet-ef
dotnet ef database update

# Step 5: Run the Application
dotnet run
```
