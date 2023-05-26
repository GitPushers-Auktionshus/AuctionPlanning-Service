using System;
using System.Security.Cryptography;
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
        private readonly ILogger<AuctionPlanningController> _logger;
        private readonly IConfiguration _config;

        // Initializes enviroment variables
        private readonly string _connectionURI;

        private readonly string _auctionsDatabase;
        private readonly string _inventoryDatabase;

        private readonly string _listingsCollectionName;
        private readonly string _articleCollectionName;

        // Initializes MongoDB database collection
        private readonly IMongoCollection<Auction> _listingsCollection;
        private readonly IMongoCollection<Article> _articleCollection;

        public MongoDBService(ILogger<AuctionPlanningController> logger, IConfiguration config, EnvVariables vaultSecrets)
        {
            _logger = logger;
            _config = config;

            try
            {
                // Retrieves enviroment variables from program.cs, from injected EnviromentVariables class
                _connectionURI = vaultSecrets.dictionary["ConnectionURI"];

                // Retrieves Auction database and collections
                _auctionsDatabase = config["AuctionsDatabase"] ?? "Auctionsdatabase missing";
                _listingsCollectionName = config["AuctionCollection"] ?? "Auctioncollection name missing";

                // Retrieves Inventory database and collection
                _inventoryDatabase = config["InventoryDatabase"] ?? "Inventorydatabase missing";
                _articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";

                _logger.LogInformation($"AuctionService secrets: ConnectionURI: {_connectionURI}");
                _logger.LogInformation($"AuctionService Database and Collections: Auctiondatabase: {_auctionsDatabase}, Auctionsdatabase: {_listingsCollectionName}");
                _logger.LogInformation($"Inventory Database and Collections: Inventorydatabase: {_auctionsDatabase}, Articlecollection: {_articleCollectionName}");

            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving enviroment variables");

                throw;
            }

            try
            {
                // Sets MongoDB client
                var mongoClient = new MongoClient(_connectionURI);

                // Sets MongoDB Database
                var auctionsDatabase = mongoClient.GetDatabase(_auctionsDatabase);
                var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);

                // Collections
                _listingsCollection = auctionsDatabase.GetCollection<Auction>(_listingsCollectionName);
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
            _logger.LogInformation($"[*] AddAuction(AuctionDTO auctionDTO) called: Adding a new auction to the database\nStartDate: {auctionDTO.StartDate}\nEndDate: {auctionDTO.EndDate}\nArticleID: {auctionDTO.ArticleID}");

            try
            {
                Article auctionArticle = new Article();

                // Finds the article matching the article ID from the article DTO and adds it to an article object
                auctionArticle = await _articleCollection.Find(x => x.ArticleID == auctionDTO.ArticleID).FirstOrDefaultAsync();

                // Returns null if if can't find an article with the matching ID
                if (auctionArticle == null)
                {
                    _logger.LogInformation("Error finding article for auction");

                    throw new Exception();
                }
                else
                {
                    // Creates an auction 
                    Auction auction = new Auction
                    {
                        AuctionID = ObjectId.GenerateNewId().ToString(),
                        HighestBid = 0,
                        Bids = new List<Bid>(),
                        StartDate = auctionDTO.StartDate,
                        EndDate = auctionDTO.EndDate,
                        Views = 0,
                        Article = auctionArticle,
                        Comments = new List<Comment>()
                    };

                    // Adds the auction to the listing collection
                    await _listingsCollection.InsertOneAsync(auction);

                    return auction;
                }
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
            _logger.LogInformation($"[*] GetAllAuctions() called: Fetching all auctions from the database");

            try
            {
                List<Auction> allAuctions = new List<Auction>();

                // Finds all documents in the listing collection and adds them to a list of auction objects
                allAuctions = await _listingsCollection.Find(_ => true).ToListAsync<Auction>();

                if (allAuctions == null)
                {
                    _logger.LogInformation("No auctions found");
                }

                return allAuctions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }

        }

        //GET - Return a list of all active auctions
        public async Task<List<Auction>> GetAllActiveAuctions()
        {
            _logger.LogInformation($"getAllActive endpoint called");

            try
            {
                List<Auction> allAuctions = new List<Auction>();
                List<Auction> activeAuctions = new List<Auction>();

                // Finds all documents in the listing collection and adds them to a list of auction objects
                allAuctions = await _listingsCollection.Find(_ => true).ToListAsync<Auction>();

                if (allAuctions == null)
                {
                    _logger.LogInformation("No auctions found");

                    return null;
                }
                else
                {
                    // Loops through all auctions in the list and adds the currently active ones to a new list
                    foreach (var auction in allAuctions)
                    {
                        // Determines whether they're active or not based on the current date and time
                        if (auction.StartDate <= DateTime.Now && auction.EndDate >= DateTime.Now)
                        {
                            activeAuctions.Add(auction);
                        }
                    }
                    if (activeAuctions == null)
                    {
                        _logger.LogInformation("No active auctions found");

                        return null;
                    }
                    else
                    {
                        _logger.LogInformation($"{activeAuctions.Count} active auctions found");

                        // Return a list of all active auctions added in the above foreach loop
                        return activeAuctions;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed calling getAllActiveAuctions: {ex.Message}");

                throw;
            }

        }

        //GET - Return a list of all categories matching a given categorycode
        public async Task<List<Category>> GetCategory(string categoryCode)
        {
            _logger.LogInformation($"[*] GetCategory(string categoryCode) called: Fetching a list of items in the given category");

            try
            {
                List<Auction> auctions = new List<Auction>();
                List<Category> categories = new List<Category>();

                // Finds all auctions matching the given catogory code and adds them to the auction list
                auctions = await _listingsCollection.Find(x => x.Article.Category == categoryCode).ToListAsync<Auction>();

                if (categories == null)
                {
                    _logger.LogInformation($"No auctions found with categoryCode {categoryCode}");
                }
                else
                {
                    // Loops through all aucitons in the auctions list and creates Category objects from each single one
                    foreach (var auction in auctions)
                    {
                        Category newcategory = new Category
                        {
                            CategoryCode = categoryCode,
                            CategoryName = auction.Article.Name,
                            ItemDescription = auction.Article.Description,
                            AuctionDate = auction.StartDate
                        };

                        _logger.LogInformation($"New category added to list: Code: {newcategory.CategoryCode}, Name: {newcategory.CategoryName}, Description: {newcategory.ItemDescription}, AuctionDate: {newcategory.AuctionDate}");

                        // Adds each object to a list containing Category objects
                        categories.Add(newcategory);
                    }
                }

                return categories;
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
            try
            {
                _logger.LogInformation($"[*] DeleteAuction(string auctionId) called: Deleting an auction with the auctionId: {auctionId}");

                Auction deleteAuction = new Auction();

                // Finds the auction matching the given Article ID in the listing collection
                deleteAuction = await _listingsCollection.Find(x => x.AuctionID == auctionId).FirstOrDefaultAsync();

                if (deleteAuction == null)
                {
                    _logger.LogInformation("No auctions found to be deleted");

                    return null;
                }
                else
                {
                    // Creates a filter, that filtes on the property "AuctionID" and matchin it with the given id in the request
                    FilterDefinition<Auction> filter = Builders<Auction>.Filter.Eq("AuctionID", auctionId);

                    // Deletes the auction in the listing collection using the filter provided from above
                    await _listingsCollection.DeleteOneAsync(filter);

                    _logger.LogInformation($"id got deleted: {auctionId}");

                    return deleteAuction;
                }
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
            _logger.LogInformation($"[*] GetAuctionByID(string auctionId) called: Fetching an auction with auctionId {auctionId}");

            try
            {
                // Find the auction in the listing collection based on the provided id
                Auction auction = await _listingsCollection.Find(x => x.AuctionID == auctionId).FirstOrDefaultAsync();

                if (auction == null)
                {
                    _logger.LogError($"Error finding auction: {auctionId}");

                    // Returns null if the auction isnt found
                    return null;
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
            _logger.LogInformation($"[*] UpdateAuction(string auctionId, AuctionDTO auctionDTO) called: Updating the action with auctionId: {auctionId}");

            try
            {
                // Finds the auction to be updated in the listing collection using the provided ID
                Auction existingAuction = await _listingsCollection.Find(x => x.AuctionID == auctionId).FirstOrDefaultAsync();

                if (existingAuction == null)
                {
                    _logger.LogError($"Error finding auction: {auctionId}");

                    return null;
                }

                // Finds the article in the article collection matching the article ID in the DTO
                Article article = await _articleCollection.Find(x => x.ArticleID == auctionDTO.ArticleID).FirstOrDefaultAsync();

                if (article == null)
                {
                    _logger.LogError($"Error finding article with ID: {auctionDTO.ArticleID}");

                    return null;
                }
                // Update the properties of the existing auction with the values from the DTO
                existingAuction.StartDate = auctionDTO.StartDate;
                existingAuction.EndDate = auctionDTO.EndDate;
                existingAuction.Article = article;

                // Replace the existing auction in the database with the updated auction
                await _listingsCollection.ReplaceOneAsync(x => x.AuctionID == auctionId, existingAuction);

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
