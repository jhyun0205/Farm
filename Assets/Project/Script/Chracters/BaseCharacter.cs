using CoreEngine;
using CoreEngine.Actor;
using CoreEngine.Pool;
using Farm.Character.Move;
using Farm.Character.StateMachine;
using Farm.Egg;
using Farm.Fishing;
using UnityEngine;

namespace Farm.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BaseCharacter : BaseActorHostExtended, IActorHost, IPoolable, ITickable, IFixedTickable
    {
        #region featrue
        [SerializeField] protected CharacterAnimFeature animFeature = new();
        [SerializeField] protected CharacterStateController stateController = new();
        [SerializeField] protected CharacterTileChecker tileChecker = new();
        [SerializeField] protected CharacterInventory inventory = new();
        [SerializeField] protected CharacterQuestBook questBook = new();
        [SerializeField] protected CharacterCropDataSheet cropDataSheet = new();
        [SerializeField] protected CharacterEggEncyclopedia eggEncyclopedia = new();
        [SerializeField] protected CharacterActionController actionController = new();
        [SerializeField] protected CharacterMoveFeature moveFeature = new();
        [SerializeField] protected FishModule fishFeature = new();
        #endregion

        public TickGroup TickGroup => TickGroup.Character;

        public FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;

        public IPoolReleaser Releaser { get; set; }

        protected override void RegisterFeatures()
        {
            FeatureHandler.RegisterFeature(animFeature);
            FeatureHandler.RegisterFeature(stateController);
            FeatureHandler.RegisterFeature(tileChecker);
            FeatureHandler.RegisterFeature(inventory);
            FeatureHandler.RegisterFeature(questBook);
            FeatureHandler.RegisterFeature(cropDataSheet);
            FeatureHandler.RegisterFeature(eggEncyclopedia);
            FeatureHandler.RegisterFeature(actionController);
            FeatureHandler.RegisterFeature(moveFeature);
            FeatureHandler.RegisterFeature(fishFeature);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            stateController.StartState();
        }

        public virtual void Tick(float deltaTime)
        {
            FeatureHandler.Tick_InitializedFeatures(deltaTime);
        }

        public virtual void FixedTick(float fixedDeltaTime)
        {
            FeatureHandler.FixedTick_InitializedFeatures(fixedDeltaTime);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            stateController.OnValidate();
        }
    }
}


