using Observables;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    public class Coloring_DI : MonoInstaller
    {
        [SerializeField] private GameController _gameController;

        [SerializeField] protected ObservableValue<Paintable_Identifier_Basic> _tapObject = new("TapObject");

        public override void InstallBindings()
        {
            Container.Bind<GameController>().FromInstance(_gameController);
            Container.Bind<ObservableValue<Paintable_Identifier_Basic>>().FromInstance(_tapObject);
        }
    }
}