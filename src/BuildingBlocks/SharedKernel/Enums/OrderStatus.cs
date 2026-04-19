namespace SharedKernel.Enums
{
    public enum OrderStatus
    {
        Pending,                

        ReservingInventory,    
        InventoryReserved,     
        InventoryFailed,        
        
        ProcessingPayment,      
        PaymentCompleted,       
        PaymentFailed,          

        Completed,             
        Cancelled
    }
}
