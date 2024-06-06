namespace StardewTestMod1;

public class PlayerInfo
{
    public Dictionary<int, ClothSet> ClothSets = new Dictionary<int, ClothSet>();
    public int SelectedClothSetIndex;
    
    public PlayerInfo(ClothSet clothSet, int index = 0)
    {
        this.ClothSets.Add(0, clothSet);
        this.SelectedClothSetIndex = 0;
    }
}