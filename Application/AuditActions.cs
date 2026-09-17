namespace SubastaYa.Application
{
    public enum AuditAction
    {
        CREATE,
        UPDATE,
        DELETE,
        AUCTION_STATUS_CHANGED,
        AUCTION_TIME_EXTENDED,
        BID_REJECTED,
        WALLET_MANUAL_CREDIT,
        WALLET_WITHDRAWAL
    }
}