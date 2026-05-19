using Coocking;

namespace InterestGames
{
    public class Makeup_Pomada : Makeup_MakeupItemsBase
    {
        protected override void OnDrop(Dropable_Basic basic)
        {
            controller.pomada[3] = controller.pomada[droppables.IndexOf(basic)];
            basic.DoReset();
            TryWin();
        }
    }
}