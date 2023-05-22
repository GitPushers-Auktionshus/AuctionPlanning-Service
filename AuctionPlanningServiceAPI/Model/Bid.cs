using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace AuctionPlanningServiceAPI.Model
{
    public class Bid
    {
        [BsonId]
        public string BidID { get; set; }
        [BsonElement]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        public int Price { get; set; }
        public User Bidder { get; set; }

        public Bid()
        {
        }
    }
}
