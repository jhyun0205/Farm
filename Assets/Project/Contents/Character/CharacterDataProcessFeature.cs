using CoreEngine.Actor;
using CoreEngine.Facades;
using CoreEngine.GameData;
// 질?문
namespace Farm.Character
{
    public class CharacterDataProcessFeature<TTable, TRecord> : BaseActorFeature, IActorFeature
        where TTable : BaseDataTable<TRecord> where TRecord : class, IDataRecord, new()
    {
        protected DataTableHandler<TTable, TRecord> _handler;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            GameDataManager manager = CoreFacade.GetManager<GameDataManager>();

            _handler = new (manager.GetTable<TTable>());
        }

        protected TRecord GetRecord(int id)
        {
            return _handler.GetRecord(id);
        }
    }
}

