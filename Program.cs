using SSProjectSolution.Business;
using SSProjectSolution.Data;
using SSProjectSolution.Services;
using Serilog;
using FluentValidation.AspNetCore;
using SSProjectSolution.Validators;
using SSProjectSolution.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SSProjectSolution.SignalR;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RateQuotationCreateDtoValidator>());

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<RateQuotationProfile>());
builder.Services.AddHttpContextAccessor();

builder.Services.AddSignalR();

// JWT Auth Setup
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };

        // SignalR requires token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/printhub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Register Data Connection
builder.Services.AddSingleton<DapperDBConnection>();

// Register Repositories
builder.Services.AddScoped<SSProjectSolution.Repositories.IInwardRepository, SSProjectSolution.Repositories.InwardRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IOutwardRepository, SSProjectSolution.Repositories.OutwardRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IEmployeeRepository, SSProjectSolution.Repositories.EmployeeRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IDcDetailRepository, SSProjectSolution.Repositories.DcDetailRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IRateQuotationRepository, SSProjectSolution.Repositories.RateQuotationRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IPrintJobRepository, SSProjectSolution.Repositories.PrintJobRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IStockRepository, SSProjectSolution.Repositories.StockRepository>();

// Register Service Layer
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IInwardService, InwardService>();
builder.Services.AddScoped<IOutwardService, OutwardService>();
builder.Services.AddScoped<SSProjectSolution.Services.IStockService, SSProjectSolution.Services.StockService>();
builder.Services.AddScoped<IDcFilterService, DcFilterService>();

// Print Module Services
builder.Services.Configure<SSProjectSolution.Settings.PrintSettings>(builder.Configuration.GetSection("PrintModule"));
builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
builder.Services.AddScoped<IPdfSaveService, PdfSaveService>();
builder.Services.AddScoped<IPrintService, PrintService>();
builder.Services.AddScoped<IDeliveryChallanService, DeliveryChallanService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IFileValidator, FileValidator>();
builder.Services.AddScoped<IPrinterValidator, PrinterValidator>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPrintWorkflowService, PrintWorkflowService>();
builder.Services.AddScoped<IPrinterHealthService, PrinterHealthService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IStatusFilterService, StatusFilterService>();
builder.Services.AddScoped<IDcDetailService, DcDetailService>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();
builder.Services.AddScoped<IRateQuotationService, RateQuotationService>();

// Register Business Layer
builder.Services.AddScoped<IUserBusiness, UserBusiness>();
builder.Services.AddScoped<ICompanyBusiness, CompanyBusiness>();
builder.Services.AddScoped<IInwardBusiness, InwardBusiness>();

builder.Services.Configure<WhatsAppSettings>(
    builder.Configuration.GetSection(
        "WhatsAppSettings"));

builder.Services.AddHttpClient<
    IWhatsAppService,
    WhatsAppService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseRouting();

// Enable CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PrintHub>("/printhub");

app.Run();
