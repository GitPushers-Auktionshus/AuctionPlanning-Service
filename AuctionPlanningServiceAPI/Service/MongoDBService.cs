using System;
using AuctionPlanningServiceAPI.Controllers;
using AuctionPlanningServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using static System.Net.Mime.MediaTypeNames;


namespace AuctionPlanningServiceAPI.Service
{

    // Inherits from our interface - can be changed to eg. a SQL database
    public class MongoDBService : IAuctionPlanningRepository

    {
        private readonly ILogger<AuctionPlanningServiceController> _logger;

        private readonly IConfiguration _config;

        private readonly string _connectionURI;

        private readonly string _auctionsDatabase;

        private readonly string _inventoryDatabase;

        private readonly string _auctionCollectionName;

        private readonly string _articleCollectionName;

        private readonly IMongoCollection<Auction> _auctionCollection;
        private readonly IMongoCollection<Article> _articleCollection;

        public MongoDBService(ILogger<AuctionPlanningServiceController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            try
            {
        
            // // Auction database and collections
            // _auctionsDatabase = config["AuctionsDatabase"] ?? "Auctionsdatabase missing";
            // _auctionCollectionName = config["AuctionCollection"] ?? "Auctioncollection name missing";


            // // Inventory database and collection
            // _inventoryDatabase = config["InventoryDatabase"] ?? "Inventorydatabase missing";
            // _articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";

            // _logger.LogInformation($"AuctionService secrets: ConnectionURI: {_connectionURI}");
            // _logger.LogInformation($"AuctionService Database and Collections: Auctiondatabase: {_auctionsDatabase}, Auctionsdatabase: {_auctionsDatabase}");

            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving environment variables");

                throw;
            }

            _connectionURI = "mongodb://admin:1234@localhost:27018/";

            //Auction database and collections
            _auctionsDatabase = "Auctions";
            _auctionCollectionName = "listings";

            //Inventory database and collection
            _inventoryDatabase = "inventoryDatabase";
            _articleCollectionName = "article";

            try
            {
                // Sets MongoDB client
                var mongoClient = new MongoClient(_connectionURI);
                _logger.LogInformation($"[*] CONNECTION_URI: {_connectionURI}");


                // Sets MongoDB Database
                var auctionsDatabase = mongoClient.GetDatabase(_auctionsDatabase);
                _logger.LogInformation($"[*] DATABASE: {_auctionsDatabase}");

                var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);
                _logger.LogInformation($"[*] DATABASE: {_inventoryDatabase}");


                // Collections
                _auctionCollection = auctionsDatabase.GetCollection<Auction>(_auctionCollectionName);
                _logger.LogInformation($"[*] COLLECTION: {_auctionCollectionName}");

                _articleCollection = inventoryDatabase.GetCollection<Article>(_articleCollectionName);
                _logger.LogInformation($"[*] COLLECTION: {_articleCollectionName}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error trying to connect to database: {ex.Message}");

                throw;
            }
        }

        // Adds an auction 
        public async Task<Auction> AddAuction(AuctionDTO auctionDTO)
        {
            try
            {
                _logger.LogInformation($"[*]: AddAuction(AuctionDTO auctionDTO) called: Adding a new auction instance to the database.\nHighestBid: {auctionDTO.HighestBid}\nBidCounter: {auctionDTO.BidCounter}\nStartDate: {auctionDTO.StartDate}\nEndDate: {auctionDTO.EndDate}\nViews: {auctionDTO.Views}\nArticleID: {auctionDTO.ArticleID}");

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

                return auction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }
        //GET - Return a list of all auctions
        public async Task<List<Auction>> GetAllAuctions()
        {
            _logger.LogInformation($"[*] GettAllAuctions() called: Fetching all auctions in the database");

            try
            {
                List<Auction> allAuctions = new List<Auction>();

                allAuctions = await _auctionCollection.Find(_ => true).ToListAsync<Auction>();

                return allAuctions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }

        }
        //DELETE - Removes an auction
        public async Task<Auction> DeleteAuction(string auctionId)
        {
            _logger.LogInformation($"[*] DeleteAuction(string auctionId) called: Deleting auction in the database with auctionId {auctionId}");

            try
            {
                Auction deleteAuction = new Auction();

                deleteAuction = await _auctionCollection.Find(x => x.AuctionID == auctionId).FirstAsync<Auction>();

                FilterDefinition<Auction> filter = Builders<Auction>.Filter.Eq("AuctionID", auctionId);

                await _auctionCollection.DeleteOneAsync(filter);

                _logger.LogInformation($"Auction with id {auctionId} got deleted");

                return deleteAuction;

            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }

        }

        // GET - Retrieves an auction by ID
        public async Task<Auction> GetAuctionByID(string auctionId)
        {
            _logger.LogInformation($"[*] GetAuctionByID(string auctionId) called: Fetching auction information from auctionId {auctionId}");

            try
            {
                Auction auction = await _auctionCollection.Find(x => x.AuctionID == auctionId).FirstOrDefaultAsync();

                if (auction == null)
                {
                    _logger.LogError($"Error finding auction: {auctionId}");
                }

                return auction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }



        // PUT - Updates an auction
        public async Task<Auction> UpdateAuction(string auctionId, AuctionDTO auctionDTO)
        {
            _logger.LogInformation($"[*] UpdateAuction(string auctionId, AuctionDTO auctionDTO) called: Updating the auction with auctiondId {auctionId}");

            try
            {
                Auction existingAuction = await _auctionCollection.Find(x => x.AuctionID == auctionId).FirstOrDefaultAsync();

                if (existingAuction == null)
                {
                    _logger.LogError($"Error finding auction: {auctionId}");
                }

                // Update the properties of the existing auction with the values from the DTO
                existingAuction.HighestBid = auctionDTO.HighestBid;
                existingAuction.BidCounter = auctionDTO.BidCounter;
                existingAuction.StartDate = auctionDTO.StartDate;
                existingAuction.EndDate = auctionDTO.EndDate;
                existingAuction.Views = auctionDTO.Views;
                existingAuction.ArticleID = auctionDTO.ArticleID;

                // Replace the existing auction in the database with the updated auction
                await _auctionCollection.ReplaceOneAsync(x => x.AuctionID == auctionId, existingAuction);

                return existingAuction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");
                
                throw;
            }
        }
    }

}
