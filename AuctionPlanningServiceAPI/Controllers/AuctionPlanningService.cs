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

        //Auction database and collections
        _auctionsDatabase = "auctionsDatabase";
        _auctionCollectionName = "Auction";

        //Inventory database and collection
        _inventoryDatabase = "inventoryDatabase";
        _articleCollectionName = "article";


        _logger.LogInformation($"AuctionService secrets: ConnectionURI: {_connectionURI}");
        _logger.LogInformation($"AuctionService Database and Collections: Auctiondatabase: {_auctionsDatabase}, Auctionsdatabase: {_auctionsDatabase}");
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

    //POST - Adds a new auction
    [HttpPost("addAuction")]
    public async Task<IActionResult> AddAuction(AuctionDTO auctionDTO)
    {
        _logger.LogInformation($"POST: addAuction called, HighestBid: {auctionDTO.HighestBid}, BidCounter: {auctionDTO.BidCounter}, StartDate: {auctionDTO.StartDate}, EndDate: {auctionDTO.EndDate}, Views: {auctionDTO.Views}, ArticleID: {auctionDTO.ArticleID}");


        Auction auction = new Auction
        {
            AuctionID = ObjectId.GenerateNewId().ToString(),
            HighestBid = auctionDTO.HighestBid,
            BidCounter = auctionDTO.BidCounter,
            StartDate = auctionDTO.StartDate,
            EndDate = auctionDTO.EndDate,
            Views = auctionDTO.Views,
            ArticleID = auctionDTO.ArticleID,
        };


        await _auctionCollection.InsertOneAsync(auction);

        return Ok(auction);
    }

    //GET - Return a list of all auctions
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAllAuctions()
    {
        _logger.LogInformation($"getAll endpoint called");

        try
        {
            List<Auction> allAuctions = new List<Auction>();

            allAuctions = await _auctionCollection.Find(_ => true).ToListAsync<Auction>();

            return Ok(allAuctions);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed calling getAllAuctions: {ex.Message}");

            throw;
        }

    }

    //DELETE - Removes an auction
    [HttpDelete("deleteAuction/{id}")]
    public async Task<IActionResult> DeleteAuction(string id)
    {
        try
        {
            _logger.LogInformation($"DELETE auction called with id: {id}");

            Auction deleteAuction = new Auction();

            deleteAuction = await _auctionCollection.Find(x => x.AuctionID == id).FirstAsync<Auction>();

            FilterDefinition<Auction> filter = Builders<Auction>.Filter.Eq("AuctionID", id);

            await _auctionCollection.DeleteOneAsync(filter);

            _logger.LogInformation($"id got deleted: {id}");

            return Ok(deleteAuction);

        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed while trying to deleteAuction: {ex.Message}");

            throw;
        }

    }

    // GET - Retrieves an auction by ID
    [HttpGet("getAuction/{id}")]
    public async Task<IActionResult> GetAuction(string id)
    {
        try
        {
            _logger.LogInformation($"GET auction called with ID: {id}");

            Auction auction = await _auctionCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

            if (auction == null)
            {
                return NotFound();
            }
            _logger.LogInformation($"id called as: {id}");
            return Ok(auction);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in getAuction: {ex.Message}");
            throw;
        }
    }

    // PUT - Updates an auction
    [HttpPut("updateAuction/{id}")]
    public async Task<IActionResult> UpdateAuction(string id, AuctionDTO auctionDTO)
    {
        try
        {
            _logger.LogInformation($"PUT auction called with ID: {id}");

            Auction existingAuction = await _auctionCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

            if (existingAuction == null)
            {
                return NotFound();
            }

            // Update the properties of the existing auction with the values from the DTO
            existingAuction.HighestBid = auctionDTO.HighestBid;
            existingAuction.BidCounter = auctionDTO.BidCounter;
            existingAuction.StartDate = auctionDTO.StartDate;
            existingAuction.EndDate = auctionDTO.EndDate;
            existingAuction.Views = auctionDTO.Views;
            existingAuction.ArticleID = auctionDTO.ArticleID;

            // Replace the existing auction in the database with the updated auction
            await _auctionCollection.ReplaceOneAsync(x => x.AuctionID == id, existingAuction);

            _logger.LogInformation($"Updated auction called with ID: {id}");

            return Ok(existingAuction);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in updateAuction: {ex.Message}");
            throw;
        }
    }


}



