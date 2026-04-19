using SSProjectSolution.Business;
using SSProjectSolution.Data;
using SSProjectSolution.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register Data Connection
builder.Services.AddSingleton<DapperDBConnection>();

// Register Repositories
builder.Services.AddScoped<SSProjectSolution.Repositories.IInwardRepository, SSProjectSolution.Repositories.InwardRepository>();
builder.Services.AddScoped<SSProjectSolution.Repositories.IOutwardRepository, SSProjectSolution.Repositories.OutwardRepository>();

// Register Service Layer
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IInwardService, InwardService>();
builder.Services.AddScoped<IOutwardService, OutwardService>();
builder.Services.AddScoped<IDcFilterService, DcFilterService>();
builder.Services.AddScoped<IPrintService, PrintService>();

// Register Business Layer
builder.Services.AddScoped<IUserBusiness, UserBusiness>();
builder.Services.AddScoped<ICompanyBusiness, CompanyBusiness>();
builder.Services.AddScoped<IInwardBusiness, InwardBusiness>();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Enable CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
