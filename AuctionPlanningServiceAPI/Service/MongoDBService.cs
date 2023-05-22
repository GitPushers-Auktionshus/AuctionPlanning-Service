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

        private readonly string _connectionURI;

        private readonly string _auctionsDatabase;
        private readonly string _inventoryDatabase;

        private readonly string _listingsCollectionName;
        private readonly string _articleCollectionName;


        private readonly IMongoCollection<Auction> _listingsCollection;
        private readonly IMongoCollection<Article> _articleCollection;

        public MongoDBService(ILogger<AuctionPlanningController> logger, IConfiguration config, EnviromentVariables vaultSecrets)
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
            try
            {
                _logger.LogInformation($"POST: addAuction called, StartDate: {auctionDTO.StartDate}, EndDate: {auctionDTO.EndDate}, ArticleID: {auctionDTO.ArticleID}");

                Article auctionArticle = new Article();

                auctionArticle = await _articleCollection.Find(x => x.ArticleID == auctionDTO.ArticleID).FirstOrDefaultAsync();

                if (auctionArticle == null)
                {
                    _logger.LogInformation("Error finding article for auction");
                    return null;
                }
                else
                {
                    Auction auction = new Auction
                    {
                        AuctionID = ObjectId.GenerateNewId().ToString(),
                        HighestBid = 0,
                        Bids = new List<Bid>(),
                        StartDate = auctionDTO.StartDate,
                        EndDate = auctionDTO.EndDate,
                        Views = 0,
                        Article = auctionArticle,
                    };

                    await _listingsCollection.InsertOneAsync(auction);

                    return auction;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in add auction: {ex.Message}");

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

                allAuctions = await _listingsCollection.Find(_ => true).ToListAsync<Auction>();

                if (allAuctions == null)
                {
                    _logger.LogInformation("No auctions found");
                }

                return allAuctions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed calling getAllAuctions: {ex.Message}");

                throw;
            }

        }

        //GET - Return a list of all categories matching a given categorycode
        public async Task<List<Category>> GetCategory(string categoryCode)
        {
            _logger.LogInformation($"getCategory endpoint called");

            try
            {
                List<Auction> auctions = new List<Auction>();
                List<Category> categories = new List<Category>();

                auctions = await _listingsCollection.Find(x => x.Article.Category == categoryCode).ToListAsync<Auction>();

                if (categories == null)
                {
                    _logger.LogInformation($"No auctions found with categoryCode {categoryCode}");
                }
                else
                {
                    foreach (var auction in auctions)
                    {
                        Category newcategory = new Category
                        {
                            CategoryCode = categoryCode,
                            CategoryName = auction.Article.Name,
                            ItemDescription = auction.Article.Description,
                            AuctionDate = auction.StartDate
                        };

                        categories.Add(newcategory);
                    }
                }

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed calling getCategory: {ex.Message}");

                throw;
            }

        }

        //DELETE - Removes an auction
        public async Task<Auction> DeleteAuction(string id)
        {
            try
            {
                _logger.LogInformation($"DeleteAuction called with Auction ID = {id}");

                Auction deleteAuction = new Auction();

                deleteAuction = await _listingsCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

                if (deleteAuction == null)
                {
                    _logger.LogInformation("No auctions found to be deleted");

                    return null;
                }
                else
                {
                    FilterDefinition<Auction> filter = Builders<Auction>.Filter.Eq("AuctionID", id);

                    await _listingsCollection.DeleteOneAsync(filter);

                    _logger.LogInformation($"id got deleted: {id}");

                    return deleteAuction;
                }
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

                Auction auction = await _listingsCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

                if (auction == null)
                {
                    _logger.LogError($"Error finding auction: {id}");

                    return null;
                }

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

                Auction existingAuction = await _listingsCollection.Find(x => x.AuctionID == id).FirstOrDefaultAsync();

                if (existingAuction == null)
                {
                    _logger.LogError($"Error finding auction: {id}");

                    return null;
                }

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
                await _listingsCollection.ReplaceOneAsync(x => x.AuctionID == id, existingAuction);

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
