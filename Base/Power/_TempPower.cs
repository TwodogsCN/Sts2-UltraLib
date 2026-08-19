using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace UltraLib.Base.Power;

public abstract class TempPower : CustomPowerModel, ITemporaryPower
{
	private bool _shouldIgnoreNextInstance;

	public override PowerType Type
	{
		get
		{
			if (!IsPositive)
			{
				return PowerType.Debuff;
			}
			return PowerType.Buff;
		}
	}

	public override PowerStackType StackType => PowerStackType.Counter;

	public abstract AbstractModel OriginModel { get; }

	public abstract PowerModel InternallyAppliedPower { get; }

	protected virtual bool IsPositive => true;

	private int Sign
	{
		get
		{
			if (!IsPositive)
			{
				return -1;
			}
			return 1;
		}
	}

	public override LocString Title
	{
		get
		{
			AbstractModel originModel = OriginModel;
			if (!(originModel is CardModel cardModel))
			{
				if (!(originModel is PotionModel potionModel))
				{
					if (originModel is RelicModel relicModel)
					{
						return relicModel.Title;
					}
					throw new InvalidOperationException();
				}
				return potionModel.Title;
			}
			return cardModel.TitleLocString;
		}
	}

	public override LocString Description => new LocString("powers", IsPositive ? "TEMPORARY_POWER_UP.description" : "TEMPORARY_POWER_DOWN.description");

	protected override string SmartDescriptionLocKey
	{
		get
		{
			if (!IsPositive)
			{
				return "TEMPORARY_POWER_DOWN.smartDescription";
			}
			return "TEMPORARY_POWER_UP.smartDescription";
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			var list = new List<IHoverTip>();
			IEnumerable<IHoverTip> collection;
			var originModel = OriginModel;

			if (originModel is CardModel card)
			{
				collection = new[] { HoverTipFactory.FromCard(card) };
			}
			else if (originModel is PotionModel potion)
			{
				collection = new[] { HoverTipFactory.FromPotion(potion) };
			}
			else if (originModel is RelicModel relic)
			{
				collection = HoverTipFactory.FromRelic(relic);
			}
			else
			{
				throw new InvalidOperationException();
			}

			list.AddRange(collection);
			list.Add(HoverTipFactory.FromPower<StrengthPower>());

			return list.AsReadOnly();
		}
	}

	public void IgnoreNextInstance()
	{
		_shouldIgnoreNextInstance = true;
	}

	public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (_shouldIgnoreNextInstance)
		{
			_shouldIgnoreNextInstance = false;
		}
		else
		{
			PowerModel powerModel = InternallyAppliedPower;
			await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), powerModel.ToMutable(), target, (decimal)Sign * amount, applier, cardSource, silent: true);
		}
	}
	
	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
		CardModel? cardSource)
	{
		if (amount != Amount && power == this)
		{
			if (_shouldIgnoreNextInstance)
			{
				_shouldIgnoreNextInstance = false;
			}
			else
			{
				PowerModel powerModel = InternallyAppliedPower;
				await PowerCmd.Apply(choiceContext, powerModel.ToMutable(), Owner, (decimal)Sign * amount, applier, cardSource, silent: true);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == Owner.Side)
		{
			Flash();
			await PowerCmd.Remove(this);
			await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, -Sign * Amount, Owner, null);
		}
	}
}
