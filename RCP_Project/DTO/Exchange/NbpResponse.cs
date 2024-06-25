namespace RCP_Project.DTO.Exchange;

public class NbpResponse
{
    public string Table { get; set; }
    public string Currency { get; set; }
    public string Code { get; set; }
    public List<Rate> Rates { get; set; }
}