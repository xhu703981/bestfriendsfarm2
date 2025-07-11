using Terraria;
using Terraria.ModLoader;
using BestFriendsFarm.Systems; // Import your BondingPlayer namespace

public class PetBonusPlayer : ModPlayer
{
    public override void UpdateEquips()
    {
        var bondingPlayer = Player.GetModPlayer<BondingPlayer>();
        int dogType = ModContent.NPCType<BestFriendsFarm.Animals.Dog>();
        int catType = ModContent.NPCType<BestFriendsFarm.Animals.Cat>();
        int cowType = ModContent.NPCType<BestFriendsFarm.Animals.Cow>();
        string dogBond = bondingPlayer.bondLevels.ContainsKey(dogType) && bondingPlayer.bondLevels[dogType].Count > 0
            ? string.Join(", ", bondingPlayer.bondLevels[dogType].Values)
            : "0";
        string catBond = bondingPlayer.bondLevels.ContainsKey(catType) && bondingPlayer.bondLevels[catType].Count > 0
            ? string.Join(", ", bondingPlayer.bondLevels[catType].Values)
            : "0";
        string cowBond = bondingPlayer.bondLevels.ContainsKey(cowType) && bondingPlayer.bondLevels[cowType].Count > 0
            ? string.Join(", ", bondingPlayer.bondLevels[cowType].Values)
            : "0";
        // Main.NewText($"[DEBUG] Dog bond: {dogBond} | Cat bond: {catBond} | Cow bond: {cowBond}");
        // Dog bonus: +5 max life
        int dogBuff = ModContent.BuffType<BestFriendsFarm.Buffs.DogBondBuff>();
        if (bondingPlayer.IsBondedWith(dogType))
        {
            Player.statLifeMax2 += 5;
            Player.AddBuff(dogBuff, 2);
        }
        else
        {
            Player.ClearBuff(dogBuff);
        }
        // Cat bonus: +10% movement speed
        int catBuff = ModContent.BuffType<BestFriendsFarm.Buffs.CatBondBuff>();
        if (bondingPlayer.IsBondedWith(catType))
        {
            Player.moveSpeed += 0.10f;
            Player.AddBuff(catBuff, 2);
        }
        else
        {
            Player.ClearBuff(catBuff);
        }
        // Cow bonus: +6 defense
        int cowBuff = ModContent.BuffType<BestFriendsFarm.Buffs.CowBondBuff>();
        if (bondingPlayer.IsBondedWith(cowType))
        {
            Player.statDefense += 6;
            Player.AddBuff(cowBuff, 2);
        }
        else
        {
            Player.ClearBuff(cowBuff);
        }
    }

    public override void UpdateLifeRegen()
    {
        var bondingPlayer = Player.GetModPlayer<BondingPlayer>();
        // Dog bonus: +10 life regen
        if (bondingPlayer.IsBondedWith(Terraria.ModLoader.ModContent.NPCType<BestFriendsFarm.Animals.Dog>()))
        {
            Player.lifeRegen += 10;
        }
    }
}
