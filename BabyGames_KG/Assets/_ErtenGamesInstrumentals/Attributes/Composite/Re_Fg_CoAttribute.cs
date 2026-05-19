using Sirenix.OdinInspector;
using System;

namespace CustomAttributes
{
    [IncludeMyAttributes]
    [Required, FoldoutGroup("Components")]
    public class Re_Fg_CoAttribute : Attribute
    {
        public Re_Fg_CoAttribute()
        {

        }
    }

    [IncludeMyAttributes]
    [Required, FoldoutGroup("References")]
    public class Re_Fg_RefAttribute : Attribute
    {
        public Re_Fg_RefAttribute()
        {

        }
    }
}
