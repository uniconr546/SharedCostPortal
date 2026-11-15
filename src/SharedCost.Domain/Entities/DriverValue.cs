namespace SharedCost.Domain.Entities;

public class DriverValue
{
    public string MaDriverValue { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public string MaTarget { get; set; } = string.Empty;
    public string KyPhanBo { get; set; } = string.Empty;
    public decimal GiaTriDriver { get; set; }
    public string? DonViTinh { get; set; }
    public string? NguonDuLieu { get; set; }

    public virtual Driver? Driver { get; set; }
    public virtual Target? Target { get; set; }
    public virtual ICollection<SimulationResult> SimulationResults { get; set; } = new List<SimulationResult>();
    public virtual ICollection<AllocationResult> AllocationResults { get; set; } = new List<AllocationResult>();
}
