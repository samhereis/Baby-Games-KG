using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Coloring.ForParents
{
    public class ForParents_TabButton : MonoBehaviour
    {
        [field: SerializeField] public Button button { get; private set; }

        [SerializeField][FoldoutGroup("Components")] private Image _selectionImage;
        [SerializeField][FoldoutGroup("Components")] private TMP_Text _textNameSelection;
        [SerializeField][FoldoutGroup("Components")] private Image _iconSelection;

        [SerializeField][FoldoutGroup("Settings")] private Color _primaryImage;
        [SerializeField][FoldoutGroup("Settings")] private Color _secondaryImage;

        public void EnableSelection()
        {
            _selectionImage.transform.DOScale(1, 0.25f).SetEase(Ease.InOutBack);
            _textNameSelection.color = _secondaryImage;
            _iconSelection.color = _secondaryImage;
        }

        public void DisableSelection()
        {
            _selectionImage.transform.DOScale(0, 0.25f).SetEase(Ease.InOutBack);
            _textNameSelection.color = _primaryImage;
            _iconSelection.color = _primaryImage;
        }
    }
}