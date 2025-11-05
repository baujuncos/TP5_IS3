# TikTask2.0 - Architecture and Technical Details

## System Architecture

TikTask2.0 follows a clean, layered architecture with clear separation between frontend, backend, and data layers.

```
┌─────────────────────────────────────────────────────┐
│                   User Browser                       │
└─────────────────┬───────────────────────────────────┘
                  │ HTTP/HTTPS
                  ▼
┌─────────────────────────────────────────────────────┐
│              Blazor Server (Frontend)                │
│  ┌─────────────────────────────────────────────┐   │
│  │  Components (Pages)                          │   │
│  │  - Login.razor                               │   │
│  │  - Register.razor                            │   │
│  │  - Tasks.razor                               │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │  Services                                    │   │
│  │  - ApiService (HTTP Client)                  │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────┬───────────────────────────────────┘
                  │ HTTP REST API + JWT
                  ▼
┌─────────────────────────────────────────────────────┐
│           ASP.NET Core Web API (Backend)             │
│  ┌─────────────────────────────────────────────┐   │
│  │  Controllers                                 │   │
│  │  - AuthController                            │   │
│  │  - TasksController                           │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │  Services                                    │   │
│  │  - TokenService (JWT)                        │   │
│  │  - DatabaseSeeder                            │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │  Middleware                                  │   │
│  │  - Authentication                            │   │
│  │  - Authorization                             │   │
│  │  - CORS                                      │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────┬───────────────────────────────────┘
                  │ Entity Framework Core
                  ▼
┌─────────────────────────────────────────────────────┐
│                  SQLite Database                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  Tables:                                     │   │
│  │  - Users (Id, Username, Email, PasswordHash, │   │
│  │           Role, CreatedAt)                   │   │
│  │  - Tasks (Id, Title, Description, DueDate,   │   │
│  │           IsCompleted, CreatedAt, UserId)    │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## Technology Stack

### Backend (TikTask2.0.API)

| Component | Technology | Purpose |
|-----------|------------|---------|
| Framework | ASP.NET Core 9.0 | Web API hosting |
| ORM | Entity Framework Core 9.0 | Database access |
| Database | SQLite | Data persistence |
| Authentication | JWT Bearer | Secure API access |
| Password Hashing | BCrypt.Net | Secure password storage |
| Serialization | System.Text.Json | JSON handling |

### Frontend (TikTask2.0.Web)

| Component | Technology | Purpose |
|-----------|------------|---------|
| Framework | Blazor Server 9.0 | Interactive UI |
| HTTP Client | System.Net.Http.Json | API communication |
| Styling | Bootstrap 5 | Responsive design |
| Rendering | Server-side | Fast initial load |

## Data Models

### User Entity
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }        // Unique
    public string Email { get; set; }           // Unique
    public string PasswordHash { get; set; }    // BCrypt hashed
    public string Role { get; set; }            // "User" or "Admin"
    public DateTime CreatedAt { get; set; }
    public ICollection<TaskItem> Tasks { get; set; }
}
```

### TaskItem Entity
```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }             // Foreign key
    public User User { get; set; }              // Navigation property
}
```

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login and get JWT token |

### Task Management Endpoints

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/api/tasks` | Yes | User/Admin | Get current user's tasks |
| GET | `/api/tasks/all` | Yes | Admin | Get all users' tasks |
| GET | `/api/tasks/{id}` | Yes | User/Admin | Get specific task |
| POST | `/api/tasks` | Yes | User/Admin | Create new task |
| PUT | `/api/tasks/{id}` | Yes | User/Admin | Update task |
| PATCH | `/api/tasks/{id}/complete` | Yes | User/Admin | Toggle completion |
| DELETE | `/api/tasks/{id}` | Yes | User/Admin | Delete task |

## Authentication Flow

```
1. User Registration
   ┌─────────┐    POST /api/auth/register    ┌─────────┐
   │ Client  │ ──────────────────────────────>│   API   │
   └─────────┘                                └────┬────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │ Hash password│
                                            │  (BCrypt)    │
                                            └──────┬───────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │ Save to DB   │
                                            └──────┬───────┘
                                                   │
   ┌─────────┐    JWT Token + User Info    ┌─────▼────┐
   │ Client  │ <──────────────────────────  │   API    │
   └─────────┘                              └──────────┘

2. User Login
   ┌─────────┐    POST /api/auth/login      ┌─────────┐
   │ Client  │ ──────────────────────────────>│   API   │
   └─────────┘                                └────┬────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │ Verify creds │
                                            │  (BCrypt)    │
                                            └──────┬───────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │Generate JWT  │
                                            │  token       │
                                            └──────┬───────┘
   ┌─────────┐    JWT Token + User Info    ┌─────▼────┐
   │ Client  │ <──────────────────────────  │   API    │
   └─────────┘                              └──────────┘

3. Authenticated Request
   ┌─────────┐    GET /api/tasks            ┌─────────┐
   │ Client  │    Bearer: {JWT Token}        │   API   │
   │         │ ──────────────────────────────>│         │
   └─────────┘                                └────┬────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │ Validate JWT │
                                            └──────┬───────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │Extract UserId│
                                            └──────┬───────┘
                                                   │
                                                   ▼
                                            ┌──────────────┐
                                            │ Query Tasks  │
                                            └──────┬───────┘
   ┌─────────┐    Task Data (JSON)         ┌─────▼────┐
   │ Client  │ <──────────────────────────  │   API    │
   └─────────┘                              └──────────┘
```

## Security Features

### 1. Password Security
- **Hashing Algorithm**: BCrypt with automatic salt generation
- **Work Factor**: Default BCrypt work factor (cost ~10-12)
- **Storage**: Only password hash stored, never plain text

### 2. JWT Token Security
- **Algorithm**: HMAC-SHA256
- **Expiration**: 7 days
- **Claims**: UserId, Username, Email, Role
- **Validation**: Issuer, Audience, Lifetime, Signature

### 3. Authorization
- **Role-based**: User and Admin roles
- **Claim-based**: User ID extracted from token
- **Endpoint protection**: `[Authorize]` and `[Authorize(Roles = "Admin")]`

### 4. CORS
- **Development**: Allow all origins (for local testing)
- **Production**: Should be restricted to specific domains

## Database Schema

```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'User',
    CreatedAt TEXT NOT NULL
);

CREATE TABLE Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    DueDate TEXT NOT NULL,
    IsCompleted INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UserId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
```

## Configuration

### JWT Configuration (appsettings.json)
```json
{
  "Jwt": {
    "Key": "32+ character secret key",
    "Issuer": "TikTask2.0",
    "Audience": "TikTask2.0Users"
  }
}
```

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tiktask.db"
  }
}
```

### API URL Configuration (Web app)
```json
{
  "ApiUrl": "http://localhost:5001"
}
```

## Deployment Considerations

### Development
- HTTP allowed
- HTTPS redirection disabled
- CORS allows all origins
- Detailed logging enabled
- SQLite file in local directory

### Production
- HTTPS enforced
- HTTPS redirection enabled
- CORS restricted to specific origins
- Minimal logging
- Database path configured via environment variable
- JWT key from secure configuration (Azure Key Vault)

## Performance Characteristics

### API Response Times (typical)
- Login/Register: < 200ms
- Get Tasks: < 50ms
- Create/Update Task: < 100ms
- Delete Task: < 50ms

### Scalability
- **Concurrent Users**: 100+ (Blazor Server limits)
- **Database**: SQLite suitable for small to medium workloads
- **Upgrade Path**: Migrate to Azure SQL Database for larger scale

## Monitoring and Logging

### Built-in Logging
- ASP.NET Core logging to console
- Request/Response logging
- Error logging with stack traces

### Production Monitoring (Recommended)
- Application Insights for Azure
- Custom metrics for task operations
- Alert rules for errors and performance

## Future Enhancements

### Planned Features
1. Email verification on registration
2. Password reset functionality
3. Task categories and tags
4. Task priority levels
5. File attachments for tasks
6. Task comments and activity log
7. Search and filtering
8. Task due date reminders
9. Export tasks to CSV/PDF
10. Mobile responsive improvements

### Technical Improvements
1. Add unit tests (xUnit)
2. Add integration tests
3. Implement repository pattern
4. Add caching (Redis)
5. Add rate limiting
6. Implement SignalR for real-time updates
7. Add API versioning
8. Implement audit logging
9. Add health checks
10. Containerize for Kubernetes

## References

### Official Documentation
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Blazor](https://docs.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT Authentication](https://jwt.io)
- [Azure DevOps](https://docs.microsoft.com/azure/devops)

### NuGet Packages
- Microsoft.EntityFrameworkCore.Sqlite (9.0.10)
- Microsoft.EntityFrameworkCore.Design (9.0.10)
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.10)
- System.IdentityModel.Tokens.Jwt (8.14.0)
- BCrypt.Net-Next (4.0.3)
- System.Net.Http.Json (9.0.10)
