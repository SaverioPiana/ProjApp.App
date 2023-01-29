using AvantiPoint.MobileAuth;

var builder = WebApplication.CreateBuilder(args);

builder.AddMobileAuth();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// maps https://{host}/mobileauth/{Apple|Google|Microsoft}
app.MapMobileAuthRoute();

app.Run();