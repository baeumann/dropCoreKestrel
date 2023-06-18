using System.Text;
using dropCoreKestrel;

var builder = WebApplication.CreateBuilder(args);

if(File.Exists("avyan_blue.pem") && File.Exists("privkey.pem")) {
    Console.WriteLine("USING PRODUCTIVE CERTIFICATE");
    builder.Configuration["Kestrel:Certificates:Default:Path"] = "avyan_blue.pem";
    builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = "privkey.pem";
} else {
    Console.WriteLine("USING DEVELOPER CERTIFICATE");
}

builder.WebHost.UseKestrel();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Logging.SetMinimumLevel(LogLevel.Error);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseResponseCompression();

app.Urls.Add("http://*:80");
app.Urls.Add("https://*:443");

RequestStatistics requestStatistics = new RequestStatistics();
ParagraphInjector paragraphInjector = new ParagraphInjector("paragraphs.file");
FileCache fileCache = new FileCache();

string pageToReturn = paragraphInjector.InjectInto(File.ReadAllText("main.html"));
string styleSheet = File.ReadAllText("style.css");
byte[] font = File.ReadAllBytes("Roboto-Mono-regular.woff2");
byte[] favicon = File.ReadAllBytes("favicon.png");

app.MapGet("/", async context =>
                {
                    requestStatistics.RequestIncoming();
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
app.MapGet("/video/{filename}", async context =>
                {
                    string pathString = context.Request.Path;
                    string[] splitPath = pathString.Split("/");
                    string fileName = splitPath[splitPath.Length-1];
                    string filePath = Path.Combine("videos", fileName + ".webm");

                    if(File.Exists(filePath)) {
                        context.Response.ContentType = "video/webm";
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
                    paragraphInjector = new ParagraphInjector("paragraphs.file");
                    pageToReturn = paragraphInjector.InjectInto(File.ReadAllText("main.html"));
                    styleSheet = File.ReadAllText("style.css");
                    font = File.ReadAllBytes("iAWriterDuospace.woff2");
                    favicon = File.ReadAllBytes("favicon.png");
                    fileCache.Clear();

                    context.Response.StatusCode = 404;
                    await Task.Run(() => Thread.Sleep(10));
                });

var uuidString = Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
uuidString = Convert.ToBase64String(Encoding.UTF8.GetBytes(uuidString));
Console.WriteLine("STATS LISTENING ON > /" + uuidString);
app.MapGet("/" + uuidString, async context =>
                {
                    context.Response.ContentType = "text/html";
                    string statisticsString = "<meta http-equiv=\"refresh\" content=\"2\" /><h2>RPS [<b>" + requestStatistics.RequestRate + "</b>]<br>PEAK-RPS [" + requestStatistics.PeakRequestsPerSecond + "]</h2> <br>TODAY [" + requestStatistics.RequeustsThisDay + "] <br>LAST 7-DAYS [" + requestStatistics.RequestsOfLastSevenDaysAsString() + "] <br>UPTIME [" + requestStatistics.UptimeAsString() + "]";
                    await context.Response.WriteAsync(statisticsString);
                });

app.Run();
