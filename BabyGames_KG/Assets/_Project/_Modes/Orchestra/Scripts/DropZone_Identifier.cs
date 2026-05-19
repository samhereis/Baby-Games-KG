using _Project._Modes.Orchestra.Scripts;
using AllIn1SpriteShader;
using Extentions;
using Identifiers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Modes.Sorting
{
    public class DropZone_Identifier : IdentifierBase
    {
        [field: SerializeField] public List<SpriteRenderer> spriteRenderer { get; private set; }
        [field: SerializeField] public List<AllIn1Shader> allIn1Shader { get; private set; }

        [Button]
        public void Initialize(OrchestraWaveUnit orchestraWaveUnit)
        {
            allIn1Shader = TryGetAll_List<AllIn1Shader>();

            foreach (var item in spriteRenderer)
            {
                item.material = Resources.Load<Material>("Materials/Wave_DragZone");
                item.sortingLayerName = orchestraWaveUnit.orchestraCharacter_Identifier.sortingLayerName;
            }

            foreach (var item in allIn1Shader)
            {
                item.ForceSetNewMaterial(item.GetComponent<Renderer>().material);
            }

            IsDroppingCorrectly(false);
        }

        public void IsDroppingCorrectly(bool isValid)
        {
            return;

            if (isValid)
            {

                foreach (var item in allIn1Shader)
                {
                    item.SetGrayScale(false);
                }
            }
            else
            {

                foreach (var item in allIn1Shader)
                {
                    item.SetGrayScale(true);
                }
            }
        }
    }
}