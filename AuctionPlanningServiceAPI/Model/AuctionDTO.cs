using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.ConstrainedExecution;

namespace AuctionPlanningServiceAPI.Model
{
    public class AuctionDTO
    {
        public int HighestBid { get; set; }
        public int BidCounter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Views { get; set; }
        public string ArticleID { get; set; }

        public AuctionDTO()
        {
        }
    }
}

