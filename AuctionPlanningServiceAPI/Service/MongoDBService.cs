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

        private readonly string _auctionDatabase;
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
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving enviroment variables");

                throw;
            }

            _connectionURI = "mongodb://admin:1234@localhost:27018/";

            //Auction database and collections
            _auctionsDatabase = "Auctions";
            _auctionCollectionName = "listings";

            //Inventory database and collection
            _inventoryDatabase = "inventoryDatabase";
            _articleCollectionName = "article";


            _logger.LogInformation($"AuctionService secrets: ConnectionURI: {_connectionURI}");
            _logger.LogInformation($"AuctionService Database and Collections: Auctions: {_auctionsDatabase}, Auctions: {_auctionsDatabase}");
            try
            {
                // Sets MongoDB client
                var mongoClient = new MongoClient(_connectionURI);

                // Sets MongoDB Database
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

        // Adds an auction 
        public async Task<Auction> AddAuction(AuctionDTO auctionDTO)
        {
            try
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

                return auction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error auction: {ex.Message}");

                throw;
            }
        }
        //GET - Return a list of all auctions
        public async Task<List<Auction>> GetAllAuctions()
        {
            _logger.LogInformation($"getAll endpoint called");

            try
            {
                List<Auction> allAuctions = new List<Auction>();

                allAuctions = await _auctionCollection.Find(_ => true).ToListAsync<Auction>();

                return allAuctions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed calling getAllAuctions: {ex.Message}");

                throw;
            }

        }
        //DELETE - Removes an auction
        public async Task<Auction> DeleteAuction(string id)
        {
            _logger.LogInformation($"DeleteAuction called with Auction ID = {id}");

            try
            {
                Auction deleteAuction = new Auction();

                deleteAuction = await _auctionCollection.Find(x => x.AuctionID == id).FirstAsync<Auction>();

                FilterDefinition<Auction> filter = Builders<Auction>.Filter.Eq("AuctionID", id);

                await _auctionCollection.DeleteOneAsync(filter);

                _logger.LogInformation($"id got deleted: {id}");

                return deleteAuction;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed while trying to deleteAuction: {ex.Message}");

                throw;
            }

        }

        // GET - Retrieves an auction by ID
        public async Task<Auction> GetAuctionByID(string id)
        {
           
            try
            {

                _logger.LogInformation($"GET auction called with ID: {id}");

                Auction auction = await _auctionCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

                if (auction == null)
                {
                    _logger.LogError($"Error finding auction: {id}");
                }
                _logger.LogInformation($"id called as: {id}");
                return auction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getAuction: {ex.Message}");
                throw;
            }
        }



        // PUT - Updates an auction
        public async Task<Auction> UpdateAuction(string id, AuctionDTO auctionDTO)
        {
            try
            {
                _logger.LogInformation($"PUT auction called with ID: {id}");

                Auction existingAuction = await _auctionCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

                if (existingAuction == null)
                {
                    _logger.LogError($"Error finding auction: {id}");
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

                return existingAuction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in updateAuction: {ex.Message}");
                throw;
            }
        }
    }

}
