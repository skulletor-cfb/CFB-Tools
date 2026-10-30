namespace MaddenFranchise.Strategies;

/// <summary>
/// Shape required by CommonAlgorithms.Save - implemented by FranchiseFileTable (for whole-file
/// packing) and FranchiseFileTable2Field (for compact-table2 string packing).
/// </summary>
public interface IPatchUnit
{
    bool IsChanged { get; set; }
    int Index { get; }
    int Offset { get; set; }
    byte[] HexData { get; }
    int LengthAtLastSave { get; set; }
}
