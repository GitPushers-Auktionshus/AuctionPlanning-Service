using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace AuctionPlanningServiceAPI.Model
{

    public class Auction
    {
        [BsonId]
        public string AuctionID { get; set; }
        public int HighestBid { get; set; }
        public int BidCounter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Views { get; set; }
        public int ArticleID { get; set; }
        public List<Auction> AuctionList { get; set; }
        public Auction(string auctionID, int highestBid, int bidCounter, DateTime startDate, DateTime endDate, int views, int articleID)
        {
            this.AuctionID = auctionID;
            this.HighestBid = highestBid;
            this.BidCounter = bidCounter;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Views = views;
            this.ArticleID = articleID;
        }

        public Auction()
        {
        }
    }
}
