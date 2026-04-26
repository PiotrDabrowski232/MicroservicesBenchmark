namespace SharedKernel.Enums
{
    public enum OrderStatus
    {
        Pending,                

        ReservingInventory,    
        InventoryReserved,     
        InventoryFailed,        
        
        ProcessingPayment,        
        PaymentFailed,          

        Completed
    }
}
