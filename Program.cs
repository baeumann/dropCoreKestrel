using System.Text;
using dropCoreKestrel;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Metadata;
using System.Linq;

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
byte[] favicon = File.ReadAllBytes("favicon.png");

app.MapGet("/", async context =>
                {
                    requestStatistics.RequestIncoming();
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(pageToReturn);
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
    // === JSON API ===
    if (context.Request.Query.ContainsKey("json"))
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var stats = new
        {
            rps = requestStatistics.RequestRate,
            peak_rps = requestStatistics.PeakRequestsPerSecond,
            today = requestStatistics.RequeustsThisDay,
            last_7_days = requestStatistics.RequestsThisWeek.Take(7).Reverse().ToArray(), // only last 7 full days
            uptime = requestStatistics.UptimeAsValue(),
            generated_at = DateTime.UtcNow.ToString("o")
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(stats));
        return;
    }

    // === FULL HTML DASHBOARD (PERFECT LAYOUT) ===
    context.Response.ContentType = "text/html; charset=utf-8";

    const string html = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Server Stats • Live</title>
            <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
            <style>
                :root{--bg:#0d1117;--card:#161b22;--border:#30363d;--text:#c9d1d9;--accent:#58a6ff;--rps:#79c0ff}
                body{font-family:system-ui,sans-serif;background:var(--bg);color:var(--text);margin:0;padding:20px}
                .c{max-width:1100px;margin:auto}
                h1{text-align:center;color:var(--accent);margin:30px 0 10px}
                .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:20px;margin:30px 0}
                .card{background:var(--card);border:1px solid var(--border);border-radius:12px;padding:20px;text-align:center}
                .card h3{margin:0 0 10px;color:#8b949e;font-size:0.95rem}
                .v{font-size:2.8rem;font-weight:bold;color:var(--accent);margin:10px 0}
                .rps{font-size:4.5rem;color:var(--rps)}
                canvas{background:var(--card);border-radius:12px;margin-top:15px}
                footer{text-align:center;margin:40px 0 20px;color:#8b949e;font-size:0.9rem}
                .spark{height:70px}
                .chart-container{background:var(--card);border:1px solid var(--border);border-radius:12px;padding:20px;margin-top:30px}
            </style>
        </head>
        <body>
            <div class="c">
                <h1>Server Statistics</h1>

                <!-- 4 MAIN STATS -->
                <div class="grid">
                    <div class="card">
                        <h3>Requests / Second</h3>
                        <div class="v rps" id="rps">0.0</div>
                        <canvas id="rpsChart" class="spark"></canvas>
                    </div>
                    <div class="card"><h3>Peak RPS</h3><div class="v" id="peak">0</div></div>
                    <div class="card"><h3>Requests Today</h3><div class="v" id="today">0</div></div>
                    <div class="card"><h3>Uptime</h3><div class="v" id="uptime">0 min</div></div>
                </div>

                <!-- 7-DAY CHART BELOW -->
                <div class="chart-container">
                    <h2 style="text-align:center;color:var(--accent);margin-top:0">Requests – Last 7 Days</h2>
                    <canvas id="weekly" height="300"></canvas>
                </div>

                <footer>Last update: <span id="ts">—</span> • Refreshes every 3s</footer>
            </div>

            <script>
                const jsonUrl = location.pathname + "?json=1";
                const rpsHistory = [];
                let counter = 0;

                // RPS Sparkline – perfect working
                const rpsChart = new Chart(document.getElementById('rpsChart'), {
                    type: 'line',
                    data: { datasets: [{
                        data: [],
                        borderColor: '#79c0ff',
                        backgroundColor: 'rgba(121, 192, 255, 0.15)',
                        borderWidth: 2.5,
                        pointRadius: 0,
                        tension: 0.4,
                        fill: true
                    }]},
                    options: {
                        animation: false,
                        parsing: false,
                        plugins: { legend: { display: false }, tooltip: { enabled: false } },
                        scales: { x: { type: 'linear', display: false }, y: { display: false, suggestedMin: 0 } }
                    }
                });

                // 7-day chart – only 7 full days (no "Today" label)
                const weekly = new Chart(document.getElementById('weekly'), {
                    type: 'bar',
                    data: {
                        labels: ['7d ago','6d ago','5d ago','4d ago','3d ago','2d ago','Yesterday'],
                        datasets: [{ data: [0,0,0,0,0,0,0], backgroundColor: '#58a6ff', borderRadius: 8 }]
                    },
                    options: {
                        plugins: { legend: { display: false } },
                        scales: {
                            y: { beginAtZero: true, grid: { color: '#30363d' }, ticks: { color: '#8b949e' } },
                            x: { grid: { display: false }, ticks: { color: '#8b949e' } }
                        }
                    }
                });

                async function update() {
                    try {
                        const res = await fetch(jsonUrl + "&_=" + Date.now());
                        const d = await res.json();

                        document.getElementById('rps').textContent = Number(d.rps).toFixed(1);
                        document.getElementById('peak').textContent = d.peak_rps;
                        document.getElementById('today').textContent = Number(d.today).toLocaleString();

                        const days = d.uptime;
                        document.getElementById('uptime').textContent = 
                            days < 1 ? `${Math.round(days*1440)} min` : `${days.toFixed(3)} days`;

                        // Update 7-day chart (7 values only)
                        weekly.data.datasets[0].data = d.last_7_days;
                        weekly.update('quiet');

                        // RPS history
                        counter++;
                        rpsHistory.push({ x: counter, y: d.rps });
                        if (rpsHistory.length > 60) rpsHistory.shift();
                        rpsChart.data.datasets[0].data = rpsHistory;
                        rpsChart.update('quiet');

                        document.getElementById('ts').textContent = new Date(d.generated_at).toLocaleTimeString();
                    } catch (e) { console.error(e); }
                }

                update();
                setInterval(update, 3000);
            </script>
        </body>
        </html>
        """;

    await context.Response.WriteAsync(html);
});

// Ensure this runs after all MapGet/MapPost calls are registered:
app.Lifetime.ApplicationStarted.Register(() =>
{
    PrintMessage("SERVER RUNNING");

    try
    {
        var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "no explicit urls";
        PrintMessage("LISTENING ON: " + urls);
    }
    catch (Exception ex)
    {
        PrintMessage("Could not read URLs: " + ex.Message);
    }
});

// optional: notify on shutdown
app.Lifetime.ApplicationStopped.Register(() => PrintMessage("SERVER STOPPED"));

app.Run();

void PrintMessage(string message) {
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write(DateTime.Now.ToShortDateString() + " ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write(DateTime.Now.ToShortTimeString() + " ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(message + "\n");
}
