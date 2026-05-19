using Coocking;

namespace InterestGames
{
    public class Makeup_Zakolki : Makeup_MakeupItemsBase
    {
        protected override void OnDrop(Dropable_Basic basic)
        {
            controller.zakolki[3] = controller.zakolki[droppables.IndexOf(basic)];
            basic.DoReset();
            TryWin();
        }
    }
}