using Identifiers;
using UnityEngine;

namespace WhoLivesWhere
{
    public class WLW_TraktorSeat : IdentifierBase
    {
        public Transform holder;

        public bool hasCharacter => Get<SpriteRenderer>();
    }
}