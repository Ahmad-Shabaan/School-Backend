using Application;
using Application.Interfaces;
using Infrastructure;
using Presentation.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", policy =>
//    {

//        //policy.WithOrigins("https://book-wise-ecru.vercel.app", "https://localhost:5173")  // React origin ClientUrl
//        // .AllowAnyHeader()
//        // .AllowAnyMethod()
//        // .AllowCredentials();
//        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();//WithOrigins("https://localhost:4200");
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://school-lime-seven.vercel.app", "https://localhost:5173")  // React origin ClientUrl
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer =
        scope.ServiceProvider
            .GetRequiredService<IAppDbInitializer>();

    await initializer.InitializeAsync();
}

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
