using _Project._Modes.Orchestra.Scripts.SO;
using DataClasses;
using Helpers;
using Modes.Puzzle;
using Services;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Identifiers
{
    public class PuzzleActivity_Identifier : _ActivityBase_Identifier
    {
        [SerializeField] private List<Puzzle> _puzzles = new();

        [Inject] private Puzzle_Data _data;

        private Gameplay_GameState_Puzzle_Model _model;

        public void Construct(Gameplay_GameState_Puzzle_Model model)
        {
            _model = model;
        }

        private void Awake()
        {
            DiService.Inject(this);
        }

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = ActivityType.Puzzle;
        }

        [Button]
        public async Task Initialize()
        {
            _puzzles = TryGetAll_List<Puzzle>();

            if (_puzzles.Exists(x => x.isCompleted.value == false))
            {
                var puzzle = _puzzles.First(x => x.isCompleted.value == false);
                foreach (var item in _puzzles)
                {
                    item.gameObject.SetActive(item == puzzle);
                }

                puzzle.isCompleted.AddListener(OnAPuzzleCompleted);
                await puzzle.Initialize();
            }
            else
            {
                _model?.hasWon.ChangeValue(true);
            }
        }

        private async void OnAPuzzleCompleted(bool isCompleted)
        {
            var confetti = await _data.confetti.InstantiateAsync();
            confetti.transform.position = transform.position;
            confetti.Play();
            await AsyncHelper.NextFrame();

            await Initialize();
        }
    }
}