using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Coloring.CheckForParents
{
    public class MathController : MonoBehaviour
    {
        private Action<bool> _onMathCompleted;

        [SerializeField] private TMP_Text _firstNumberText;
        [SerializeField] private TMP_Text _secondNumberText;
        [SerializeField] private TMP_Text _answerText;

        public bool hasInput = false;
        private int _input = -1;

        private int _answer { get => _leftNumber + _rightNumber; }
        private int _leftNumber { set; get; }
        private int _rightNumber { set; get; }

        public void GenerateMathTask(Action<bool> onMathCompleted)
        {
            _answerText.text = "";

            _input = -1;
            _onMathCompleted = onMathCompleted;

            int random1 = UnityEngine.Random.Range(1, 90);
            int random2 = UnityEngine.Random.Range(1, 9);
            var listOfNumbers = new List<int> { random1, random2 };

            _leftNumber = listOfNumbers[0];
            _rightNumber = listOfNumbers[1];

            _firstNumberText.text = _leftNumber.ToString();
            _secondNumberText.text = _rightNumber.ToString();
        }

        public void ClearTask()
        {
            _onMathCompleted = null;
        }

        public IEnumerator ShowTaskAgain(Action onFinishAnimation)
        {
            _answerText.text = "";
            yield return new WaitForSeconds(0.3f);
            onFinishAnimation();
        }

        public IEnumerator CorrectAnswerAnimation(Action OnAnimationEnded)
        {
            _answerText.transform.DOScale(1.1f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                _answerText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            });

            yield return new WaitForSeconds(0.5f);

            OnAnimationEnded?.Invoke();
        }

        public void ClearAnswer()
        {
            _input = -1;
            hasInput = false;
            _answerText.text = "";
        }

        public void Input(int userInput)
        {
            if (_input != -1 && _input != 0)
            {
                hasInput = false;
                _input = _input * 10 + userInput;
            }
            else
            {
                hasInput = true;
                _input = userInput;

            }
            _answerText.text = _input.ToString();
            CheckAnswer(_input);
        }

        public void CheckAnswer(int userAnswer)
        {
            int digitCount = (int)Math.Floor(Math.Log10(userAnswer) + 1);

            if (userAnswer == _answer)
            {
                _onMathCompleted(true);
            }
            else if (userAnswer != _answer && digitCount == 2)
            {
                _onMathCompleted(false);
            }
        }
    }
}
