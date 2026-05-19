using CarTuning;
using Spine.Unity;
using System.Threading.Tasks;

namespace InterestGames
{
    public class Princess_WinState : Princess_StateBase
    {
        public override Task Enable()
        {
            _model.hasWon.ChangeValue(true);
            return base.Enable();
        }
    }
}