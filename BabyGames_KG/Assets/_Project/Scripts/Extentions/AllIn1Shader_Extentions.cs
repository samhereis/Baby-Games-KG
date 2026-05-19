using AllIn1SpriteShader;
using UnityEngine;
using UnityEngine.UI;

namespace Extentions
{
    public static class AllIn1Shader_Extentions
    {
        public static void ForceSetNewMaterial(this AllIn1Shader allIn1Shader, Material material = null)
        {
            if (allIn1Shader == null)
            {
                return;
            }

            allIn1Shader.CleanMaterial();

            if (material != null)
            {
                if (allIn1Shader.transform is RectTransform)
                {
                    allIn1Shader.GetComponent<Graphic>().material = material;
                }
                else
                {
                    allIn1Shader.GetComponent<Renderer>().sharedMaterial = material;
                }
            }
            else
            {
                allIn1Shader.MakeNewMaterial(true);
                allIn1Shader.FindCurrMaterial().shader = Shader.Find("AllIn1SpriteShader/AllIn1Urp2dRenderer");
            }
        }

        public static void SetOutline(this AllIn1Shader allIn1Shader, bool isActive, float amount)
        {
            if (allIn1Shader == null)
            {
                return;
            }

            allIn1Shader.SetKeyword("OUTBASE_ON", isActive);
            allIn1Shader.SetKeyword("ONLYOUTLINE_ON", true);
            allIn1Shader.SetKeyword("ZOOMUV_ON", isActive);

            allIn1Shader.FindCurrMaterial().SetFloat("_ZoomUvAmount", 1.25f);
            allIn1Shader.FindCurrMaterial().SetFloat("_OutlineWidth", amount);
        }

        public static void SetGrayScale(this AllIn1Shader allIn1Shader, bool isActive, float? greyscaleLuminosity = null, float? greyscaleBlend = null)
        {
            if (allIn1Shader == null)
            {
                return;
            }

            allIn1Shader.SetKeyword("GREYSCALE_ON", isActive);

            if (greyscaleLuminosity != null)
            {
                allIn1Shader.FindCurrMaterial().SetFloat("_GreyscaleLuminosity", greyscaleLuminosity.Value);
            }

            if (greyscaleBlend != null)
            {
                allIn1Shader.FindCurrMaterial().SetFloat("_GreyscaleBlend", greyscaleBlend.Value);
            }
        }
    }
}