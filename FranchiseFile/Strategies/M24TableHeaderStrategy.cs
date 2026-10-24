namespace MaddenFranchise.Strategies;

/// <summary>Port of strategies/common/header/m24/M24TableHeaderStrategy.js.</summary>
public static class M24TableHeaderStrategy
{
    public static TableHeader ParseHeader(byte[] data)
    {
        var header = M20TableHeaderStrategy.ParseHeader(data);
        header.Table3Length = header.Data1Pad3;
        header.HasThirdTable = header.Table3Length > 0;
        header.Table3StartIndex = header.Table2StartIndex + header.Table2Length;
        return header;
    }
}
