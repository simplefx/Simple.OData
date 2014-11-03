namespace Simple.OData.Client.Tests
{
    public class Order
    {
        public int OrderID { get; set; }

        public OrderDetail[] OrderDetails { get; set; }
    }
}