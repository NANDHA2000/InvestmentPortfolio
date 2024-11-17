

using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);


// Set EPPlus LicenseContext
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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
    options.AddPolicy("AllowEverything",
        policy => policy.AllowAnyOrigin()   // Allows all origins
                        .AllowAnyMethod()   // Allows all HTTP methods
                        .AllowAnyHeader()); // Allows all headers
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowEverything");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
