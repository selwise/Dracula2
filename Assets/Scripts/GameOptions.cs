using UnityEngine;

public sealed class GameOptions : MonoBehaviour
{
    [Header("Starting Character")]
    public bool startAsRenfield = true;

    public AdventureCharacter StartingCharacter
    {
        get { return startAsRenfield ? AdventureCharacter.Renfield : AdventureCharacter.Dracula; }
    }
}
