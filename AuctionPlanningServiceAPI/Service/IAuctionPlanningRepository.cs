using System;
using AuctionPlanningServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;


namespace AuctionPlanningServiceAPI.Service
{
    public interface IAuctionPlanningRepository
    {
        /// <summary>
        /// Adds an auction to the database
        /// </summary>
        /// <param name="auctionDTO"></param>
        /// <returns>The auction created</returns>
        public Task<Auction> AddAuction(AuctionDTO auctionDTO);

        /// <summary>
        /// Gets a list containing all auctions
        /// </summary>
        /// <returns>A list of all auctions</returns>
        public Task<List<Auction>> GetAllAuctions();

        /// <summary>
        /// Gets a list containing all active auctions
        /// </summary>
        /// <returns>A list of all active auctions</returns>
        public Task<List<Auction>> GetAllActiveAuctions();

        /// <summary>
        /// Gets a list of all items within the provided category code
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <returns>A list with articles matching the category code</returns>
        public Task<List<Category>> GetCategory(string categoryCode);

        /// <summary>
        /// Deletes a selected auction based on an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The deleted article</returns>
        public Task<Auction> DeleteAuction(string id);

        /// <summary>
        /// Gets a specific auction based on a provided ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The auction matching with the ID</returns>
        public Task<Auction> GetAuctionByID(string id);

        /// <summary>
        /// Updates a specific auction based on a provided ID and a DTO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auctionDTO"></param>
        /// <returns>The updated auction</returns>
        public Task<Auction> UpdateAuction(string id, AuctionDTO auctionDTO);

    }
}