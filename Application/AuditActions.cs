namespace SubastaYa.Application
{
    public static class AuditActions
    {
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";

        public const string AUCTION_STATUS_CHANGED = "AUCTION_STATUS_CHANGED";
        public const string AUCTION_TIME_EXTENDED = "AUCTION_TIME_EXTENDED";
        public const string BID_REJECTED = "BID_REJECTED";
        public const string WALLET_MANUAL_CREDIT = "WALLET_MANUAL_CREDIT";
    }
}