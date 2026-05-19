using Coocking;

namespace InterestGames
{
    public class Makeup_Obodok : Makeup_MakeupItemsBase
    {
        protected override void OnDrop(Dropable_Basic basic)
        {
            controller.obodok[3] = controller.obodok[droppables.IndexOf(basic)];
            basic.DoReset();
            TryWin();
        }
    }
}