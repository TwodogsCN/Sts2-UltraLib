using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using UltraLib.Base.Label.Card;
using UltraLib.Base.Utils;
using UltraLib.Variables;

namespace UltraLib.Test;

[Pool(typeof(TokenCardPool))]
public sealed class TestCard : CustomCardModel
{
    public TestCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get { yield break; }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return PlusCardKeyWord.InTest;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new ReturnVar(4m);
            yield return new EmpowerVar(ModelDb.Power<DexterityPower>().ToMutable(), 5);
            yield break;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureHelper.GainBlock(Owner.Creature, 10m, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        CardHelper.ApplyKeyword(this, [PlusCardKeyWord.Isomorphism]);
        this.AddReturnVar(999m);
    }

    //public override string PortraitPath => "";
}