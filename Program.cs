using System.Text;
using dropCoreKestrel;

Console.WriteLine(
"    _              \n" +
" __| |_ _ ___ _ __ \n" +
"/ _` | '_/ _ \\ '_ \\\n" +
"\\__,_|_| \\___/ .__/ v1.1\n" +
"             |_|  \n");

var builder = WebApplication.CreateBuilder(args);

if(File.Exists("yourcert.pem") && File.Exists("privkey.pem")) {
    PrintMessage("USING PRODUCTIVE CERTIFICATE");
    builder.Configuration["Kestrel:Certificates:Default:Path"] = "yourcert.pem";
    builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = "privkey.pem";
} else {
    PrintMessage("USING DEVELOPER CERTIFICATE");
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
byte[] font = File.ReadAllBytes("Inter-Regular.woff2");
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
                    string filePath = Path.Combine("image", fileName + ".png");

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
                    string filePath = Path.Combine("video", fileName + ".webm");

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

var uuidString = Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
uuidString = Convert.ToBase64String(Encoding.UTF8.GetBytes(uuidString));
PrintMessage("STATS LISTENING ON > /" + uuidString);
app.MapGet("/" + uuidString, async context =>
                {
                    context.Response.ContentType = "text/html";
                    string statisticsString = "<meta http-equiv=\"refresh\" content=\"2\" /><h2>RPS [<b>" + requestStatistics.RequestRate + "</b>]<br>PEAK-RPS [" + requestStatistics.PeakRequestsPerSecond + "]</h2> <br>TODAY [" + requestStatistics.RequeustsThisDay + "] <br>LAST 7-DAYS [" + requestStatistics.RequestsOfLastSevenDaysAsString() + "] <br>UPTIME [" + requestStatistics.UptimeAsString() + "]";
                    await context.Response.WriteAsync(statisticsString);
                });

app.Run();


void PrintMessage(string message) {
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write(DateTime.Now.ToShortDateString() + " ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write(DateTime.Now.ToShortTimeString() + " ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(message + "\n");
}
