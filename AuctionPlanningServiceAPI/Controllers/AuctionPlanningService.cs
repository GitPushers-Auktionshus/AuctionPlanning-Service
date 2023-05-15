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
using static System.Net.Mime.MediaTypeNames;
using MongoDB.Driver;
using AuctionPlanningServiceAPI.Model;
using MongoDB.Bson;

namespace AuctionPlanningServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionPlanningServiceController : ControllerBase
{
    private readonly ILogger<AuctionPlanningServiceController> _logger;

    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _connectionURI;

    private readonly string _auctionDatabase;
    private readonly string _auctionsDatabase;

    private readonly string _inventoryDatabase;

    private readonly string _auctionCollectionName;

    private readonly string _articleCollectionName;


    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly IMongoCollection<Article> _articleCollection;
    private readonly IConfiguration _config;

    public AuctionPlanningServiceController(ILogger<AuctionPlanningServiceController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        _secret = config["Secret"] ?? "Secret missing";
        _issuer = config["Issuer"] ?? "Issue'er missing";
        _connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";

        /*
        // Auction database and collections
        _auctionsDatabase = config["AuctionsDatabase"] ?? "Auctionsdatabase missing";
        _auctionCollectionName = config["AuctionCollection"] ?? "Auctioncollection name missing";


        // Inventory database and collection
        _inventoryDatabase = config["InventoryDatabase"] ?? "Inventorydatabase missing";
        _articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";

        _logger.LogInformation($"AuctionService secrets: ConnectionURI: {_connectionURI}");
        _logger.LogInformation($"AuctionService Database and Collections: Auctiondatabase: {_auctionsDatabase}, Auctionsdatabase: {_auctionsDatabase}");
        */

        _inventoryDatabase = "inventoryDatabase";
        _articleCollectionName = "article";

        _auctionsDatabase = "auctionsDatabase";
        _auctionCollectionName = "Auction";

        try
        {
            // Client
            var mongoClient = new MongoClient(_connectionURI);

            // Databases
            var auctionsDatabase = mongoClient.GetDatabase(_auctionsDatabase);
            var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);

            // Collections
            _auctionCollection = auctionsDatabase.GetCollection<Auction>(_auctionCollectionName);
            _articleCollection = inventoryDatabase.GetCollection<Article>(_articleCollectionName);


        }
        catch (Exception ex)
        {
            _logger.LogError($"Error trying to connect to database: {ex.Message}");
            throw;
        }
    }

    //POST - Adds a new article
    [HttpPost("addAuction")]
    public async Task<IActionResult> AddAuction(AuctionDTO auctionDTO)
    {
        _logger.LogInformation($"POST: addAuction kaldt, HighestBid: {auctionDTO.HighestBid}, BidCounter: {auctionDTO.BidCounter}, StartDate: {auctionDTO.StartDate}, EndDate: {auctionDTO.EndDate}, Views: {auctionDTO.Views}, ArticleID: {auctionDTO.ArticleID}");


        Auction auction = new Auction
        {
            AuctionID = ObjectId.GenerateNewId().ToString(),
            HighestBid = auctionDTO.HighestBid,
            BidCounter = auctionDTO.BidCounter,
            StartDate = auctionDTO.StartDate,
            EndDate = auctionDTO.EndDate,
            Views = auctionDTO.Views,
            ArticleID = auctionDTO.ArticleID,
            AuctionList = new List<Auction>(),
        };


        await _auctionCollection.InsertOneAsync(auction);

        return Ok(auction);
    }


}



