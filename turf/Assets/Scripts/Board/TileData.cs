// One instance per board cell. Source of truth for tile state.
// Never drives logic — only stores state that BoardData queries.
public class TileData
{
    public TileType       Type             { get; private set; }
    public BaseMatchProfile Owner          { get; private set; }
    public bool           IsRevealed       { get; private set; }  // ever seen by any player
    public bool           CaveUsed         { get; set; }          // cave teleport consumed
    public bool           WizardUsed       { get; set; }          // wizard global claim consumed
    public bool           BarbarianCharged { get; set; }          // barbarian has already charged

    public bool IsUnclaimed => Owner == null;

    public TileData(TileType type)
    {
        Type = type;
    }

    public void SetOwner(BaseMatchProfile newOwner)
    {
        Owner = newOwner;
    }

    public void Reveal()
    {
        IsRevealed = true;
    }

    // Mountains and Desolate tiles can never be claimed by anyone.
    public bool CanBeClaimed()
    {
        return Type != TileType.Mountain && Type != TileType.Desolate;
    }
}
