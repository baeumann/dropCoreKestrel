var builder = WebApplication.CreateBuilder(args);

// builder.Configuration["Kestrel:Certificates:Default:Path"] = "avyan_blue.pem";
// builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = "privkey.pem";
builder.WebHost.UseKestrel();

var app = builder.Build();

app.UseHttpsRedirection();

app.Urls.Add("http://*:80");
app.Urls.Add("https://*:443");

string pageToReturn = File.ReadAllText("main.html");
string styleSheet = File.ReadAllText("style.css");
byte[] font = File.ReadAllBytes("iAWriterDuospace.woff2");
byte[] favicon = File.ReadAllBytes("favicon.png");

app.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(pageToReturn);
                });
app.MapGet("/font", async context =>
                {
                    context.Response.ContentType = "application/font-woff2";
                    await context.Response.Body.WriteAsync(font);
                });
app.MapGet("/favicon", async context =>
                {
                    context.Response.ContentType = "image/png";
                    await context.Response.Body.WriteAsync(favicon);
                });
app.MapGet("/style", async context =>
                {
                    context.Response.ContentType = "text/css";
                    await context.Response.WriteAsync(styleSheet);
                });

app.Run();
