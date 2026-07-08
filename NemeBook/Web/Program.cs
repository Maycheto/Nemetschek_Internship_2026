using Data;
using Data.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Services.Dtos.Registration;
using Services.Interfaces;
using Services.Interfaces.Chats;
using Services.Interfaces.Classes;
using Services.Interfaces.ClassSubjects;
using Services.Interfaces.Grades;
using Services.Interfaces.Parents;
using Services.Interfaces.Registration;
using Services.Interfaces.Security;
using Services.Interfaces.Students;
using Services.Interfaces.Subjects;
using Services.Interfaces.Teachers;
using Services.Options;
using Services.Repositories;
using Services.Services.Accounts;
using Services.Services.Auth;
using Services.Services.Chats;
using Services.Services.Classes;
using Services.Services.ClassSubjects;
using Services.Services.Email;
using Services.Services.Grades;
using Services.Services.Notifications;
using Services.Services.Parents;
using Services.Services.Registration;
using Services.Services.Security;
using Services.Services.Students;
using Services.Services.Subjects;
using Services.Services.Teachers;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

if (IsRunningInContainer() && LooksLikeLocalOnlyConnection(connectionString))
{
    var saPassword = builder.Configuration["MSSQL_SA_PASSWORD"] ?? "Your_strong_password_123!";
    connectionString = $"Server=mssql,1433;Database=NemeBook;User Id=sa;Password={saPassword};TrustServerCertificate=True;Encrypt=False;";
}

builder.Services.AddDbContext<NemeBookDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddTransient<RateLimitingOptions>();

// Register repositories.
builder.Services.AddScoped<IAbsenceRepository, AbsenceRepository>();
builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IClassScheduleEntryRepository, ClassScheduleEntryRepository>();
builder.Services.AddScoped<IClassSubjectRepository, ClassSubjectRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IRegistrationInvitationRepository, RegistrationInvitationRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Grade Repository
builder.Services.AddScoped<IGradeRepository, GradeRepository>();

// Grade Service
builder.Services.AddScoped<IGradeService, GradeService>();

//User Service
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IParentService, ParentService>();
builder.Services.AddScoped<IClassSubjectService, ClassSubjectService>();

// Register services.
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IEmailService>(serviceProvider => serviceProvider.GetRequiredService<EmailService>());
builder.Services.AddScoped<IRegistrationEmailSender>(serviceProvider => serviceProvider.GetRequiredService<EmailService>());
builder.Services.AddScoped<IRegistrationImportParser, ExcelRegistrationImportParser>();
builder.Services.AddScoped<IInvitationTokenService, InvitationTokenService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.Configure<RegistrationEmailOptions>(
    builder.Configuration.GetSection("RegistrationEmail"));

builder.Services.Configure<SmtpOptions>(options =>
{
    var emailSection = builder.Configuration.GetSection("Email");

    options.Host = emailSection["SmtpHost"] ?? string.Empty;
    options.Port = emailSection.GetValue<int>("SmtpPort");
    options.Username = emailSection["SmtpUsername"] ?? string.Empty;
    options.Password = emailSection["SmtpPassword"] ?? string.Empty;
    options.FromEmail = emailSection["FromEmail"];
    options.FromName = emailSection["FromName"];
});

// Add Cookie Authentication.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

await EnsureDatabaseReadyAndMigratedAsync(app);
await SeedPrincipalAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!isRunningInContainer)
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static bool IsRunningInContainer()
{
    return string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);
}

static bool LooksLikeLocalOnlyConnection(string connectionString)
{
    return connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
           || connectionString.Contains("Server=localhost", StringComparison.OrdinalIgnoreCase)
           || connectionString.Contains("Data Source=localhost", StringComparison.OrdinalIgnoreCase);
}

static async Task EnsureDatabaseReadyAndMigratedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NemeBookDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");

    const int maxAttempts = 15;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database is ready and migrations are applied.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Database not ready yet. Retry {Attempt}/{MaxAttempts}...", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    await dbContext.Database.MigrateAsync();
}

static async Task SeedPrincipalAsync(WebApplication app)
{
    var firstName = app.Configuration["SeedPrincipal:FirstName"];
    var lastName = app.Configuration["SeedPrincipal:LastName"];
    var email = app.Configuration["SeedPrincipal:Email"];
    var password = app.Configuration["SeedPrincipal:Password"];

    if (string.IsNullOrWhiteSpace(firstName) ||
        string.IsNullOrWhiteSpace(lastName) ||
        string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var registrationService = scope.ServiceProvider.GetRequiredService<IRegistrationService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PrincipalSeeder");

    var result = await registrationService.SeedPrincipalAsync(
        new SeedPrincipalRequest
        {
            FirstName = firstName,
            MiddleName = app.Configuration["SeedPrincipal:MiddleName"],
            LastName = lastName,
            Email = email,
            PhoneNumber = app.Configuration["SeedPrincipal:PhoneNumber"],
            Password = password
        });

    logger.LogInformation(
        "Principal seed completed. Created: {Created}, UserId: {UserId}",
        result.Created,
        result.UserId);
}