using Sirenix.OdinInspector;
using System;

namespace CustomAttributes
{
    [IncludeMyAttributes]
    [FoldoutGroup("Debug")]
    public class Fg_DeAttribute : Attribute
    {
        public Fg_DeAttribute()
        {

        }
    }
    [IncludeMyAttributes]
    [FoldoutGroup("Components")]
    public class Fg_CoAttribute : Attribute
    {
        public Fg_CoAttribute()
        {

        }
    }

    [IncludeMyAttributes]
    [FoldoutGroup("Events")]
    public class Fg_EvAttribute : Attribute
    {
        public Fg_EvAttribute()
        {

        }
    }

    [IncludeMyAttributes]
    [FoldoutGroup("Settigns")]
    public class Fg_SeAttribute : Attribute
    {
        public Fg_SeAttribute()
        {

        }
    }
}