namespace RCP_Project.DTO;

public class ContractCreateDTO
{
    public int ClientID { get; set; }
    public int SoftwareID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int SupportExtensionYears { get; set; }
}