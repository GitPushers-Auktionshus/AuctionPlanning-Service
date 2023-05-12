using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using RabbitMQ.Client;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using AuctionPlanningServiceAPI;

namespace AuctionPlanningServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionPlanningServiceAPI : ControllerBase
{
    private readonly ILogger<AuctionPlanningServiceAPI> _logger;
    private readonly string _filePath;
    private readonly string _hostName;

    public AuctionPlanningServiceAPI(ILogger<AuctionPlanningServiceAPI> logger, IConfiguration config)
    {
        _logger = logger;
        // Henter miljø variabel "FilePath" og "HostnameRabbit" fra docker-compose
        _filePath = config["FilePath"] ?? "/srv";
        //_logger.LogInformation("FilePath er sat til: [$_filePath]"); virker måske
        _hostName = config["HostnameRabbit"];

        _logger.LogInformation($"Filepath: {_filePath}");
        _logger.LogInformation($"Connection: {_hostName}");

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"Auction responding from {_ipaddr}");
    }

    // Opretter en AuctionDTO
    [Authorize]
    [HttpPost("opretbooking")]
    public IActionResult OpretBooking(AuctionDTO auctionDTO)
    {
        AuctionDTO auctionDTO = new AuctionDTO
        {
            AuctionID = auctionDTO.AuctionID,
            HighestBid = auctionDTO.HighestBid,
            BidCounter = auctionDTO.BidCounter,
            StartDate = auctionDTO.StartDate,
            EndDate = auctionDTO.EndDate,
            Views = auctionDTO.Views,
            ArticleID = auctionDTO.ArticleID
        };

        try
        {
            //Opretter forbindelse til RabbitMQ
            var factory = new ConnectionFactory
            {
                HostName = _hostName
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "AuctionService", type: ExchangeType.Topic);

            // Opretter en kø "hello" hvis den ikke allerede findes i vores rabbitmq-server
            //channel.QueueDeclare(queue: "hello",
            //                     durable: false,
            //                     exclusive: false,
            //                     autoDelete: false,
            //                     arguments: null);

            // Serialiseres til JSON
            string message = JsonSerializer.Serialize(auctionDTO);

            // Konverteres til byte-array
            var body = Encoding.UTF8.GetBytes(message);

            // Sendes til hello-køen
            channel.BasicPublish(exchange: "AuctionService",
                                 routingKey: "AuctionDTO",
                                 basicProperties: null,
                                 body: body);


            _logger.LogInformation("AuctionDTO oprettet");

            Console.WriteLine($"[*] Auction sendt:\n\tAuctionID: {auctionDTO.AuctionID}\n\tHighestBid: {auctionDTO.HighestBid}\n\tBidCounter: {auctionDTO.BidCounter}\n\tStartDate: {auctionDTO.StartDate}\n\tEndDate: {auctionDTO.EndDate}\n\tViews: {auctionDTO.Views}\n\tArticleID: {auctionDTO.ArticleID}");

        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500);
        }
        return Ok(auctionDTO);

    }

    // Henter CSV-fil
    [Authorize]
    [HttpGet("modtag")]
    public async Task<IActionResult> ModtagAuctionDTO()
    {
        try
        {
            //Læser indholdet af CSV-fil fra filsti (_filePath)
            var bytes = await System.IO.File.ReadAllBytesAsync(Path.Combine(_filePath, "AuktionsListe.csv"));

            _logger.LogInformation("AuktionsListe.csv fil modtaget");

            // Returnere CSV-filen med indholdet
            return File(bytes, "text/csv", Path.GetFileName(Path.Combine(_filePath, "AuktionsListe.csv")));

        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500);
        }

    }


    [Authorize]
    [HttpGet("version")]
    public IEnumerable<string> Get()
    {
        var properties = new List<string>();
        var assembly = typeof(Program).Assembly;
        foreach (var attribute in assembly.GetCustomAttributesData())
        {
            properties.Add($"{attribute.AttributeType.Name} - {attribute.ToString()}");
            _logger.LogInformation("Version blevet kaldt");
        }
        return properties;

    }

}




