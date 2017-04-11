using SportsStore.Domain.Entities;

namespace SportsStore.Domain.Abstract
{
    public interface IOderProcessor
    {
        void ProcessOrder(Cart cart, ShippingDetails shippingDetails);
    }
}