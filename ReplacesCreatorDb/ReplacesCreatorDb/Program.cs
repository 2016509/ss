using ReplacesCreatorDb.DatabaseHandler;
using Serilog;
using Serilog.Events;
using Serilog.Sinks;
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}", //{Properties:j} {NewLine} {Exception}
        restrictedToMinimumLevel: LogEventLevel.Debug)
    .CreateLogger();

Log.Information("Инициализация..");

await RequestHandler.NewRequestsHandel();

Console.ReadLine();