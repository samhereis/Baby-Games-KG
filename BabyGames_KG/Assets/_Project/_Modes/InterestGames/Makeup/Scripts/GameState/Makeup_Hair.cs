using Coocking;
using System.Threading.Tasks;
using UnityEngine;

namespace InterestGames
{
    public class Makeup_Hair : Makeup_MakeupItemsBase
    {
        public Transform pannel;

        public override async Task Enter()
        {
            await pannel_world.Appear();
            await base.Enter();
        }

        protected override void OnDrop(Dropable_Basic basic)
        {
            base.OnDrop(basic);
            basic.DoReset();

            controller.hairIndex = droppables.IndexOf(basic);

            TryWin();
        }
    }
}