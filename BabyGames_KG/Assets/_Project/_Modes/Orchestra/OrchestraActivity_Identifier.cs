using _Project._Modes.Orchestra.Scripts;
using _Project._Modes.Orchestra.Scripts.SO;
using DataClasses;
using UnityEngine;

namespace Identifiers
{
    public class OrchestraActivity_Identifier : _ActivityBase_Identifier
    {
        private Orchestra_Data _orchestra_Data;

        public void Initialize(Orchestra_Data orchestra_Data)
        {
            _orchestra_Data = orchestra_Data;

            Get<OrchestraController>().doConfetti -= DoConfettin;
            Get<OrchestraController>().doConfetti += DoConfettin;
        }

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = ActivityType.Orchestra;
        }

        private async void DoConfettin(Transform dropZone_Identifier)
        {
            var confetti = await _orchestra_Data.confetti.InstantiateAsync();
            confetti.transform.position = dropZone_Identifier.position;

            confetti.Play();
        }
    }
}