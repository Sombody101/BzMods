using System.Collections.Generic;
using _TechInfo = Nautilus.Crafting.RecipeData;

namespace Common.Crafting;

// intermediate class that used for conversion to/from SMLHelper's TechData (GAME_SN) or RecipeData (GAME_BZ)
class TechInfo
{
    public record Ing(TechType techType, int amount = 1);

    public int craftAmount { get; init; } = 1;
    public List<Ing> ingredients { get; init; } = [];
    public List<TechType> linkedItems { get; init; } = [];

    public TechInfo(params Ing[] ingredients)
    {
        this.ingredients.AddRange(ingredients);
    }

    public TechInfo(TechInfo techInfo) : this([.. techInfo.ingredients])
    {
        craftAmount = techInfo.craftAmount;
        linkedItems.AddRange(techInfo.linkedItems);
    }

    public static implicit operator _TechInfo(TechInfo techInfo)
    {
        _TechInfo result = new()
        {
            craftAmount = techInfo.craftAmount,
            LinkedItems = techInfo.linkedItems
        };

        techInfo.ingredients.ForEach(ing => result.Ingredients.Add(new(ing.techType, ing.amount)));

        return result;
    }

    public static implicit operator TechInfo(_TechInfo techInfo)
    {
        TechInfo result = new()
        {
            craftAmount = techInfo.craftAmount,
            linkedItems = techInfo.LinkedItems
        };

        techInfo.Ingredients.ForEach(ing => result.ingredients.Add(new(ing.techType, ing.amount)));

        return result;
    }
}