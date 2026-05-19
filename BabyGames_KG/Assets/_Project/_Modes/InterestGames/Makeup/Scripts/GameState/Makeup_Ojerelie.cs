using Coocking;

namespace InterestGames
{
    public class Makeup_Ojerelie : Makeup_MakeupItemsBase
    {
        protected override void OnDrop(Dropable_Basic basic)
        {
            controller.ojereliya[3] = controller.ojereliya[droppables.IndexOf(basic)];
            basic.DoReset();
            TryWin();
        }
    }
}