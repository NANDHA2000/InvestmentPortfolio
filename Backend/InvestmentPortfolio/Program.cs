using InvestmentPortfolio.Framework.Helper;
using InvestmentPortfolio.Repository.IRepository;
using InvestmentPortfolio.Repository.Repository;
using InvestmentPortfolio.Service.IService;
using InvestmentPortfolio.Service.Service;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<FileHelper>();
builder.Services.AddScoped<IVaultService, VaultService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IMutualFundService, MutualFundService>();

builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IVaultRepository, VaultRepository>();


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);


// Set EPPlus LicenseContext
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


/*builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



var app = builder.Build();

app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowEverything");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
