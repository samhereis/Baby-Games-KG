using Coocking;

namespace InterestGames
{
    public class Makeup_Rumyana : Makeup_MakeupItemsBase
    {
        protected override void OnDrop(Dropable_Basic basic)
        {
            controller.rumyana[3] = controller.rumyana[droppables.IndexOf(basic)];
            basic.DoReset();
            TryWin();
        }
    }
}