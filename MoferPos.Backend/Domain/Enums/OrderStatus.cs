namespace MoferPOS.Backend.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,      // cart exists locally or created but not finalized
    Completed = 1,  // only these count in analytics
    Voided = 2,
    Refunded = 3
}
