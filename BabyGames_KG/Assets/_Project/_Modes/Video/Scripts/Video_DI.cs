using UnityEngine;
using UnityEngine.Video;
using Zenject;

namespace Video
{
    public class Video_DI : MonoInstaller
    {
        [SerializeField] private VideoPlayer _fullScreenRect;
        [SerializeField] private Camera _camera;

        public override void InstallBindings()
        {
            Container.Bind<VideoPlayer>().FromInstance(_fullScreenRect).AsSingle();
            Container.Bind<Camera>().FromInstance(_camera).AsSingle();
        }
    }
}