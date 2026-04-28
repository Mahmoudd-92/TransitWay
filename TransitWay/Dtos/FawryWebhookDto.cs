namespace TransitWay.Dtos
{
    public class FawryWebhookDto
    {
        public string FawryRefNum { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
