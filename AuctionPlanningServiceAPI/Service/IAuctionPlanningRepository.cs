using System;
using AuctionPlanningServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;


namespace AuctionPlanningServiceAPI.Service
{
    public interface IAuctionPlanningRepository
    {
        public Task<Auction> AddAuction(AuctionDTO auctionDTO);

        public Task<List<Auction>> GetAllAuctions();

        public Task<List<Category>> GetCategory(string categoryCode);

        public Task<Auction> DeleteAuction(string id);

        public Task<Auction> GetAuctionByID(string id);

        public Task<Auction> UpdateAuction(string id, AuctionDTO auctionDTO);

    }
}