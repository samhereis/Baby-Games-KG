using CustomAttributes;
using Observables;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Modes.Puzzle
{
    public class Puzzle : StateMachineBase, ISelfValidator
    {
        public ObservableValue<bool> isCompleted;

        [field: SerializeField, Re_Fg_Co] public Transform wholeImage { get; set; }

        [field: SerializeField, Re_Fg_Co, Space] public List<SpriteRenderer> mainImage { get; set; } = new();
        [field: SerializeField, Re_Fg_Co] public Sprite mainImage_Sprite { get; set; }


        [field: SerializeField, Re_Fg_Co, Space] public List<PuzzlePiece> puzzles { get; set; } = new();
        [field: SerializeField, Re_Fg_Co] public List<Sprite> puzzles_Sprite { get; set; } = new();

        public AudioSource _soundPlayer;
        [SerializeField] private AudioClip _completeAudio;

        public void Validate(SelfValidationResult result)
        {
            foreach (var sprite in mainImage)
            {
                sprite.sprite = mainImage_Sprite;
            }

            for (int i = 0; i < puzzles_Sprite.Count; i++)
            {
                puzzles[i].SetSprite(puzzles_Sprite[i]);

                puzzles[i].Validate();
            }
        }

        public async Task Initialize()
        {
            foreach (var item in _allStates)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in _allStates)
            {
                await item.PreInittialize();
            }

            ChangeState(_startState);
        }

        public void PlayFinishAudio()
        {
            if (_completeAudio != null)
            {
                _soundPlayer.loop = false;
                _soundPlayer.clip = _completeAudio;
                _soundPlayer.Play();
            }
        }

        public void Complete()
        {
            isCompleted.value = true;
        }
    }
}