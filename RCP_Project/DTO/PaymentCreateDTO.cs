namespace RCP_Project.DTO;

public class PaymentCreateDTO
{
    public int ContractID { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}