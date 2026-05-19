using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Modes.Puzzle
{
    public class ObjectDivier_Grid : ObjectDividerBase
    {
        [SerializeField] private int vertical = 2;
        [SerializeField] private int horizontal = 2;
        [SerializeField] private Color grayColor;

        public override async Task Divide()
        {
            await base.Divide();

            if (puzzle == null)
            {
                puzzle = FindFirstObjectByType<Puzzle>(FindObjectsInactive.Exclude);
            }

            _spawnedItems = GetComponentsInChildren<PuzzlePiece>(true).ToList();

            puzzle.wholeImage.transform.SetParent(transform, true);
            DoOutlineAnimation();
            await SeparateAsync();
            puzzle.wholeImage.transform.parent = puzzle.transform;
        }

        protected override void Separate()
        {
            puzzle.mainImage[0].color = grayColor;
            base.Separate();
        }
    }
}