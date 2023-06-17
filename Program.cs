using System.Diagnostics;
using dropCoreKestrel;

var builder = WebApplication.CreateBuilder(args);

// builder.Configuration["Kestrel:Certificates:Default:Path"] = "avyan_blue.pem";
// builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = "privkey.pem";
builder.WebHost.UseKestrel();

var app = builder.Build();

app.UseHttpsRedirection();

app.Urls.Add("http://*:80");
app.Urls.Add("https://*:443");

ParagraphInjector paragraphInjector = new ParagraphInjector("paragraphs.file");
FileCache fileCache = new FileCache();

string pageToReturn = paragraphInjector.InjectInto(File.ReadAllText("main.html"));
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
app.MapGet("/image/{filename}", async context =>
                {
                    string pathString = context.Request.Path;
                    string[] splitPath = pathString.Split("/");
                    string fileName = splitPath[splitPath.Length-1];
                    string filePath = Path.Combine("images", fileName + ".png");

                    if(File.Exists(filePath)) {
                        context.Response.ContentType = "image/png";
                        await context.Response.Body.WriteAsync(fileCache.Load(filePath));
                    } else {
                        context.Response.StatusCode = 404;
                    }
                });
app.MapGet("/style", async context =>
                {
                    context.Response.ContentType = "text/css";
                    await context.Response.WriteAsync(styleSheet);
                });

app.MapGet("/update", async context =>
                {
                    Console.WriteLine("UPDATING RESOURCES");
                    pageToReturn = paragraphInjector.InjectInto(File.ReadAllText("main.html"));
                    styleSheet = File.ReadAllText("style.css");
                    font = File.ReadAllBytes("iAWriterDuospace.woff2");
                    favicon = File.ReadAllBytes("favicon.png");
                    fileCache.Clear();

                    context.Response.StatusCode = 404;
                });

app.Run();
