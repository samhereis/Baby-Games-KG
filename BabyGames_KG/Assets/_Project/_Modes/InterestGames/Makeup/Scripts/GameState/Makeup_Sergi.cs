using Coocking;

namespace InterestGames
{
    public class Makeup_Sergi : Makeup_MakeupItemsBase
    {
        protected override void OnDrop(Dropable_Basic basic)
        {
            controller.sergi[3] = controller.sergi[droppables.IndexOf(basic)];
            basic.DoReset();
            TryWin();
        }
    }
}